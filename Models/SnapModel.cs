using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBCloning.Models
{

    public class ResourceBody
    {
        public string ResourceName { get; set; }
        public string ResourceType { get; set; }
        public string HostName { get; set; }
        public string RunAsNames { get; set; }
        public List<MountInfo> MountPaths { get; set; }
        public List<FootPrintObject> FootPrint { get; set; }
        public PluginParams PluginParams { get; set; }
    }

    public class MountInfo
    {
        public string MountPath { get; set; }
    }

    public class BackupBody
    {
        public string name { get; set; }
    }

    public class ProtectionBody
    {
        public ProtectionGroup ProtectionGroup { get; set; }
    }
    public class ProtectionGroup
    {
        public ProtectionConfiguration Configuration { get; set; }
        public List<ProtectionPolicy> Policies { get; set; }
        public string ProtectionGroupType { get; set; }
    }

    public class ProtectionConfiguration
    {
        [JsonProperty("@type")]
        public string type { get; set; }
        public string Name { get; set; }
        public PluginConfiguration PluginConfiguration { get; set; }
        public string ConfigurationType { get; set; }
    }

    public class PluginConfiguration
    {
        [JsonProperty("@type")]
        public string type { get; set; }
        public bool FileSystemConsistentSnapshot { get; set; }
    }

    public class ProtectionPolicy
    {
        public string Name { get; set; }
    }

    public class SnapBackupResponse
    {
        public Job Job { get; set; }
        public List<BackUp> Backups { get; set; }
    }
    public class Job
    {
        public string Name { get; set; }
    }
    public class BackUp
    {
        public string BackupName { get; set; }
        public string BackupId { get; set; }
        public string BackupTime { get; set; }
        public bool IsClone { get; set; }
        public BackupAuth Auth { get; set; }
    }

    public class BackupAuth
    {
        public string RunAsName { get; set; }
    }

    public class SnapShotPutBody
    {
        public string RunAsNames { get; set; }
        public List<FootPrintObject> FootPrint { get; set; }
        public PluginParams PluginParams { get; set; }
    }

    public class FootPrintObject
    {
        public string SVMName { get; set; }
        public List<VolumeMapping> VolAndLunsMapping { get; set; }
    }

    public class VolumeMapping
    {
        public VolumeName VolumeName { get; set; }
    }

    public class PluginParams
    {
        public List<PluginData> Data { get; set; }
    }

    public class PluginData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class VolumeName
    {
        public string Name { get; set; }
    }

    public class CloneBody
    {
        public CloneConfiguration CloneConfiguration { get; set; }
        public List<Backups> Backups { get; set; }
    }

    public class CloneConfigurationApplication
    {
        [JsonProperty("$type")]
        public string type { get; set; }
        public List<string> MountCmd { get; set; }
        public List<string> PostCloneCreateCmd { get; set; }
        public string Host { get; set; }
    }

    public class CloneConfiguration
    {
        [JsonProperty("$type")]
        public string type { get; set; }
        public string Suffix { get; set; }
        public CloneConfigurationApplication CloneConfigurationApplication { get; set; }
    }

    public class RestoreBody
    {
        public RestoreConfiguration Configuration { get; set; }
        public Backups BackupInfo { get; set; }
        public int RestoreLastBackup { get; set; }
        public string PluginCode { get; set; }
    }

    public class RestoreConfiguration
    {
        [JsonProperty("$type")]
        public string type { get; set; }
        public List<string> MountCommands { get; set; }
        public List<string> UnMountCommands { get; set; }
    }

    public class PrimaryBackup
    {
        public string BackupName { get; set; }
    }

    public class Backups
     {
         public PrimaryBackup PrimaryBackup { get; set; }
     }

//Auth
public class AuthRequest
    {
        public UserOperationContext UserOperationContext { get; set; }
    }

    public class UserOperationContext
    {
        public User User { get; set; }
    }


    public class User
    {
        public string Name { get; set; }
        public string Passphrase { get; set; }
        public string Rolename { get; set; }
    }
}

