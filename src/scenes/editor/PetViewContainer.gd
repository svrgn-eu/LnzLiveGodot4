extends Control

@onready var camera_holder = get_tree().root.get_node("Root/SceneRoot/SubViewportContainer/SubViewport/CameraHolder") as Node3D
@onready var camera = camera_holder.get_node("Camera3D") as Camera3D
@onready var label = get_tree().root.get_node("Root/SceneRoot/Label")
@onready var cube = get_tree().root.get_node("Root/PetRoot/MeshInstance3D") as Node3D
@onready var tex = get_tree().root.get_node("Root/SceneRoot/SubViewportContainer") as SubViewportContainer
@onready var popup = get_tree().root.get_node("Root/SceneRoot/Popup") as Window

var last_selected
var selecting_on = false

func _gui_input(event):
	if event is InputEventMouseMotion:
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
			var motion = event.relative as Vector2
			camera_holder.rotation.x += motion.y * 0.01
			camera_holder.rotation.y += motion.x * -0.01
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT) or Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
			var motion = event.relative as Vector2
			camera.transform.origin.x += motion.x * 0.001 / tex.scale.x
			camera.transform.origin.y += motion.y * 0.001 / tex.scale.x

		label.global_position = event.global_position

		# try and do a raycast
		# the center of the actual texture == (500,500)
		if selecting_on:
			var real_center = position + size / 2.0
			var real_mouse_pos = event.position
			var offset = real_mouse_pos - real_center
			offset /= tex.scale
			var final_pos = Vector2(500,500) + offset

			var from = camera.project_ray_origin(final_pos)
			var to = from + camera.project_ray_normal(final_pos) * 950
			var space_state = camera.get_world_3d().direct_space_state
			var query = PhysicsRayQueryParameters3D.create(from, to)
			query.collide_with_areas = true
			query.collide_with_bodies = false
			var result = space_state.intersect_ray(query)
			if !result.is_empty():
				label.show()
				deal_with_last_selected()
				result.collider.get_parent()._on_Area_mouse_entered()
				last_selected = result.collider.get_parent()
			else:
				deal_with_last_selected()
				last_selected = null
				label.hide()

	elif event is InputEventMouseButton and event.pressed:
		if event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			tex.pivot_offset = tex.size / 2.0
			tex.scale = tex.scale / 2.0
		elif event.button_index == MOUSE_BUTTON_WHEEL_UP:
			tex.pivot_offset = tex.size / 2.0
			tex.scale = tex.scale * 2.0
		elif event.double_click and event.button_index == MOUSE_BUTTON_LEFT and last_selected_is_valid():
			last_selected.selected()
	pass
			
func _unhandled_key_input(event):
	if event.pressed and last_selected_is_valid():
		last_selected._input(event)
		
func last_selected_is_valid():
	return last_selected != null and is_instance_valid(last_selected)

func deal_with_last_selected():
	if last_selected != null and is_instance_valid(last_selected):
		last_selected._on_Area_mouse_exited()
				
func _on_Node_ball_mouse_enter(ball_info):
	label.text = str(ball_info.ball_no)

func _on_SelectCheckBox_toggled(button_pressed):
	selecting_on = button_pressed
	if !selecting_on:
		if last_selected_is_valid():
			last_selected._on_Area_mouse_exited()
		last_selected = null
		label.hide()

func _on_HelpButton_pressed():
	popup.popup_centered()

func _on_LnzTextEdit_mouse_entered():
	if last_selected_is_valid():
		last_selected._on_Area_mouse_exited()
	last_selected = null
	label.hide()

func _ready():
	_on_PetViewContainer_resized()
	if tex:
		tex.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST

func _on_PetViewContainer_resized():
	if tex == null:
		if has_node("/root/Root/SceneRoot/SubViewportContainer"):
			tex = get_node("/root/Root/SceneRoot/SubViewportContainer") as SubViewportContainer
		elif has_node("../../../SubViewportContainer"):
			tex = get_node("../../../SubViewportContainer") as SubViewportContainer
		else:
			return
	var size_diff = tex.size / 2.0 - self.size / 2.0
	tex.global_position = self.global_position - size_diff

func _on_PetViewContainer_sort_children():
	_on_PetViewContainer_resized()
