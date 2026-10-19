using System.Collections.Generic;
using Godot;

namespace PetzRendering.Parsers;

public static class PetzTextureLoader
{
	private static readonly Dictionary<string, Texture2D> Cache = new();

	public static Image? LoadPetzBmp(string path)
	{
		using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (f == null)
		{
			return null;
		}

		var magic = f.Get16();
		if (magic != 0x4D42) // "BM"
		{
			f.Close();
			return Image.LoadFromFile(path);
		}

		f.Seek(10);
		var dataOffset = f.Get32();
		f.Seek(18);
		var width = (int)f.Get32();
		var height = (int)f.Get32();
		f.Seek(28);
		var bpp = f.Get16();
		f.Seek(30);
		var compression = f.Get32();

		// If not 8bpp uncompressed, fallback to standard image
		if (bpp != 8 || compression != 0)
		{
			f.Close();
			return Image.LoadFromFile(path);
		}

		var isTopDown = false;
		if (height < 0)
		{
			height = -height;
			isTopDown = true;
		}

		var rowStride = (width + 3) & ~3;
		var img = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);

		for (var y = 0; y < height; y++)
		{
			var targetY = isTopDown ? y : (height - 1 - y);
			f.Seek(dataOffset + (uint)(y * rowStride));
			var rowBytes = f.GetBuffer(width);
			for (var x = 0; x < width; x++)
			{
				var idx = rowBytes[x];
				img.SetPixel(x, targetY, new Color(idx / 255.0f, 0.0f, 0.0f, 1.0f));
			}
		}

		return img;
	}

	public static Texture2D? GetTexture(string filename, ResourcePreloader? preloader = null)
	{
		if (Cache.TryGetValue(filename, out var cached))
		{
			return cached;
		}

		var resPath = "res://resources/textures/" + filename;
		var userPath = "user://resources/textures/" + filename;
		var chosenPath = "";
		if (FileAccess.FileExists(userPath))
		{
			chosenPath = userPath;
		}
		else if (FileAccess.FileExists(resPath))
		{
			chosenPath = resPath;
		}

		if (!string.IsNullOrEmpty(chosenPath))
		{
			var img = LoadPetzBmp(chosenPath);
			if (img != null)
			{
				var tex = ImageTexture.CreateFromImage(img);
				Cache[filename] = tex;
				return tex;
			}
		}

		if (preloader != null && preloader.HasResource(filename))
		{
			if (preloader.GetResource(filename) is Texture2D preloadedTex)
			{
				Cache[filename] = preloadedTex;
				return preloadedTex;
			}
		}

		if (ResourceLoader.Exists(resPath))
		{
			if (ResourceLoader.Load(resPath) is Texture2D loadedTex)
			{
				Cache[filename] = loadedTex;
				return loadedTex;
			}
		}

		return null;
	}

	public static void ClearCache()
	{
		Cache.Clear();
	}
}
