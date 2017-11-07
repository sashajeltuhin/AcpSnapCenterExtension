using System.Runtime.Serialization;

namespace DBCloning.Models
{
    [DataContract]
    public class SnapSession
    {
        private string clonetype;
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
        //auxiliary props for PUT request
        private string runasname;
        private string svmname;
        private string volumename;
        [DataMember]
        public string RunAsName { get => runasname; set => runasname = value; }
        [DataMember]
        public string SvmName { get => svmname; set => svmname = value; }
        [DataMember]
        public string VolumeName { get => volumename; set => volumename = value; }

        [DataMember]
        public string CloneType { get => clonetype; set => clonetype = value; }
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
            return !string.IsNullOrEmpty(this.CloneType) && !string.IsNullOrEmpty(this.Plugin) && !string.IsNullOrEmpty(this.Url) && !string.IsNullOrEmpty(this.Admin) && !string.IsNullOrEmpty(this.Pass)
                && !string.IsNullOrEmpty(this.DbName) && !string.IsNullOrEmpty(this.HostName) && !string.IsNullOrEmpty(this.CloneHostName) && !string.IsNullOrEmpty(this.Policy) && !string.IsNullOrEmpty(this.MountPath)
                && !string.IsNullOrEmpty(this.MountScript) && !string.IsNullOrEmpty(this.LeafIP);
        }

        public string toString()
        {
            return $"Clone Type = {CloneType}; Url = {this.Url}; Pass = {this.Pass}; Admin = {this.Admin}; Host = {this.hostName}; CloneHost = {this.CloneHostName}; DB = {this.DbName}; DBKey = {this.DbKey}; Policy = {this.Policy}; Plugin = {this.Plugin}; MountScript = {this.MountScript}; MountPath = {this.MountPath}; Leaf IP = {this.LeafIP}; RunAsName = {RunAsName}; SvmName = {SvmName}; VolumeName = {VolumeName}; BackupName = {BackupName}";
        }

        public static string BuildCloneName(string dbName, string appAlias)
        {
            return string.Format("{0}_{1}", dbName, appAlias); //clones have appname suffix
        }

        public SnapSession Clone()
        {
            SnapSession snapSession = new SnapSession();
            snapSession.dbKey = string.Empty;
            snapSession.Plugin = this.Plugin;
            snapSession.AppName = this.AppName;
            snapSession.Url = this.Url;
            snapSession.Admin = this.Admin;
            snapSession.Pass = this.Pass;
            snapSession.DbName = this.DbName;
            snapSession.HostName = this.CloneHostName;
            snapSession.CloneHostName = this.CloneHostName;
            snapSession.Policy = this.Policy;
            snapSession.MountPath = this.MountPath;
            snapSession.MountScript = this.MountScript;
            snapSession.LeafIP = this.LeafIP;
            snapSession.BackUpJobID = string.Empty;
            snapSession.BackUpID = string.Empty;
            snapSession.BackupName = string.Empty;
            snapSession.RunAsName = this.RunAsName;
            snapSession.SvmName = this.SvmName;
            snapSession.VolumeName = this.VolumeName;
            snapSession.CloneType = this.CloneType;
            return snapSession;
        }
    }
}
