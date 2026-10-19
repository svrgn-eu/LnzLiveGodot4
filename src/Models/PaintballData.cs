using Godot;

namespace PetzRendering.Models;

public sealed class PaintballData
{
	public int Base { get; set; }
	public int Size { get; set; } = 1;
	public Vector3 NormalisedPosition { get; set; } = Vector3.Zero;
	public int ColorIndex { get; set; }
	public int OutlineColorIndex { get; set; }
	public int Outline { get; set; } = -1;
	public float ZAdd { get; set; }
	public int Fuzz { get; set; }
	public int TextureId { get; set; } = -1;
	public int Anchored { get; set; }

	public PaintballData(
		int @base,
		int size,
		Vector3 position,
		int colorIndex,
		int outlineColorIndex,
		int outline = -1,
		int fuzz = 0,
		float zAdd = 0.0f,
		int textureId = -1,
		int anchored = 0,
		bool alreadyNormalized = false)
	{
		Size = size;
		NormalisedPosition = alreadyNormalized ? position : position.Normalized();
		ColorIndex = colorIndex;
		OutlineColorIndex = outlineColorIndex;
		Outline = outline;
		Base = @base;
		Fuzz = fuzz;
		ZAdd = zAdd;
		TextureId = textureId;
		Anchored = anchored;
	}

	public PaintballData Clone()
	{
		return new PaintballData(
			Base,
			Size,
			NormalisedPosition,
			ColorIndex,
			OutlineColorIndex,
			Outline,
			Fuzz,
			ZAdd,
			TextureId,
			Anchored,
			alreadyNormalized: true);
	}
}
