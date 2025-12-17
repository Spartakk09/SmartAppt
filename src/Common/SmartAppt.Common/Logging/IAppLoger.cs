namespace SmartAppt.Common.Logging;

public interface IAppLogger<T>
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception ex);
}
