extends Node

func _unhandled_key_input(event):
	if event.pressed and event.control and event.keycode == KEY_SPACE:
		show_popup_menu()
		
func show_popup_menu():
	$SceneRoot/ToolsMenu.position = Vector2i(get_viewport().get_mouse_position())
	$SceneRoot/ToolsMenu.popup()
