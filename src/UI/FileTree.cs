using System;
using Godot;
using PetzRendering.Parsers;

namespace PetzRendering.UI;

public partial class FileTree : Tree
{
	[Signal]
	public delegate void ExampleFileSelectedEventHandler(string filepath);

	[Signal]
	public delegate void UserFileSelectedEventHandler(string filepath);

	[Signal]
	public delegate void BackupFileEventHandler();

	[Export]
	public string ExampleFileLocation { get; set; } = "res://resources/examples/";

	[Export]
	public string UserFileLocation { get; set; } = "user://resources/";

	private TreeItem? _examples;
	private TreeItem? _localStorage;
	private TreeItem? _root;
	private TreeItem? _localStorageTextures;

	private Window? _renameDialog;
	private ResourcePreloader? _preloader;
	private PopupMenu? _itemPopupMenu;

	public override void _Ready()
	{
		_renameDialog = GetTree().Root.GetNodeOrNull<Window>("Root/SceneRoot/RenameDialog");
		_preloader = GetTree().Root.GetNodeOrNull<ResourcePreloader>("Root/ResourcePreloader");
		_itemPopupMenu = GetNodeOrNull<PopupMenu>("ItemPopupMenu");

		ItemMouseSelected += OnTreeItemMouseSelected;

		_root = CreateItem();
		_examples = CreateItem(_root);
		_examples.SetText(0, "Examples");

		using var dir = DirAccess.Open(ExampleFileLocation);
		if (dir != null)
		{
			dir.ListDirBegin();
			var filename = dir.GetNext();
			while (!string.IsNullOrEmpty(filename))
			{
				if (filename.EndsWith(".lnz", StringComparison.OrdinalIgnoreCase))
				{
					var newItem = CreateItem(_examples);
					newItem.SetText(0, filename);
					newItem.SetMetadata(0, ExampleFileLocation + filename);
				}

				filename = dir.GetNext();
			}

			dir.ListDirEnd();
		}

		Rescan(null);
		RescanTextures();
	}

	public void OnTreeItemActivated()
	{
		var selected = GetSelected();
		if (selected == null)
		{
			return;
		}

		var filepath = selected.GetMetadata(0).AsString();
		var parent = selected.GetParent();
		if (parent == _examples)
		{
			EmitSignal(SignalName.ExampleFileSelected, filepath);
		}
		else if (parent == _localStorage)
		{
			EmitSignal(SignalName.UserFileSelected, filepath);
		}

		ReleaseFocus();
	}

	public void Rescan(string? selectedFilepath)
	{
		if (_root == null)
		{
			return;
		}

		if (_localStorage != null)
		{
			_root.RemoveChild(_localStorage);
			_localStorage.Free();
		}

		_localStorage = CreateItem(_root, 1);
		_localStorage.SetText(0, "Local Storage");
		ScanLocalStorage(selectedFilepath);
	}

	public void RescanTextures()
	{
		if (_root == null)
		{
			return;
		}

		var wasCollapsed = true;
		if (_localStorageTextures != null)
		{
			wasCollapsed = _localStorageTextures.Collapsed;
			_root.RemoveChild(_localStorageTextures);
			_localStorageTextures.Free();
		}

		_localStorageTextures = CreateItem(_root, 2);
		_localStorageTextures.Collapsed = wasCollapsed;
		_localStorageTextures.SetText(0, "Local Textures");
		ScanLocalTextures();
	}

	private void ScanLocalStorage(string? selectedFilepath)
	{
		if (_localStorage == null)
		{
			return;
		}

		using var dir2 = DirAccess.Open(UserFileLocation);
		if (dir2 == null)
		{
			return;
		}

		dir2.ListDirBegin();
		var filename = dir2.GetNext();
		while (!string.IsNullOrEmpty(filename))
		{
			if (filename.EndsWith(".lnz", StringComparison.OrdinalIgnoreCase))
			{
				var newItem = CreateItem(_localStorage);
				var fullPath = UserFileLocation + filename;
				newItem.SetText(0, filename);
				newItem.SetMetadata(0, fullPath);
				if (fullPath == selectedFilepath)
				{
					newItem.Select(0);
				}
			}

			filename = dir2.GetNext();
		}

		dir2.ListDirEnd();
	}

	private void ScanLocalTextures()
	{
		if (_localStorageTextures == null)
		{
			return;
		}

		using var dir2 = DirAccess.Open(UserFileLocation + "/textures");
		if (dir2 == null)
		{
			return;
		}

		dir2.ListDirBegin();
		var filename = dir2.GetNext();
		while (!string.IsNullOrEmpty(filename))
		{
			if (filename.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
			{
				var newItem = CreateItem(_localStorageTextures);
				newItem.SetText(0, filename);
				newItem.SetMetadata(0, UserFileLocation + filename);
				var img = PetzTextureLoader.LoadPetzBmp(UserFileLocation + "/textures/" + filename);
				if (img != null && _preloader != null)
				{
					var tex = ImageTexture.CreateFromImage(img);
					_preloader.AddResource(filename, tex);
				}
			}

			filename = dir2.GetNext();
		}

		dir2.ListDirEnd();
	}

	private void OnTreeItemMouseSelected(Vector2 mousePosition, long mouseButtonIndex)
	{
		if (mouseButtonIndex == (long)MouseButton.Right)
		{
			OnTreeItemRmbSelected(mousePosition);
		}
	}

	public void OnTreeItemRmbSelected(Vector2 pos)
	{
		if (_itemPopupMenu == null)
		{
			return;
		}

		var item = GetSelected();
		if (item == null)
		{
			return;
		}

		_itemPopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
		var isNotLocalStorage = item.GetParent() != _localStorage;
		_itemPopupMenu.SetItemDisabled(0, isNotLocalStorage);
		_itemPopupMenu.SetItemDisabled(1, isNotLocalStorage);
		_itemPopupMenu.SetItemDisabled(2, isNotLocalStorage);
		_itemPopupMenu.Popup();
	}

	public void OnItemPopupMenuIdPressed(long id)
	{
		var item = GetSelected();
		if (item == null)
		{
			return;
		}

		if (id == 0) // delete file
		{
			var filepath = item.GetMetadata(0).AsString();
			DirAccess.RemoveAbsolute(filepath);
			Rescan(null);
		}
		else if (id == 1) // rename file
		{
			var filepath = item.GetMetadata(0).AsString();
			if (_renameDialog != null)
			{
				_renameDialog.Popup();
				var lineEdit = _renameDialog.GetNodeOrNull<LineEdit>("LineEdit");
				if (lineEdit != null)
				{
					lineEdit.Text = filepath.GetFile();
				}
			}
		}
		else if (id == 2) // back up
		{
			EmitSignal(SignalName.BackupFile);
		}
	}

	public void OnRenameDialogConfirmed()
	{
		var item = GetSelected();
		if (item == null || _renameDialog == null)
		{
			return;
		}

		var filepath = item.GetMetadata(0).AsString();
		var lineEdit = _renameDialog.GetNodeOrNull<LineEdit>("LineEdit");
		if (lineEdit == null)
		{
			return;
		}

		var newFilename = lineEdit.Text;
		var newFilepath = filepath.Replace(filepath.GetFile(), newFilename, StringComparison.Ordinal);
		DirAccess.RenameAbsolute(filepath, newFilepath);
		Rescan(newFilepath);
		EmitSignal(SignalName.UserFileSelected, newFilepath);
	}

	public void OnItemPopupMenuAboutToShow()
	{
		var clickedItem = GetSelected();
		if (clickedItem == null || _itemPopupMenu == null)
		{
			return;
		}

		var textLnz = GetTree().Root.GetNodeOrNull<LnzTextEdit>(
			"Root/SceneRoot/HSplitContainer/HSplitContainer/TextPanelContainer/LnzTextEdit");
		var clickedFilepath = clickedItem.GetMetadata(0).AsString();
		_itemPopupMenu.SetItemDisabled(2, textLnz == null || textLnz.Filepath != clickedFilepath);
	}

	public void OnLnzTextEditFileBackedUp()
	{
		var selected = GetSelected();
		Rescan(selected?.GetMetadata(0).AsString());
	}
}
