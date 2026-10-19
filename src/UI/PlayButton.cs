using Godot;

namespace PetzRendering.UI;

public partial class PlayButton : Button
{
	private Timer? _timer;

	public override void _Ready()
	{
		_timer = GetNodeOrNull<Timer>("Timer");
	}

	public void OnButtonToggled(bool buttonPressed)
	{
		_timer ??= GetNodeOrNull<Timer>("Timer");
		if (buttonPressed)
		{
			_timer?.Start();
		}
		else
		{
			_timer?.Stop();
		}
	}
}
