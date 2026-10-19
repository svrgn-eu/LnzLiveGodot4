using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using PetzRendering.Core;
using PetzRendering.Models;
using PetzRendering.Parsers;
using PetzRendering.Services;

namespace PetzRendering.Rendering;

public partial class DogGenerator : Node
{
	private sealed class CollatedPetData
	{
		public Dictionary<int, BallData> Balls { get; init; } = new();
		public Dictionary<int, AddBallData> Addballs { get; init; } = new();
		public Dictionary<int, List<PaintballData>> Paintballs { get; init; } = new();
		public Dictionary<int, bool> Omissions { get; set; } = new();
	}

	private static readonly PackedScene BallScene = GD.Load<PackedScene>("res://Scenes/Rendering/Ball.tscn");
	private static readonly PackedScene PaintballScene = GD.Load<PackedScene>("res://Scenes/Rendering/Paintball.tscn");
	private static readonly PackedScene LineScene = GD.Load<PackedScene>("res://Scenes/Rendering/Line.tscn");

	[Export]
	public float PixelWorldSize { get; set; } = 0.002f;

	[Export]
	public bool DrawBalls { get; set; } = true;

	[Export]
	public bool DrawAddballs { get; set; } = true;

	[Export]
	public bool DrawLines { get; set; } = true;

	[Export]
	public bool DrawPaintballs { get; set; } = true;

	private List<BallData> _balls = new();
	private readonly Dictionary<int, IPetBallNode> _ballMap = new();
	private readonly Dictionary<int, List<Paintball>> _paintballMap = new();
	private readonly Dictionary<int, Line> _linesMap = new();

	private BhdParser? _bhd;
	private LnzParser? _lnz;
	private int _currentAnimation;
	private int _currentFrame;
	private BdtParser? _currentBdt;
	private Vector3 _bellyPosition = Vector3.Zero;
	private ResourcePreloader? _preloader;

	private IKeyBallsService KeyBalls => ServiceBootstrapper.GetService<IKeyBallsService>();
	private ILogService Logger => ServiceBootstrapper.GetService<ILogService>();

	[Signal]
	public delegate void AnimationLoadedEventHandler(int numOfFrames);

	[Signal]
	public delegate void BhdLoadedEventHandler(int numOfAnimations);

	[Signal]
	public delegate void BallMouseEnterEventHandler(string ballLabel);

	[Signal]
	public delegate void BallMouseExitEventHandler(int ballNo);

	[Signal]
	public delegate void BallSelectedEventHandler(int section, int ballNo, bool isAddball, int maxAddballNo);

	[Signal]
	public delegate void AddballDeletedEventHandler(int ballNo);

	[Signal]
	public delegate void BallTranslationChangedEventHandler(int ballNo, Vector3 newPosition);

	[Signal]
	public delegate void BallTranslationsDoneEventHandler();

	public override void _Ready()
	{
		_preloader = GetTree().Root.GetNodeOrNull<ResourcePreloader>("Root/ResourcePreloader");
	}

	public void SetAnimation(int animIndex)
	{
		if (_bhd == null || _lnz == null)
		{
			return;
		}

		_currentAnimation = animIndex;
		var species = _lnz.Species == (int)Species.Dog ? "DOG" : "CAT";
		var animFrames = _bhd.GetFrameOffsetsFor(animIndex);
		_currentBdt = new BdtParser($"{species}{animIndex}.bdt", animFrames, _bhd.NumBalls);
		SetFrame(0);
		EmitSignal(SignalName.AnimationLoaded, animFrames.Count);
	}

	public void OnFrameSliderValueChanged(double frame)
	{
		SetFrame((int)Math.Round(frame));
	}

	public void SetFrame(int frame)
	{
		if (_bhd == null || _currentBdt == null || _lnz == null)
		{
			return;
		}

		if (frame < 0 || frame >= _currentBdt.Frames.Count)
		{
			return;
		}

		_currentFrame = frame;
		_balls = new List<BallData>(_bhd.NumBalls);
		for (var n = 0; n < _bhd.NumBalls; n++)
		{
			var x = _currentBdt.Frames[frame][n];
			_balls.Add(new BallData(_bhd.BallSizes[n], x.Position, n, x.Rotation));
		}

		InitVisualBalls(_lnz, newCreate: false);
	}

	private void InitBallData(int species)
	{
		_bhd = species == (int)Species.Dog
			? new BhdParser("res://resources/animations/DOG.bhd")
			: new BhdParser("res://resources/animations/CAT.bhd");

		if (_currentAnimation >= _bhd.AnimationRanges.Count)
		{
			_currentAnimation = 0;
			_currentFrame = 0;
		}

		EmitSignal(SignalName.BhdLoaded, _bhd.AnimationRanges.Count);
		var firstAnimFrames = _bhd.GetFrameOffsetsFor(_currentAnimation);
		if (_currentFrame >= firstAnimFrames.Count)
		{
			_currentFrame = 0;
		}

		var bdtPrefix = species == (int)Species.Dog ? "DOG" : "CAT";
		var bdt = new BdtParser($"{bdtPrefix}{_currentAnimation}.bdt", firstAnimFrames, _bhd.NumBalls);
		EmitSignal(SignalName.AnimationLoaded, firstAnimFrames.Count);
		_currentBdt = bdt;

		_balls = new List<BallData>(_bhd.NumBalls);
		if (bdt.Frames.Count > _currentFrame)
		{
			for (var n = 0; n < _bhd.NumBalls; n++)
			{
				var frameBall = bdt.Frames[_currentFrame][n];
				_balls.Add(new BallData(_bhd.BallSizes[n], frameBall.Position, n, frameBall.Rotation));
			}
		}

		KeyBalls.MaxBaseBallNum = _bhd.NumBalls;
	}

	public void GeneratePet(string filePath)
	{
		var lnzInfo = new LnzParser(filePath);
		_lnz = lnzInfo;
		KeyBalls.Species = (Species)lnzInfo.Species;
		InitBallData(lnzInfo.Species);
		InitVisualBalls(lnzInfo, newCreate: true);
	}

	private void InitVisualBalls(LnzParser lnzInfo, bool newCreate = false)
	{
		var baseBalls = CollateBaseBallData();

		// Duplicate addball and paintball data to prevent movements/scales being applied multiple times across frames
		var addballs = new Dictionary<int, AddBallData>(lnzInfo.Addballs.Count);
		foreach (var (k, a) in lnzInfo.Addballs)
		{
			addballs[k] = a.Clone();
		}

		var paintballs = new Dictionary<int, List<PaintballData>>(lnzInfo.Paintballs.Count);
		foreach (var (k, ar) in lnzInfo.Paintballs)
		{
			var clonedList = new List<PaintballData>(ar.Count);
			foreach (var a in ar)
			{
				clonedList.Add(a.Clone());
			}

			paintballs[k] = clonedList;
		}

		var collatedData = new CollatedPetData
		{
			Balls = baseBalls,
			Addballs = addballs,
			Paintballs = paintballs,
			Omissions = lnzInfo.Omissions
		};

		MungeBalls(collatedData, lnzInfo);
		ApplyExtensions(collatedData, lnzInfo);
		ApplySizes(collatedData, lnzInfo);
		GenerateBalls(collatedData, lnzInfo.Species, lnzInfo.TextureList, newCreate);
		ApplyProjections();
		GenerateLines(lnzInfo.Lines, newCreate);
	}

	private Dictionary<int, BallData> CollateBaseBallData()
	{
		var ballDataMap = new Dictionary<int, BallData>(_balls.Count);
		foreach (var ball in _balls)
		{
			ballDataMap[ball.BallNo] = ball;
		}

		return ballDataMap;
	}

	private void ApplyExtensions(CollatedPetData allBallData, LnzParser lnz)
	{
		var baseBallDict = allBallData.Balls;

		IReadOnlyList<IReadOnlyList<int>> legs;
		IReadOnlyList<int> bodyExt;
		IReadOnlyList<int> faceExt;
		List<int> headExt;
		IReadOnlyList<IReadOnlyList<int>> footExt;
		IReadOnlyDictionary<int, IReadOnlyList<int>> earExt;

		if (lnz.Species == (int)Species.Dog)
		{
			legs = KeyBalls.LegsDog;
			bodyExt = KeyBalls.BodyExtDog;
			faceExt = KeyBalls.FaceExtDog;
			headExt = KeyBalls.HeadExtDog.ToList();
			footExt = KeyBalls.FootExtDog;
			earExt = KeyBalls.EarExtDog;
		}
		else
		{
			legs = KeyBalls.LegsCat;
			bodyExt = KeyBalls.BodyExtCat;
			faceExt = KeyBalls.FaceExtCat;
			headExt = KeyBalls.HeadExtCat.ToList();
			footExt = KeyBalls.FootExtCat;
			earExt = KeyBalls.EarExtCat;

			foreach (var b in KeyBalls.EyesCat.Keys)
			{
				headExt.Remove(b);
			}
		}

		// Legs
		foreach (var ballNo in legs[0])
		{
			var ball = baseBallDict[ballNo];
			var pos = ball.Position;
			if (ballNo == legs[0][0] || ballNo == legs[0][1])
			{
				pos.Y += Mathf.Abs(pos.Y * (lnz.LegExtensions.X / 100.0f));
			}
			else
			{
				pos.Y += lnz.LegExtensions.X;
			}

			ball.Position = pos;
		}

		foreach (var ballNo in legs[1])
		{
			var ball = baseBallDict[ballNo];
			var pos = ball.Position;
			if (ballNo == legs[1][0] || ballNo == legs[1][1])
			{
				pos.Y += Mathf.Abs(pos.Y * Mathf.Abs(lnz.LegExtensions.Y / 100.0f));
			}
			else
			{
				pos.Y += lnz.LegExtensions.Y;
			}

			ball.Position = pos;
		}

		// Body
		var specialBall = bodyExt[0];
		foreach (var ballNo in bodyExt)
		{
			if (ballNo == specialBall)
			{
				continue;
			}

			var ball = baseBallDict[ballNo];
			var pos = ball.Position;
			pos.Z += lnz.BodyExtension * 2;
			ball.Position = pos;
		}

		{
			var sb = baseBallDict[specialBall];
			var pos = sb.Position;
			pos.Z += lnz.BodyExtension;
			sb.Position = pos;
		}

		// Face
		var headBallKey = headExt[0];
		foreach (var ballNo in faceExt)
		{
			var ball = baseBallDict[ballNo];
			var pos = ball.Position;
			pos.Z -= lnz.FaceExtension;
			ball.Position = pos;
		}

		// Head enlargement
		var headPos = baseBallDict[headBallKey].Position;
		foreach (var ballNo in headExt)
		{
			var ball = baseBallDict[ballNo];
			if (ballNo != headBallKey)
			{
				var modV = ball.Position - headPos;
				modV *= lnz.HeadEnlargement.X / 100.0f;
				modV += headPos;
				ball.Position = new Vector3(Mathf.Floor(modV.X), Mathf.Floor(modV.Y), Mathf.Floor(modV.Z));
			}

			ball.Size = (int)Mathf.Floor(ball.Size * (lnz.HeadEnlargement.X / 100.0f));
			ball.Size += (int)lnz.HeadEnlargement.Y;
		}

		// Feet
		foreach (var footGroup in footExt)
		{
			var footPos = baseBallDict[footGroup[0]].Position;
			foreach (var ballNo in footGroup)
			{
				var ball = baseBallDict[ballNo];
				if (ballNo != footGroup[0])
				{
					var modV = ball.Position - footPos;
					modV *= lnz.FootEnlargement.X / 100.0f;
					modV += footPos;
					ball.Position = new Vector3(Mathf.Floor(ball.Position.X), Mathf.Floor(ball.Position.Y), Mathf.Floor(ball.Position.Z));
				}

				ball.Size = (int)Mathf.Floor(ball.Size * (lnz.FootEnlargement.X / 100.0f));
				ball.Size += (int)lnz.FootEnlargement.Y;
			}
		}

		// Ears
		foreach (var (baseBallNo, earTips) in earExt)
		{
			var baseBall = baseBallDict[baseBallNo];
			foreach (var k in earTips)
			{
				var earBall = baseBallDict[k];
				var vectorFromBase = earBall.Position - baseBall.Position;
				vectorFromBase *= lnz.EarExtension / 100.0f;
				earBall.Position = baseBall.Position + vectorFromBase;
			}
		}
	}

	private static void MungeBalls(CollatedPetData allBallData, LnzParser lnz)
	{
		var baseBallDict = allBallData.Balls;
		var lnzBalls = lnz.Balls;
		foreach (var k in baseBallDict.Keys.ToList())
		{
			if (!lnzBalls.TryGetValue(k, out var v) || !baseBallDict.TryGetValue(k, out var b))
			{
				continue;
			}

			b.Size += v.Size;
			b.OutlineColorIndex = v.OutlineColorIndex;
			b.Outline = v.Outline;
			b.Fuzz = v.Fuzz;

			if (lnz.Moves.TryGetValue(k, out var moves))
			{
				foreach (var m in moves)
				{
					var moveBase = b;
					var rot = moveBase.Rotation;
					if (m.RelativeTo != 0 && baseBallDict.TryGetValue(m.RelativeTo, out var relBall))
					{
						rot = relBall.Rotation;
					}

					b.Position = moveBase.Position + ApplyMovementWithRotation(m.Position, rot);
				}
			}

			b.TextureId = v.TextureId;
			b.ColorIndex = v.ColorIndex;
			baseBallDict[k] = b;
		}
	}

	private static Vector3 ApplyMovementWithRotation(Vector3 vec, Vector3 rotEuler)
	{
		var q = Quaternion.FromEuler(new Vector3(
			Mathf.DegToRad(rotEuler.X),
			Mathf.DegToRad(rotEuler.Y),
			Mathf.DegToRad(rotEuler.Z)));
		return q * vec;
	}

	private void ApplyProjections()
	{
		if (_lnz == null)
		{
			return;
		}

		foreach (var projectBallData in _lnz.ProjectBall)
		{
			if (!_ballMap.TryGetValue(projectBallData.Ball, out var visualBallNode) ||
				!_ballMap.TryGetValue(projectBallData.Base, out var staticBallNode))
			{
				continue;
			}

			var visualBall = visualBallNode.AsNode3D;
			var staticBall = staticBallNode.AsNode3D;
			var vec = visualBall.GlobalTransform.Origin - staticBall.GlobalTransform.Origin;
			var basePos = staticBall.GlobalTransform.Origin;
			var globalTransform = visualBall.GlobalTransform;
			globalTransform.Origin = basePos + (vec * (projectBallData.Amount / 100.0f));
			visualBall.GlobalTransform = globalTransform;
		}
	}

	private static void ApplySizes(CollatedPetData allBallData, LnzParser lnz)
	{
		foreach (var ball in allBallData.Balls.Values)
		{
			var rawSize = (float)(ball.Size - 2);
			rawSize = Mathf.Round(rawSize * (lnz.Scales.Y / 255.0f));
			rawSize -= 1.0f - (rawSize % 2.0f);
			ball.Size = (int)rawSize;
			ball.Position *= lnz.Scales.X / 255.0f;
		}

		foreach (var ball in allBallData.Addballs.Values)
		{
			var rawSize = (float)(ball.Size - 2);
			rawSize = Mathf.Round(rawSize * (lnz.Scales.Y / 255.0f));
			rawSize -= 1.0f - (rawSize % 2.0f);
			ball.Size = (int)rawSize;
			ball.Position *= lnz.Scales.X / 255.0f;
		}
	}

	private Node GetRoot()
	{
		if (Engine.IsEditorHint())
		{
			return GetTree().EditedSceneRoot.GetNode("PetRoot");
		}

		return GetTree().Root.GetNode("Root/PetRoot");
	}

	private void GenerateBalls(
		CollatedPetData allBallData,
		int species,
		IReadOnlyList<TextureEntry> textureList,
		bool newCreate)
	{
		var ballData = allBallData.Balls;
		var addballData = allBallData.Addballs;
		var paintballData = allBallData.Paintballs;
		var omissions = allBallData.Omissions;

		var root = GetRoot();
		var parent = root.GetNode<Node3D>("petholder/balls");
		var pbParent = root.GetNode<Node3D>("petholder/paintballs");
		var abParent = root.GetNode<Node3D>("petholder/addballs");

		if (newCreate)
		{
			foreach (var c in parent.GetChildren())
			{
				parent.RemoveChild(c);
				c.QueueFree();
			}

			foreach (var c in pbParent.GetChildren())
			{
				pbParent.RemoveChild(c);
				c.QueueFree();
			}

			foreach (var c in abParent.GetChildren())
			{
				abParent.RemoveChild(c);
				c.QueueFree();
			}

			_ballMap.Clear();
			_paintballMap.Clear();
		}

		var bellyBallKey = species == (int)Species.Dog ? KeyBalls.BellyDog : KeyBalls.BellyCat;
		var bellyPos = ballData[bellyBallKey].Position;
		bellyPos.Y *= -1.0f;
		bellyPos *= PixelWorldSize;
		_bellyPosition = bellyPos;

		var eyes = species == (int)Species.Dog ? KeyBalls.EyesDog : KeyBalls.EyesCat;

		foreach (var (key, ball) in ballData)
		{
			IPetBallNode visualBall;
			if (eyes.TryGetValue(key, out var eyeballKey)) // treat irises like paintballs
			{
				Paintball irisBall;
				if (newCreate)
				{
					irisBall = PaintballScene.Instantiate<Paintball>();
					irisBall.AddToGroup("balls");
					irisBall.ZAdd = 10.0f;
					irisBall.OverrideBallNo = ball.BallNo;
					irisBall.ColorIndex = ball.ColorIndex;
					irisBall.BallMouseEnter += SignalBallMouseEnter;
					irisBall.BallMouseExit += SignalBallMouseExit;
					irisBall.BallSelected += SignalBallSelected;
				}
				else
				{
					irisBall = (Paintball)_ballMap[key];
				}

				var baseBall = ballData[eyeballKey];
				irisBall.BaseBallSize = baseBall.Size;
				var bbp = baseBall.Position;
				bbp.Y *= -1.0f;
				bbp *= PixelWorldSize;
				irisBall.BaseBallPosition = bbp;

				var rotatedPos = ball.Position;
				rotatedPos.Y *= -1.0f;
				var t = irisBall.Transform;
				t.Origin = rotatedPos * PixelWorldSize;
				irisBall.Transform = t;

				visualBall = irisBall;
			}
			else
			{
				Ball stdBall;
				if (newCreate)
				{
					stdBall = BallScene.Instantiate<Ball>();
					stdBall.AddToGroup("balls");
					stdBall.BallMouseEnter += SignalBallMouseEnter;
					stdBall.BallMouseExit += SignalBallMouseExit;
					stdBall.BallSelected += SignalBallSelected;
				}
				else
				{
					stdBall = (Ball)_ballMap[key];
				}

				stdBall.PetCenter = _bellyPosition;
				var rotatedPos = ball.Position;
				rotatedPos.Y *= -1.0f;
				var t = stdBall.Transform;
				t.Origin = rotatedPos * PixelWorldSize;
				stdBall.Transform = t;
				stdBall.BallNo = ball.BallNo;

				if (newCreate)
				{
					if (ball.TextureId > -1 && ball.TextureId < textureList.Count)
					{
						var texInfo = textureList[ball.TextureId];
						stdBall.TransparentColor = texInfo.TransparentColor;
						stdBall.Texture = PetzTextureLoader.GetTexture(texInfo.Filename, _preloader);
					}

					stdBall.ColorIndex = ball.ColorIndex;
					stdBall.OutlineColorIndex = ball.OutlineColorIndex;
				}

				visualBall = stdBall;
			}

			if (newCreate)
			{
				visualBall.BallSize = GetRealBallSize(ball.Size);
				visualBall.Outline = ball.Outline;
				visualBall.OutlineColorIndex = ball.OutlineColorIndex;
				visualBall.FuzzAmount = Math.Clamp(ball.Fuzz / 2, 0, 5);
			}

			visualBall.AsNode3D.RotationDegrees = ball.Rotation;
			if (newCreate)
			{
				parent.AddChild(visualBall.AsNode3D);
				visualBall.AsNode3D.Owner = root;
			}

			_ballMap[ball.BallNo] = visualBall;
			if (!DrawBalls)
			{
				visualBall.VisibleOverride = false;
			}

			if (omissions.TryGetValue(key, out var isOmitted) && isOmitted)
			{
				visualBall.Omitted = true;
				visualBall.VisibleOverride = false;
			}
		}

		foreach (var (key, ball) in addballData)
		{
			if (!_ballMap.TryGetValue(ball.Base, out var parentBall))
			{
				continue;
			}

			Ball visualBall;
			if (newCreate)
			{
				visualBall = BallScene.Instantiate<Ball>();
				parentBall.AsNode3D.AddChild(visualBall);
				visualBall.Owner = root;
				visualBall.AddToGroup("addballs");
				visualBall.ZAdd = ball.Size / 10.0f;
				visualBall.BallSize = ball.Size;
				visualBall.BallMouseEnter += SignalBallMouseEnter;
				visualBall.BallSelected += SignalBallSelected;
				visualBall.BallDeleted += SignalBallDeleted;
			}
			else
			{
				visualBall = (Ball)_ballMap[key];
			}

			var totalPos = ball.Position;
			totalPos.Y *= -1.0f;
			var t = visualBall.Transform;
			t.Origin = totalPos * PixelWorldSize;
			visualBall.Transform = t;

			if (newCreate)
			{
				visualBall.Outline = ball.Outline;
				visualBall.FuzzAmount = Math.Clamp(ball.Fuzz / 2, 0, 5);
				visualBall.BallNo = ball.BallNo;
				visualBall.BaseBallNo = ball.Base;
				visualBall.OutlineColorIndex = ball.OutlineColorIndex;
			}

			visualBall.Scale = Vector3.One;
			if (newCreate)
			{
				if (ball.TextureId > -1 && ball.TextureId < textureList.Count)
				{
					var texInfo = textureList[ball.TextureId];
					visualBall.TransparentColor = texInfo.TransparentColor;
					visualBall.Texture = PetzTextureLoader.GetTexture(texInfo.Filename, _preloader);
				}

				visualBall.ColorIndex = ball.ColorIndex;
			}

			_ballMap[ball.BallNo] = visualBall;
			if (!DrawAddballs)
			{
				visualBall.VisibleOverride = false;
			}

			if (omissions.TryGetValue(key, out var isOmitted) && isOmitted)
			{
				visualBall.Omitted = true;
				visualBall.VisibleOverride = false;
			}
		}

		foreach (var (key, paintballsForBaseBall) in paintballData)
		{
			if (!_ballMap.TryGetValue(key, out var parentBallNode) || parentBallNode.Omitted)
			{
				continue;
			}

			var baseBallSize = ballData.TryGetValue(key, out var bData)
				? bData.Size
				: addballData.TryGetValue(key, out var abData)
					? abData.Size
					: parentBallNode.BallSize;

			paintballsForBaseBall.Reverse();
			var count = 0;
			foreach (var paintball in paintballsForBaseBall)
			{
				var rawFinalSize = baseBallSize * (paintball.Size / 100.0f);
				rawFinalSize -= 1.0f - (rawFinalSize % 2.0f);
				var finalSize = (int)rawFinalSize;

				Paintball visualBall;
				if (newCreate)
				{
					visualBall = PaintballScene.Instantiate<Paintball>();
					parentBallNode.AsNode3D.AddChild(visualBall);
					visualBall.Owner = root;
					visualBall.AddToGroup("paintballs");
					visualBall.PaintballMouseEnter += SignalPaintballMouseEnter;
					visualBall.PaintballMouseExit += SignalPaintballMouseExit;
					if (paintball.TextureId > -1 && paintball.TextureId < textureList.Count)
					{
						var texInfo = textureList[paintball.TextureId];
						visualBall.TransparentColor = texInfo.TransparentColor;
						visualBall.Texture = PetzTextureLoader.GetTexture(texInfo.Filename, _preloader);
					}
					else
					{
						visualBall.TransparentColor = paintball.ColorIndex;
					}

					visualBall.ColorIndex = paintball.ColorIndex;
				}
				else
				{
					visualBall = _paintballMap[key][count];
				}

				visualBall.BaseBallPosition = parentBallNode.AsNode3D.GlobalTransform.Origin;
				var t = visualBall.Transform;
				t.Origin = paintball.NormalisedPosition * new Vector3(1, -1, 1) * (baseBallSize / 2.0f) * PixelWorldSize;
				visualBall.Transform = t;

				visualBall.BallSize = finalSize;
				visualBall.BaseBallSize = baseBallSize;
				visualBall.OutlineColorIndex = paintball.OutlineColorIndex;
				visualBall.Outline = paintball.Outline;
				visualBall.FuzzAmount = Math.Clamp(paintball.Fuzz / 2, 0, 5);
				visualBall.ZAdd = count * 1.0f;
				visualBall.BaseBallNo = paintball.Base;
				count += 1;

				if (newCreate)
				{
					if (!_paintballMap.TryGetValue(key, out var ar))
					{
						ar = new List<Paintball>();
						_paintballMap[key] = ar;
					}

					ar.Add(visualBall);
				}

				if (!DrawPaintballs)
				{
					visualBall.VisibleOverride = false;
				}
			}
		}
	}

	private static int GetRealBallSize(int ballSize) => ballSize;

	private void GenerateLines(IReadOnlyList<LineData> lineData, bool newCreate)
	{
		if (_lnz == null)
		{
			return;
		}

		var root = GetRoot();
		var parent = root.GetNode<Node3D>("petholder/lines");
		if (newCreate)
		{
			foreach (var c in parent.GetChildren())
			{
				parent.RemoveChild(c);
				c.QueueFree();
			}

			_linesMap.Clear();
		}

		var i = 0;
		foreach (var line in lineData)
		{
			if (!_ballMap.TryGetValue(line.Start, out var start) ||
				!_ballMap.TryGetValue(line.End, out var end))
			{
				Logger.LogInfo($"Could not make a line between {line.Start} and {line.End}", nameof(DogGenerator));
				continue;
			}

			if (_lnz.Omissions.ContainsKey(line.Start) || _lnz.Omissions.ContainsKey(line.End))
			{
				continue;
			}

			Line visualLine;
			if (newCreate)
			{
				visualLine = LineScene.Instantiate<Line>();
				visualLine.AddToGroup("lines");
			}
			else
			{
				visualLine = _linesMap[i];
			}

			var startPos = start.AsNode3D.GlobalTransform.Origin;
			var targetPos = end.AsNode3D.GlobalTransform.Origin;
			var distance = (targetPos - startPos).Length();
			var middlePoint = startPos.Lerp(targetPos, 0.5f);

			if (targetPos == middlePoint)
			{
				var gt = visualLine.GlobalTransform;
				gt.Origin = middlePoint;
				visualLine.GlobalTransform = gt;

				var rot = visualLine.RotationDegrees;
				rot.X += 90.0f;
				visualLine.RotationDegrees = rot;

				var s = visualLine.Scale;
				s.Y = distance;
				visualLine.Scale = s;
			}
			else
			{
				var dir = (targetPos - middlePoint).Normalized();
				var upVec = Mathf.Abs(dir.Dot(Vector3.Up)) > 0.99f ? Vector3.Back : Vector3.Up;
				visualLine.LookAtFromPosition(middlePoint, targetPos, upVec);

				var rot = visualLine.RotationDegrees;
				rot.X += 90.0f;
				visualLine.RotationDegrees = rot;

				var s = visualLine.Scale;
				s.Y = distance;
				visualLine.Scale = s;
			}

			if (newCreate)
			{
				visualLine.Texture = start.Texture;
				visualLine.TransparentColor = start.TransparentColor;
				visualLine.ColorIndex = line.ColorIndex == -1 ? start.ColorIndex : line.ColorIndex;
				visualLine.RColorIndex = line.RColorIndex == -1 ? start.ColorIndex : line.RColorIndex;
				visualLine.LColorIndex = line.LColorIndex == -1 ? start.ColorIndex : line.LColorIndex;
			}

			visualLine.BallWorldPos1 = startPos;
			visualLine.BallWorldPos2 = targetPos;
			visualLine.PetCenter = _bellyPosition;
			visualLine.FuzzAmount = Math.Clamp(line.Fuzz / 2, 0, 5);

			var finalLineWidth = new Vector2(start.BallSize, end.BallSize) * (new Vector2(line.SThick, line.EThick) / 100.0f);
			visualLine.LineWidths = finalLineWidth;
			_linesMap[i] = visualLine;

			if (!DrawLines)
			{
				visualLine.Hide();
			}

			if (newCreate)
			{
				parent.AddChild(visualLine);
				visualLine.Owner = root;
			}

			i++;
		}
	}

	public void OnOptionButtonFileSelected(string fileName)
	{
		GeneratePet(fileName);
	}

	public void OnOptionButtonFileSaved(string fileName)
	{
		GeneratePet(fileName);
	}

	public void OnAnimPickerItemSelected(long index)
	{
		if (_bhd != null && index >= 0 && index < _bhd.AnimationRanges.Count)
		{
			SetAnimation((int)index);
		}
	}

	public void OnAnimPickerTextEntered(string newText)
	{
		if (int.TryParse(newText, out var i) && _bhd != null && i >= 0 && i < _bhd.AnimationRanges.Count)
		{
			SetAnimation(i);
		}
	}

	public void OnBallCheckBoxToggled(bool buttonPressed)
	{
		DrawBalls = buttonPressed;
		GetTree().CallGroup("balls", Ball.MethodName.SetVisibleOverride, buttonPressed);
	}

	public void OnAddballCheckBoxToggled(bool buttonPressed)
	{
		DrawAddballs = buttonPressed;
		GetTree().CallGroup("addballs", Ball.MethodName.SetVisibleOverride, buttonPressed);
	}

	public void OnPaintballCheckBoxToggled(bool buttonPressed)
	{
		DrawPaintballs = buttonPressed;
		GetTree().CallGroup("paintballs", Paintball.MethodName.SetVisibleOverride, buttonPressed);
	}

	public void OnLineCheckBoxToggled(bool buttonPressed)
	{
		DrawLines = buttonPressed;
		GetTree().CallGroup("lines", Node3D.MethodName.SetVisible, buttonPressed);
	}

	private void SignalBallMouseEnter(int ballNo)
	{
		EmitSignal(SignalName.BallMouseEnter, ballNo.ToString());
	}

	private void SignalBallMouseExit(int ballNo)
	{
		EmitSignal(SignalName.BallMouseExit, ballNo);
	}

	private void SignalPaintballMouseEnter(int baseBallNo)
	{
		EmitSignal(SignalName.BallMouseEnter, $"Paintball on {baseBallNo}");
	}

	private void SignalPaintballMouseExit()
	{
		EmitSignal(SignalName.BallMouseExit, 0);
	}

	private void SignalBallSelected(int ballNo, int section)
	{
		if (_lnz == null || !_ballMap.TryGetValue(ballNo, out var ball))
		{
			return;
		}

		var isAddball = ball.BaseBallNo != -1 && !ball.IsIris;
		var maxAddballNo = _lnz.Balls.Count > 0 ? _lnz.Balls.Keys.Max() + 1 : 67;
		EmitSignal(SignalName.BallSelected, section, ballNo, isAddball, maxAddballNo);
	}

	private void SignalBallDeleted(int ballNo)
	{
		if (_ballMap.TryGetValue(ballNo, out var ball) && ball.BaseBallNo != -1)
		{
			EmitSignal(SignalName.AddballDeleted, ballNo);
		}
	}

	public void OnLnzTextEditFindBall(int ballNo)
	{
		if (_ballMap.TryGetValue(ballNo, out var ball))
		{
			ball.Flash();
		}
	}

	public void OnToolsMenuPrintBallColors()
	{
		if (_lnz == null)
		{
			return;
		}

		var sb = new StringBuilder();
		foreach (var (b, ball) in _ballMap)
		{
			if (ball.IsIris)
			{
				continue;
			}

			int group;
			int textureId;
			if (b < 67 && _lnz.Balls.TryGetValue(b, out var baseData))
			{
				group = baseData.Group;
				textureId = baseData.TextureId;
			}
			else if (_lnz.Addballs.TryGetValue(b, out var addData))
			{
				group = addData.Group;
				textureId = addData.TextureId;
			}
			else
			{
				continue;
			}

			var thisBallString = $"{ball.BallNo},\t\t{ball.ColorIndex},\t\t{group},\t\t{textureId.ToString().Replace('0', '3')}";
			if (sb.Length > 0)
			{
				sb.Append('\n');
			}

			sb.Append(thisBallString);
			Logger.LogInfo(thisBallString, nameof(DogGenerator));
		}

		DisplayServer.ClipboardSet(sb.ToString());
	}
}
