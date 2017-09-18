using System.Runtime.Serialization;

namespace DBCloning.Models
{
    [DataContract]
    public class SnapSession
    {
        private string plugin;
        private string url;
        private string admin;
        private string pass;
        private string dbKey;
        private string dbName;
        private string hostName;
        private string cloneHostName;
        private string appName;
        private string policy;
        private string mountScript;
        private string leafIP;
        private string mountPath;
        private string backUpJobID;
        private string backUpID;
        private string backupName;

        [DataMember]
        public string DbKey { get => dbKey; set => dbKey = value; }
        [DataMember]
        public string DbName { get => dbName; set => dbName = value; }
        [DataMember]
        public string HostName { get => hostName; set => hostName = value; }
        [DataMember]
        public string AppName { get => appName; set => appName = value; }
        [DataMember]
        public string Url { get => url; set => url = value; }
        [DataMember]
        public string Admin { get => admin; set => admin = value; }
        [DataMember]
        public string Pass { get => pass; set => pass = value; }
        [DataMember]
        public string Policy { get => policy; set => policy = value; }
        [DataMember]
        public string Plugin { get => plugin; set => plugin = value; }
        [DataMember]
        public string MountScript { get => mountScript; set => mountScript = value; }
        [DataMember]
        public string LeafIP { get => leafIP; set => leafIP = value; }
        [DataMember]
        public string MountPath { get => mountPath; set => mountPath = value; }
        [DataMember]
        public string BackUpJobID { get => backUpJobID; set => backUpJobID = value; }
        [DataMember]
        public string BackUpID { get => backUpID; set => backUpID = value; }
        [DataMember]
        public string CloneHostName { get => cloneHostName; set => cloneHostName = value; }
        [DataMember]
        public string BackupName { get => backupName; set => backupName = value; }

        public bool isValid()
        {
            return !string.IsNullOrEmpty(this.Plugin) && !string.IsNullOrEmpty(this.Url) && !string.IsNullOrEmpty(this.Admin) && !string.IsNullOrEmpty(this.Pass)
                && !string.IsNullOrEmpty(this.DbName) && !string.IsNullOrEmpty(this.HostName) && !string.IsNullOrEmpty(this.CloneHostName) && !string.IsNullOrEmpty(this.Policy) && !string.IsNullOrEmpty(this.MountPath)
                && !string.IsNullOrEmpty(this.MountScript) && !string.IsNullOrEmpty(this.LeafIP);
        }

        public string toString()
        {
            return $"Url = {this.Url}; Pass = {this.Pass}; Admin = {this.Admin}; Host = {this.hostName}; CloneHost = {this.CloneHostName}; DB = {this.DbName}; DBKey = {this.DbKey}; Policy = {this.Policy}; Plugin = {this.Plugin}; MountScript = {this.MountScript}; MountPath = {this.MountPath}; Leaf IP = {this.LeafIP}";
        }
    }
}
