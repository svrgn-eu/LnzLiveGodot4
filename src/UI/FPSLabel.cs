using System.Globalization;
using Godot;

namespace PetzRendering.UI;

public partial class FPSLabel : Label
{
	public override void _Process(double delta)
	{
		Text = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
	}
}
