using Godot;

namespace PetzRendering.Rendering;

public partial class Line : Node3D
{
	private static readonly Texture2D PaletteTexture = GD.Load<Texture2D>("res://resources/textures/petzpalette.png");

	private Vector2 _lineWidths = new(10, 10);
	private int _fuzzAmount;
	private int _colorIndex;
	private int _lColorIndex;
	private int _rColorIndex;
	private Vector3 _ballWorldPos1 = Vector3.Zero;
	private Vector3 _ballWorldPos2 = Vector3.Zero;
	private Texture2D? _texture;
	private int _transparentColor;
	private Vector3 _petCenter = Vector3.Zero;

	private MeshInstance3D? _meshInstance;
	private ShaderMaterial? _shaderMaterial;

	[Export]
	public Vector2 LineWidths
	{
		get => _lineWidths;
		set => SetLineWidth(value);
	}

	[Export]
	public int FuzzAmount
	{
		get => _fuzzAmount;
		set => SetFuzzAmount(value);
	}

	[Export]
	public int ColorIndex
	{
		get => _colorIndex;
		set => SetColorIndex(value);
	}

	[Export]
	public int LColorIndex
	{
		get => _lColorIndex;
		set => SetLColorIndex(value);
	}

	[Export]
	public int RColorIndex
	{
		get => _rColorIndex;
		set => SetRColorIndex(value);
	}

	[Export]
	public Vector3 BallWorldPos1
	{
		get => _ballWorldPos1;
		set => SetBallWorldPos1(value);
	}

	[Export]
	public Vector3 BallWorldPos2
	{
		get => _ballWorldPos2;
		set => SetBallWorldPos2(value);
	}

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
	public Vector3 PetCenter
	{
		get => _petCenter;
		set => SetPetCenter(value);
	}

	public override void _Ready()
	{
		GetShaderMaterial()?.SetShaderParameter("palette", PaletteTexture);
	}

	public void SetLineWidth(Vector2 newValue)
	{
		_lineWidths = newValue;
		var mat = GetShaderMaterial();
		mat?.SetShaderParameter("line_width_in_pixels_start", (int)newValue.X);
		mat?.SetShaderParameter("line_width_in_pixels_end", (int)newValue.Y);
	}

	public void SetFuzzAmount(int newValue)
	{
		_fuzzAmount = newValue;
		GetShaderMaterial()?.SetShaderParameter("fuzz_amount", newValue);
	}

	public void SetRColorIndex(int newValue)
	{
		_rColorIndex = newValue;
		GetShaderMaterial()?.SetShaderParameter("r_color_index", newValue);
	}

	public void SetLColorIndex(int newValue)
	{
		_lColorIndex = newValue;
		GetShaderMaterial()?.SetShaderParameter("l_color_index", newValue);
	}

	public void SetBallWorldPos1(Vector3 newValue)
	{
		_ballWorldPos1 = newValue;
		GetShaderMaterial()?.SetShaderParameter("ball_world_pos1", newValue);
	}

	public void SetBallWorldPos2(Vector3 newValue)
	{
		_ballWorldPos2 = newValue;
		GetShaderMaterial()?.SetShaderParameter("ball_world_pos2", newValue);
	}

	public void SetColorIndex(int newValue)
	{
		_colorIndex = newValue;
		GetShaderMaterial()?.SetShaderParameter("color_index", newValue);
	}

	public void SetTexture(Texture2D? newValue)
	{
		_texture = newValue;
		var mat = GetShaderMaterial();
		if (mat == null)
		{
			return;
		}

		mat.SetShaderParameter("line_texture", newValue ?? default(Variant));
		if (newValue != null)
		{
			mat.SetShaderParameter("has_texture", true);
			mat.SetShaderParameter("texture_size", newValue.GetSize());
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
		GetShaderMaterial()?.SetShaderParameter("z_center", newValue);
	}

	private MeshInstance3D? GetMeshInstance() =>
		_meshInstance ??= GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

	private ShaderMaterial? GetShaderMaterial() =>
		_shaderMaterial ??= GetMeshInstance()?.MaterialOverride as ShaderMaterial;
}
