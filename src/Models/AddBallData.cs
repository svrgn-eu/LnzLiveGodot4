using Godot;

namespace PetzRendering.Models;

public sealed class AddBallData
{
	public int Base { get; set; }
	public int Size { get; set; } = 1;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public int ColorIndex { get; set; } = -1;
	public int Outline { get; set; } = -1;
	public float ZAdd { get; set; }
	public int Fuzz { get; set; }
	public int Group { get; set; } = -1;
	public int BodyArea { get; set; } = 1;
	public int BallNo { get; set; } = -1;
	public int TextureId { get; set; } = -1;
	public int OutlineColorIndex { get; set; }

	public AddBallData(
		int @base,
		int ballNo,
		int size,
		Vector3 position,
		int colorIndex = -1,
		int outlineColorIndex = 0,
		int outline = -1,
		int fuzz = 0,
		float zAdd = 0.0f,
		int group = -1,
		int bodyArea = 1,
		int textureId = -1)
	{
		Size = size;
		Position = position;
		ColorIndex = colorIndex;
		Outline = outline;
		Base = @base;
		ZAdd = zAdd;
		Fuzz = fuzz;
		Group = group;
		BodyArea = bodyArea;
		BallNo = ballNo;
		TextureId = textureId;
		OutlineColorIndex = outlineColorIndex;
	}

	public AddBallData Clone()
	{
		return new AddBallData(
			Base,
			BallNo,
			Size,
			Position,
			ColorIndex,
			OutlineColorIndex,
			Outline,
			Fuzz,
			ZAdd,
			Group,
			BodyArea,
			TextureId);
	}
}
