using System;
using Godot;
using PetzRendering.Models;

namespace PetzRendering.Rendering;

public partial class Paintball : Node3D, IPetBallNode
{
	private static readonly Texture2D PaletteTexture = GD.Load<Texture2D>("res://resources/textures/petzpalette.png");

	private Vector3 _baseBallPosition = Vector3.Zero;
	private int _baseBallSize = 10;
	private int _ballSize = 10;
	private int _fuzzAmount;
	private int _outline = -1;
	private int _colorIndex = -1;
	private int _outlineColorIndex;
	private float _zAdd;
	private bool _visibleOverride = true;
	private Texture2D? _texture;
	private int _transparentColor;

	private int _oldOutline = -1;
	private int _oldOutlineColor;
	private bool _isOver;

	private MeshInstance3D? _meshInstance;
	private ShaderMaterial? _shaderMaterial;
	private CollisionShape3D? _collisionShape;
	private SphereShape3D? _sphereShape;

	[Signal]
	public delegate void PaintballMouseEnterEventHandler(int baseBallNo);

	[Signal]
	public delegate void PaintballMouseExitEventHandler();

	// Only used if it's an iris
	[Signal]
	public delegate void BallMouseEnterEventHandler(int ballNo);

	[Signal]
	public delegate void BallMouseExitEventHandler(int ballNo);

	[Signal]
	public delegate void BallSelectedEventHandler(int ballNo, int section);

	public Node3D AsNode3D => this;
	public int OldOutline => _isOver ? _oldOutline : _outline;
	public bool IsIris => OverrideBallNo != -1;

	public int BallNo
	{
		get => OverrideBallNo != -1 ? OverrideBallNo : BaseBallNo;
		set => OverrideBallNo = value;
	}

	[Export]
	public Vector3 BaseBallPosition
	{
		get => _baseBallPosition;
		set => SetBaseBallPosition(value);
	}

	[Export]
	public int BaseBallSize
	{
		get => _baseBallSize;
		set => SetBaseBallSize(value);
	}

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
	public int BaseBallNo { get; set; }

	[Export]
	public bool VisibleOverride
	{
		get => _visibleOverride;
		set => SetVisibleOverride(value);
	}

	[Export]
	public bool Omitted { get; set; }

	[Export]
	public int OverrideBallNo { get; set; } = -1;

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

	public override void _Ready()
	{
		GetShaderMaterial()?.SetShaderParameter("palette", PaletteTexture);
	}

	public void SetVisibleOverride(bool newValue)
	{
		_visibleOverride = newValue;
		var mesh = GetMeshInstance();
		var col = GetCollisionShape();
		var effectiveVisible = !Omitted && newValue;
		if (col != null)
		{
			col.Disabled = !effectiveVisible;
			col.Visible = effectiveVisible;
		}

		if (mesh != null)
		{
			mesh.Visible = effectiveVisible;
		}
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

	public void SetBaseBallPosition(Vector3 newValue)
	{
		_baseBallPosition = newValue;
		GetShaderMaterial()?.SetShaderParameter("base_world_position", newValue);
	}

	public void SetBaseBallSize(int newValue)
	{
		_baseBallSize = newValue;
		GetShaderMaterial()?.SetShaderParameter("base_ball_size", newValue);
	}

	public void SetBallSize(int newValue)
	{
		_ballSize = newValue;
		GetShaderMaterial()?.SetShaderParameter("ball_size", newValue);
		var a = _ballSize * 0.25f;
		var shape = GetSphereShape();
		if (shape != null)
		{
			shape.Radius = Math.Max(0.001f, a * 0.008f);
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
		_outlineColorIndex = newValue;
		GetShaderMaterial()?.SetShaderParameter("outline_color_index", newValue);
	}

	public void Selected()
	{
		if (OverrideBallNo != -1)
		{
			EmitSignal(SignalName.BallSelected, OverrideBallNo, (int)Section.Ball);
		}
	}

	public void Flash()
	{
		// Irises/paintballs do not have a FlashTimer node, but implement IPetBallNode safely.
	}

	public void OnAreaMouseEntered()
	{
		_oldOutline = _outline;
		_oldOutlineColor = _outlineColorIndex;
		SetOutline(3);
		SetOutlineColorIndex(0);
		if (OverrideBallNo != -1)
		{
			EmitSignal(SignalName.BallMouseEnter, OverrideBallNo);
		}
		else
		{
			EmitSignal(SignalName.PaintballMouseEnter, BaseBallNo);
		}

		_isOver = true;
	}

	public void OnAreaMouseExited()
	{
		SetOutline(_oldOutline);
		SetOutlineColorIndex(_oldOutlineColor);
		if (OverrideBallNo != -1)
		{
			EmitSignal(SignalName.BallMouseExit, OverrideBallNo);
		}
		else
		{
			EmitSignal(SignalName.PaintballMouseExit);
		}

		_isOver = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true } && _isOver)
		{
			GetViewport().SetInputAsHandled();
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
}
