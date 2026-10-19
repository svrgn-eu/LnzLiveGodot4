using System.Collections.Generic;
using System.Globalization;
using Godot;
using PetzRendering.Core;
using PetzRendering.Services;

namespace PetzRendering.UI;

public partial class ToolsMenu : PopupMenu
{
	public enum RecolorAction
	{
		Entire = 0,
		Legs = 1,
		Tail = 2,
		Head = 3,
		Snout = 4,
		Ears = 5,
		Paws = 6,
		Nose = 7
	}

	private RecolorAction _currentAction;

	private IKeyBallsService KeyBalls => ServiceBootstrapper.GetService<IKeyBallsService>();

	[Signal]
	public delegate void ColorEntirePetEventHandler(string colorIndex, string outlineColorIndex);

	[Signal]
	public delegate void ColorPartPetEventHandler(
		Godot.Collections.Array<int> coreBallNos,
		string colorIndex,
		string outlineColorIndex,
		string part);

	[Signal]
	public delegate void AddBallEventHandler(Node3D selectedBall);

	[Signal]
	public delegate void CopyLToREventHandler();

	[Signal]
	public delegate void RecolorEventHandler(
		Godot.Collections.Dictionary<string, string> recolors,
		bool ballsOn,
		bool ballOutlinesOn,
		bool paintballsOn,
		bool linesOn);

	[Signal]
	public delegate void MoveHeadEventHandler(int x, int y, int z);

	[Signal]
	public delegate void PrintBallColorsEventHandler();

	public override void _Ready()
	{
		var recolorMenu = GetNodeOrNull<PopupMenu>("RecolorMenu");
		if (recolorMenu != null)
		{
			AddSubmenuNodeItem("Color...", recolorMenu);
		}

		AddItem("Add ball");
		AddItem("Copy L to R");
		AddItem("Move head");
		AddItem("Copy ball colors to clipboard");
	}

	public void OnLineEditGuiInput(InputEvent @event)
	{
		if (!@event.IsActionPressed(InputActions.UiTextSubmit))
		{
			return;
		}

		var parent = GetParent();
		var baseColor = parent.GetNode<LineEdit>("ColorPopup/VBoxContainer/LineEdit").Text;
		var outlineColor = parent.GetNode<LineEdit>("ColorPopup/VBoxContainer/LineEdit2").Text;

		if (_currentAction == RecolorAction.Entire)
		{
			EmitSignal(SignalName.ColorEntirePet, baseColor, outlineColor);
		}
		else
		{
			var coreBallNos = new List<int>();
			var isDog = KeyBalls.Species == Species.Dog;

			switch (_currentAction)
			{
				case RecolorAction.Legs:
				{
					var legs = isDog ? KeyBalls.LegsDog : KeyBalls.LegsCat;
					var feet = isDog ? KeyBalls.FootExtDog : KeyBalls.FootExtCat;
					coreBallNos.AddRange(legs[0]);
					coreBallNos.AddRange(legs[1]);
					foreach (var ar in feet)
					{
						foreach (var v in ar)
						{
							coreBallNos.Remove(v);
						}
					}
					break;
				}
				case RecolorAction.Tail:
					coreBallNos.AddRange(isDog ? KeyBalls.TailDog : KeyBalls.TailCat);
					break;
				case RecolorAction.Head:
					coreBallNos.AddRange(isDog ? KeyBalls.HeadExtDog : KeyBalls.HeadExtCat);
					break;
				case RecolorAction.Snout:
					coreBallNos.AddRange(isDog ? KeyBalls.FaceExtDog : KeyBalls.FaceExtCat);
					break;
				case RecolorAction.Ears:
				{
					var ears = isDog ? KeyBalls.EarExtDog : KeyBalls.EarExtCat;
					foreach (var tipList in ears.Values)
					{
						coreBallNos.AddRange(tipList);
					}
					coreBallNos.AddRange(ears.Keys);
					break;
				}
				case RecolorAction.Paws:
				{
					var feet = isDog ? KeyBalls.FootExtDog : KeyBalls.FootExtCat;
					foreach (var ar in feet)
					{
						coreBallNos.AddRange(ar);
					}
					break;
				}
				case RecolorAction.Nose:
					coreBallNos.AddRange(isDog ? KeyBalls.NoseDog : KeyBalls.NoseCat);
					break;
			}

			var part = _currentAction.ToString().ToUpperInvariant();
			var godotBallNos = new Godot.Collections.Array<int>(coreBallNos);
			EmitSignal(SignalName.ColorPartPet, godotBallNos, baseColor, outlineColor, part);
		}
	}

	public void OnRecolorMenuIdPressed(long id)
	{
		if (id == 8) // Color Swap
		{
			GetParent().GetNode<Window>("RecolorPopup").PopupCentered();
		}
		else
		{
			_currentAction = (RecolorAction)id;
			var colorPopup = GetParent().GetNode<Popup>("ColorPopup");
			colorPopup.Position = (Vector2I)GetViewport().GetMousePosition();
			colorPopup.Popup();
		}
	}

	public void OnToolsMenuIndexPressed(long index)
	{
		if (index == 2) // Copy L to R
		{
			EmitSignal(SignalName.CopyLToR);
		}
		else if (index == 1) // Add ball
		{
			var viewContainer = GetTree().Root.GetNodeOrNull<PetViewContainer>(
				"Root/SceneRoot/HSplitContainer/HSplitContainer/PetViewContainer");
			if (viewContainer != null && viewContainer.LastSelectedIsValid())
			{
				EmitSignal(SignalName.AddBall, viewContainer.LastSelected!.AsNode3D);
			}
		}
		else if (index == 3) // Move head
		{
			var options = GetParent().GetNode<Popup>("HeadMovePopup");
			options.PopupCentered();
		}
		else if (index == 4) // Print colors
		{
			EmitSignal(SignalName.PrintBallColors);
		}
	}

	public void OnToolsMenuAboutToShow()
	{
		var viewContainer = GetTree().Root.GetNodeOrNull<PetViewContainer>(
			"Root/SceneRoot/HSplitContainer/HSplitContainer/PetViewContainer");
		SetItemDisabled(1, viewContainer == null || !viewContainer.LastSelectedIsValid());
	}

	public void OnRecolorPopupConfirmed()
	{
		var popup = GetParent().GetNode("RecolorPopup/VBoxContainer");
		var lines = popup.GetNode("RecolorLines").GetChildren();
		var recolors = new Godot.Collections.Dictionary<string, string>();

		foreach (var l in lines)
		{
			var originalColor = (l.GetChild(0) as LineEdit)?.Text ?? "";
			var newColor = (l.GetChild(2) as LineEdit)?.Text ?? "";
			if (string.IsNullOrEmpty(originalColor) || string.IsNullOrEmpty(newColor))
			{
				continue;
			}

			recolors[originalColor] = newColor;
		}

		var ballsOn = popup.GetNode<CheckBox>("CheckContainer/Balls").ButtonPressed;
		var ballOutlinesOn = popup.GetNode<CheckBox>("CheckContainer/Ball outlines").ButtonPressed;
		var paintballsOn = popup.GetNode<CheckBox>("CheckContainer/Paintballs").ButtonPressed;
		var linesOn = popup.GetNode<CheckBox>("CheckContainer/Lines").ButtonPressed;

		EmitSignal(SignalName.Recolor, recolors, ballsOn, ballOutlinesOn, paintballsOn, linesOn);
	}

	public void OnClearButtonPressed()
	{
		var popup = GetParent().GetNode("RecolorPopup/VBoxContainer");
		var lines = popup.GetNode("RecolorLines").GetChildren();
		foreach (var l in lines)
		{
			if (l.GetChild(0) is LineEdit leftEdit)
			{
				leftEdit.Text = "";
			}

			if (l.GetChild(2) is LineEdit rightEdit)
			{
				rightEdit.Text = "";
			}
		}

		foreach (var cb in popup.GetNode("CheckContainer").GetChildren())
		{
			if (cb is CheckBox checkBox)
			{
				checkBox.ButtonPressed = true;
			}
		}
	}

	public void OnHeadMoveLineEditGuiInput(InputEvent @event)
	{
		if (!@event.IsActionPressed(InputActions.UiTextSubmit))
		{
			return;
		}

		var popup = GetParent().GetNode("HeadMovePopup/VBoxContainer");
		var xText = popup.GetNode<LineEdit>("HeadMoveLineEditX").Text;
		var yText = popup.GetNode<LineEdit>("HeadMoveLineEditY").Text;
		var zText = popup.GetNode<LineEdit>("HeadMoveLineEditZ").Text;

		_ = int.TryParse(xText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var x);
		_ = int.TryParse(yText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y);
		_ = int.TryParse(zText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var z);

		EmitSignal(SignalName.MoveHead, x, y, z);
	}
}
