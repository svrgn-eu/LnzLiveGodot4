extends OptionButton

func _ready():
	disabled = true
	add_item("Anim: -")
	get_popup().max_size = Vector2i(0, 400)

func _on_bhd_loaded(num_of_animations: int):
	var prev_selected = selected
	clear()
	for i in num_of_animations:
		add_item("Anim " + str(i), i)
	disabled = num_of_animations == 0
	if num_of_animations > 0:
		if prev_selected >= 0 and prev_selected < num_of_animations:
			select(prev_selected)
		else:
			select(0)
