using System;
using Godot;
using PetzRendering.Core;
using PetzRendering.Models;

namespace PetzRendering.Rendering;

public interface IPetBallNode
{
	Node3D AsNode3D { get; }
	int BallNo { get; set; }
	int BaseBallNo { get; set; }
	int BallSize { get; set; }
	int FuzzAmount { get; set; }
	int Outline { get; set; }
	int OldOutline { get; }
	int ColorIndex { get; set; }
	int OutlineColorIndex { get; set; }
	float ZAdd { get; set; }
	Texture2D? Texture { get; set; }
	int TransparentColor { get; set; }
	bool VisibleOverride { get; set; }
	bool Omitted { get; set; }
	bool IsIris { get; }
	void SetVisibleOverride(bool newValue);
	void OnAreaMouseEntered();
	void OnAreaMouseExited();
	void Selected();
	void Flash();
}

public partial class Ball : Node3D, IPetBallNode
{
	private static readonly Texture2D PaletteTexture = GD.Load<Texture2D>("res://resources/palettes/petzpalette.png");

	private int _ballSize = 10;
	private int _fuzzAmount;
	private int _outline = -1;
	private int _colorIndex;
	private int _outlineColorIndex;
	private float _zAdd;
	private Texture2D? _texture;
	private int _transparentColor;
	private bool _visibleOverride = true;
	private Vector3 _petCenter = Vector3.Zero;

	private int _oldOutline = -1;
	private int _oldOutlineColor;
	private bool _isOver;
	private int _timerCount;

	private MeshInstance3D? _meshInstance;
	private ShaderMaterial? _shaderMaterial;
	private CollisionShape3D? _collisionShape;
	private SphereShape3D? _sphereShape;
	private Timer? _flashTimer;

	[Signal]
	public delegate void BallMouseEnterEventHandler(int ballNo);

	[Signal]
	public delegate void BallMouseExitEventHandler(int ballNo);

	[Signal]
	public delegate void BallSelectedEventHandler(int ballNo, int section);

	[Signal]
	public delegate void BallDeletedEventHandler(int ballNo);

	public Node3D AsNode3D => this;
	public int OldOutline => _isOver ? _oldOutline : _outline;
	public bool IsIris => false;

	[Export]
	public int BallSize
	{
		get => _ballSize;
		set => SetBallSize(value);
	}

	[Export]
	public int FuzzAmount
	{
		get => _fuzzAmount;
		set => SetFuzzAmount(value);
	}

	[Export]
	public int Outline
	{
		get => _outline;
		set => SetOutline(value);
	}

	[Export]
	public int ColorIndex
	{
		get => _colorIndex;
		set => SetColorIndex(value);
	}

	[Export]
	public int OutlineColorIndex
	{
		get => _outlineColorIndex;
		set => SetOutlineColorIndex(value);
	}

	[Export]
	public float ZAdd
	{
		get => _zAdd;
		set => SetZAdd(value);
	}

	[Export]
	public int BallNo { get; set; }

	[Export]
	public int BaseBallNo { get; set; } = -1;

	[Export]
	public Texture2D? Texture
	{
		get => _texture;
		set => SetTexture(value);
	}

	[Export]
	public int TransparentColor
	{
		get => _transparentColor;
		set => SetTransparentColor(value);
	}

	[Export]
	public bool VisibleOverride
	{
		get => _visibleOverride;
		set => SetVisibleOverride(value);
	}

	[Export]
	public bool Omitted { get; set; }

	[Export]
	public Vector3 PetCenter
	{
		get => _petCenter;
		set => SetPetCenter(value);
	}

	public override void _Ready()
	{
		GetShaderMaterial()?.SetShaderParameter("palette", PaletteTexture);
	}

	public void SetVisibleOverride(bool newValue)
	{
		_visibleOverride = newValue;
		var mesh = GetMeshInstance();
		var col = GetCollisionShape();
		if (!Omitted)
		{
			if (mesh != null) mesh.Visible = newValue;
			if (col != null)
			{
				col.Disabled = !newValue;
				col.Visible = newValue;
			}
		}
		else
		{
			if (mesh != null) mesh.Visible = false;
			if (col != null)
			{
				col.Disabled = true;
				col.Visible = false;
			}
		}
	}

	public void SetBallSize(int newValue)
	{
		_ballSize = newValue;
		GetShaderMaterial()?.SetShaderParameter("ball_size", newValue);
		var a = _ballSize * 0.05f;
		var shape = GetSphereShape();
		if (shape != null)
		{
			shape.Radius = Math.Max(0.001f, a * 0.02f);
			shape.Margin = 0.0001f;
		}
	}

	public void SetFuzzAmount(int newValue)
	{
		_fuzzAmount = newValue;
		GetShaderMaterial()?.SetShaderParameter("fuzz_amount", newValue);
	}

	public void SetOutline(int newValue)
	{
		_outline = newValue;
		GetShaderMaterial()?.SetShaderParameter("outline", newValue);
	}

	public void SetColorIndex(int newValue)
	{
		_colorIndex = newValue;
		GetShaderMaterial()?.SetShaderParameter("color_index", newValue);
	}

	public void SetOutlineColorIndex(int newValue)
	{
		GetShaderMaterial()?.SetShaderParameter("outline_color_index", newValue);
		_outlineColorIndex = newValue;
	}

	public void SetZAdd(float newValue)
	{
		_zAdd = newValue;
		GetShaderMaterial()?.SetShaderParameter("z_add", newValue);
	}

	public void SetTexture(Texture2D? newValue)
	{
		_texture = newValue;
		var mat = GetShaderMaterial();
		if (mat == null)
		{
			return;
		}

		mat.SetShaderParameter("ball_texture", newValue ?? default(Variant));
		if (newValue != null)
		{
			mat.SetShaderParameter("texture_size", newValue.GetSize());
			mat.SetShaderParameter("has_texture", true);
		}
		else
		{
			mat.SetShaderParameter("has_texture", false);
		}
	}

	public void SetTransparentColor(int newValue)
	{
		_transparentColor = newValue;
		GetShaderMaterial()?.SetShaderParameter("transparent_index", newValue);
	}

	public void SetPetCenter(Vector3 newValue)
	{
		_petCenter = newValue;
		GetShaderMaterial()?.SetShaderParameter("z_center_pet_world", newValue);
	}

	public void OnAreaMouseEntered()
	{
		_isOver = true;
		TurnOnHighlight();
		EmitSignal(SignalName.BallMouseEnter, BallNo);
	}

	public void TurnOnHighlight()
	{
		_oldOutline = _outline;
		_oldOutlineColor = _outlineColorIndex;
		SetOutline(3);
		SetOutlineColorIndex(0);
	}

	public void TurnOffHighlight()
	{
		SetOutline(_oldOutline);
		SetOutlineColorIndex(_oldOutlineColor);
	}

	public void OnAreaMouseExited()
	{
		_isOver = false;
		TurnOffHighlight();
		EmitSignal(SignalName.BallMouseExit, BallNo);
	}

	public void Selected()
	{
		EmitSignal(SignalName.BallSelected, BallNo, (int)Section.Ball);
	}

	public void OnAreaInputEvent(Node camera, InputEvent @event, Vector3 clickPosition, Vector3 clickNormal, long shapeIdx)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, DoubleClick: true })
		{
			Selected();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!_isOver)
		{
			return;
		}

		if (@event.IsActionPressed(InputActions.OpenToolsMenu))
		{
			return;
		}

		if (@event.IsActionPressed(InputActions.SelectBallSection, exactMatch: true))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.BallSelected, BallNo, (int)Section.Ball);
		}
		else if (@event.IsActionPressed(InputActions.SelectMoveSection, exactMatch: true))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.BallSelected, BallNo, (int)Section.Move);
		}
		else if (@event.IsActionPressed(InputActions.SelectProjectSection, exactMatch: true))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.BallSelected, BallNo, (int)Section.Project);
		}
		else if (@event.IsActionPressed(InputActions.SelectLineSection, exactMatch: true))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.BallSelected, BallNo, (int)Section.Line);
		}
		else if (@event.IsActionPressed(InputActions.DeleteBall, exactMatch: true))
		{
			GetViewport().SetInputAsHandled();
			EmitSignal(SignalName.BallDeleted, BallNo);
		}
	}

	public void Flash()
	{
		_timerCount = 0;
		if (!_isOver)
		{
			TurnOnHighlight();
			GetFlashTimer()?.Start();
		}
	}

	public void OnFlashTimerTimeout()
	{
		_timerCount += 1;
		if (!_isOver)
		{
			if (_timerCount % 2 == 1)
			{
				TurnOffHighlight();
			}
			else
			{
				TurnOnHighlight();
			}

			if (_timerCount > 4)
			{
				GetFlashTimer()?.Stop();
			}
		}
	}

	private MeshInstance3D? GetMeshInstance() =>
		_meshInstance ??= GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

	private ShaderMaterial? GetShaderMaterial() =>
		_shaderMaterial ??= GetMeshInstance()?.MaterialOverride as ShaderMaterial;

	private CollisionShape3D? GetCollisionShape() =>
		_collisionShape ??= GetNodeOrNull<CollisionShape3D>("Area3D/CollisionShape3D");

	private SphereShape3D? GetSphereShape() =>
		_sphereShape ??= GetCollisionShape()?.Shape as SphereShape3D;

	private Timer? GetFlashTimer() =>
		_flashTimer ??= GetNodeOrNull<Timer>("FlashTimer");
}
