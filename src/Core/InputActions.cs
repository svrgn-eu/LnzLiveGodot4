using Godot;

namespace PetzRendering.Core;

/// <summary>
/// Centralized Godot <see cref="InputMap"/> action names for keyboard bindings configured in <c>project.godot</c>.
/// </summary>
public static class InputActions
{
	public static readonly StringName SaveFile = new("save_file");
	public static readonly StringName FindBall = new("find_ball");
	public static readonly StringName OpenToolsMenu = new("open_tools_menu");
	public static readonly StringName SelectBallSection = new("select_ball_section");
	public static readonly StringName SelectMoveSection = new("select_move_section");
	public static readonly StringName SelectProjectSection = new("select_project_section");
	public static readonly StringName SelectLineSection = new("select_line_section");
	public static readonly StringName DeleteBall = new("delete_ball");

	/// <summary>
	/// Built-in Godot UI action triggered by Enter / Numpad Enter inside text inputs.
	/// </summary>
	public static readonly StringName UiTextSubmit = new("ui_text_submit");

	/// <summary>
	/// Ensures all custom project input actions are registered in <see cref="InputMap"/>
	/// as a fallback if <c>project.godot</c> bindings were not loaded.
	/// </summary>
	public static void EnsureRegistered()
	{
		RegisterKeyActionIfMissing(SaveFile, ctrlPressed: true, Key.S);
		RegisterKeyActionIfMissing(FindBall, ctrlPressed: true, Key.Q);
		RegisterKeyActionIfMissing(OpenToolsMenu, ctrlPressed: true, Key.Space);
		RegisterKeyActionIfMissing(SelectBallSection, ctrlPressed: false, Key.B, Key.Z);
		RegisterKeyActionIfMissing(SelectMoveSection, ctrlPressed: false, Key.M, Key.X);
		RegisterKeyActionIfMissing(SelectProjectSection, ctrlPressed: false, Key.P, Key.C);
		RegisterKeyActionIfMissing(SelectLineSection, ctrlPressed: false, Key.L, Key.V);
		RegisterKeyActionIfMissing(DeleteBall, ctrlPressed: false, Key.Delete);
	}

	private static void RegisterKeyActionIfMissing(StringName action, bool ctrlPressed, params Key[] keys)
	{
		if (InputMap.HasAction(action))
		{
			return;
		}

		InputMap.AddAction(action, 0.2f);
		foreach (var key in keys)
		{
			var keyEvent = new InputEventKey
			{
				Keycode = key,
				CtrlPressed = ctrlPressed
			};
			InputMap.ActionAddEvent(action, keyEvent);
		}
	}
}
