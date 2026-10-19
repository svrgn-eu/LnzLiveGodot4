using System;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using PetzRendering.Services;

namespace PetzRendering.Core;

/// <summary>
/// Godot Autoload singleton that initializes the Microsoft.Extensions.DependencyInjection
/// service container on startup and exposes application-wide services.
/// </summary>
public partial class ServiceBootstrapper : Node
{
	private static IServiceProvider? _services;

	public static ServiceBootstrapper? Instance { get; private set; }

	public static IServiceProvider Services => _services ??= BuildServiceProvider();

	public static T GetService<T>() where T : notnull
	{
		return Services.GetRequiredService<T>();
	}

	public override void _EnterTree()
	{
		Instance = this;
		_services ??= BuildServiceProvider();
		var logger = GetService<ILogService>();
		logger.LogInfo("ServiceBootstrapper initialized dependency injection container.", nameof(ServiceBootstrapper));
	}

	public static IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configureOverrides = null)
	{
		var serviceCollection = new ServiceCollection();
		ConfigureServices(serviceCollection);
		configureOverrides?.Invoke(serviceCollection);
		_services = serviceCollection.BuildServiceProvider();
		return _services;
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<ISettingsService, SettingsService>();
		services.AddSingleton<ILogService, LogService>();
		services.AddSingleton<IFeatureService, FeatureService>();
		services.AddSingleton<IKeyBallsService, KeyBallsService>();
	}
}
