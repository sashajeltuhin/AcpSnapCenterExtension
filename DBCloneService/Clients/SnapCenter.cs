using Apprenda.Services.Logging;
using DBCloning.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DBCloning.Clients
{
    public class SnapCenter
    {
        private int CLONE_DELAY_MAX_ATTEMPTS = 20;
        private int CLONE_QUERY_INTERVAL = 5000;
        private int BACKUP_DETAIL_MAX_ATTEMPTS = 20;
        private int BACKUP_QUERY_INTERVAL = 5000;
        private readonly ILogger log = LogManager.Instance().GetLogger(typeof(ACP));
        private readonly RestClient client;
        private string token;
        private static string snapUrl = string.Empty;

        private SnapCenter(string baseUrl)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            this.client = new RestClient(baseUrl);
        }

        internal static async Task<SnapCenter> NewSnapCenterSession(SnapSession session)
        {
            snapUrl = session.Url;
            var client = new SnapCenter(snapUrl);
            User u = new User { Name = session.Admin, Passphrase = session.Pass, Rolename = "SnapCenterAdmin" };
            UserOperationContext c = new UserOperationContext { User = u };
            AuthRequest body = new AuthRequest { UserOperationContext = c };

            try
            {
                var authResponse = await client.SendRequestAsync<dynamic>(Method.POST, "api/3.0/auth/login?TokenNeverExpires=true", body);
                client.log.Info($"Payload: {authResponse.Payload}");
                client.log.Info($"User obj in payload: { authResponse.Payload.User}");
                string t = authResponse.Payload.User.Token;
                t = t.Trim('\r', '\n');
                client.token = t;
                client.log.Info($"Token after trimming: {client.token}");
                return client;
            }
            catch (Exception ex)
            {
                client.log.Error($"Error getting authentication token: {ex}");
                throw;
            }
        }

        internal async Task<string> CreateResource(SnapSession session)
        {
            string answer = string.Empty;
            try
            {
                log.Info($"Token = {this.token}");
                ResourceBody body = new ResourceBody();
                body.ResourceName = session.DbName;
                body.ResourceType = "Database";
                body.HostName = session.HostName;
                body.RunAsNames = $"{session.DbName}_{session.Plugin}";
                body.MountPaths = new System.Collections.Generic.List<MountInfo>();
                MountInfo mi = new MountInfo();
                mi.MountPath = session.MountPath;
                body.MountPaths.Add(mi);
                VolumeMapping vm = new VolumeMapping();
                VolumeName vname = new VolumeName();
                vname.Name = session.VolumeName;
                vm.VolumeName = vname;
                FootPrintObject fp = new FootPrintObject();
                fp.SVMName = session.SvmName;
                fp.VolAndLunsMapping = new System.Collections.Generic.List<VolumeMapping>();
                fp.VolAndLunsMapping.Add(vm);
                body.FootPrint = new System.Collections.Generic.List<FootPrintObject>();
                body.FootPrint.Add(fp);
                PluginParams pluginParams = new PluginParams();
                pluginParams.Data = new System.Collections.Generic.List<PluginData>();
                PluginData ms = new PluginData();
                ms.Key = "MASTER_SLAVE";
                ms.Value = "N";
                pluginParams.Data.Add(ms);
                body.PluginParams = pluginParams;

                var response = await this.SendRequestAsync<dynamic>(Method.POST, $"api/3.0/plugins/MySQL/resources", body, false);
                log.Info($"Payload: {response.Payload}");
                string dbKey = string.Empty;
                answer = response.Response.Content.ToString();
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while getting creating resource key: {ex}");
            }
            return answer;
        }

        internal async Task<string> GetDbKey(SnapSession session, bool originalDetails)
        {
            try
            {
                log.Info($"Token = {this.token}");
                var response = await this.SendRequestAsync<dynamic>(Method.GET, $"api/3.0/hosts/{session.HostName}/plugins/MySQL/resources?ResourceType=Database&ResourceName={session.DbName}");
                log.Info($"Payload: {response.Payload}");
                string dbKey = string.Empty;
                
                if (response.Response.StatusCode.ToString() == "OK")
                {
                    dbKey = response.Payload.Resources[0].OperationResults[0].Target.Key;
                    dbKey = dbKey.Trim('\r', '\n');
                    
                    log.Info($"dbKey after trimming: {dbKey}");
                  

                    if (originalDetails)
                    {   // RunAsName and SvmName are only coming from the original
                        session.RunAsName = response.Payload.Resources[0].OperationResults[0].Target.Auth.RunAsName;
                        session.SvmName = response.Payload.Resources[0].OperationResults[0].Target.SmAppFileStorageGroups[0].StorageFootPrint.StorageSystemResources[0].Volume.Vserver;
                        // session.junctionPath = response.Payload.Resources[0].OperationResults[0].Target.SmAppFileStorageGroups[0].StorageFootPrint.StorageSystemResources[0].Volume.JunctionPath;
                    }

                    // volume name are unique for each instance
                    session.VolumeName = response.Payload.Resources[0].OperationResults[0].Target.SmAppFileStorageGroups[0].StorageFootPrint.StorageSystemResources[0].Volume.Name;
                }
                return dbKey;

            }
            catch (Exception ex)
            {
                this.log.Error($"Error while getting DB key: {ex}");
                throw ex;
            }
        }

        //Function to Get Clone Junction Path   
        internal async Task<string> GetJunctionPath (SnapSession session, bool originalDetails)
        {
            try
            {
                log.Info($"Token = {this.token}");
                var response = await this.SendRequestAsync<dynamic>(Method.GET, $"api/3.0/hosts/{session.HostName}/plugins/MySQL/resources?ResourceType=Database&ResourceName={session.DbName}");
                log.Info($"Payload: {response.Payload}");
               
                string junctionPath = string.Empty;
                if (response.Response.StatusCode.ToString() == "OK")
                {
                    
                    junctionPath = response.Payload.Resources[0].OperationResults[0].Target.SmAppFileStorageGroups[0].StorageFootPrint.StorageSystemResources[0].Volume.JunctionPath;
                    
                    log.Info($"JunectionPath after trimming: {junctionPath}");

                
                }
                return junctionPath;

            }
            catch (Exception ex)
            {
                this.log.Error($"Error while getting Junction Path: {ex}");
                throw ex;
            }
        }

        internal async Task<string> ProtectResource(SnapSession session)
        {
            try
            {
                ProtectionBody body = new ProtectionBody();
                body.ProtectionGroup = new ProtectionGroup();
                body.ProtectionGroup.ProtectionGroupType = "Backup";
                ProtectionPolicy p = new ProtectionPolicy();
                p.Name = session.Policy;
                body.ProtectionGroup.Policies = new System.Collections.Generic.List<ProtectionPolicy>();
                body.ProtectionGroup.Policies.Add(p);
                PluginConfiguration pconf = new PluginConfiguration();
                pconf.type = "SMCoreContracts.SmSCBackupConfiguration, SMCoreContracts";
                pconf.FileSystemConsistentSnapshot = false;
                ProtectionConfiguration conf = new ProtectionConfiguration();
                conf.type = "SMCoreContracts.SmSCBackupConfiguration, SMCoreContracts";
                conf.Name = "rg_internal";
                conf.ConfigurationType = "ProtectionGroup";
                conf.PluginConfiguration = pconf;
                body.ProtectionGroup.Configuration = conf;
                var response = await this.SendRequestAsync<dynamic>(Method.POST, $"api/3.0/plugins/{session.Plugin}/resources/{session.DbKey}/protect", body, false);
                this.log.Info($"Protect Resource Response: {response.Response.Content.ToString()}");
                return response.Response.Content.ToString();
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while protecting resource with key {session.DbKey}: {ex}");
                throw;
            }
        }

        internal async Task<string> BackUp(SnapSession session)
        {
            try
            {
                BackupBody body = new BackupBody();
                body.name = session.Policy;
                var response = await this.SendRequestAsync<dynamic>(Method.POST, $"api/3.0/plugins/{session.Plugin}/resources/{session.DbKey}/backup",body,false);
             
                log.Info($"Payload Backup: {response.Payload}");
                string jobUri = string.Empty;
                for (int x = 0; x < response.Response.Headers.Count; x++)
                {
                    Parameter p = response.Response.Headers[x];
                    log.Info($"Backup Header: {p.Name} = {p.Value}");
                    if (p.Name == "JobURI")
                    {
                        jobUri = p.Value.ToString();
                        break;
                    }
                }
                string backUpJobID = string.Empty;
                if (string.IsNullOrEmpty(jobUri) == false)
                {
                    jobUri = jobUri.Trim('\r', '\n');
                    log.Info($"jobUri: {jobUri}");
                    string[] paths = jobUri.Split('/');
                    if (paths.Length > 0)
                    {
                        backUpJobID = paths[paths.Length - 1];
                        log.Info($"backUpJobID: {backUpJobID}");
                    }
                }
                else
                {
                    string errmsg = "Backup Header joburi not found";
                    log.Info(errmsg);
                    throw new Exception(errmsg);
                }
                return backUpJobID;
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while performing backup: {ex}");
                throw;
            }
        }

        internal async Task<BackUp> BackUpDetails(SnapSession session)
        {
            try
            {
                ClientResponse<SnapBackupResponse> response = await this.SendRequestAsync<SnapBackupResponse>(Method.GET, $"api/3.0/backups?JobId={session.BackUpJobID}");
                log.Info($"Payload Backup Detail: {response.Payload}");
                BackUp theBackup = null;
                if (response.Payload.Backups != null)
                {
                    //todo: check for the right one
                    foreach(BackUp b in response.Payload.Backups)
                    {
                        theBackup = b;
                        log.Info($"The Backup Detail: {theBackup.BackupId} {theBackup.BackupName}");
                    }

                }
                if(theBackup == null)
                {
                    log.Info($"No backup details for jobid {session.BackUpJobID}");
                }
                
                return theBackup;


            }
            catch (Exception ex)
            {
                this.log.Error($"Error while getting backup details for job id {session.BackUpJobID}: {ex}");
                throw;
            }
        }

        internal async Task<string> Clone(SnapSession session)
        {
            try
            {
                PrimaryBackup b = new PrimaryBackup();
                b.BackupName = session.BackupName;
                CloneBody body = new CloneBody();
                CloneConfigurationApplication cloneConfApp = new CloneConfigurationApplication();
                cloneConfApp.type = "SMCoreContracts.SmSCCloneConfiguration, SMCoreContracts";
                cloneConfApp.MountCmd = new System.Collections.Generic.List<string>();
                cloneConfApp.MountCmd.Add($"mount {session.LeafIP}:%mysql_vol_Clone {session.MountPath}");
                cloneConfApp.PostCloneCreateCmd = new System.Collections.Generic.List<string>();
                cloneConfApp.PostCloneCreateCmd.Add($"{session.MountScript}");
                cloneConfApp.Host = session.CloneHostName;
                CloneConfiguration conf = new CloneConfiguration();
                conf.type = "SMCoreContracts.SmCloneConfiguration, SMCoreContracts";
                conf.Suffix = string.Empty; // string.Format("_{0}", session.AppName); //set app alias to distiguish clones
                conf.CloneConfigurationApplication = cloneConfApp;
                body.CloneConfiguration = conf;
                body.Backups = new System.Collections.Generic.List<Backups>();
                Backups back = new Backups();
                back.PrimaryBackup = b;
                body.Backups.Add(back);
                var response = await this.SendRequestAsync<dynamic>(Method.POST, $"api/3.0/plugins/{session.Plugin}/resources/{session.DbKey}/clone", body, false);
                return response.Response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while cloning {session.DbKey}: {ex}");
                throw;
            }
        }

        internal async Task<string> CloneOriginal(SnapSession snapSession)
        {
            try
            {
                //create resource in SnapCenter, if does not exist
                //string r = await CreateResource(snapSession);
                //log.Info($"Called create resource. Response: {r}.");

                snapSession.DbKey = await GetDbKey(snapSession, true);
                log.Info($"DB Key received {snapSession.DbKey}.");

                try
                {
                    this.log.Info($"Protecting resource");
                    await ProtectResource(snapSession);
                }
                catch(Exception protectEx)
                {
                    log.Error($"Protect resource failed. Contnuing cloning...", protectEx);
                }

                this.log.Info($"Initiating backup");
                snapSession.BackUpJobID = await BackUp(snapSession);

                this.log.Info($"Getting backup details for jobID {snapSession.BackUpJobID}");

                BackUp b = null;
                int attempts = 0;
                bool stop = false;
                while (!stop)
                {
                    b = await BackUpDetails(snapSession);
                    attempts++;
                    this.log.Info($"Tried getting backup details for jobid {snapSession.BackUpJobID}. Attempt {attempts}");
                    stop = attempts >= BACKUP_DETAIL_MAX_ATTEMPTS || b != null;
                    if (b == null)
                    {
                        if (stop)
                        {
                            string err = $"The number of attempts {attempts} to get back up details exceeded the configurable threshold of {BACKUP_DETAIL_MAX_ATTEMPTS}. Stopping cloning as backup details are unavailable";
                            this.log.Info(err);
                            throw new Exception(err);

                        }
                        else
                        {
                            this.log.Info($"Backup details not available yet. Will try again in 5 sec");
                            Thread.Sleep(BACKUP_QUERY_INTERVAL);
                        }
                    }
                    else
                    {
                        this.log.Info($"Backup details loaded. Stoping the loop: {stop}");
                    }
                }

                snapSession.BackUpID = b.BackupId;
                snapSession.BackupName = b.BackupName;

                this.log.Info($"Cloning from backup {snapSession.BackUpID}, name = {snapSession.BackupName}");
                string answer = await Clone(snapSession);
                this.log.Info($"DB Cloning complete.");
                return answer;
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while cloning {snapSession.DbName}: {ex}");
                throw;
            }
        }

        internal async Task<string> SnapshotClone(SnapSession session)
        {
            SnapSession cloneSession = session.Clone();
            try
            {
                int attempts = 0;
                bool stop = false;
                while (!stop)
                {
                    cloneSession.DbKey = await this.GetDbKey(cloneSession, false);
                    
                    this.log.Info($"Cloning session data: {cloneSession.toString()}");
                    attempts++;
                    this.log.Info($"Tried getting DB Key for {cloneSession.DbName}. Attempt {attempts}");
                    stop = attempts >= CLONE_DELAY_MAX_ATTEMPTS || !string.IsNullOrEmpty(cloneSession.DbKey);
                    if (string.IsNullOrEmpty(cloneSession.DbKey))
                    {
                        if (stop)
                        {
                            string err = $"The number of attempts {attempts} to get DB key exceeded the configurable threshold of {CLONE_DELAY_MAX_ATTEMPTS}. Stopping snapshotting as DB Key is not unavailable";
                            this.log.Info(err);
                            throw new Exception(err);

                        }
                        else
                        {
                            this.log.Info($"DB Key not available yet. Will try again in 5 sec");
                            Thread.Sleep(CLONE_QUERY_INTERVAL);
                        }
                    }
                    else
                    {
                        this.log.Info($"DB Key loaded. Stoping the loop: {stop}");
                    }
                }
                
                log.Info($"DB Key of the clone received: {cloneSession.DbKey}");
                this.log.Info($"Sending PUT request for the clone");
                await PrepForSnapshot(cloneSession);
                try
                {
                    this.log.Info($"Protecting clone");
                    await this.ProtectResource(cloneSession);
                }
                catch (Exception protectEx)
                {
                    log.Error($"Protect resource failed. Conitnuing with clone snapshot...", protectEx);
                }
                this.log.Info($"Initiating backup");
                cloneSession.BackUpJobID = await this.BackUp(cloneSession);
                return cloneSession.BackUpJobID;
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while snapshotting the clone {cloneSession.DbName}: {ex}");
                throw;
            }
        }

        internal BackUp GetCloneSnapshot(SnapSession session)
        {
            try
            {
                ClientResponse<SnapBackupResponse> response = this.SendRequest<SnapBackupResponse>(Method.GET, $"api/3.0/backups?Resource={session.DbName}");
                log.Info($"Payload Get List of backups for DB {session.DbName}: {response.Payload.ToString()}");
                if (response.Payload.Backups.Count > 0)
                {
                    log.Info($"Snapshots found. Using last...");
                    return response.Payload.Backups[response.Payload.Backups.Count - 1];
                }
                else
                {
                    log.Info($"No backup details for jobid {session.BackUpJobID}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while getting backup details for job id {session.BackUpJobID}: {ex}");
                throw;
            }
        }



        internal async Task<string> PrepForSnapshot(SnapSession session)
        {
            try
            {
                SnapShotPutBody body = new SnapShotPutBody();
                body.RunAsNames = session.RunAsName;
                VolumeMapping vm = new VolumeMapping();
                VolumeName vname = new VolumeName();
                vname.Name = session.VolumeName;
                vm.VolumeName = vname;
                FootPrintObject fp = new FootPrintObject();
                fp.SVMName = session.SvmName;
                fp.VolAndLunsMapping = new System.Collections.Generic.List<VolumeMapping>();
                fp.VolAndLunsMapping.Add(vm);
                body.FootPrint = new System.Collections.Generic.List<FootPrintObject>();
                body.FootPrint.Add(fp);
                PluginParams pluginParams = new PluginParams();
                pluginParams.Data = new System.Collections.Generic.List<PluginData>();
                PluginData port = new PluginData();
                port.Key = "PORT";
                port.Value = "3306";
                pluginParams.Data.Add(port);
                PluginData ms = new PluginData();
                ms.Key = "MASTER_SLAVE";
                ms.Value = "N";
                pluginParams.Data.Add(ms);
                PluginData host = new PluginData();
                host.Key = "HOST";
                host.Value = session.CloneHostName;
                pluginParams.Data.Add(host);
                PluginData uid = new PluginData();
                uid.Key = "CLONE_UID";
                uid.Value = SnapSession.BuildCloneName(session.DbName, session.AppName);
                pluginParams.Data.Add(uid);
                body.PluginParams = pluginParams;

                ClientResponse<SnapBackupResponse> response = await this.SendRequestAsync<SnapBackupResponse>(Method.PUT, $"api/3.0/plugins/{session.Plugin}/resources/{session.DbKey}", body, false);
                log.Info($"Payload PUT data Detail: {response.Payload}");
                
                return response.Response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                this.log.Error($"Error executing PUT data for snapshot of clone with key {session.DbKey}: {ex}");
                throw;
            }
        }

        internal async Task<string> RestoreClone(SnapSession session)
        {
            try
            {
                

                session.HostName = session.CloneHostName;
                log.Info($"restoring clone with session: {session.toString()}");
                session.DbKey = await this.GetDbKey(session, false);
                log.Info($"DB Key of the clone received: {session.DbKey}");
                // Got Junction Path : This parameter is needed for Restore API Call
                session.JunctionPath = await this.GetJunctionPath(session, false);
                log.Info($"JunctionPath of the clone received: {session.JunctionPath}");


               

                PrimaryBackup b = new PrimaryBackup();
                b.BackupName = session.BackupName;
                RestoreBody body = new RestoreBody();
                RestoreConfiguration cloneConfApp = new RestoreConfiguration();
                cloneConfApp.type = "SMCoreContracts.SmSCRestoreConfiguration, SMCoreContracts";
                //Extra Parameters needed in Restore Body:-    
                cloneConfApp.MountCommands = new System.Collections.Generic.List<string>();
                cloneConfApp.MountCommands.Add($"mount {session.LeafIP}:{session.JunctionPath} {session.MountPath} ; sleep 3; /var/lib/start-mysql.sh");

                cloneConfApp.UnMountCommands = new System.Collections.Generic.List<string>();
                cloneConfApp.UnMountCommands.Add($"killall -KILL mysqld ; sleep 3 ; umount {session.MountPath}");

                Backups back = new Backups();
                back.PrimaryBackup = b;
                body.BackupInfo = back;
                body.PluginCode = "SCC";
                body.RestoreLastBackup = 0;
                body.Configuration = cloneConfApp;

                var response = await this.SendRequestAsync<dynamic>(Method.POST, $"api/3.0/plugins/{session.Plugin}/resources/{session.DbKey}/restore", body, false);
                return response.Response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                this.log.Error($"Error while cloning {session.DbKey}: {ex}");
                throw;
            }
        }

        private async Task<ClientResponse<TPayload>> SendRequestAsync<TPayload>(Method method, string restUrl, object body = null, bool json = true)
        {
            var request = new RestRequest(restUrl, method);
            log.Info($"Request url: {this.client.BaseUrl}{restUrl}");

            if (!string.IsNullOrWhiteSpace(this.token))
            {
                log.Info($"Adding token header to the request: {this.token}");
                request.AddHeader("token", this.token);
            }

            if (body != null)
            {
                string serialized = JsonConvert.SerializeObject(body);
                log.Info($"Request body: {serialized}");
                request.AddParameter("application/json", serialized, ParameterType.RequestBody);
            }

            var response = await this.client.ExecuteTaskAsync(request);
            this.log.Info($"Response Status Code: {response.StatusCode}");
            this.log.Info($"Response Error Message: {response.ErrorMessage}");
            if (response.ErrorException != null)
            {
                this.log.Info($"Response Error Message: {response.ErrorException.ToString()}");
            }
            this.log.Info($"Response Response Status: {response.ResponseStatus}");
            this.log.Info($"Response Status Description: {response.StatusDescription}");
            this.log.Info($"Response Content: {response.Content}");

            if ((int)response.StatusCode < 400 || response.StatusCode == HttpStatusCode.NotFound)
            {
                ClientResponse < TPayload > r = new ClientResponse<TPayload>{ Response = response };
                if (json)
                {
                    r.Payload = JsonConvert.DeserializeObject<TPayload>(response.Content);
                }

                return r;
            }

            throw new Exception($"Request failed with status code {response.StatusCode}: {response.Content}");
        }

        private ClientResponse<TPayload> SendRequest<TPayload>(Method method, string restUrl, object body = null, bool json = true)
        {
            var request = new RestRequest(restUrl, method);
            log.Info($"Request url: {this.client.BaseUrl}{restUrl}");

            if (!string.IsNullOrWhiteSpace(this.token))
            {
                log.Info($"Adding token header to the request: {this.token}");
                request.AddHeader("token", this.token);
            }

            if (body != null)
            {
                string serialized = JsonConvert.SerializeObject(body);
                log.Info($"Request body: {serialized}");
                request.AddParameter("application/json", serialized, ParameterType.RequestBody);
            }

            var response = this.client.Execute(request);
            this.log.Info($"Response Status Code: {response.StatusCode}");
            this.log.Info($"Response Error Message: {response.ErrorMessage}");
            if (response.ErrorException != null)
            {
                this.log.Info($"Response Error Message: {response.ErrorException.ToString()}");
            }
            this.log.Info($"Response Response Status: {response.ResponseStatus}");
            this.log.Info($"Response Status Description: {response.StatusDescription}");
            this.log.Info($"Response Content: {response.Content}");

            if ((int)response.StatusCode < 400 || response.StatusCode == HttpStatusCode.NotFound)
            {
                ClientResponse<TPayload> r = new ClientResponse<TPayload> { Response = response };
                if (json)
                {
                    r.Payload = JsonConvert.DeserializeObject<TPayload>(response.Content);
                }

                return r;
            }

            throw new Exception($"Request failed with status code {response.StatusCode}: {response.Content}");
        }

    }
}
