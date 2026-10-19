using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using PetzRendering.Core;
using PetzRendering.Models;
using PetzRendering.Services;

namespace PetzRendering.Parsers;

public readonly record struct MoveEntry(Vector3 Position, int RelativeTo);
public readonly record struct ProjectBallEntry(int Ball, int Base, int Amount);
public readonly record struct TextureEntry(string Filename, int TransparentColor);

public sealed partial class LnzParser
{
	private static readonly Regex NumberRegex = new(@"[-.\d]+", RegexOptions.Compiled);
	private static readonly Regex TokenRegex = new(@"[\S]+", RegexOptions.Compiled);

	public int Species { get; private set; }
	public Vector2 Scales { get; private set; } = new(255, 255);
	public Vector2 LegExtensions { get; private set; } = Vector2.Zero;
	public int BodyExtension { get; private set; }
	public int FaceExtension { get; private set; }
	public int EarExtension { get; private set; }
	public Vector2 HeadEnlargement { get; private set; } = new(100, 0);
	public Vector2 FootEnlargement { get; private set; } = new(100, 0);
	public Dictionary<int, List<MoveEntry>> Moves { get; } = new();
	public Dictionary<int, BallData> Balls { get; } = new();
	public List<LineData> Lines { get; } = new();
	public Dictionary<int, AddBallData> Addballs { get; } = new();
	public Dictionary<int, List<PaintballData>> Paintballs { get; } = new();
	public Dictionary<int, bool> Omissions { get; private set; } = new();
	public List<ProjectBallEntry> ProjectBall { get; } = new();
	public List<TextureEntry> TextureList { get; } = new();
	public string FilePath { get; }

	public LnzParser(string filePath)
	{
		FilePath = filePath;
		using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			ServiceBootstrapper.GetService<ILogService>().LogWarning($"File {filePath} not found", nameof(LnzParser));
			return;
		}

		GetTextureList(file);
		GetSpecies(file);
		GetDefaultScales(file);
		GetLegExtensions(file);
		GetBodyExtension(file);
		GetFaceExtension(file);
		GetEarExtension(file);
		GetHeadEnlargement(file);
		GetFeetEnlargement(file);
		GetOmissions(file);
		GetLines(file);
		GetBalls(file);

		file.Seek(0);

		GetAddballs(file);
		GetProjectBalls(file);
		GetPaintballz(file);
		GetMoveData(file);
	}

	private static bool GetNextSection(FileAccess file, string sectionName)
	{
		file.Seek(0);
		var header = "[" + sectionName + "]";
		var thisLine = "";
		while (!thisLine.StartsWith(header, StringComparison.Ordinal) && !file.EofReached())
		{
			thisLine = file.GetLine();
		}

		return !file.EofReached();
	}

	private static List<Dictionary<string, int>> GetParsedLines(FileAccess file, string[] keys)
	{
		var returnArray = new List<Dictionary<string, int>>();
		while (true)
		{
			var line = file.GetLine().TrimStart();
			if (string.IsNullOrEmpty(line) ||
				line.StartsWith('[') ||
				file.EofReached() ||
				line.StartsWith("#2", StringComparison.Ordinal))
			{
				break;
			}

			if (line.StartsWith(';') || line.StartsWith('#'))
			{
				continue;
			}

			var parsed = NumberRegex.Matches(line);
			if (parsed.Count < keys.Length)
			{
				continue;
			}

			var dict = new Dictionary<string, int>(keys.Length);
			for (var i = 0; i < keys.Length; i++)
			{
				dict[keys[i]] = ParseInt(parsed[i].Value);
			}

			returnArray.Add(dict);
		}

		return returnArray;
	}

	private static List<Dictionary<string, string>> GetParsedLineStrings(FileAccess file, string[] keys)
	{
		var returnArray = new List<Dictionary<string, string>>();
		while (true)
		{
			var line = file.GetLine().TrimStart();
			if (string.IsNullOrEmpty(line) ||
				line.StartsWith('[') ||
				file.EofReached() ||
				line.StartsWith("#2", StringComparison.Ordinal))
			{
				break;
			}

			if (line.StartsWith(';') || line.StartsWith('#'))
			{
				continue;
			}

			var parsed = TokenRegex.Matches(line);
			if (parsed.Count < keys.Length)
			{
				continue;
			}

			var dict = new Dictionary<string, string>(keys.Length);
			for (var i = 0; i < keys.Length; i++)
			{
				dict[keys[i]] = parsed[i].Value;
			}

			returnArray.Add(dict);
		}

		return returnArray;
	}

	private void GetPaintballz(FileAccess file)
	{
		file.Seek(0);
		var thisLine = "";
		while (thisLine != "[Paint Ballz]" && !file.EofReached())
		{
			thisLine = file.GetLine();
		}

		while (true)
		{
			thisLine = file.GetLine();
			if (string.IsNullOrEmpty(thisLine) ||
				thisLine.StartsWith('[') ||
				file.EofReached() ||
				thisLine.StartsWith("#2", StringComparison.Ordinal))
			{
				break;
			}

			if (thisLine.StartsWith(';') || thisLine.StartsWith('#'))
			{
				continue;
			}

			var splitLine = NumberRegex.Matches(thisLine);
			if (splitLine.Count <= 10)
			{
				continue;
			}

			var @base = ParseInt(splitLine[0].Value);
			var diameterPercent = ParseInt(splitLine[1].Value);
			var x = ParseFloat(splitLine[2].Value);
			var y = ParseFloat(splitLine[3].Value);
			var z = ParseFloat(splitLine[4].Value);
			var color = ParseInt(splitLine[5].Value);
			var outlineColor = ParseInt(splitLine[6].Value);
			if (outlineColor == -1)
			{
				outlineColor = 0;
			}

			var fuzz = ParseInt(splitLine[7].Value);
			var outline = ParseInt(splitLine[8].Value);
			var texture = ParseInt(splitLine[10].Value);
			var anchored = splitLine.Count > 11 ? ParseInt(splitLine[11].Value) : 0;

			var paintball = new PaintballData(
				@base,
				diameterPercent,
				new Vector3(x, y, z),
				color,
				outlineColor,
				outline,
				fuzz,
				0.0f,
				texture,
				anchored);

			if (!Paintballs.TryGetValue(@base, out var pbArray))
			{
				pbArray = new List<PaintballData>();
				Paintballs[@base] = pbArray;
			}

			pbArray.Add(paintball);
		}
	}

	private void GetMoveData(FileAccess file)
	{
		file.Seek(0);
		var thisLine = "";
		while (!thisLine.StartsWith("[Move]", StringComparison.Ordinal) && !file.EofReached())
		{
			thisLine = file.GetLine();
		}

		while (true)
		{
			thisLine = file.GetLine();
			if (string.IsNullOrEmpty(thisLine) ||
				thisLine.StartsWith('[') ||
				file.EofReached() ||
				thisLine.StartsWith("#2", StringComparison.Ordinal))
			{
				break;
			}

			if (thisLine.StartsWith(';') || thisLine.StartsWith('#'))
			{
				continue;
			}

			var splitLine = NumberRegex.Matches(thisLine);
			if (splitLine.Count < 4)
			{
				continue;
			}

			var @base = ParseInt(splitLine[0].Value);
			var x = ParseInt(splitLine[1].Value);
			var y = ParseInt(splitLine[2].Value);
			var z = ParseInt(splitLine[3].Value);
			var relativeBall = splitLine.Count > 4 ? ParseInt(splitLine[4].Value) : @base;
			var pos = new Vector3(x, y, z);

			if (!Moves.TryGetValue(@base, out var ar))
			{
				ar = new List<MoveEntry>();
				Moves[@base] = ar;
			}

			ar.Add(new MoveEntry(pos, relativeBall));
			if (Balls.TryGetValue(@base, out var ball))
			{
				ball.Position = pos;
			}
		}
	}

	private void GetDefaultScales(FileAccess file)
	{
		GetNextSection(file, "Default Scales");
		var parsedLines = GetParsedLines(file, new[] { "scale" });
		if (parsedLines.Count >= 2)
		{
			Scales = new Vector2(parsedLines[0]["scale"], parsedLines[1]["scale"]);
		}
	}

	private void GetLegExtensions(FileAccess file)
	{
		GetNextSection(file, "Leg Extension");
		var parsedLines = GetParsedLines(file, new[] { "extension" });
		if (parsedLines.Count >= 2)
		{
			LegExtensions = new Vector2(parsedLines[0]["extension"], parsedLines[1]["extension"]);
		}
	}

	private void GetBodyExtension(FileAccess file)
	{
		GetNextSection(file, "Body Extension");
		var parsedLines = GetParsedLines(file, new[] { "extension" });
		if (parsedLines.Count > 0)
		{
			BodyExtension = parsedLines[0]["extension"];
		}
	}

	private void GetFaceExtension(FileAccess file)
	{
		GetNextSection(file, "Face Extension");
		var parsedLines = GetParsedLines(file, new[] { "extension" });
		if (parsedLines.Count > 0)
		{
			FaceExtension = parsedLines[0]["extension"];
		}
	}

	private void GetEarExtension(FileAccess file)
	{
		GetNextSection(file, "Ear Extension");
		var parsedLines = GetParsedLines(file, new[] { "extension" });
		if (parsedLines.Count > 0)
		{
			EarExtension = parsedLines[0]["extension"];
		}
	}

	private void GetHeadEnlargement(FileAccess file)
	{
		GetNextSection(file, "Head Enlargement");
		var parsedLines = GetParsedLines(file, new[] { "scale" });
		if (parsedLines.Count >= 2)
		{
			HeadEnlargement = new Vector2(parsedLines[0]["scale"], parsedLines[1]["scale"]);
		}
	}

	private void GetFeetEnlargement(FileAccess file)
	{
		GetNextSection(file, "Feet Enlargement");
		var parsedLines = GetParsedLines(file, new[] { "scale" });
		if (parsedLines.Count >= 2)
		{
			FootEnlargement = new Vector2(parsedLines[0]["scale"], parsedLines[1]["scale"]);
		}
	}

	private void GetOmissions(FileAccess file)
	{
		GetNextSection(file, "Omissions");
		var parsedLines = GetParsedLines(file, new[] { "ball_no" });
		Omissions = new Dictionary<int, bool>();
		foreach (var line in parsedLines)
		{
			Omissions[line["ball_no"]] = true;
		}
	}

	private void GetLines(FileAccess file)
	{
		GetNextSection(file, "Linez");
		var parsedLines = GetParsedLines(file, new[]
		{
			"start", "end", "fuzz", "color", "l_color", "r_color", "start_thickness", "end_thickness"
		});
		foreach (var line in parsedLines)
		{
			var lineData = new LineData(
				line["start"],
				line["end"],
				line["start_thickness"],
				line["end_thickness"],
				line["fuzz"],
				line["color"],
				line["l_color"],
				line["r_color"]);
			Lines.Add(lineData);
		}
	}

	private void GetBalls(FileAccess file)
	{
		GetNextSection(file, "Ballz Info");
		var parsedLines = GetParsedLines(file, new[]
		{
			"color", "outline_color", "speckle", "fuzz", "outline", "size", "group", "texture"
		});
		var i = 0;
		foreach (var line in parsedLines)
		{
			var bd = new BallData(
				line["size"],
				Vector3.Zero,
				i,
				Vector3.Zero,
				line["color"],
				line["outline_color"],
				line["outline"],
				line["fuzz"],
				0.0f,
				line["group"],
				line["texture"]);
			Balls[i] = bd;
			i++;
		}
	}

	private void GetAddballs(FileAccess file)
	{
		GetNextSection(file, "Add Ball");
		var parsedLines = GetParsedLines(file, new[]
		{
			"base", "x", "y", "z", "color", "outline_color", "speckle", "fuzz", "group", "outline", "size", "body_area", "add_group", "texture"
		});
		var maxBallNum = Balls.Count > 0 ? Balls.Keys.Max() + 1 : 67;
		foreach (var line in parsedLines)
		{
			var pos = new Vector3(line["x"], line["y"], line["z"]);
			var ball = new AddBallData(
				line["base"],
				maxBallNum,
				line["size"],
				pos,
				line["color"],
				line["outline_color"],
				line["outline"],
				line["fuzz"],
				0.0f,
				line["group"],
				line["body_area"],
				line["texture"]);
			Addballs[maxBallNum] = ball;
			maxBallNum++;
		}
	}

	private void GetProjectBalls(FileAccess file)
	{
		GetNextSection(file, "Project Ball");
		var parsedLines = GetParsedLines(file, new[] { "base", "projected", "amount" });
		foreach (var line in parsedLines)
		{
			ProjectBall.Add(new ProjectBallEntry(line["projected"], line["base"], line["amount"]));
		}
	}

	private void GetSpecies(FileAccess file)
	{
		GetNextSection(file, "Species");
		var parsedLines = GetParsedLines(file, new[] { "species" });
		Species = parsedLines.Count == 0 ? 2 : parsedLines[0]["species"];
	}

	private void GetTextureList(FileAccess file)
	{
		GetNextSection(file, "Texture List");
		var parsedLines = GetParsedLineStrings(file, new[] { "filepath", "transparent_color" });
		foreach (var line in parsedLines)
		{
			var filename = line["filepath"].GetFile();
			var transparentColor = ParseInt(line["transparent_color"]);
			TextureList.Add(new TextureEntry(filename, transparentColor));
		}
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

	private static float ParseFloat(string value)
	{
		return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal)
			? floatVal
			: 0.0f;
	}
}
