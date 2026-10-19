using Godot;

namespace PetzRendering.UI;

public partial class AnimPicker : OptionButton
{
	public override void _Ready()
	{
		Disabled = true;
		AddItem("Anim: -");
		GetPopup().MaxSize = new Vector2I(0, 400);
	}

	public void OnBhdLoaded(int numOfAnimations)
	{
		var prevSelected = Selected;
		Clear();
		for (var i = 0; i < numOfAnimations; i++)
		{
			AddItem($"Anim {i}", i);
		}

		Disabled = numOfAnimations == 0;
		if (numOfAnimations > 0)
		{
			if (prevSelected >= 0 && prevSelected < numOfAnimations)
			{
				Select(prevSelected);
			}
			else
			{
				Select(0);
			}
		}
	}
}
