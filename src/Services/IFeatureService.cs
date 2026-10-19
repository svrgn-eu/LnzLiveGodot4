using System.Collections.Generic;

namespace PetzRendering.Services;

public interface IFeatureService
{
	bool IsEnabled(string featureKey, bool defaultValue = true);
	void SetEnabled(string featureKey, bool enabled);
	IReadOnlyDictionary<string, bool> GetAllFeatures();
}
