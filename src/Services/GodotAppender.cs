using Godot;
using log4net.Appender;
using log4net.Core;

namespace PetzRendering.Services;

/// <summary>
/// Appender that redirects log messages to Godot's internal output console.
/// </summary>
public class GodotAppender : AppenderSkeleton
{
	#region Methods

	#region Append
	/// <summary>
	/// Writes a single log event to the Godot Output panel using the configured layout.
	/// </summary>
	/// <param name="loggingEvent">The log4net logging event to render.</param>
	protected override void Append(LoggingEvent loggingEvent)
	{
		string message = this.RenderLoggingEvent(loggingEvent).TrimEnd();

		if (loggingEvent.Level is not null && loggingEvent.Level >= Level.Warn)
		{
			GD.PrintErr(message);
		}
		else
		{
			GD.Print(message);
		}
	}
	#endregion Append

	#endregion Methods
}
