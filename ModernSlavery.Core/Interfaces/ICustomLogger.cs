namespace ModernSlavery.Core.Interfaces
{
    public interface ICustomLogger
    {
        void Debug(string message, object values = null);
        void Information(string message, object values = null);
        void Warning(string message, object values = null);
        void Error(string message, object values = null);
        void Fatal(string message, object values = null);

    }
}