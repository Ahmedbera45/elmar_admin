namespace WorkflowEngine.Core.Constants;

public static class Permissions
{
    public static class Process
    {
        public const string Create = "Permissions.Process.Create";
        public const string View = "Permissions.Process.View";
        public const string Edit = "Permissions.Process.Edit";
        public const string Delete = "Permissions.Process.Delete";
    }

    public static class Request
    {
        public const string Create = "Permissions.Request.Create";
        public const string View = "Permissions.Request.View";
        public const string Approve = "Permissions.Request.Approve";
        public const string Reject = "Permissions.Request.Reject";
    }
}
