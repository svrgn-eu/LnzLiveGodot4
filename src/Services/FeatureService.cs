using System;
using System.Collections.Generic;

namespace PetzRendering.Services;

public sealed class FeatureService : IFeatureService
{
	public const string FeaturesSection = "features";

	private readonly ISettingsService _settingsService;
	private readonly ILogService _logService;
	private readonly Dictionary<string, bool> _featureCache = new(StringComparer.OrdinalIgnoreCase);

	public FeatureService(ISettingsService settingsService, ILogService logService)
	{
		_settingsService = settingsService;
		_logService = logService;

		foreach (var key in _settingsService.GetSectionKeys(FeaturesSection))
		{
			_featureCache[key] = _settingsService.GetBool(FeaturesSection, key, true);
		}
	}

	public bool IsEnabled(string featureKey, bool defaultValue = true)
	{
		if (_featureCache.TryGetValue(featureKey, out var enabled))
		{
			return enabled;
		}

		var resolved = _settingsService.GetBool(FeaturesSection, featureKey, defaultValue);
		_featureCache[featureKey] = resolved;
		return resolved;
	}

	public void SetEnabled(string featureKey, bool enabled)
	{
		_featureCache[featureKey] = enabled;
		_settingsService.SetVariant(FeaturesSection, featureKey, enabled);
		_logService.LogInfo($"Feature '{featureKey}' set to {enabled}.", nameof(FeatureService));
	}

	public IReadOnlyDictionary<string, bool> GetAllFeatures()
	{
		return _featureCache;
	}
}
