using System.Collections.Generic;

namespace PetzRendering.Services;

public enum Species
{
	Cat = 1,
	Dog = 2
}

public interface IKeyBallsService
{
	Species Species { get; set; }
	int MaxBaseBallNum { get; set; }

	IReadOnlyList<IReadOnlyList<int>> LegsDog { get; }
	IReadOnlyList<IReadOnlyList<int>> LegsCat { get; }
	IReadOnlyList<int> BodyExtDog { get; }
	IReadOnlyList<int> BodyExtCat { get; }
	IReadOnlyList<int> FaceExtDog { get; }
	IReadOnlyList<int> FaceExtCat { get; }
	IReadOnlyList<int> HeadExtDog { get; }
	IReadOnlyList<int> HeadExtCat { get; }
	IReadOnlyList<IReadOnlyList<int>> FootExtDog { get; }
	IReadOnlyList<IReadOnlyList<int>> FootExtCat { get; }
	IReadOnlyDictionary<int, IReadOnlyList<int>> EarExtDog { get; }
	IReadOnlyDictionary<int, IReadOnlyList<int>> EarExtCat { get; }
	IReadOnlyDictionary<int, int> EyesDog { get; }
	IReadOnlyDictionary<int, int> EyesCat { get; }
	IReadOnlyList<int> NoseDog { get; }
	IReadOnlyList<int> NoseCat { get; }
	IReadOnlyList<int> TailDog { get; }
	IReadOnlyList<int> TailCat { get; }
	IReadOnlyList<int> TongueDog { get; }
	IReadOnlyList<int> TongueCat { get; }
	int BellyCat { get; }
	int BellyDog { get; }
	IReadOnlyList<int> SymmetryModeHideBallsCat { get; }
	IReadOnlyList<int> SymmetryModeHideBallsDog { get; }
	IReadOnlyList<int> SymmetryModeRightBallsCat { get; }
	IReadOnlyList<int> SymmetryModeRightBallsDog { get; }
}
