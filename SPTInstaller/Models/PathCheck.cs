namespace SPTInstaller.Models;

public enum PathCheckType
{
    EndsWith = 0,
    Contains = 1,
    DriveRoot = 2
}

public enum PathCheckAction
{
    Warn = 0,
    Deny = 1,
}

public class PathCheck
{
    public string Target { get; private set; }
    public PathCheckType CheckType { get; private set; }
    public PathCheckAction CheckAction { get; private set; }

    public PathCheck()
    {
    }
    
    public PathCheck(string target, PathCheckType checkType, PathCheckAction checkAction)
    {
        Target = target;
        CheckType = checkType;
        CheckAction = checkAction;
    }
}