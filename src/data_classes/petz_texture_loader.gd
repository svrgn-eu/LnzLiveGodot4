class_name PetzTextureLoader

static var _cache: Dictionary = {}

static func load_petz_bmp(path: String) -> Image:
	var f = FileAccess.open(path, FileAccess.READ)
	if !f:
		return null
	var magic = f.get_16()
	if magic != 0x4D42: # "BM"
		f.close()
		return Image.load_from_file(path)
	
	f.seek(10)
	var data_offset = f.get_32()
	f.seek(18)
	var width = f.get_32()
	var height = f.get_32()
	f.seek(28)
	var bpp = f.get_16()
	f.seek(30)
	var compression = f.get_32()
	
	# If not 8bpp uncompressed, fallback to standard image
	if bpp != 8 or compression != 0:
		f.close()
		return Image.load_from_file(path)
	
	var is_top_down = false
	if height < 0:
		height = -height
		is_top_down = true
	
	var row_stride = (width + 3) & ~3
	var img = Image.create(width, height, false, Image.FORMAT_RGB8)
	
	for y in range(height):
		var target_y = y if is_top_down else (height - 1 - y)
		f.seek(data_offset + y * row_stride)
		var row_bytes = f.get_buffer(width)
		for x in range(width):
			var idx = row_bytes[x]
			img.set_pixel(x, target_y, Color(float(idx) / 255.0, 0.0, 0.0, 1.0))
	f.close()
	return img

static func get_texture(filename: String, preloader: ResourcePreloader = null) -> Texture2D:
	if _cache.has(filename):
		return _cache[filename]
	
	var res_path = "res://resources/textures/" + filename
	var user_path = "user://resources/textures/" + filename
	var chosen_path = ""
	if FileAccess.file_exists(user_path):
		chosen_path = user_path
	elif FileAccess.file_exists(res_path):
		chosen_path = res_path
	
	if chosen_path != "":
		var img = load_petz_bmp(chosen_path)
		if img != null:
			var tex = ImageTexture.create_from_image(img)
			_cache[filename] = tex
			return tex
	
	if preloader != null and preloader.has_resource(filename):
		var tex = preloader.get_resource(filename)
		_cache[filename] = tex
		return tex
	
	if ResourceLoader.exists(res_path):
		var tex = ResourceLoader.load(res_path)
		_cache[filename] = tex
		return tex
	
	return null

static func clear_cache():
	_cache.clear()
