using Godot;
using System;
namespace SieniPeli;
public partial class PauseMenuController : Control
{
[Export] private Button _palaaPääValikkoonButton = null;
[Export] private Button _jatkaButton = null;

private SceneTree _mainMenuSceneTree = null;

private Touch _touch = null; // tää on vaa touch.cs alustus
public override void _Ready() {
{
	base._Ready();
	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
	_touch = GetNode<Touch>("/root/Node2D");
	_palaaPääValikkoonButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPääValikkoonPressed)));
	_jatkaButton.Connect(Button.SignalName.Pressed, new Callable(_touch, nameof(Touch.OnMenuButtonPressed)));
	// suoraan touch.cs lainattu metodi mikä vaa flippas sen paneelin näkyvyyden

}
}

    private void OnPääValikkoonPressed() { // menuun paluu
            string mainMenuPath = "res://Level/MainMenu.tscn";
            _mainMenuSceneTree.ChangeSceneToFile(mainMenuPath);
    }




}
