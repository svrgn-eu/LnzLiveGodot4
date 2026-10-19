using Godot;

namespace PetzRendering.Rendering;

public partial class Rotator : Node3D
{
	[Export]
	public bool Enabled { get; set; }

	[Export]
	public bool EnabledInEditor { get; set; }

	[Export]
	public float CamDist { get; set; } = 0.947f;

	private Camera3D? _cam;

	public override void _Ready()
	{
		_cam = GetTree().Root.GetNodeOrNull<Camera3D>("Node3D/SubViewportContainer/SubViewport/Camera3D");
	}

	public override void _Process(double delta)
	{
		if ((Engine.IsEditorHint() && EnabledInEditor) || (!Engine.IsEditorHint() && Enabled))
		{
			RotateObjectLocal(Vector3.Up, (float)delta);
		}
	}

	public void OnHSliderValueChanged(double value)
	{
		if (_cam != null && value != 0)
		{
			_cam.Size = (float)(1.0 / value);
		}
	}

	public void OnHSlider2ValueChanged(double value)
	{
		if (_cam == null)
		{
			return;
		}

		var transform = _cam.Transform;
		transform.Origin = Vector3.Zero;
		_cam.Transform = transform;

		var rot = _cam.RotationDegrees;
		rot.Y = (float)value + 180.0f;
		_cam.RotationDegrees = rot;

		_cam.TranslateObjectLocal(Vector3.Back * CamDist);
	}

	public void OnHSlider3ValueChanged(double value)
	{
		if (_cam == null)
		{
			return;
		}

		var transform = _cam.Transform;
		transform.Origin = Vector3.Zero;
		_cam.Transform = transform;

		var rot = _cam.RotationDegrees;
		rot.X = (float)value;
		_cam.RotationDegrees = rot;

		_cam.TranslateObjectLocal(Vector3.Back * CamDist);
	}
}
