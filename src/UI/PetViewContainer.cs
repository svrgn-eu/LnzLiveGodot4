using Godot;
using PetzRendering.Rendering;

namespace PetzRendering.UI;

public partial class PetViewContainer : PanelContainer
{
	private Node3D? _cameraHolder;
	private Camera3D? _camera;
	private Label? _label;
	private Node3D? _cube;
	private SubViewportContainer? _tex;
	private Window? _popup;

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
		_popup = GetTree().Root.GetNodeOrNull<Window>("Root/SceneRoot/Popup");

		OnPetViewContainerResized();
		if (_tex != null)
		{
			_tex.TextureFilter = TextureFilterEnum.Nearest;
		}
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
				var realCenter = Position + (Size / 2.0f);
				var realMousePos = motionEvent.Position;
				var offset = (realMousePos - realCenter) / _tex.Scale;
				var finalPos = new Vector2(500, 500) + offset;

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
				_tex.PivotOffset = _tex.Size / 2.0f;
				_tex.Scale /= 2.0f;
			}
			else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp && _tex != null)
			{
				_tex.PivotOffset = _tex.Size / 2.0f;
				_tex.Scale *= 2.0f;
			}
			else if (mouseButtonEvent.DoubleClick &&
					 mouseButtonEvent.ButtonIndex == MouseButton.Left &&
					 LastSelectedIsValid())
			{
				_lastSelected!.Selected();
			}
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

		var sizeDiff = (_tex.Size / 2.0f) - (Size / 2.0f);
		_tex.GlobalPosition = GlobalPosition - sizeDiff;
	}

	public void OnPetViewContainerSortChildren()
	{
		OnPetViewContainerResized();
	}
}
