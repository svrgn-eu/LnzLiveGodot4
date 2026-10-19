using System;
using Godot;

namespace PetzRendering.Services;

public sealed class SettingsService : ISettingsService
{
	private readonly ConfigFile _config = new();

	public string SettingsPath { get; } = "user://settings.cfg";

	public SettingsService()
	{
		Load();
	}

	public void Load()
	{
		if (FileAccess.FileExists(SettingsPath))
		{
			_config.Load(SettingsPath);
		}
	}

	public void Save()
	{
		_config.Save(SettingsPath);
	}

	public bool HasSectionKey(string section, string key)
	{
		return _config.HasSectionKey(section, key);
	}

	public Variant GetVariant(string section, string key, Variant defaultValue = default)
	{
		if (_config.HasSectionKey(section, key))
		{
			return _config.GetValue(section, key, defaultValue);
		}

		return defaultValue;
	}

	public bool GetBool(string section, string key, bool defaultValue = false)
	{
		if (_config.HasSectionKey(section, key))
		{
			return _config.GetValue(section, key, defaultValue).AsBool();
		}

		return defaultValue;
	}

	public int GetInt(string section, string key, int defaultValue = 0)
	{
		if (_config.HasSectionKey(section, key))
		{
			return _config.GetValue(section, key, defaultValue).AsInt32();
		}

		return defaultValue;
	}

	public float GetFloat(string section, string key, float defaultValue = 0.0f)
	{
		if (_config.HasSectionKey(section, key))
		{
			return _config.GetValue(section, key, defaultValue).AsSingle();
		}

		return defaultValue;
	}

	public string GetString(string section, string key, string defaultValue = "")
	{
		if (_config.HasSectionKey(section, key))
		{
			return _config.GetValue(section, key, defaultValue).AsString();
		}

		return defaultValue;
	}

	public void SetVariant(string section, string key, Variant value, bool autoSave = true)
	{
		_config.SetValue(section, key, value);
		if (autoSave)
		{
			Save();
		}
	}

	public string[] GetSectionKeys(string section)
	{
		if (!_config.HasSection(section))
		{
			return Array.Empty<string>();
		}

		return _config.GetSectionKeys(section);
	}
}
