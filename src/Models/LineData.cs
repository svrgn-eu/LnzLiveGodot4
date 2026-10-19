namespace PetzRendering.Models;

public sealed class LineData
{
	public int Start { get; set; }
	public int End { get; set; }
	public int SThick { get; set; } = 100;
	public int EThick { get; set; } = 100;
	public int Fuzz { get; set; }
	public int ColorIndex { get; set; }
	public int LColorIndex { get; set; }
	public int RColorIndex { get; set; }

	public LineData(
		int start,
		int end,
		int sThick = 100,
		int eThick = 100,
		int fuzz = 0,
		int colorIndex = 0,
		int lColorIndex = 0,
		int rColorIndex = 0)
	{
		Start = start;
		End = end;
		SThick = sThick;
		EThick = eThick;
		Fuzz = fuzz;
		RColorIndex = rColorIndex;
		LColorIndex = lColorIndex;
		ColorIndex = colorIndex;
	}
}
