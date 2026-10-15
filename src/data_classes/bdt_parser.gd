extends Node
class_name BdtParser

var frames = []

func _init(filename: String, frame_indexes: Array, balls_num: int):
	var file = FileAccess.open("res://resources/animations/"+filename, FileAccess.READ)
	if not file:
		return
	for frame_offset in frame_indexes:
		file.seek(frame_offset)
		for i in 6:
			var _spec = file.get_16()
		var tag = file.get_16()
		var balls = []
		for ball in balls_num:
			var x = get_s16(file)
			var y = get_s16(file)
			var z = get_s16(file)
			var rot1 = get_s8(file)
			var rot2 = get_s8(file)
			var rot3 = get_s8(file)
			var _tag3 = get_s8(file)
			balls.append(
				{
					tag = tag,
					position = Vector3(x, y, z),
					rotation = Vector3(rot1, rot2, rot3)
				}
			)
		frames.append(balls)
	file.close()

static func get_s16(file: FileAccess) -> int:
	var u = file.get_16()
	return u - 65536 if u >= 32768 else u

static func get_s8(file: FileAccess) -> int:
	var u = file.get_8()
	return u - 256 if u >= 128 else u
