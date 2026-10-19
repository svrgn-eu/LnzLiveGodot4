using Godot;

namespace PetzRendering.Services;

public interface ISettingsService
{
	string SettingsPath { get; }
	void Load();
	void Save();
	bool HasSectionKey(string section, string key);
	Variant GetVariant(string section, string key, Variant defaultValue = default);
	bool GetBool(string section, string key, bool defaultValue = false);
	int GetInt(string section, string key, int defaultValue = 0);
	float GetFloat(string section, string key, float defaultValue = 0.0f);
	string GetString(string section, string key, string defaultValue = "");
	void SetVariant(string section, string key, Variant value, bool autoSave = true);
	string[] GetSectionKeys(string section);
}
