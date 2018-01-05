using Apprenda.SaaSGrid;
using Apprenda.SaaSGrid.Extensions;
using Apprenda.SaaSGrid.Extensions.DTO;
using Apprenda.Services.Logging;
using DBCloning.Clients;
using DBCloning.Models;
using DBCloning.Properties;
using SocModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBCloning
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class DBCloneService : DeveloperPortalExtensionServiceBase, IDBCloneService
    {
        private readonly ILogger log = LogManager.Instance().GetLogger(typeof(DBCloneService));
        private SnapSession snapSession;

        public SnapSession GetSnapCenterSession()
        {
            return this.snapSession;
        }

        public override void OnPromotingVersion(
          ReadOnlyVersionDataDTO version,
          ApplicationVersionStageDTO proposedStage)
        {
            Task.Run(async () =>
            {
                try
                {
                    this.snapSession = new SnapSession();
                    log.Info($"Authenticating with Apprenda. App = {version.ApplicationAlias}, Token=  {SessionContext.Instance.SessionToken}.");
                    snapSession.AppName = version.ApplicationAlias;
                    var acpClient = new ACP(SessionContext.Instance.SessionToken);

                    try
                    {
                        log.Info($"Loading Developer properties");
                   
                        var customProperties = acpClient.GetComponentCustomProperties(
                            version.ApplicationAlias, version.Alias, string.Empty).Payload;
                        log.Info($"Loaded Developer custom properties");
                        foreach (CustomProperty cp in customProperties)
                        {
                            log.Info($"Prop Name: {cp.PropertyModel.Name}, Value: {cp.Values.First()}");
                        }
                        var devDbCloneType = customProperties.First(p =>
                            p.PropertyModel.Name == CustomProperties.DBCloneType);
                        log.Info($"Loaded clone type property and it is not null {devDbCloneType != null}");
                        if (devDbCloneType != null)
                        {
                            this.snapSession.CloneType = devDbCloneType.Values.First();
                        }
                        log.Info($"Loaded Developer settings for clone type {this.snapSession.CloneType}");
                    }
                    catch(Exception ex)
                    {
                        log.Error("Issues loading developer properties. Falling back on SOC defaults", ex);
                    }

                    log.Info($"Loading SOC properties");
                    
                    var socprops = (await acpClient.GetAllCustomPropertiesAsync()).Payload;
                    if (socprops != null)
                    {
                        if (socprops.items != null && socprops.items.Count > 0)
                        {
                            log.Info($"Loaded SOC properties");
                            var snapCloneType = socprops.items.First(p =>
                            p.name == CustomProperties.DBCloneType);

                            if (string.IsNullOrEmpty(this.snapSession.CloneType) || (snapCloneType != null && snapCloneType.valueOptions.defaultValues != null && snapCloneType.valueOptions.defaultValues.Count > 0))
                            {
                                this.snapSession.CloneType = snapCloneType.valueOptions.defaultValues[0];
                            }

                            var snapPluginProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapPlugin);

                            if (snapPluginProp != null && snapPluginProp.valueOptions.defaultValues != null && snapPluginProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.Plugin = snapPluginProp.valueOptions.defaultValues[0];
                            }


                            var snapUrlProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapCenterUrl);

                            if (snapUrlProp != null && snapUrlProp.valueOptions.defaultValues != null && snapUrlProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.Url = snapUrlProp.valueOptions.defaultValues[0];
                            }


                            var snapAdminProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapCenterAdmin);
                            if (snapAdminProp != null && snapAdminProp.valueOptions.defaultValues != null && snapAdminProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.Admin = snapAdminProp.valueOptions.defaultValues[0];
                            }

                            var snapPassProp = socprops.items.First(p =>
                                p.name == CustomProperties.SnapCenterPassword);
                            if (snapPassProp != null && snapPassProp.valueOptions.defaultValues != null && snapPassProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.Pass = snapPassProp.valueOptions.defaultValues[0];
                            }

                            var dbNameProp = socprops.items.First(p =>
                                    p.name == CustomProperties.SnapDBName);
                            if (dbNameProp != null && dbNameProp.valueOptions.defaultValues != null && dbNameProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.DbName = dbNameProp.valueOptions.defaultValues[0];
                            }

                        
                            var hostNameProp = socprops.items.First(p =>
                                    p.name == CustomProperties.SnapDBHost);

                            if (hostNameProp != null && hostNameProp.valueOptions.defaultValues != null && hostNameProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.HostName = hostNameProp.valueOptions.defaultValues[0];
                            }

                            var cloneHostNameProp = socprops.items.First(p =>
                                   p.name == CustomProperties.SnapDBCloneHost);
                            this.log.Info($"Processing clone hostname settings");
                            if (cloneHostNameProp != null && cloneHostNameProp.valueOptions.defaultValues != null && cloneHostNameProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.CloneHostName = cloneHostNameProp.valueOptions.defaultValues[0];
                                this.log.Info($"Going with the default value {this.snapSession.CloneHostName}.");
                            }
                            else if (cloneHostNameProp != null && cloneHostNameProp.valueOptions.possibleValues != null && cloneHostNameProp.valueOptions.possibleValues.Count == 1)
                            {
                                this.log.Info($"Default is not set. Only one possible value found = {cloneHostNameProp.valueOptions.possibleValues[0]}. Use it");
                                this.snapSession.CloneHostName = cloneHostNameProp.valueOptions.possibleValues[0];     
                            }
                            else if (cloneHostNameProp != null && cloneHostNameProp.valueOptions.possibleValues != null && cloneHostNameProp.valueOptions.possibleValues.Count > 0)
                            {
                                this.log.Info($"Default is not set and multiple servers are configured. Will figure out which of the configured servers is less busy");
                                string preferred = FindTargetNode(acpClient, cloneHostNameProp.valueOptions.possibleValues);
                                this.log.Info($"The preferred server is {preferred}. Will use it");
                                this.snapSession.CloneHostName = preferred;
                            }
                            else
                            {
                                log.Error($"Error cloning Db {snapSession.DbName} for application {version.ApplicationAlias}: The Clone Host Name property is not configured");
                                return;
                            }
                            this.log.Info($"CloneHostName set to {this.snapSession.CloneHostName}.");

                            var policyProp = socprops.items.First(p =>
                                    p.name == CustomProperties.SnapPolicy);

                            if (policyProp != null && policyProp.valueOptions.defaultValues != null && policyProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.Policy = policyProp.valueOptions.defaultValues[0];
                            }

                            var snapScriptProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapMountScript);

                            if (snapScriptProp != null && snapScriptProp.valueOptions.defaultValues != null && snapScriptProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.MountScript = snapScriptProp.valueOptions.defaultValues[0];
                            }

                            var snapDataLeafProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapDataLeafIP);

                            if (snapDataLeafProp != null && snapDataLeafProp.valueOptions.defaultValues != null && snapDataLeafProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.LeafIP = snapDataLeafProp.valueOptions.defaultValues[0];
                            }


      


                            var snapMountPathProp = socprops.items.First(p =>
                            p.name == CustomProperties.SnapMountPath);

                            if (snapMountPathProp != null && snapMountPathProp.valueOptions.defaultValues != null && snapMountPathProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.MountPath = snapMountPathProp.valueOptions.defaultValues[0];
                            }
                        }
                    }
                    else
                    {
                        this.log.Info($"Cannot load SOC properties. Will try to use configs");
                        this.snapSession.Plugin = Settings.Default.SnapPlugin;
                        this.snapSession.Url = Settings.Default.SnapCenterUrl;
                        this.snapSession.Admin = Settings.Default.SnapCenterAdmin;
                        this.snapSession.Pass = Settings.Default.SnapCenterPassword;
                        this.snapSession.DbName = Settings.Default.SnapDBName;
                        this.snapSession.HostName = Settings.Default.SnapDBHost;
                        this.snapSession.CloneHostName = Settings.Default.SnapDBCloneHost;
                        this.snapSession.Policy = Settings.Default.SnapPolicy;
                        this.snapSession.MountPath = Settings.Default.SnapMountPath;
                        this.snapSession.MountScript = Settings.Default.SnapMountScript;
                        this.snapSession.LeafIP = Settings.Default.SnapDataLeafIP;
                    }

                    this.log.Info($"Snap session: {this.snapSession.toString()}"); 
                    if (this.snapSession.isValid())
                    {
                        //check the type of cloning requested
                        switch (this.snapSession.CloneType)
                        {
                            case DBCloneTypes.CloneOriginal:
                                CloneOriginal(snapSession);
                                break;
                            case DBCloneTypes.RestoreClone:
                                RestoreClone(snapSession);
                                break;
                            default: 
                                {
                                    BackUp b = FindSnapshot(snapSession);
                                    if (b == null) //if clones do not exist, clone the original
                                    {
                                        this.log.Info($"Clone not found. Performing cloning of the original");
                                        CloneOriginal(snapSession);
                                    }
                                    else
                                    {
                                        this.log.Info($"Clone found. Do nothing. Will use existing copy");
                                        //do nothing is the app needs to conect to the existing clone
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        this.log.Info($"Snap data incomplete. Aborting...");
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error cloning Db {snapSession.DbName} for application {version.ApplicationAlias}: {ex}");
                }
            }).GetAwaiter().GetResult();
        }

        private void CloneOriginal(SnapSession snapSession)
        {
            Task.Run(async () =>
            { 
                log.Info($"Initiating DB cloning for  {snapSession.AppName}. SnapSession: {snapSession.toString()}");
                this.log.Info($"Obtaining DB key {snapSession.DbName}.");
                SnapCenter snapClient = await SnapCenter.NewSnapCenterSession(snapSession);
                await snapClient.CloneOriginal(snapSession);

                string snapshotID = await snapClient.SnapshotClone(snapSession);
                this.log.Info($"Clone backup complete. ID: {snapshotID}");
            }).GetAwaiter().GetResult();
         }

        private void RestoreClone(SnapSession snapSession)
        {
            Task.Run(async () =>
            {
                log.Info($"Trying to restore DB for application  {snapSession.AppName}. SnapSession: {snapSession.toString()}");
                SnapCenter snapClient = await SnapCenter.NewSnapCenterSession(snapSession);
                BackUp b = snapClient.GetCloneSnapshot(snapSession);
                this.log.Info($"GetCloneSnapshot existed and b is not null: {b != null}");
                if (b!=null)
                {
                    //restore snapshot
                    snapSession.BackupName = b.BackupName;
                    string answer = await snapClient.RestoreClone(snapSession);
                    this.log.Info($"Clone restore complete. Response: {answer}");
                }
                else
                {
                    this.log.Info($"No snapshots found proceding to cloning the original.");
                    await snapClient.CloneOriginal(snapSession);

                    string snapshotID = await snapClient.SnapshotClone(snapSession);
                    this.log.Info($"Clone backup complete. ID: {snapshotID}");
                }
            }).GetAwaiter().GetResult();
        }

        private BackUp FindSnapshot(SnapSession snapSession)
        {
            BackUp b = null;
            Task.Run(async () =>
            {
                this.log.Info($"Obtaining DB key {snapSession.DbName}.");
                SnapCenter snapClient = await SnapCenter.NewSnapCenterSession(snapSession);
                b = snapClient.GetCloneSnapshot(snapSession);
            }).GetAwaiter().GetResult();
            return b;
        }


        private string FindTargetNode(ACP acpClient, List<string> requested)
        {
            string hostName = string.Empty;
            try
            {
                NodePageResourceBase data = acpClient.GetNodeData();
                double smallest = double.MaxValue;
                foreach(Node n in data.items)
                {
                    foreach(NodeRole nr in n.nodeRoles)
                    {
                        double perMem = nr.allocatedMemory / nr.totalMemory;
                        if (smallest > perMem && requested.Contains(n.name))
                        {
                            smallest = perMem;
                            hostName = n.name;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                this.log.Error($"Issues when finding the least loaded node on Apprenda: {ex}");
            }
            
            return hostName;
        }
    }
}
