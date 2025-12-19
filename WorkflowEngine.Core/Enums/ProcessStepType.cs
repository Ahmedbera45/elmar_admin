namespace WorkflowEngine.Core.Enums;

public enum ProcessStepType
{
    Start = 1,
    UserTask = 2,
    SystemTask = 3,
    Signing = 4,
    Normal = 5,
    Approval = 6,
    End = 99
}
