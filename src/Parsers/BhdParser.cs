using System.Collections.Generic;
using Godot;

namespace PetzRendering.Parsers;

public readonly record struct BhdAnimationRange(
	int NumOfOffsets,
	int End,
	int Start,
	int ActualStart);

public sealed class BhdParser
{
	public List<BhdAnimationRange> AnimationRanges { get; } = new();
	public int NumBalls { get; private set; }
	public string FilePath { get; }
	public int FramesOffset { get; private set; }
	public List<int> BallSizes { get; } = new();

	public BhdParser(string filePath)
	{
		FilePath = filePath;
		using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return;
		}

		FramesOffset = file.Get16();
		_ = file.Get32();
		NumBalls = file.Get16();
		file.Seek(file.GetPosition() + 30);

		for (var i = 0; i < NumBalls; i++)
		{
			BallSizes.Add(file.Get16());
		}

		var animationCount = file.Get16();
		for (var i = 0; i < animationCount; i++)
		{
			var start = i > 0 ? AnimationRanges[i - 1].End : 0;
			var end = (int)file.Get16();
			var numOfOffsets = end - start;
			AnimationRanges.Add(new BhdAnimationRange(
				NumOfOffsets: numOfOffsets,
				End: end,
				Start: FramesOffset + (start * 4),
				ActualStart: start));
		}
	}

	public List<uint> GetFrameOffsetsFor(int index)
	{
		var result = new List<uint>();
		if (index < 0 || index >= AnimationRanges.Count)
		{
			return result;
		}

		var animRange = AnimationRanges[index];
		using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return result;
		}

		file.Seek((ulong)animRange.Start);
		for (var i = 0; i < animRange.NumOfOffsets; i++)
		{
			result.Add(file.Get32());
		}

		return result;
	}
}
