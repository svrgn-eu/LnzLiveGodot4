namespace PetzRendering.Services;

public enum LogLevel
{
	Debug = 0,
	Info = 1,
	Warning = 2,
	Error = 3
}

public interface ILogService
{
	LogLevel MinimumLevel { get; set; }
	void LogDebug(string message, string? category = null);
	void LogInfo(string message, string? category = null);
	void LogWarning(string message, string? category = null);
	void LogError(string message, string? category = null);
}
