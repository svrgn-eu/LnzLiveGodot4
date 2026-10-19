using System;
using Godot;

namespace PetzRendering.Services;

public sealed class LogService : ILogService
{
	public const string SettingsSection = "logging";
	public const string MinimumLevelKey = "minimum_level";

	public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

	public LogService(ISettingsService settingsService)
	{
		var configuredLevel = settingsService.GetString(SettingsSection, MinimumLevelKey, nameof(LogLevel.Info));
		if (Enum.TryParse<LogLevel>(configuredLevel, ignoreCase: true, out var parsedLevel))
		{
			MinimumLevel = parsedLevel;
		}
	}

	public void LogDebug(string message, string? category = null)
	{
		if (MinimumLevel <= LogLevel.Debug)
		{
			GD.Print(FormatMessage("DEBUG", message, category));
		}
	}

	public void LogInfo(string message, string? category = null)
	{
		if (MinimumLevel <= LogLevel.Info)
		{
			GD.Print(FormatMessage("INFO", message, category));
		}
	}

	public void LogWarning(string message, string? category = null)
	{
		if (MinimumLevel <= LogLevel.Warning)
		{
			var formatted = FormatMessage("WARN", message, category);
			GD.PushWarning(formatted);
			GD.Print(formatted);
		}
	}

	public void LogError(string message, string? category = null)
	{
		if (MinimumLevel <= LogLevel.Error)
		{
			var formatted = FormatMessage("ERROR", message, category);
			GD.PushError(formatted);
			GD.PrintErr(formatted);
		}
	}

	private static string FormatMessage(string level, string message, string? category)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss");
		return string.IsNullOrWhiteSpace(category)
			? $"[{timestamp}] [{level}] {message}"
			: $"[{timestamp}] [{level}] [{category}] {message}";
	}
}
