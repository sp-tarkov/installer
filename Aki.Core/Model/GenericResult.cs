namespace SPT_AKI_Installer.Aki.Core.Model
{
    public class GenericResult
    {
        public string Message { get; private set; }

        public bool Succeeded { get; private set; }
        public bool NonCritical { get; private set; }

        protected GenericResult(string message, bool succeeded, bool nonCritical = false)
        {
            Message = message;
            Succeeded = succeeded;
            NonCritical = nonCritical;
        }

        public static GenericResult FromSuccess(string message = "") => new GenericResult(message, true);
        public static GenericResult FromError(string errorMessage) => new GenericResult(errorMessage, false);
        public static GenericResult FromWarning(string warningMessage) => new GenericResult(warningMessage, false, true);
    }
}
