using Godot;
using PetzRendering.Core;

namespace PetzRendering.UI;

public partial class UnhandledInputHandler : Node
{
	private PopupMenu? _toolsMenu;

	public override void _Ready()
	{
		_toolsMenu = GetNodeOrNull<PopupMenu>("SceneRoot/ToolsMenu");
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.OpenToolsMenu))
		{
			GetViewport().SetInputAsHandled();
			ShowPopupMenu();
		}
	}

	private void ShowPopupMenu()
	{
		_toolsMenu ??= GetNodeOrNull<PopupMenu>("SceneRoot/ToolsMenu");
		if (_toolsMenu != null)
		{
			_toolsMenu.Position = (Vector2I)GetViewport().GetMousePosition();
			_toolsMenu.Popup();
		}
	}
}
