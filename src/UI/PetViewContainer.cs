using System;
using Godot;
using PetzRendering.Rendering;

namespace PetzRendering.UI;

public partial class PetViewContainer : PanelContainer
{
	private const float PixelWorldSize = 0.002f;
	private const int DefaultViewportSize = 1000;
	private const int MaxViewportSize = 8192;

	private Node3D? _cameraHolder;
	private Camera3D? _camera;
	private Label? _label;
	private Node3D? _cube;
	private SubViewportContainer? _tex;
	private SubViewport? _subViewport;
	private Window? _popup;
	private PopupMenu? _toolsMenu;

	private IPetBallNode? _lastSelected;
	private bool _selectingOn;

	public IPetBallNode? LastSelected => _lastSelected;

	public override void _Ready()
	{
		_cameraHolder = GetTree().Root.GetNodeOrNull<Node3D>("Root/SceneRoot/SubViewportContainer/SubViewport/CameraHolder");
		_camera = _cameraHolder?.GetNodeOrNull<Camera3D>("Camera3D");
		_label = GetTree().Root.GetNodeOrNull<Label>("Root/SceneRoot/Label");
		_cube = GetTree().Root.GetNodeOrNull<Node3D>("Root/PetRoot/MeshInstance3D");
		_tex = GetTree().Root.GetNodeOrNull<SubViewportContainer>("Root/SceneRoot/SubViewportContainer");
		_subViewport = _tex?.GetNodeOrNull<SubViewport>("SubViewport");
		_popup = GetTree().Root.GetNodeOrNull<Window>("Root/SceneRoot/Popup");
		_toolsMenu = GetTree().Root.GetNodeOrNull<PopupMenu>("Root/SceneRoot/ToolsMenu");

		OnPetViewContainerResized();
		if (_tex != null)
		{
			_tex.TextureFilter = TextureFilterEnum.Nearest;
		}

		if (OS.GetCmdlineUserArgs().Contains("--capture-test"))
		{
			_ = RunCaptureTestAsync();
		}
	}

	private async System.Threading.Tasks.Task RunCaptureTestAsync()
	{
		var gen = GetTree().Root.GetNodeOrNull<PetGenerator>("Root/PetRoot/Node");
		gen?.GeneratePet("res://resources/examples/snowball.lnz");
		for (var i = 0; i < 5; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GetViewport().GetTexture().GetImage().SavePng(@"C:\Users\efilt\.gemini\antigravity\brain\571e46d6-12d9-4749-a03b-886a4b2c4825\scratch\center.png");
		if (_camera != null)
		{
			var t = _camera.Transform;
			t.Origin.X = -0.6f;
			_camera.Transform = t;
		}
		for (var i = 0; i < 5; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GetViewport().GetTexture().GetImage().SavePng(@"C:\Users\efilt\.gemini\antigravity\brain\571e46d6-12d9-4749-a03b-886a4b2c4825\scratch\panned_neg.png");
		if (_camera != null)
		{
			var t = _camera.Transform;
			t.Origin.X = 0.6f;
			_camera.Transform = t;
		}
		for (var i = 0; i < 5; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GetViewport().GetTexture().GetImage().SavePng(@"C:\Users\efilt\.gemini\antigravity\brain\571e46d6-12d9-4749-a03b-886a4b2c4825\scratch\panned_pos.png");
		GetTree().Quit();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left) && _cameraHolder != null)
			{
				var motion = motionEvent.Relative;
				var rot = _cameraHolder.Rotation;
				rot.X += motion.Y * 0.01f;
				rot.Y += motion.X * -0.01f;
				_cameraHolder.Rotation = rot;
			}

			if ((Input.IsMouseButtonPressed(MouseButton.Right) || Input.IsMouseButtonPressed(MouseButton.Middle)) &&
				_camera != null && _tex != null && _tex.Scale.X != 0)
			{
				var motion = motionEvent.Relative;
				var t = _camera.Transform;
				t.Origin.X += motion.X * 0.001f / _tex.Scale.X;
				t.Origin.Y += motion.Y * 0.001f / _tex.Scale.X;
				_camera.Transform = t;
			}

			if (_label != null)
			{
				_label.GlobalPosition = motionEvent.GlobalPosition;
			}

			// Raycast against 3D balls/paintballs (center of actual texture == (500, 500))
			if (_selectingOn && _camera != null && _tex != null)
			{
				var realCenter = Size / 2.0f;
				var realMousePos = motionEvent.Position;
				var offset = (realMousePos - realCenter) / _tex.Scale;
				var finalPos = (_tex.Size / 2.0f) + offset;

				var from = _camera.ProjectRayOrigin(finalPos);
				var to = from + (_camera.ProjectRayNormal(finalPos) * 950.0f);
				var spaceState = _camera.GetWorld3D().DirectSpaceState;
				var query = PhysicsRayQueryParameters3D.Create(from, to);
				query.CollideWithAreas = true;
				query.CollideWithBodies = false;
				var result = spaceState.IntersectRay(query);
				if (result.Count > 0 &&
					result.TryGetValue("collider", out var colliderVariant) &&
					colliderVariant.AsGodotObject() is Node colliderNode &&
					colliderNode.GetParent() is IPetBallNode hitBall)
				{
					_label?.Show();
					DealWithLastSelected();
					hitBall.OnAreaMouseEntered();
					_lastSelected = hitBall;
				}
				else
				{
					DealWithLastSelected();
					_lastSelected = null;
					_label?.Hide();
				}
			}
		}
		else if (@event is InputEventMouseButton { Pressed: true } mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown && _tex != null)
			{
				ZoomAtPosition(mouseButtonEvent.Position, 0.5f);
			}
			else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp && _tex != null)
			{
				ZoomAtPosition(mouseButtonEvent.Position, 2.0f);
			}
			else if (mouseButtonEvent.DoubleClick &&
					 mouseButtonEvent.ButtonIndex == MouseButton.Left &&
					 LastSelectedIsValid())
			{
				_lastSelected!.Selected();
			}
		}
	}

	private void ZoomAtPosition(Vector2 mousePosition, float zoomFactor)
	{
		if (_tex == null || _tex.Scale.X == 0 || _tex.Scale.Y == 0 || zoomFactor <= 0)
		{
			return;
		}

		var oldScale = _tex.Scale;
		_tex.Scale *= zoomFactor;
		var newScale = _tex.Scale;

		UpdateViewportSizeAndPosition();

		if (_camera != null)
		{
			var cursorOffset = mousePosition - (Size / 2.0f);
			var t = _camera.Transform;
			t.Origin.X -= cursorOffset.X * PixelWorldSize * ((1.0f / oldScale.X) - (1.0f / newScale.X));
			t.Origin.Y -= cursorOffset.Y * PixelWorldSize * ((1.0f / oldScale.Y) - (1.0f / newScale.Y));
			_camera.Transform = t;
		}
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsPressed() && LastSelectedIsValid())
		{
			_lastSelected!.AsNode3D._Input(@event);
		}
	}

	public bool LastSelectedIsValid()
	{
		return _lastSelected != null && IsInstanceValid(_lastSelected.AsNode3D);
	}

	private void DealWithLastSelected()
	{
		if (LastSelectedIsValid())
		{
			_lastSelected!.OnAreaMouseExited();
		}
	}

	public void OnNodeBallMouseEnter(string ballLabel)
	{
		if (_label != null)
		{
			_label.Text = ballLabel;
		}
	}

	public void OnSelectCheckBoxToggled(bool buttonPressed)
	{
		_selectingOn = buttonPressed;
		if (!_selectingOn)
		{
			if (LastSelectedIsValid())
			{
				_lastSelected!.OnAreaMouseExited();
			}

			_lastSelected = null;
			_label?.Hide();
		}
	}

	public void OnHelpButtonPressed()
	{
		_popup?.PopupCentered();
	}

	public void OnHelpPopupCloseRequested()
	{
		_popup?.Hide();
	}

	public void OnToolsButtonPressed()
	{
		_toolsMenu ??= GetTree().Root.GetNodeOrNull<PopupMenu>("Root/SceneRoot/ToolsMenu");
		if (_toolsMenu != null)
		{
			_toolsMenu.Position = (Vector2I)GetViewport().GetMousePosition();
			_toolsMenu.Popup();
		}
	}

	public void OnLnzTextEditMouseEntered()
	{
		if (LastSelectedIsValid())
		{
			_lastSelected!.OnAreaMouseExited();
		}

		_lastSelected = null;
		_label?.Hide();
	}

	public void OnPetViewContainerResized()
	{
		UpdateViewportSizeAndPosition();
	}

	private void UpdateViewportSizeAndPosition()
	{
		if (_tex == null)
		{
			if (HasNode("/root/Root/SceneRoot/SubViewportContainer"))
			{
				_tex = GetNode<SubViewportContainer>("/root/Root/SceneRoot/SubViewportContainer");
			}
			else if (HasNode("../../../SubViewportContainer"))
			{
				_tex = GetNode<SubViewportContainer>("../../../SubViewportContainer");
			}
			else
			{
				return;
			}
		}

		_subViewport ??= _tex.GetNodeOrNull<SubViewport>("SubViewport");
		_cameraHolder ??= _subViewport?.GetNodeOrNull<Node3D>("CameraHolder");
		_camera ??= _cameraHolder?.GetNodeOrNull<Camera3D>("Camera3D");

		var scaleX = Math.Max(0.0001f, Math.Abs(_tex.Scale.X));
		var scaleY = Math.Max(0.0001f, Math.Abs(_tex.Scale.Y));
		var requiredWidth = (int)MathF.Ceiling(Size.X / scaleX);
		var requiredHeight = (int)MathF.Ceiling(Size.Y / scaleY);
		var requiredSize = Math.Max(requiredWidth, requiredHeight);
		var targetSize = Math.Clamp(Math.Max(DefaultViewportSize, requiredSize), DefaultViewportSize, MaxViewportSize);
		if (targetSize % 2 != 0)
		{
			targetSize += 1;
		}

		if (_subViewport != null && (_subViewport.Size.X != targetSize || _subViewport.Size.Y != targetSize))
		{
			_subViewport.Size = new Vector2I(targetSize, targetSize);
		}

		var targetVec = new Vector2(targetSize, targetSize);
		if (_tex.Size != targetVec)
		{
			_tex.Size = targetVec;
		}

		if (_camera != null)
		{
			_camera.Size = targetSize * PixelWorldSize;
		}

		_tex.PivotOffset = _tex.Size / 2.0f;
		var sizeDiff = (_tex.Size / 2.0f) - (Size / 2.0f);
		_tex.Position = GlobalPosition - sizeDiff;
	}

	public void OnPetViewContainerSortChildren()
	{
		OnPetViewContainerResized();
	}
}
