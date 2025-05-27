namespace SPTInstaller.Interfaces;

public interface IResult
{
    public bool Succeeded { get; }
    public string Message { get; }
}