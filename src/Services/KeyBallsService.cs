using System.Collections.Generic;
using System.Linq;

namespace PetzRendering.Services;

public sealed class KeyBallsService : IKeyBallsService
{
	public Species Species { get; set; } = Species.Dog;
	public int MaxBaseBallNum { get; set; } = 67;

	public IReadOnlyList<IReadOnlyList<int>> LegsDog { get; } = new int[][]
	{
		new[] { 7, 31, 9, 10, 11, 13, 23, 33, 34, 35, 37, 47 }, // front legs
		new[] { 40, 16, 0, 12, 20, 21, 22, 24, 36, 44, 45, 46 }  // back legs
	};

	public IReadOnlyList<IReadOnlyList<int>> LegsCat { get; } = new int[][]
	{
		new[] { 12, 13, 16, 17, 18, 19, 20, 21, 22, 23, 63, 64 }, // front
		new[] { 0, 1, 32, 33, 34, 35, 41, 42, 49, 50, 51, 52, 53, 54 } // back
	};

	public IReadOnlyList<int> BodyExtDog { get; } = new[]
	{
		49, 0, 12, 16, 20, 21, 22, 24, 36, 44, 45, 46, 43, 19, 40, 57, 58, 59, 60, 61, 62
	};

	public IReadOnlyList<int> BodyExtCat { get; } = new[]
	{
		2, 3, 0, 1, 32, 33, 34, 35, 41, 42, 49, 50, 51, 52, 53, 54, 25, 26, 43, 44, 45, 46, 47, 48
	};

	public IReadOnlyList<int> FaceExtDog { get; } = new[]
	{
		51, 53, 55, 56, 63, 64, 17, 41, 15, 39
	};

	public IReadOnlyList<int> FaceExtCat { get; } = new[]
	{
		7, 30, 31, 37, 40, 57, 58, 59, 60, 61, 62, 29
	};

	public IReadOnlyList<int> HeadExtDog { get; } = new[]
	{
		52, 1, 2, 3, 4, 5, 6, 8, 14, 15, 17, 25, 26, 27, 28, 29, 30, 32, 38, 39, 41, 51, 53, 55, 56, 63, 64
	};

	public IReadOnlyList<int> HeadExtCat { get; } = new[]
	{
		24, 4, 5, 7, 8, 9, 10, 11, 14, 15, 27, 28, 29, 30, 31, 37, 40, 55, 56, 57, 58, 59, 60, 61, 62
	};

	public IReadOnlyList<IReadOnlyList<int>> FootExtDog { get; } = new int[][]
	{
		new[] { 12, 20, 21, 22 },
		new[] { 13, 9, 10, 11 },
		new[] { 36, 44, 45, 46 },
		new[] { 37, 33, 34, 35 }
	};

	public IReadOnlyList<IReadOnlyList<int>> FootExtCat { get; } = new int[][]
	{
		new[] { 22, 16, 17, 18 },
		new[] { 23, 19, 20, 21 },
		new[] { 41, 34, 49, 50, 51 },
		new[] { 42, 35, 52, 53, 54 }
	};

	public IReadOnlyDictionary<int, IReadOnlyList<int>> EarExtDog { get; } = new Dictionary<int, IReadOnlyList<int>>
	{
		{ 4, new[] { 5, 6 } },
		{ 28, new[] { 29, 30 } }
	};

	public IReadOnlyDictionary<int, IReadOnlyList<int>> EarExtCat { get; } = new Dictionary<int, IReadOnlyList<int>>
	{
		{ 8, new[] { 9 } },
		{ 10, new[] { 11 } }
	};

	public IReadOnlyDictionary<int, int> EyesDog { get; } = new Dictionary<int, int>
	{
		{ 14, 8 },
		{ 38, 32 }
	};

	public IReadOnlyDictionary<int, int> EyesCat { get; } = new Dictionary<int, int>
	{
		{ 27, 14 },
		{ 28, 15 }
	};

	public IReadOnlyList<int> NoseDog { get; } = new[] { 17, 41, 55 };
	public IReadOnlyList<int> NoseCat { get; } = new[] { 37 };

	public IReadOnlyList<int> TailDog { get; } = new[] { 57, 58, 59, 60, 61, 62 };
	public IReadOnlyList<int> TailCat { get; } = new[] { 43, 44, 45, 46, 47, 48 };

	public IReadOnlyList<int> TongueDog { get; } = new[] { 77, 63, 64 };
	public IReadOnlyList<int> TongueCat { get; } = new[] { 55, 56 };

	public int BellyCat => 2;
	public int BellyDog => 48;

	public IReadOnlyList<int> SymmetryModeHideBallsCat { get; } = new[]
	{
		0, 4, 8, 9, 12, 14, 16, 17, 18, 22, 25, 27, 30, 32, 34, 38, 41, 49, 50, 51, 57, 58, 59, 63
	};

	public IReadOnlyList<int> SymmetryModeHideBallsDog { get; } = Enumerable.Range(0, 24).ToArray();

	public IReadOnlyList<int> SymmetryModeRightBallsCat { get; } = new[]
	{
		1, 5, 10, 11, 13, 15, 19, 20, 21, 23, 26, 28, 31, 33, 35, 39, 42, 52, 53, 54, 60, 61, 62, 64
	};

	public IReadOnlyList<int> SymmetryModeRightBallsDog { get; } = Enumerable.Range(24, 24).ToArray();
}
