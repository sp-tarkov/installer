namespace SPT_AKI_Installer.Aki.Core.Model
{
    public class GenericResult
    {
        public string Message { get; private set; }

        public bool Succeeded { get; private set; }

        protected GenericResult(string message, bool succeeded)
        {
            Message = message;
            Succeeded = succeeded;
        }

        public static GenericResult FromSuccess(string message = "") => new GenericResult(message, true);
        public static GenericResult FromError(string errorMessage) => new GenericResult(errorMessage, false);
    }
}
