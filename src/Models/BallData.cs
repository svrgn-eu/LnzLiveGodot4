using Godot;

namespace PetzRendering.Models;

public sealed class BallData
{
	public int BallNo { get; set; } = -1;
	public int Size { get; set; } = 1;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public int ColorIndex { get; set; }
	public int Outline { get; set; } = -1;
	public float ZAdd { get; set; }
	public int Fuzz { get; set; }
	public int Group { get; set; } = -1;
	public int TextureId { get; set; } = -1;
	public Vector3 Rotation { get; set; } = Vector3.Zero;
	public int OutlineColorIndex { get; set; }

	public BallData(
		int size,
		Vector3 position,
		int ballNo,
		Vector3? rotation = null,
		int colorIndex = 0,
		int outlineColorIndex = 0,
		int outline = -1,
		int fuzz = 0,
		float zAdd = 0.0f,
		int group = -1,
		int textureId = -1)
	{
		Size = size;
		Position = position;
		Outline = outline;
		BallNo = ballNo;
		ZAdd = zAdd;
		Fuzz = fuzz;
		Group = group;
		TextureId = textureId;
		ColorIndex = colorIndex;
		Rotation = rotation ?? Vector3.Zero;
		OutlineColorIndex = outlineColorIndex;
	}

	public BallData Clone()
	{
		return new BallData(
			Size,
			Position,
			BallNo,
			Rotation,
			ColorIndex,
			OutlineColorIndex,
			Outline,
			Fuzz,
			ZAdd,
			Group,
			TextureId);
	}
}
