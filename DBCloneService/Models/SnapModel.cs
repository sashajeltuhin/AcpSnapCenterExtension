using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBCloning.Models
{
    public class ProtectionBody
    {
        public ProtectionGroup ProtectionGroup { get; set; }
    }

    public class BackupBody
    {
        public string name { get; set; }
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
    }

    public class CloneBody
    {
        public CloneConfiguration CloneConfiguration { get; set; }
        public List<PrimaryBackup> Backups { get; set; }
    }

    public class CloneConfigurationApplication
    {
        [JsonProperty("@type")]
        public string type { get; set; }
        public List<string> MountCmd { get; set; }
        public List<string> PostCloneCreateCmd { get; set; }
        public string Host { get; set; }
    }

    public class CloneConfiguration
    {
        [JsonProperty("@type")]
        public string type { get; set; }
        public string Suffix { get; set; }
        public CloneConfigurationApplication CloneConfigurationApplication { get; set; }
    }

    public class PrimaryBackup
    {
        public string BackupName { get; set; }
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
