# Log4Net Logging Service with Godot Appender and Rolling Logfile

## 📖 Overview
Replace the basic `GD.Print`-only `LogService` with a structured, configurable **Apache log4net** logging implementation (`ILogService` / `LogService`). This feature adds a custom `GodotAppender` that routes formatted log events directly to Godot's Output panel (`GD.Print` and `GD.PrintErr`) alongside a `RollingFileAppender` that persists rotating log files into Godot's user data directory (`user://logs/petzrendering.log`).

## 🎯 Goals & Use Cases
- **Goal 1:** Provide a battle-tested, structured logging pipeline backed by `log4net` and registered as a singleton in the `Microsoft.Extensions.DependencyInjection` container (`ServiceBootstrapper`).
- **Goal 2:** Output real-time formatted log messages to the Godot Editor / console via a custom `GodotAppender` (`AppenderSkeleton` implementation) while simultaneously writing persistent, size-rolled log files to disk (`user://logs/petzrendering.log`).
- **Goal 3:** Allow external configuration of log levels, layouts, and appenders via a declarative `res://log4net.config` XML configuration file without recompiling C# code.
- **Use Case A:** During development in the Godot Editor, a developer triggers `.lnz` parsing or 3D pet generation; `INFO` and `DEBUG` messages appear in the Godot Output panel via `GD.Print`, while `WARN`, `ERROR`, and `FATAL` messages are highlighted as errors via `GD.PrintErr`.
- **Use Case B:** When troubleshooting an issue after a session or in an exported build, a user or developer inspects `user://logs/petzrendering.log` (and its size-rolled backups) containing full timestamps, thread IDs, log levels, logger names, and exception stack traces.

## 👥 Target Audience / User Roles
- **Developers & Maintainers:** Engineers working on the C# Petz rendering and `.lnz` editing codebase who need structured diagnostics, exception logging, and persistent log history.
- **Advanced Users / Bug Reporters:** Users who can attach `petzrendering.log` from their Godot user data folder when reporting parsing or rendering issues.

## ⚙️ Functional Requirements
- [x] **Requirement 1:** Add the `log4net` NuGet package reference to `src/PetzRendering.csproj`.
- [x] **Requirement 2:** Implement `GodotAppender` (`PetzRendering.Services.GodotAppender`) inheriting from `log4net.Appender.AppenderSkeleton`:
  - Render log events using the configured `log4net` layout (`RenderLoggingEvent(loggingEvent).TrimEnd()`).
  - Route events with level `Level.Warn` or higher (`WARN`, `ERROR`, `FATAL`) to `GD.PrintErr`.
  - Route events below `Level.Warn` (`DEBUG`, `INFO`) to `GD.Print`.
- [x] **Requirement 3:** Create `src/log4net.config` (`res://log4net.config`) configuring:
  - `GodotAppender` with a concise `PatternLayout` for the Godot Output console (`[%date{HH:mm:ss}] [%-5level] %message%newline`).
  - `RollingFileAppender` with size-based rolling (`5MB` max size, up to `5` backup files) and a detailed `PatternLayout` including date/time, thread, level, logger, and message (`%date{yyyy-MM-dd HH:mm:ss.fff} [%thread] %-5level %logger - %message%newline`).
- [x] **Requirement 4:** Update `ILogService` and `LogService` (`src/Services/ILogService.cs`, `src/Services/LogService.cs`) to:
  - Disable `log4net.Util.LogLog.InternalDebugging` during initialization to avoid cosmetic `PlatformNotSupportedException` warnings on .NET 6+/10.
  - Load `res://log4net.config` via `ProjectSettings.GlobalizePath` and `XmlConfigurator.Configure` (with a `BasicConfigurator.Configure` fallback if the config file is missing).
  - Dynamically redirect any `RollingFileAppender` in the repository hierarchy to write into `ProjectSettings.GlobalizePath("user://logs")/petzrendering.log`, creating the directory if needed and calling `ActivateOptions()`.
  - Expose standard logging methods: `Debug(string)`, `Info(string)`, `Warn(string)`, `Error(string)`, `Error(string, Exception)`, `Fatal(string)`, `Fatal(string, Exception)`, as well as category-aware helper methods (`LogDebug`, `LogInfo`, `LogWarning`, `LogError`).

## 🎨 UI/UX Design & Interactions
- **Godot Output Console:** Log entries appear cleanly formatted as `[HH:mm:ss] [LEVEL] message`, with warnings and errors automatically routed through `GD.PrintErr` so they stand out in the Godot Editor's Output and Debugger panels.
- **Persistent Log File:** Automatically created at `<Godot user:// directory>/logs/petzrendering.log` with full millisecond timestamps: `yyyy-MM-dd HH:mm:ss.fff [thread] LEVEL Logger - message`.

## 💻 Technical Details
- **Project File (`src/PetzRendering.csproj`):**
  - Added `<PackageReference Include="log4net" Version="3.*" />`.
- **Configuration (`src/log4net.config`):**
  - XML configuration defining `GodotAppender` (`type="PetzRendering.Services.GodotAppender, PetzRendering"`) and `RollingFileAppender` (`type="log4net.Appender.RollingFileAppender"`).
- **Custom Appender (`src/Services/GodotAppender.cs`):**
  - Subclasses `AppenderSkeleton` and overrides `Append(LoggingEvent loggingEvent)`.
- **Service Layer (`src/Services/ILogService.cs`, `src/Services/LogService.cs`):**
  - Registered as a singleton (`services.AddSingleton<ILogService, LogService>()`) in `src/Core/ServiceBootstrapper.cs`.
  - Initializes the `ILoggerRepository` for `Assembly.GetExecutingAssembly()`, redirects the `RollingFileAppender` to `user://logs/petzrendering.log`, and logs `"LogService initialized."` upon startup.

## 🧪 Testing Plan
- **Compilation Check:** Run `dotnet build src/PetzRendering.csproj` and verify the project compiles with `0 Warning(s)` and `0 Error(s)`.
- **Runtime & GodotAppender Check:** Launch the project using the Godot .NET console executable (`--path src --headless --quit-after 3`) and verify that `GodotAppender` outputs formatted `INFO` messages (`LogService initialized.`, `ServiceBootstrapper initialized...`) to the Godot console output.
- **RollingFileAppender Check:** Verify that `user://logs/petzrendering.log` is created on disk in the Godot `user://` directory and contains the structured log entries written during the run.

## 🔗 Related Resources
- Service implementation files:
  - [`src/Services/GodotAppender.cs`](../../../src/Services/GodotAppender.cs)
  - [`src/Services/ILogService.cs`](../../../src/Services/ILogService.cs)
  - [`src/Services/LogService.cs`](../../../src/Services/LogService.cs)
  - [`src/log4net.config`](../../../src/log4net.config)
  - [`src/Core/ServiceBootstrapper.cs`](../../../src/Core/ServiceBootstrapper.cs)
