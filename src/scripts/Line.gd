extends Node3D

@export var line_widths = Vector2(10, 10): set = set_line_width
@export var fuzz_amount = 0: set = set_fuzz_amount
@export var color_index = 0: set = set_color_index
@export var l_color_index = 0: set = set_l_color_index
@export var r_color_index = 0: set = set_r_color_index
@export var ball_world_pos1 = Vector3.ZERO: set = set_ball_world_pos1
@export var ball_world_pos2 = Vector3.ZERO: set = set_ball_world_pos2
@export var texture: Texture2D: set = set_texture
@export var transparent_color = 0: set = set_transparent_color
@export var pet_center = Vector3.ZERO: set = set_pet_center

var palette = preload("res://resources/textures/petzpalette.png")

func _ready():
	$MeshInstance3D.material_override.set_shader_parameter("palette", palette)

func set_line_width(new_value):
	line_widths = new_value;
	$MeshInstance3D.material_override.set_shader_parameter("line_width_in_pixels_start", new_value.x);
	$MeshInstance3D.material_override.set_shader_parameter("line_width_in_pixels_end", new_value.y);
	
func set_fuzz_amount(new_value):
	fuzz_amount = new_value;
	$MeshInstance3D.material_override.set_shader_parameter("fuzz_amount", new_value);

func set_r_color_index(new_value):
	r_color_index = new_value
	$MeshInstance3D.material_override.set_shader_parameter("r_color_index", new_value);

func set_l_color_index(new_value):
	l_color_index = new_value
	$MeshInstance3D.material_override.set_shader_parameter("l_color_index", new_value);

func set_ball_world_pos1(new_value):
	ball_world_pos1 = new_value
	$MeshInstance3D.material_override.set_shader_parameter("ball_world_pos1", new_value);
	
func set_ball_world_pos2(new_value):
	ball_world_pos2 = new_value
	$MeshInstance3D.material_override.set_shader_parameter("ball_world_pos2", new_value);

func set_color_index(new_value):
	color_index = new_value
	$MeshInstance3D.material_override.set_shader_parameter("color_index", new_value)
	
func set_texture(new_value):
	texture = new_value
	$MeshInstance3D.material_override.set_shader_parameter("line_texture", new_value)
	$MeshInstance3D.material_override.set_shader_parameter("has_texture", true)
	if new_value != null:
		$MeshInstance3D.material_override.set_shader_parameter("texture_size", new_value.get_size())
	else:
		$MeshInstance3D.material_override.set_shader_parameter("has_texture", false)

func set_transparent_color(new_value):
	transparent_color = new_value
	$MeshInstance3D.material_override.set_shader_parameter("transparent_index", new_value)

func set_pet_center(new_value):
	pet_center = new_value
	$MeshInstance3D.material_override.set_shader_parameter("z_center", new_value)
