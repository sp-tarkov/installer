namespace SPTInstaller.Models;

public class ReadProcessResult : Result
{
    public string StdOut { get; }
    public string StdErr { get; }
    
    protected ReadProcessResult(string message, bool succeeded, string stdOut = "", string stdErr = "") : base(message,
        succeeded)
    {
        StdOut = stdOut;
        StdErr = stdErr;
    }
    
    public static ReadProcessResult FromSuccess(string stdOut, string stdErr) =>
        new ReadProcessResult("ok", true, stdOut, stdErr);
    
    public new static ReadProcessResult FromError(string message) => new ReadProcessResult(message, false);
}