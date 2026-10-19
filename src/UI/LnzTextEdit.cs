using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using PetzRendering.Core;
using PetzRendering.Models;
using PetzRendering.Rendering;
using PetzRendering.Services;

namespace PetzRendering.UI;

public partial class LnzTextEdit : CodeEdit
{
	private sealed class MirroredBallEntry
	{
		public string Line { get; set; } = "";
		public int NewBallNo { get; set; }
		public int? CorrespondingBall { get; set; }
	}

	private static readonly Regex NumberRegex = new(@"[-.\d]+", RegexOptions.Compiled);

	private bool _isUserFile;
	private readonly Dictionary<int, MirroredBallEntry> _ballMap = new();

	public string Filepath { get; private set; } = "";

	private IKeyBallsService KeyBalls => ServiceBootstrapper.GetService<IKeyBallsService>();
	private ILogService Logger => ServiceBootstrapper.GetService<ILogService>();

	[Signal]
	public delegate void FileSavedEventHandler(string filepath);

	[Signal]
	public delegate void FindBallEventHandler(int ballNo);

	[Signal]
	public delegate void FileBackedUpEventHandler();

	public void InsertTextAtCursor(string newText)
	{
		InsertTextAtCaret(newText);
	}

	public override void _Ready()
	{
		WrapMode = LineWrappingMode.None;
		GuttersDrawLineNumbers = true;

		var highlighter = new CodeHighlighter
		{
			NumberColor = new Color(0.82f, 0.60f, 0.40f, 1.0f),
			SymbolColor = new Color(0.545f, 0.58f, 0.62f, 1.0f),
			FunctionColor = new Color(0.902f, 0.929f, 0.953f, 1.0f),
			MemberVariableColor = new Color(0.902f, 0.929f, 0.953f, 1.0f)
		};
		highlighter.AddColorRegion("[", "]", new Color(0.302f, 0.616f, 0.878f, 1.0f), lineOnly: true);
		highlighter.AddColorRegion(";", "", new Color(0.545f, 0.58f, 0.62f, 1.0f), lineOnly: true);
		highlighter.AddColorRegion("#", "", new Color(0.545f, 0.58f, 0.62f, 1.0f), lineOnly: true);
		SyntaxHighlighter = highlighter;
	}

	public void OnExampleFileSelected(string filepath)
	{
		using var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
		if (file != null)
		{
			Text = file.GetAsText();
			Filepath = filepath;
			_isUserFile = false;
		}
	}

	public void OnUserFileSelected(string filepath)
	{
		using var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
		if (file != null)
		{
			Text = file.GetAsText();
			Filepath = filepath;
			_isUserFile = true;
		}
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.SaveFile))
		{
			GetViewport().SetInputAsHandled();
			SaveFile();
		}
	}

	public void SaveBackup()
	{
		if (_isUserFile)
		{
			DirAccess.MakeDirAbsolute("user://resources");
			var backupPath = Filepath.Replace(".lnz", "_backup.lnz", StringComparison.OrdinalIgnoreCase);
			using var file = FileAccess.Open(backupPath, FileAccess.ModeFlags.Write);
			file?.StoreString(Text);
		}
		else
		{
			SaveFile();
			SaveBackup();
			return;
		}

		EmitSignal(SignalName.FileBackedUp);
	}

	public void SaveFile()
	{
		if (string.IsNullOrEmpty(Filepath))
		{
			return;
		}

		DirAccess.MakeDirAbsolute("user://resources");
		if (_isUserFile)
		{
			using var file = FileAccess.Open(Filepath, FileAccess.ModeFlags.Write);
			file?.StoreString(Text);
		}
		else
		{
			var possibleFileName = Filepath.Replace("res://", "user://", StringComparison.Ordinal);
			if (FileAccess.FileExists(possibleFileName))
			{
				var unixTime = (long)Time.GetUnixTimeFromSystem();
				possibleFileName = possibleFileName.Replace(".lnz", $"{unixTime}.lnz", StringComparison.OrdinalIgnoreCase);
			}

			using var file = FileAccess.Open(possibleFileName, FileAccess.ModeFlags.Write);
			file?.StoreString(Text);
			Filepath = possibleFileName;
			_isUserFile = true;
		}

		EmitSignal(SignalName.FileSaved, Filepath);
	}

	public void OnNodeBallSelected(int section, int ballNo, bool isAddball, int maxAddballNo)
	{
		var actualStartPoint = -1;
		var sectionEnum = (Section)section;

		switch (sectionEnum)
		{
			case Section.Ball:
				actualStartPoint = isAddball
					? FindLineInAddballSection(ballNo - maxAddballNo)
					: FindLineInBallSection(ballNo);
				break;
			case Section.Move:
				actualStartPoint = isAddball
					? FindLineInAddballSection(ballNo - maxAddballNo)
					: FindLineInMoveSection(ballNo);
				break;
			case Section.Project:
				actualStartPoint = FindLineInProjectSection(ballNo);
				break;
			case Section.Line:
				actualStartPoint = FindLineInLinezSection(ballNo);
				break;
		}

		if (actualStartPoint == -1)
		{
			return;
		}

		SetCaretLine(actualStartPoint);
		SetCaretColumn(0);
		CenterViewportToCaret();
	}

	private int FindLineInBallSection(int ballNo)
	{
		var sectionFind = Search("[Ballz Info]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return -1;
		}

		var startPoint = sectionFind.Y + 1;
		return FindLineInBallOrAddballSection(ballNo, startPoint);
	}

	private int FindLineInAddballSection(int ballNo)
	{
		var sectionFind = Search("[Add Ball]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return -1;
		}

		var startPoint = sectionFind.Y + 1;
		return FindLineInBallOrAddballSection(ballNo, startPoint);
	}

	private int FindLineInMoveSection(int ballNo)
	{
		var sectionFind = Search("[Move]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return -1;
		}

		var currentLine = GetCaretLine();
		var startOfSection = sectionFind.Y + 1;
		var endOfSection = Search("[", 0, startOfSection, 0).Y;
		var startPoint = (currentLine >= startOfSection && currentLine < endOfSection)
			? currentLine
			: startOfSection;

		var i = 0;
		var targetPrefix = $"{ballNo} ";
		while (true)
		{
			var looped = startPoint + i;
			if (looped >= GetLineCount())
			{
				return startOfSection - 1;
			}

			var line = GetLine(looped).TrimStart(' ');
			if (line.StartsWith('['))
			{
				if (startPoint == startOfSection)
				{
					return startOfSection - 1;
				}

				startPoint = startOfSection;
				i = 0;
				continue;
			}

			if (line.StartsWith(targetPrefix, StringComparison.Ordinal))
			{
				break;
			}

			i += 1;
		}

		return startPoint + i;
	}

	private int FindLineInProjectSection(int ballNo)
	{
		var sectionFind = Search("[Project Ball]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return -1;
		}

		var currentLine = GetCaretLine();
		var startOfSection = sectionFind.Y + 1;
		var endOfSection = Search("[", 0, startOfSection, 0).Y;
		var startPoint = (currentLine >= startOfSection && currentLine < endOfSection)
			? currentLine + 1
			: startOfSection;

		var i = 0;
		var ballNoStr = ballNo.ToString(CultureInfo.InvariantCulture);
		while (true)
		{
			var looped = startPoint + i;
			if (looped >= GetLineCount())
			{
				return startOfSection - 1;
			}

			var line = GetLine(looped).TrimStart(' ');
			var parsedLine = NumberRegex.Matches(line);
			if (line.StartsWith('['))
			{
				if (startPoint == startOfSection)
				{
					return startOfSection - 1;
				}

				startPoint = startOfSection;
				i = 0;
				continue;
			}

			if (parsedLine.Count > 1 && parsedLine[1].Value == ballNoStr)
			{
				break;
			}

			i += 1;
		}

		return startPoint + i;
	}

	private int FindLineInLinezSection(int ballNo)
	{
		var sectionFind = Search("[Linez]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return -1;
		}

		var currentLine = GetCaretLine();
		var startOfSection = sectionFind.Y + 1;
		var endOfSection = Search("[", 0, startOfSection, 0).Y;
		var startPoint = (currentLine >= startOfSection && currentLine < endOfSection)
			? currentLine + 1
			: startOfSection;

		var i = 0;
		var ballNoStr = ballNo.ToString(CultureInfo.InvariantCulture);
		while (true)
		{
			var looped = startPoint + i;
			if (looped >= GetLineCount())
			{
				return startOfSection - 1;
			}

			var line = GetLine(looped).TrimStart(' ');
			var parsedLine = NumberRegex.Matches(line);
			if (line.StartsWith('['))
			{
				if (startPoint == startOfSection)
				{
					return startOfSection - 1;
				}

				startPoint = startOfSection;
				i = 0;
				continue;
			}

			if (parsedLine.Count > 1 && (parsedLine[0].Value == ballNoStr || parsedLine[1].Value == ballNoStr))
			{
				break;
			}

			i += 1;
		}

		return startPoint + i;
	}

	private int FindLineInBallOrAddballSection(int ballNo, int startPoint)
	{
		while (startPoint < GetLineCount())
		{
			var line = GetLine(startPoint);
			if (!line.TrimStart(' ').StartsWith(';'))
			{
				break;
			}

			startPoint += 1;
		}

		var i = 0;
		var j = -1;
		while (startPoint + i < GetLineCount())
		{
			var line = GetLine(startPoint + i);
			if (!line.TrimStart(' ').StartsWith(';'))
			{
				j += 1;
			}

			if (j == ballNo)
			{
				break;
			}

			i += 1;
		}

		return startPoint + i;
	}

	public void OnToolsMenuColorEntirePet(string colorIndex, string outlineColorIndex)
	{
		var ballsToExclude = GetExcludedBalls(includeNose: true);

		var sectionFind = Search("[Ballz Info]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			var startOfSection = sectionFind.Y + 1;
			var i = 0;
			while (startOfSection + i < GetLineCount())
			{
				if (ballsToExclude.Contains(i))
				{
					i += 1;
					continue;
				}

				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith(';'))
				{
					i += 1;
					continue;
				}

				if (line.StartsWith('['))
				{
					break;
				}

				var parsedLine = NumberRegex.Matches(line);
				var sb = new StringBuilder();
				for (var n = 0; n < parsedLine.Count; n++)
				{
					var item = parsedLine[n].Value;
					if (n == 0 && !string.IsNullOrEmpty(colorIndex))
					{
						sb.Append(colorIndex).Append(' ');
					}
					else if (n == 1 && !string.IsNullOrEmpty(outlineColorIndex))
					{
						sb.Append(outlineColorIndex).Append(' ');
					}
					else
					{
						sb.Append(item).Append(' ');
					}
				}

				SetLine(startOfSection + i, sb.ToString());
				i += 1;
			}
		}

		sectionFind = Search("[Add Ball]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			var startOfSection = sectionFind.Y + 1;
			var i = 0;
			while (startOfSection + i < GetLineCount())
			{
				if (ballsToExclude.Contains(i + KeyBalls.MaxBaseBallNum))
				{
					i += 1;
					continue;
				}

				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith(';'))
				{
					i += 1;
					continue;
				}

				if (line.StartsWith('['))
				{
					break;
				}

				var parsedLine = NumberRegex.Matches(line);
				if (parsedLine.Count == 0 || ballsToExclude.Contains(ParseInt(parsedLine[0].Value)))
				{
					i += 1;
					continue;
				}

				var sb = new StringBuilder();
				for (var n = 0; n < parsedLine.Count; n++)
				{
					var item = parsedLine[n].Value;
					if (n == 4 && !string.IsNullOrEmpty(colorIndex))
					{
						sb.Append(colorIndex).Append(' ');
					}
					else if (n == 5 && !string.IsNullOrEmpty(outlineColorIndex))
					{
						sb.Append(outlineColorIndex).Append(' ');
					}
					else
					{
						sb.Append(item).Append(' ');
					}
				}

				SetLine(startOfSection + i, sb.ToString());
				i += 1;
			}
		}

		SaveFile();
	}

	public void OnToolsMenuColorPartPet(
		Godot.Collections.Array<int> coreBallNos,
		string colorIndex,
		string outlineColorIndex,
		string intendedPart)
	{
		var coreSet = new HashSet<int>(coreBallNos);
		var ballsToExclude = GetExcludedBalls(includeNose: intendedPart != "NOSE");

		var sectionFind = Search("[Ballz Info]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			var startOfSection = sectionFind.Y + 1;
			var i = 0;
			while (startOfSection + i < GetLineCount())
			{
				if (ballsToExclude.Contains(i))
				{
					i += 1;
					continue;
				}

				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith(';'))
				{
					i += 1;
					continue;
				}

				if (line.StartsWith('['))
				{
					break;
				}

				if (!coreSet.Contains(i))
				{
					i += 1;
					continue;
				}

				var parsedLine = NumberRegex.Matches(line);
				var sb = new StringBuilder();
				for (var n = 0; n < parsedLine.Count; n++)
				{
					var item = parsedLine[n].Value;
					if (n == 0 && !string.IsNullOrEmpty(colorIndex))
					{
						sb.Append(colorIndex).Append(' ');
					}
					else if (n == 1 && !string.IsNullOrEmpty(outlineColorIndex))
					{
						sb.Append(outlineColorIndex).Append(' ');
					}
					else
					{
						sb.Append(item).Append(' ');
					}
				}

				SetLine(startOfSection + i, sb.ToString());
				i += 1;
			}
		}

		sectionFind = Search("[Add Ball]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			var startOfSection = sectionFind.Y + 1;
			var i = 0;
			while (startOfSection + i < GetLineCount())
			{
				if (ballsToExclude.Contains(i + KeyBalls.MaxBaseBallNum))
				{
					i += 1;
					continue;
				}

				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith(';'))
				{
					i += 1;
					continue;
				}

				if (line.StartsWith('['))
				{
					break;
				}

				var parsedLine = NumberRegex.Matches(line);
				if (parsedLine.Count == 0)
				{
					i += 1;
					continue;
				}

				var baseBall = ParseInt(parsedLine[0].Value);
				if (ballsToExclude.Contains(baseBall) || !coreSet.Contains(baseBall))
				{
					i += 1;
					continue;
				}

				var sb = new StringBuilder();
				for (var n = 0; n < parsedLine.Count; n++)
				{
					var item = parsedLine[n].Value;
					if (n == 4 && !string.IsNullOrEmpty(colorIndex))
					{
						sb.Append(colorIndex).Append(' ');
					}
					else if (n == 5 && !string.IsNullOrEmpty(outlineColorIndex))
					{
						sb.Append(outlineColorIndex).Append(' ');
					}
					else
					{
						sb.Append(item).Append(' ');
					}
				}

				SetLine(startOfSection + i, sb.ToString());
				i += 1;
			}
		}

		SaveFile();
	}

	private HashSet<int> GetExcludedBalls(bool includeNose)
	{
		var ballsToExclude = new HashSet<int>();
		if (KeyBalls.Species == Species.Cat)
		{
			ballsToExclude.UnionWith(KeyBalls.EyesCat.Keys);
			ballsToExclude.UnionWith(KeyBalls.EyesCat.Values);
			ballsToExclude.UnionWith(KeyBalls.TongueCat);
			if (includeNose)
			{
				ballsToExclude.UnionWith(KeyBalls.NoseCat);
			}
		}
		else
		{
			ballsToExclude.UnionWith(KeyBalls.EyesDog.Keys);
			ballsToExclude.UnionWith(KeyBalls.EyesDog.Values);
			ballsToExclude.UnionWith(KeyBalls.TongueDog);
			if (includeNose)
			{
				ballsToExclude.UnionWith(KeyBalls.NoseDog);
			}
		}

		return ballsToExclude;
	}

	private int GetCorrespondingRightBall(int leftBallIndex)
	{
		if (leftBallIndex < 67)
		{
			if (KeyBalls.Species == Species.Cat)
			{
				if (leftBallIndex is 8 or 9)
				{
					return leftBallIndex + 2;
				}

				if (leftBallIndex is 16 or 17 or 18 or 49 or 50 or 51 or 57 or 58 or 59)
				{
					return leftBallIndex + 3;
				}

				return leftBallIndex + 1;
			}

			return leftBallIndex + 24;
		}

		var corr = _ballMap[leftBallIndex].CorrespondingBall!.Value;
		return _ballMap[corr].NewBallNo;
	}

	private int GetCorrespondingLeftBall(int rightBallIndex)
	{
		if (rightBallIndex < 67)
		{
			if (KeyBalls.Species == Species.Cat)
			{
				if (rightBallIndex is 10 or 11)
				{
					return rightBallIndex - 2;
				}

				if (rightBallIndex is 19 or 20 or 21 or 52 or 53 or 54 or 60 or 61 or 62)
				{
					return rightBallIndex - 3;
				}

				return rightBallIndex - 1;
			}

			return rightBallIndex - 24;
		}

		var corr = _ballMap[rightBallIndex].CorrespondingBall!.Value;
		return _ballMap[corr].NewBallNo;
	}

	public void OnToolsMenuCopyLToR()
	{
		SaveBackup();

		_ballMap.Clear();
		var sectionFind = Search("[Ballz Info]", 0, 0, 0);
		var startOfSection = sectionFind.Y + 1;
		var i = 0;

		var leftBallsList = KeyBalls.Species == Species.Cat
			? KeyBalls.SymmetryModeHideBallsCat.ToList()
			: KeyBalls.SymmetryModeHideBallsDog.ToList();
		var rightBallsList = KeyBalls.Species == Species.Cat
			? KeyBalls.SymmetryModeRightBallsCat.ToList()
			: KeyBalls.SymmetryModeRightBallsDog.ToList();
		var middleBallsList = new List<int>();

		if (leftBallsList.Count != rightBallsList.Count)
		{
			Logger.LogWarning("Symmetry ball list size mismatch.", nameof(LnzTextEdit));
		}

		for (var n = 0; n < 66; n++)
		{
			if (!leftBallsList.Contains(n) && !rightBallsList.Contains(n))
			{
				middleBallsList.Add(n);
			}
		}

		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('['))
			{
				break;
			}

			if (leftBallsList.Contains(i) || middleBallsList.Contains(i))
			{
				var d = new MirroredBallEntry { Line = line, NewBallNo = i };
				if (leftBallsList.Contains(i))
				{
					var parsedLine = NumberRegex.Matches(line);
					string mirroredLine;
					if (parsedLine.Count > 4 && parsedLine[4].Value is "0" or "-2")
					{
						var sb = new StringBuilder();
						for (var p = 0; p < parsedLine.Count; p++)
						{
							if (p == 4)
							{
								sb.Append(parsedLine[p].Value == "0" ? "-2 " : "0 ");
							}
							else
							{
								sb.Append(parsedLine[p].Value).Append(' ');
							}
						}

						mirroredLine = sb.ToString();
					}
					else
					{
						mirroredLine = line;
					}

					var corrRight = GetCorrespondingRightBall(i);
					d.CorrespondingBall = corrRight;
					SetLine(startOfSection + corrRight, mirroredLine);
					_ballMap[corrRight] = new MirroredBallEntry
					{
						Line = mirroredLine,
						CorrespondingBall = i,
						NewBallNo = corrRight
					};
				}

				_ballMap[i] = d;
			}

			i += 1;
		}

		// AddBalls
		sectionFind = Search("[Add Ball]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		var ballNo = 67;
		var ballsToAddTemp = new List<MirroredBallEntry>();
		var newBallCount = 67;

		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			if (ballNo <= 76)
			{
				_ballMap[ballNo] = new MirroredBallEntry { Line = line, NewBallNo = newBallCount };
				newBallCount += 1;
			}
			else
			{
				var firstSpaceIdx = line.IndexOf(' ');
				var firstToken = firstSpaceIdx > 0 ? line[..firstSpaceIdx] : line;
				var baseBall = ParseInt(firstToken);

				if (leftBallsList.Contains(baseBall))
				{
					leftBallsList.Add(ballNo);
					_ballMap[ballNo] = new MirroredBallEntry { Line = line, NewBallNo = newBallCount };
					newBallCount += 1;
					var correspondingRightBall = GetCorrespondingRightBall(baseBall);
					var parsedLine = NumberRegex.Matches(line);
					var sb = new StringBuilder();
					for (var p = 0; p < parsedLine.Count; p++)
					{
						var val = parsedLine[p].Value;
						if (p == 0)
						{
							sb.Append(correspondingRightBall).Append(' ');
						}
						else if (p == 1)
						{
							sb.Append((ParseInt(val) * -1.0f).ToString(CultureInfo.InvariantCulture)).Append(' ');
						}
						else if (p == 9 && val is "0" or "-2")
						{
							sb.Append(val == "0" ? "-2 " : "0 ");
						}
						else
						{
							sb.Append(val).Append(' ');
						}
					}

					ballsToAddTemp.Add(new MirroredBallEntry
					{
						Line = sb.ToString(),
						CorrespondingBall = ballNo
					});
				}
				else if (middleBallsList.Contains(baseBall))
				{
					var parsedLine = NumberRegex.Matches(line);
					var xPos = ParseInt(parsedLine[1].Value);
					if (xPos > 0)
					{
						_ballMap[ballNo] = new MirroredBallEntry { Line = line, NewBallNo = newBallCount };
						newBallCount += 1;
						leftBallsList.Add(ballNo);
						var sb = new StringBuilder();
						for (var p = 0; p < parsedLine.Count; p++)
						{
							var val = parsedLine[p].Value;
							if (p == 1)
							{
								sb.Append((ParseInt(val) * -1.0f).ToString(CultureInfo.InvariantCulture)).Append(' ');
							}
							else if (p == 9 && val is "0" or "-2")
							{
								sb.Append(val == "0" ? "-2 " : "0 ");
							}
							else
							{
								sb.Append(val).Append(' ');
							}
						}

						ballsToAddTemp.Add(new MirroredBallEntry
						{
							Line = sb.ToString(),
							CorrespondingBall = ballNo
						});
					}
					else if (xPos == 0)
					{
						_ballMap[ballNo] = new MirroredBallEntry { Line = line, NewBallNo = newBallCount };
						newBallCount += 1;
						middleBallsList.Add(ballNo);
					}
				}
			}

			i += 1;
			ballNo += 1;
		}

		var addCount = _ballMap.Keys.Max() + 1;
		foreach (var b in ballsToAddTemp)
		{
			b.NewBallNo = newBallCount;
			_ballMap[addCount] = b;
			_ballMap[b.CorrespondingBall!.Value].CorrespondingBall = addCount;
			addCount += 1;
			newBallCount += 1;
		}

		var linesInAddballSection = i;

		// Lines
		sectionFind = Search("[Linez]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		var linesList = new List<string>();
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var startBall = ParseInt(parsedLine[0].Value);
			var endBall = ParseInt(parsedLine[1].Value);

			if (leftBallsList.Contains(startBall) || leftBallsList.Contains(endBall))
			{
				if (_ballMap.TryGetValue(startBall, out var startEntry) &&
					_ballMap.TryGetValue(endBall, out var endEntry))
				{
					var finalStartBall = startEntry.NewBallNo;
					int? additionalStartBall = leftBallsList.Contains(startBall)
						? GetCorrespondingRightBall(startBall)
						: null;
					var finalEndBall = endEntry.NewBallNo;
					int? additionalEndBall = leftBallsList.Contains(endBall)
						? GetCorrespondingRightBall(endBall)
						: null;

					if (additionalEndBall.HasValue || additionalStartBall.HasValue)
					{
						var effStart = additionalStartBall ?? finalStartBall;
						var effEnd = additionalEndBall ?? finalEndBall;
						var sbMirror = new StringBuilder();
						for (var p = 0; p < parsedLine.Count; p++)
						{
							if (p == 0) sbMirror.Append(effStart).Append(' ');
							else if (p == 1) sbMirror.Append(effEnd).Append(' ');
							else sbMirror.Append(parsedLine[p].Value).Append(' ');
						}

						linesList.Add(sbMirror.ToString());
					}

					var sbOrig = new StringBuilder();
					for (var p = 0; p < parsedLine.Count; p++)
					{
						if (p == 0) sbOrig.Append(finalStartBall).Append(' ');
						else if (p == 1) sbOrig.Append(finalEndBall).Append(' ');
						else sbOrig.Append(parsedLine[p].Value).Append(' ');
					}

					linesList.Add(sbOrig.ToString());
				}
			}
			else if (middleBallsList.Contains(startBall) && middleBallsList.Contains(endBall))
			{
				if (_ballMap.TryGetValue(startBall, out var startEntry) &&
					_ballMap.TryGetValue(endBall, out var endEntry))
				{
					var sb = new StringBuilder();
					for (var p = 0; p < parsedLine.Count; p++)
					{
						if (p == 0) sb.Append(startEntry.NewBallNo).Append(' ');
						else if (p == 1) sb.Append(endEntry.NewBallNo).Append(' ');
						else sb.Append(parsedLine[p].Value).Append(' ');
					}

					linesList.Add(sb.ToString());
				}
			}

			i += 1;
		}

		var linesInLinezSection = i;

		// Moves
		sectionFind = Search("[Move]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		var movesList = new List<string>();
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var moveBallNo = ParseInt(parsedLine[0].Value);
			if (leftBallsList.Contains(moveBallNo))
			{
				movesList.Add(line);
				var sb = new StringBuilder();
				for (var p = 0; p < parsedLine.Count; p++)
				{
					if (p == 0)
					{
						sb.Append(GetCorrespondingRightBall(moveBallNo)).Append(' ');
					}
					else if (p == 1)
					{
						sb.Append((ParseInt(parsedLine[p].Value) * -1.0f).ToString(CultureInfo.InvariantCulture)).Append(' ');
					}
					else
					{
						sb.Append(parsedLine[p].Value).Append(' ');
					}
				}

				movesList.Add(sb.ToString());
			}
			else if (middleBallsList.Contains(moveBallNo))
			{
				movesList.Add(line);
			}

			i += 1;
		}

		var linesInMoveSection = i;

		// Projections
		sectionFind = Search("[Project Ball]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		var projectionsList = new List<string>();
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var baseBallNo = ParseInt(parsedLine[0].Value);
			var moveBallNo = ParseInt(parsedLine[1].Value);

			if (leftBallsList.Contains(moveBallNo))
			{
				if (moveBallNo > 66)
				{
					var sbOrig = new StringBuilder();
					for (var p = 0; p < parsedLine.Count; p++)
					{
						if (p == 0) sbOrig.Append(_ballMap[baseBallNo].NewBallNo).Append(' ');
						else if (p == 1) sbOrig.Append(_ballMap[moveBallNo].NewBallNo).Append(' ');
						else sbOrig.Append(parsedLine[p].Value).Append(' ');
					}

					projectionsList.Add(sbOrig.ToString());
				}
				else
				{
					projectionsList.Add(line);
				}

				var sbMirror = new StringBuilder();
				for (var p = 0; p < parsedLine.Count; p++)
				{
					if (p == 0)
					{
						if (leftBallsList.Contains(baseBallNo))
						{
							sbMirror.Append(GetCorrespondingRightBall(baseBallNo)).Append(' ');
						}
						else if (!middleBallsList.Contains(baseBallNo))
						{
							sbMirror.Append(GetCorrespondingLeftBall(baseBallNo)).Append(' ');
						}
						else
						{
							sbMirror.Append(_ballMap[baseBallNo].NewBallNo).Append(' ');
						}
					}
					else if (p == 1)
					{
						if (leftBallsList.Contains(moveBallNo))
						{
							sbMirror.Append(GetCorrespondingRightBall(moveBallNo)).Append(' ');
						}
						else
						{
							sbMirror.Append(_ballMap[moveBallNo].NewBallNo).Append(' ');
						}
					}
					else
					{
						sbMirror.Append(parsedLine[p].Value).Append(' ');
					}
				}

				projectionsList.Add(sbMirror.ToString());
			}
			else if (middleBallsList.Contains(moveBallNo))
			{
				var sb = new StringBuilder();
				for (var p = 0; p < parsedLine.Count; p++)
				{
					if (p == 0)
					{
						if (leftBallsList.Contains(baseBallNo))
						{
							sb.Append(GetCorrespondingRightBall(baseBallNo)).Append(' ');
						}
						else if (!middleBallsList.Contains(baseBallNo))
						{
							sb.Append(GetCorrespondingLeftBall(baseBallNo)).Append(' ');
						}
						else
						{
							sb.Append(_ballMap[baseBallNo].NewBallNo).Append(' ');
						}
					}
					else if (p == 1)
					{
						sb.Append(_ballMap[moveBallNo].NewBallNo).Append(' ');
					}
					else
					{
						sb.Append(parsedLine[p].Value);
					}
				}

				projectionsList.Add(sb.ToString());
			}

			i += 1;
		}

		var linesInProjectionsSection = i;

		// Paintballs
		sectionFind = Search("[Paint Ballz]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		var paintballsList = new List<string>();
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var baseBallNo = ParseInt(parsedLine[0].Value);
			if (leftBallsList.Contains(baseBallNo))
			{
				var newBaseBallNo = _ballMap[baseBallNo].NewBallNo;
				var sbOrig = new StringBuilder();
				for (var p = 0; p < parsedLine.Count; p++)
				{
					if (p == 0) sbOrig.Append(newBaseBallNo).Append(' ');
					else sbOrig.Append(parsedLine[p].Value).Append(' ');
				}

				paintballsList.Add(sbOrig.ToString());

				var sbFlip = new StringBuilder();
				for (var p = 0; p < parsedLine.Count; p++)
				{
					if (p == 0)
					{
						sbFlip.Append(GetCorrespondingRightBall(baseBallNo)).Append(' ');
					}
					else if (p == 2)
					{
						var flippedX = float.Parse(parsedLine[p].Value, CultureInfo.InvariantCulture) * -1.0f;
						sbFlip.Append(flippedX.ToString(CultureInfo.InvariantCulture)).Append(' ');
					}
					else
					{
						sbFlip.Append(parsedLine[p].Value).Append(' ');
					}
				}

				paintballsList.Add(sbFlip.ToString());
			}
			else if (middleBallsList.Contains(baseBallNo))
			{
				var xPos = float.Parse(parsedLine[2].Value, CultureInfo.InvariantCulture);
				if (xPos >= 0.0f)
				{
					var newBaseBallNo = _ballMap[baseBallNo].NewBallNo;
					var sbOrig = new StringBuilder();
					for (var p = 0; p < parsedLine.Count; p++)
					{
						if (p == 0) sbOrig.Append(newBaseBallNo).Append(' ');
						else sbOrig.Append(parsedLine[p].Value).Append(' ');
					}

					paintballsList.Add(sbOrig.ToString());

					if (xPos > 0.0f)
					{
						var sbFlip = new StringBuilder();
						for (var p = 0; p < parsedLine.Count; p++)
						{
							if (p == 0)
							{
								sbFlip.Append(newBaseBallNo).Append(' ');
							}
							else if (p == 2)
							{
								sbFlip.Append((xPos * -1.0f).ToString(CultureInfo.InvariantCulture)).Append(' ');
							}
							else
							{
								sbFlip.Append(parsedLine[p].Value).Append(' ');
							}
						}

						paintballsList.Add(sbFlip.ToString());
					}
				}
			}

			i += 1;
		}

		var linesInPaintballSection = i;

		// Replace [Add Ball] lines
		sectionFind = Search("[Add Ball]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		Select(startOfSection, 0, startOfSection + linesInAddballSection - 1, 999);
		Cut();
		SetCaretLine(startOfSection);
		SetCaretColumn(0);
		var addballLines = _ballMap
			.Where(kv => kv.Key > 66)
			.OrderBy(kv => kv.Key)
			.Select(kv => kv.Value.Line);
		InsertTextAtCursor(string.Join("\n", addballLines).Trim());

		// Replace [Linez] lines
		sectionFind = Search("[Linez]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		Select(startOfSection, 0, startOfSection + linesInLinezSection - 1, 999);
		Cut();
		SetCaretLine(startOfSection);
		SetCaretColumn(0);
		InsertTextAtCursor(string.Join("\n", linesList).Trim());

		// Replace [Move] lines
		sectionFind = Search("[Move]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		Select(startOfSection, 0, startOfSection + linesInMoveSection - 1, 999);
		Cut();
		SetCaretLine(startOfSection);
		SetCaretColumn(0);
		InsertTextAtCursor(string.Join("\n", movesList).Trim());

		// Replace [Project Ball] lines
		sectionFind = Search("[Project Ball]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		Select(startOfSection, 0, startOfSection + linesInProjectionsSection - 1, 999);
		Cut();
		SetCaretLine(startOfSection);
		SetCaretColumn(0);
		InsertTextAtCursor(string.Join("\n", projectionsList).Trim());

		// Replace [Paint Ballz] lines
		sectionFind = Search("[Paint Ballz]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		Select(startOfSection, 0, startOfSection + linesInPaintballSection - 1, 999);
		Cut();
		SetCaretLine(startOfSection);
		SetCaretColumn(0);
		InsertTextAtCursor(string.Join("\n", paintballsList).Trim());

		_ballMap.Clear();
		SaveFile();
	}

	public void OnTreeBackupFile()
	{
		SaveBackup();
	}

	public void OnToolsMenuAddBall(Node3D selectedVisualBall)
	{
		if (selectedVisualBall is not IPetBallNode petBall)
		{
			return;
		}

		var realBaseBall = petBall.BaseBallNo != -1 ? petBall.BaseBallNo : petBall.BallNo;

		var sectionFind = Search("[Add Ball]", 0, 0, 0);
		var startOfSection = sectionFind.Y + 1;
		var i = 0;
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			i += 1;
		}

		var linesInAddballSection = i;
		var newBallNo = 67 + i;
		var newBallCursorPosition = startOfSection + linesInAddballSection;
		SetCaretLine(newBallCursorPosition);
		SetCaretColumn(0);

		var position = petBall.BaseBallNo != -1
			? petBall.AsNode3D.Transform.Origin * 1000.0f
			: Vector3.Zero;

		var newAddballText =
			$"{realBaseBall} {(int)position.X} {(int)position.Y} {(int)position.Z} {petBall.ColorIndex} {petBall.OutlineColorIndex} 0 {petBall.FuzzAmount} 0 {petBall.OldOutline} 30 0 0 0 -1\n";
		InsertTextAtCursor(newAddballText);

		// Add line
		sectionFind = Search("[Linez]", 0, 0, 0);
		startOfSection = sectionFind.Y + 1;
		i = 0;
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			i += 1;
		}

		SetCaretLine(startOfSection + i);
		SetCaretColumn(0);
		var newLineText = $"{newBallNo} {petBall.BallNo} 0 -1 -1 -1 95 95 -1 0\n";
		InsertTextAtCursor(newLineText);
		SetCaretLine(newBallCursorPosition);
		CenterViewportToCaret();

		SaveFile();
	}

	public void OnLnzTextEditGuiInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.FindBall))
		{
			AcceptEvent();
			var nearestSectionStart = Search("[", (uint)SearchFlags.Backwards, GetCaretLine(), 0);
			if (nearestSectionStart.Y != -1)
			{
				var nearestSection = GetLine(nearestSectionStart.Y);
				if (nearestSection == "[Ballz Info]")
				{
					var lineNumber = GetCaretLine() - nearestSectionStart.Y - 1;
					EmitSignal(SignalName.FindBall, lineNumber);
					return;
				}

				if (nearestSection == "[Add Ball]")
				{
					var lineNumber = GetCaretLine() - nearestSectionStart.Y + 66;
					EmitSignal(SignalName.FindBall, lineNumber);
					return;
				}
			}

			if (int.TryParse(GetWordUnderCaret(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var wordBall))
			{
				EmitSignal(SignalName.FindBall, wordBall);
			}
		}
	}

	public void OnNodeAddballDeleted(int ballNo)
	{
		var lineNo = FindLineInAddballSection(ballNo - 67);
		Select(lineNo, 0, lineNo + 1, 0);
		Cut();

		// Linez
		var sectionFind = Search("[Linez]", 0, 0, 0);
		var startOfSection = sectionFind.Y + 1;
		var i = 0;
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith('[') || string.IsNullOrEmpty(line))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var startBall = ParseInt(parsedLine[0].Value);
			var endBall = ParseInt(parsedLine[1].Value);
			if (startBall == ballNo || endBall == ballNo)
			{
				Select(startOfSection + i, 0, startOfSection + i + 1, 0);
				Cut();
				continue;
			}

			if (startBall > ballNo || endBall > ballNo)
			{
				if (startBall > ballNo) startBall -= 1;
				if (endBall > ballNo) endBall -= 1;
				var startOfRest = parsedLine[2].Index;
				var replacedLine = $"{startBall} {endBall} {line[startOfRest..]}";
				SetLine(startOfSection + i, replacedLine);
			}

			i += 1;
		}

		// Omissions
		sectionFind = Search("[Omissions]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			startOfSection = sectionFind.Y + 1;
			i = 0;
			while (startOfSection + i < GetLineCount())
			{
				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith('[') || string.IsNullOrEmpty(line))
				{
					break;
				}

				var omittedBall = ParseInt(line);
				if (omittedBall == ballNo)
				{
					Select(startOfSection + i, 0, startOfSection + i + 1, 0);
					Cut();
					continue;
				}

				if (omittedBall > ballNo)
				{
					SetLine(startOfSection + i, (omittedBall - 1).ToString(CultureInfo.InvariantCulture));
				}

				i += 1;
			}
		}

		// Projections
		sectionFind = Search("[Project Ball]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			startOfSection = sectionFind.Y + 1;
			i = 0;
			while (startOfSection + i < GetLineCount())
			{
				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith('[') || string.IsNullOrEmpty(line))
				{
					break;
				}

				var parsedLine = NumberRegex.Matches(line);
				var moveBallNo = ParseInt(parsedLine[1].Value);
				if (moveBallNo == ballNo)
				{
					Select(startOfSection + i, 0, startOfSection + i + 1, 0);
					Cut();
					continue;
				}

				if (moveBallNo > ballNo)
				{
					var replaceLine = $"{parsedLine[0].Value} {moveBallNo - 1} {line[parsedLine[2].Index..]}";
					SetLine(startOfSection + i, replaceLine);
				}

				i += 1;
			}
		}

		// PaintBallz
		sectionFind = Search("[Paint Ballz]", 0, 0, 0);
		if (sectionFind.Y != -1)
		{
			startOfSection = sectionFind.Y + 1;
			i = 0;
			while (startOfSection + i < GetLineCount())
			{
				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith('[') || string.IsNullOrEmpty(line))
				{
					break;
				}

				var spaceIdx = line.IndexOf(' ');
				if (spaceIdx > 0)
				{
					var baseBallNo = ParseInt(line[..spaceIdx]);
					if (baseBallNo == ballNo)
					{
						Select(startOfSection + i, 0, startOfSection + i + 1, 0);
						Cut();
						continue;
					}

					if (baseBallNo > ballNo)
					{
						var replaceLine = $"{baseBallNo} {line[(spaceIdx + 1)..]}";
						SetLine(startOfSection + i, replaceLine);
					}
				}

				i += 1;
			}
		}

		SaveFile();
	}

	public void OnToolsMenuRecolor(
		Godot.Collections.Dictionary<string, string> recolorInfo,
		bool ballsOn,
		bool ballOutlinesOn,
		bool paintballsOn,
		bool linesOn)
	{
		SaveBackup();

		var ballsToExclude = GetExcludedBalls(includeNose: true);

		if (ballsOn || ballOutlinesOn)
		{
			var sectionFind = Search("[Ballz Info]", 0, 0, 0);
			if (sectionFind.Y != -1)
			{
				var startOfSection = sectionFind.Y + 1;
				var i = 0;
				while (startOfSection + i < GetLineCount())
				{
					if (ballsToExclude.Contains(i))
					{
						i += 1;
						continue;
					}

					var line = GetLine(startOfSection + i).TrimStart(' ');
					if (line.StartsWith('[') || i > GetLineCount())
					{
						break;
					}

					if (line.StartsWith(';') || string.IsNullOrEmpty(line))
					{
						i += 1;
						continue;
					}

					var parsedLine = NumberRegex.Matches(line);
					var color = parsedLine[0].Value;
					var outlineColor = parsedLine[1].Value;
					if ((recolorInfo.ContainsKey(color) && ballsOn) ||
						(recolorInfo.ContainsKey(outlineColor) && ballOutlinesOn))
					{
						var sb = new StringBuilder();
						for (var n = 0; n < parsedLine.Count; n++)
						{
							var item = parsedLine[n].Value;
							if (n == 0 && recolorInfo.TryGetValue(item, out var newCol) && ballsOn)
							{
								sb.Append(newCol).Append(' ');
							}
							else if (n == 1 && recolorInfo.TryGetValue(item, out var newOutCol) && ballOutlinesOn)
							{
								sb.Append(newOutCol).Append(' ');
							}
							else
							{
								sb.Append(item).Append(' ');
							}
						}

						SetLine(startOfSection + i, sb.ToString());
					}

					i += 1;
				}
			}

			sectionFind = Search("[Add Ball]", 0, 0, 0);
			if (sectionFind.Y != -1)
			{
				var startOfSection = sectionFind.Y + 1;
				var i = 0;
				while (startOfSection + i < GetLineCount())
				{
					if (ballsToExclude.Contains(i + KeyBalls.MaxBaseBallNum))
					{
						i += 1;
						continue;
					}

					var line = GetLine(startOfSection + i).TrimStart(' ');
					if (line.StartsWith('[') || i > GetLineCount())
					{
						break;
					}

					if (line.StartsWith(';') || string.IsNullOrEmpty(line))
					{
						i += 1;
						continue;
					}

					var parsedLine = NumberRegex.Matches(line);
					if (ballsToExclude.Contains(ParseInt(parsedLine[0].Value)))
					{
						i += 1;
						continue;
					}

					var color = parsedLine[4].Value;
					var outlineColor = parsedLine[5].Value;
					if ((recolorInfo.ContainsKey(color) && ballsOn) ||
						(recolorInfo.ContainsKey(outlineColor) && ballOutlinesOn))
					{
						var sb = new StringBuilder();
						for (var n = 0; n < parsedLine.Count; n++)
						{
							var item = parsedLine[n].Value;
							if (n == 4 && recolorInfo.TryGetValue(item, out var newCol) && ballsOn)
							{
								sb.Append(newCol).Append(' ');
							}
							else if (n == 5 && recolorInfo.TryGetValue(item, out var newOutCol) && ballOutlinesOn)
							{
								sb.Append(newOutCol).Append(' ');
							}
							else
							{
								sb.Append(item).Append(' ');
							}
						}

						SetLine(startOfSection + i, sb.ToString());
					}

					i += 1;
				}
			}
		}

		if (paintballsOn)
		{
			var sectionFind = Search("[Paint Ballz]", 0, 0, 0);
			if (sectionFind.Y != -1)
			{
				var startOfSection = sectionFind.Y + 1;
				var i = 0;
				while (startOfSection + i < GetLineCount())
				{
					if (ballsToExclude.Contains(i + KeyBalls.MaxBaseBallNum))
					{
						i += 1;
						continue;
					}

					var line = GetLine(startOfSection + i).TrimStart(' ');
					if (line.StartsWith('[') || i > GetLineCount())
					{
						break;
					}

					if (line.StartsWith(';') || string.IsNullOrEmpty(line))
					{
						i += 1;
						continue;
					}

					var parsedLine = NumberRegex.Matches(line);
					if (ballsToExclude.Contains(ParseInt(parsedLine[0].Value)))
					{
						i += 1;
						continue;
					}

					var color = parsedLine[5].Value;
					if (recolorInfo.TryGetValue(color, out var newColor))
					{
						var sb = new StringBuilder();
						for (var n = 0; n < parsedLine.Count; n++)
						{
							var item = parsedLine[n].Value;
							sb.Append(n == 5 ? newColor : item).Append(' ');
						}

						SetLine(startOfSection + i, sb.ToString());
					}

					i += 1;
				}
			}
		}

		if (linesOn)
		{
			var sectionFind = Search("[Linez]", 0, 0, 0);
			if (sectionFind.Y != -1)
			{
				var startOfSection = sectionFind.Y + 1;
				var i = 0;
				while (startOfSection + i < GetLineCount())
				{
					var line = GetLine(startOfSection + i).TrimStart(' ');
					if (line.StartsWith('[') || string.IsNullOrEmpty(line) || i > GetLineCount())
					{
						break;
					}

					var parsedLine = NumberRegex.Matches(line);
					var mainColor = parsedLine[3].Value;
					var lColor = parsedLine[4].Value;
					var rColor = parsedLine[5].Value;

					if (recolorInfo.ContainsKey(mainColor) ||
						recolorInfo.ContainsKey(lColor) ||
						recolorInfo.ContainsKey(rColor))
					{
						var sb = new StringBuilder();
						for (var n = 0; n < parsedLine.Count; n++)
						{
							if (n == 3 && recolorInfo.TryGetValue(mainColor, out var newMain))
							{
								sb.Append(newMain).Append(' ');
							}
							else if (n == 4 && recolorInfo.TryGetValue(lColor, out var newL))
							{
								sb.Append(newL).Append(' ');
							}
							else if (n == 5 && recolorInfo.TryGetValue(rColor, out var newR))
							{
								sb.Append(newR).Append(' ');
							}
							else
							{
								sb.Append(parsedLine[n].Value).Append(' ');
							}
						}

						SetLine(startOfSection + i, sb.ToString());
					}

					i += 1;
				}
			}
		}

		SaveFile();
	}

	public void OnToolsMenuMoveHead(int x, int y, int z)
	{
		var headBalls = KeyBalls.Species == Species.Cat
			? KeyBalls.HeadExtCat.ToList()
			: KeyBalls.HeadExtDog.ToList();

		var sectionFind = Search("[Move]", 0, 0, 0);
		if (sectionFind.Y == -1)
		{
			return;
		}

		var startOfSection = sectionFind.Y + 1;
		var i = 0;
		while (startOfSection + i < GetLineCount())
		{
			var line = GetLine(startOfSection + i).TrimStart(' ');
			if (line.StartsWith(';') || string.IsNullOrEmpty(line))
			{
				i += 1;
				continue;
			}

			if (line.StartsWith('['))
			{
				break;
			}

			var parsedLine = NumberRegex.Matches(line);
			var ballId = ParseInt(parsedLine[0].Value);
			if (!headBalls.Contains(ballId))
			{
				i += 1;
				continue;
			}

			headBalls.Remove(ballId);
			var sb = new StringBuilder();
			for (var n = 0; n < parsedLine.Count; n++)
			{
				var item = parsedLine[n].Value;
				if (n == 1) sb.Append(ParseInt(item) + x).Append(' ');
				else if (n == 2) sb.Append(ParseInt(item) + y).Append(' ');
				else if (n == 3) sb.Append(ParseInt(item) + z).Append(' ');
				else sb.Append(item).Append(' ');
			}

			SetLine(startOfSection + i, sb.ToString());
			i += 1;
		}

		foreach (var b in headBalls)
		{
			SetCaretLine(startOfSection + i);
			SetCaretColumn(0);
			InsertTextAtCursor($"{b} {x} {y} {z}\n");
		}

		SaveFile();
	}

	public void OnNodeBallTranslationChanged(int ballNo, Vector3 newPosition)
	{
		if (ballNo < 67)
		{
			var done = false;
			var sectionFind = Search("[Move]", 0, 0, 0);
			var startOfSection = sectionFind.Y + 1;
			var i = 0;
			while (startOfSection + i < GetLineCount())
			{
				var line = GetLine(startOfSection + i).TrimStart(' ');
				if (line.StartsWith(';') || string.IsNullOrEmpty(line))
				{
					i += 1;
					continue;
				}

				if (line.StartsWith('['))
				{
					if (!done)
					{
						Logger.LogWarning($"Did not find move for head rotation for ball {ballNo}", nameof(LnzTextEdit));
						SetCaretLine(startOfSection + i);
						SetCaretColumn(0);
						InsertTextAtCursor($"{ballNo} {(int)newPosition.X} {(int)(-newPosition.Y)} {(int)newPosition.Z}\n");
					}

					break;
				}

				var parsedLine = NumberRegex.Matches(line);
				if (ParseInt(parsedLine[0].Value) != ballNo)
				{
					i += 1;
					continue;
				}

				done = true;
				var sb = new StringBuilder();
				for (var n = 0; n < parsedLine.Count; n++)
				{
					var item = parsedLine[n].Value;
					if (n == 1) sb.Append((int)newPosition.X).Append(' ');
					else if (n == 2) sb.Append((int)(-newPosition.Y)).Append(' ');
					else if (n == 3) sb.Append((int)newPosition.Z).Append(' ');
					else sb.Append(item).Append(' ');
				}

				SetLine(startOfSection + i, sb.ToString());
				i += 1;
			}
		}
		else
		{
			var lineNo = FindLineInAddballSection(ballNo - 67);
			var line = GetLine(lineNo);
			var parsedLine = NumberRegex.Matches(line);
			var sb = new StringBuilder();
			for (var n = 0; n < parsedLine.Count; n++)
			{
				var item = parsedLine[n].Value;
				if (n == 1) sb.Append((int)newPosition.X).Append(' ');
				else if (n == 2) sb.Append((int)(-newPosition.Y)).Append(' ');
				else if (n == 3) sb.Append((int)newPosition.Z).Append(' ');
				else sb.Append(item).Append(' ');
			}

			SetLine(lineNo, sb.ToString());
		}
	}

	public void OnNodeBallTranslationsDone()
	{
		SaveFile();
	}

	private static int ParseInt(string value)
	{
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal))
		{
			return intVal;
		}

		if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal))
		{
			return (int)floatVal;
		}

		return 0;
	}
}
