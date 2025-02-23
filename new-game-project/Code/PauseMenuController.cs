using Godot;
using System;
namespace SieniPeli;
public partial class PauseMenuController : Control
{
[Export] private Button _palaaTasoValikkoonButton = null;
[Export] private Button _jatkaButton = null;

private SceneTree _mainMenuSceneTree = null;

private Touch _touch = null;
public override void _Ready() {
{
	base._Ready();
	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
	_touch = GetNode<Touch>("/root/Node2D");
	_palaaTasoValikkoonButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTasoValikkoonPressed)));
	_jatkaButton.Connect(Button.SignalName.Pressed, new Callable(_touch, nameof(Touch.OnMenuButtonPressed)));

}
}

    private void OnTasoValikkoonPressed() {
            string mainMenuPath = "res://Level/MainMenu.tscn";
            _mainMenuSceneTree.ChangeSceneToFile(mainMenuPath);
    }




}
