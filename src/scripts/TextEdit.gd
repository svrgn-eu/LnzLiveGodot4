extends TextEdit

var current_file_name
signal file_saved 

func open_file(file_name):
	current_file_name = file_name
	var file = FileAccess.open("res://resources/" + current_file_name, FileAccess.READ)
	if file:
		text = file.get_as_text()

func save_file():
	var file = FileAccess.open("res://resources/" + current_file_name, FileAccess.WRITE)
	if file:
		file.store_string(text)

func _unhandled_key_input(event):
	if Input.is_key_pressed(KEY_CTRL) and event.pressed and event.keycode == KEY_S:
		save_file()
		emit_signal("file_saved")
		print("saved file")

func _on_OptionButton_file_selected(file_name):
	open_file(file_name)
