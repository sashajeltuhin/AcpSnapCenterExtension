namespace DBCloning.Models
{
    public static class CustomProperties
    {
        public const string DBCloneType = "DBCloneType";
        public const string DBUser = "DBUser";
        public const string DBUserCreds = "DBUserCreds";
        public const string SnapPlugin = "SnapPlugin"; //"MySQL"
        public const string SnapCenterUrl = "SnapCenterUrl";
        public const string SnapCenterAdmin = "SnapCenterAdmin";
        public const string SnapCenterPassword = "SnapCenterPassword";
        public const string SnapDBName = "SnapDBName";
        public const string SnapDBHost = "SnapDBHost";
        public const string SnapDBCloneHost = "SnapDBCloneHost";
        public const string SnapPolicy = "SnapPolicy";
        public const string SnapMountScript = "SnapMountScript";
        public const string SnapDataLeafIP = "SnapDataLeafIP"; //for clone
        public const string SnapMountPath = "SnapMountPath"; //for clone

        public static string SnapMYSQLStart { get; set; }
    }

    public static class DBCloneTypes
    {
        //None, RestoreClone, CloneOriginal, KeepExisting 
        public const string None = "None";
        public const string RestoreClone = "RestoreClone";
        public const string CloneOriginal = "CloneOriginal";
        public const string KeepExisting = "KeepExisting";
    }
}
 