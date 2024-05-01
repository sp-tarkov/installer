using SPTInstaller.Interfaces;

namespace SPTInstaller.Models;

public class Result : IResult
{
    public bool Succeeded { get; private set; }
    
    public string Message { get; private set; }
    
    protected Result(string message, bool succeeded)
    {
        Message = message;
        Succeeded = succeeded;
    }
    
    public static Result FromSuccess(string message = "") => new(message, true);
    public static Result FromError(string message) => new(message, false);
}