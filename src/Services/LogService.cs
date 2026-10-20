using System;
using System.IO;
using System.Reflection;
using System.Text;
using Godot;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace PetzRendering.Services;

/// <summary>
/// Implementation of the structured logging service backed by Apache log4net.
/// </summary>
public class LogService : ILogService
{
	#region Properties

	/// <summary>
	/// The underlying log4net logger instance used for all logging operations.
	/// </summary>
	private readonly ILog _logger;

	#endregion Properties

	#region Constructors

	#region LogService
	/// <summary>
	/// Initializes log4net from the project's log4net.config file
	/// and redirects the file appender output to the Godot user data directory.
	/// </summary>
	public LogService()
	{
		// Suppress log4net's own internal debug output.
		// On .NET 6+ (Godot Mono) log4net probes ConfigurationManager.AppSettings
		// during static init, which throws PlatformNotSupportedException.
		// Disabling InternalDebugging silences those cosmetic errors before we
		// configure the repository ourselves via XmlConfigurator below.
		log4net.Util.LogLog.InternalDebugging = false;

		// Pre-populate LogFilePath property so XmlConfigurator activates RollingFileAppender directly in user://logs/
		string userLogDir = ProjectSettings.GlobalizePath("user://logs");
		if (!Directory.Exists(userLogDir))
		{
			Directory.CreateDirectory(userLogDir);
		}
		GlobalContext.Properties["LogFilePath"] = Path.Combine(userLogDir, "petzrendering.log");

		// Resolve the Godot resource path to an absolute filesystem path
		string configPath = ProjectSettings.GlobalizePath("res://log4net.config");

		ILoggerRepository repository = LogManager.GetRepository(
			Assembly.GetExecutingAssembly()
		);

		if (File.Exists(configPath))
		{
			XmlConfigurator.Configure(repository, new FileInfo(configPath));
		}
		else if (Godot.FileAccess.FileExists("res://log4net.config"))
		{
			using var configFile = Godot.FileAccess.Open("res://log4net.config", Godot.FileAccess.ModeFlags.Read);
			if (configFile != null)
			{
				using var stream = new MemoryStream(Encoding.UTF8.GetBytes(configFile.GetAsText()));
				XmlConfigurator.Configure(repository, stream);
			}
			else
			{
				BasicConfigurator.Configure(repository);
			}
		}
		else
		{
			// Fallback: configure with basic defaults if config file is missing
			BasicConfigurator.Configure(repository);
		}

		// Redirect the RollingFileAppender to write inside user://logs/
		this.RedirectFileAppender(repository);

		_logger = LogManager.GetLogger(
			Assembly.GetExecutingAssembly(),
			"PetzRendering"
		);

		_logger.Info("LogService initialized.");
	}
	#endregion LogService

	#endregion Constructors

	#region Methods

	#region RedirectFileAppender
	/// <summary>
	/// Finds the RollingFileAppender in the log4net configuration and
	/// updates its file path to the Godot user data directory.
	/// </summary>
	/// <param name="repository">The log4net logger repository.</param>
	private void RedirectFileAppender(ILoggerRepository repository)
	{
		if (repository is Hierarchy hierarchy)
		{
			foreach (IAppender appender in hierarchy.Root.Appenders)
			{
				if (appender is RollingFileAppender rollingFileAppender)
				{
					string userLogDir = ProjectSettings.GlobalizePath("user://logs");

					if (!Directory.Exists(userLogDir))
					{
						Directory.CreateDirectory(userLogDir);
					}

					rollingFileAppender.File = Path.Combine(userLogDir, "petzrendering.log");
					rollingFileAppender.ActivateOptions();
				}
			}
		}
	}
	#endregion RedirectFileAppender

	#region Debug
	/// <summary>
	/// Logs a message at the DEBUG level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public void Debug(string message)
	{
		this._logger.Debug(message);
	}
	#endregion Debug

	#region Info
	/// <summary>
	/// Logs a message at the INFO level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public void Info(string message)
	{
		this._logger.Info(message);
	}
	#endregion Info

	#region Warn
	/// <summary>
	/// Logs a message at the WARN level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public void Warn(string message)
	{
		this._logger.Warn(message);
	}
	#endregion Warn

	#region Error
	/// <summary>
	/// Logs a message at the ERROR level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public void Error(string message)
	{
		this._logger.Error(message);
	}

	/// <summary>
	/// Logs a message and exception at the ERROR level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The exception to log.</param>
	public void Error(string message, Exception exception)
	{
		this._logger.Error(message, exception);
	}
	#endregion Error

	#region Fatal
	/// <summary>
	/// Logs a message at the FATAL level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public void Fatal(string message)
	{
		this._logger.Fatal(message);
	}

	/// <summary>
	/// Logs a message and exception at the FATAL level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The exception to log.</param>
	public void Fatal(string message, Exception exception)
	{
		this._logger.Fatal(message, exception);
	}
	#endregion Fatal

	#region Category Helpers
	public void LogDebug(string message, string? category = null) => Debug(FormatCategory(message, category));
	public void LogInfo(string message, string? category = null) => Info(FormatCategory(message, category));
	public void LogWarning(string message, string? category = null) => Warn(FormatCategory(message, category));
	public void LogError(string message, string? category = null) => Error(FormatCategory(message, category));

	private static string FormatCategory(string message, string? category)
	{
		return string.IsNullOrWhiteSpace(category) ? message : $"[{category}] {message}";
	}
	#endregion Category Helpers

	#endregion Methods
}
