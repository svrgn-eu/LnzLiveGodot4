using System.Collections.Generic;
using Godot;

namespace PetzRendering.Parsers;

public readonly record struct BdtBallFrame(
	int Tag,
	Vector3 Position,
	Vector3 Rotation);

public sealed class BdtParser
{
	public List<List<BdtBallFrame>> Frames { get; } = new();

	public BdtParser(string filename, IReadOnlyList<uint> frameIndexes, int ballsNum)
	{
		using var file = FileAccess.Open("res://resources/animations/" + filename, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return;
		}

		foreach (var frameOffset in frameIndexes)
		{
			file.Seek(frameOffset);
			for (var i = 0; i < 6; i++)
			{
				_ = file.Get16();
			}

			var tag = (int)file.Get16();
			var balls = new List<BdtBallFrame>(ballsNum);
			for (var ball = 0; ball < ballsNum; ball++)
			{
				var x = GetS16(file);
				var y = GetS16(file);
				var z = GetS16(file);
				var rot1 = GetS8(file);
				var rot2 = GetS8(file);
				var rot3 = GetS8(file);
				_ = GetS8(file);

				balls.Add(new BdtBallFrame(
					Tag: tag,
					Position: new Vector3(x, y, z),
					Rotation: new Vector3(rot1, rot2, rot3)));
			}

			Frames.Add(balls);
		}
	}

	public static int GetS16(FileAccess file)
	{
		var u = (int)file.Get16();
		return u >= 32768 ? u - 65536 : u;
	}

	public static int GetS8(FileAccess file)
	{
		var u = (int)file.Get8();
		return u >= 128 ? u - 256 : u;
	}
}
