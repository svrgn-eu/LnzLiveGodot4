using System;
using Godot;

namespace PetzRendering.UI;

public partial class FrameSlider : HSlider
{
	public void OnAnimationLoaded(int numOfFrames)
	{
		MaxValue = Math.Max(0, numOfFrames - 1);
		SetValueNoSignal(0);
	}

	public void OnTimerTimeout()
	{
		if (Math.Abs(Value - MaxValue) < 0.0001)
		{
			Value = 0;
		}
		else
		{
			Value += 1;
		}
	}
}
