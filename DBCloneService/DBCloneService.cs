using Apprenda.SaaSGrid;
using Apprenda.SaaSGrid.Extensions;
using Apprenda.SaaSGrid.Extensions.DTO;
using Apprenda.Services.Logging;
using DBCloning.Clients;
using DBCloning.Models;
using DBCloning.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
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
                    //log.Info($"Loading components  for {version.ApplicationAlias}, {version.Alias}");
                    //var components = (await acpClient.GetComponentsAsync(
                    //    version.ApplicationAlias, version.Alias)).Payload;
                    //log.Info($"Loaded components for {version.ApplicationAlias}, {version.Alias}");
                    //if (components != null)
                    //{
                    //    foreach (Component c in components)
                    //    {
                    //        log.Info($"Component: {c.Name} {c.Alias}");
                    //        var props = (await acpClient.GetComponentCustomPropertiesAsync(
                    //            version.ApplicationAlias, version.Alias, c.Alias)).Payload;
                    //        foreach (CustomProperty p in props)
                    //        {
                    //            log.Info($"Props: {p.PropertyModel.Name}");
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    log.Info($"No components found for {version.ApplicationAlias}, {version.Alias}");
                    //}


                    log.Info($"Loading SOC properties");
                    
                    var socprops = (await acpClient.GetAllCustomPropertiesAsync()).Payload;
                    if (socprops != null)
                    {
                        if (socprops.items != null && socprops.items.Count > 0)
                        {
                            log.Info($"Loaded SOC properties");
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


                            //todo: dynamically, based on policies, figure out a host where clones need to be created
                            var hostNameProp = socprops.items.First(p =>
                                    p.name == CustomProperties.SnapDBHost);

                            if (hostNameProp != null && hostNameProp.valueOptions.defaultValues != null && hostNameProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.HostName = hostNameProp.valueOptions.defaultValues[0];
                            }

                            var cloneHostNameProp = socprops.items.First(p =>
                                   p.name == CustomProperties.SnapDBCloneHost);

                            if (cloneHostNameProp != null && cloneHostNameProp.valueOptions.defaultValues != null && cloneHostNameProp.valueOptions.defaultValues.Count > 0)
                            {
                                this.snapSession.CloneHostName = cloneHostNameProp.valueOptions.defaultValues[0];
                            }

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
                        log.Info($"Initiating DB cloning for  {version.ApplicationAlias}. SnapSession: {snapSession.toString()}");
                        this.log.Info($"Obtaining DB key {snapSession.DbName}.");
                        SnapCenter snapClient = await SnapCenter.NewSnapCenterSession(snapSession);
                        snapSession.DbKey = await snapClient.GetDbKey(snapSession);
                        log.Info($"DB Key received {snapSession.DbKey}.");
                    
                        this.log.Info($"Protecting resource");
                        await snapClient.ProtectResource(this.snapSession);

                        this.log.Info($"Initiating backup");
                        snapSession.BackUpJobID = await snapClient.BackUp(snapSession);

                        this.log.Info($"Getting backup details for jobID {snapSession.BackUpJobID}");
                        BackUp b = await snapClient.BackUpDetails(snapSession);
                        snapSession.BackUpID = b.BackupId;
                        snapSession.BackupName = b.BackupName;

                        this.log.Info($"Cloning");
                        await snapClient.Clone(snapSession);
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
    }
}
