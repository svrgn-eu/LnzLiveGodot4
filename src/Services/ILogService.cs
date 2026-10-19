using System;

namespace PetzRendering.Services;

/// <summary>
/// Contract for the structured log4net logging service.
/// </summary>
public interface ILogService
{
	/// <summary>
	/// Logs a message at the DEBUG level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Debug(string message);

	/// <summary>
	/// Logs a message at the INFO level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Info(string message);

	/// <summary>
	/// Logs a message at the WARN level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Warn(string message);

	/// <summary>
	/// Logs a message at the ERROR level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Error(string message);

	/// <summary>
	/// Logs a message and exception at the ERROR level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The exception to log.</param>
	void Error(string message, Exception exception);

	/// <summary>
	/// Logs a message at the FATAL level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Fatal(string message);

	/// <summary>
	/// Logs a message and exception at the FATAL level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The exception to log.</param>
	void Fatal(string message, Exception exception);

	/// <summary>
	/// Logs a message with an optional category prefix at the DEBUG level.
	/// </summary>
	void LogDebug(string message, string? category = null);

	/// <summary>
	/// Logs a message with an optional category prefix at the INFO level.
	/// </summary>
	void LogInfo(string message, string? category = null);

	/// <summary>
	/// Logs a message with an optional category prefix at the WARN level.
	/// </summary>
	void LogWarning(string message, string? category = null);

	/// <summary>
	/// Logs a message with an optional category prefix at the ERROR level.
	/// </summary>
	void LogError(string message, string? category = null);
}
