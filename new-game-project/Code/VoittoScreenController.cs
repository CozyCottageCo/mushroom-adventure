using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class VoittoScreenController : TextureRect
{
[Export] private Button _palaaTasoValikkoonButton = null;
[Export] private AudioStreamPlayer2D _nappiAudio = null;


private SceneTree _mainMenuSceneTree = null;
private LevelSelectionController levelSelectionController;

private Touch _touch = null; // tää on vaa touch.cs alustus

private SettingsController settings = null;
public override void _Ready() {
{

        UpdateUIText();

	base._Ready();



	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
	_touch = GetNode<Touch>("/root/Node2D"); // root node, nappeja kii
	_palaaTasoValikkoonButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTasoValikkoonPressed)));



}
}



	private async void OnTasoValikkoonPressed() {
		_nappiAudio.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		GD.Print("Tasovalik pressed");
		GetTree().Paused = false;
		string currentScenePath = GetTree().CurrentScene.SceneFilePath; // Get full path
		string currentSceneName = currentScenePath.GetFile().GetBaseName();
		GD.Print(currentSceneName);
		 if (currentSceneName.StartsWith("Level"))
            {
                string currentLevel = currentSceneName.Replace("Level", ""); //otetaan kaikki paitsi numero pois
                int.TryParse(currentLevel, out int levelNumber); // muutetaan intiksi
		int levelSelectNumber = 0;
		if (levelNumber > 0 && levelNumber < 5) {
			levelSelectNumber = 1;
		} else if (levelNumber > 4 && levelNumber < 9) {
			levelSelectNumber = 2;
		} else if (levelNumber > 8 && levelNumber < 13) {
			levelSelectNumber = 3;
		} else if (levelNumber > 12 && levelNumber < 17) {
			levelSelectNumber = 4;
		}
		if (currentSceneName == "Levelhuussi") {
			levelSelectNumber = 3;
		}
		if (currentSceneName == "Levelmökki") {
			levelSelectNumber = 4;
		}
		string levelSelectionPath = $"res://Level/LevelSelect{levelSelectNumber}.tscn";
		GD.Print(levelSelectionPath);
		SceneTransition sceneTransition = GetNode<SceneTransition>("/root/SceneTransition");
	    sceneTransition.FadeToScene(levelSelectionPath);
		//_mainMenuSceneTree.ChangeSceneToFile(levelSelectionPath);

	}
	}

	private void UpdateUIText()
        {
            _palaaTasoValikkoonButton.Text = Tr("level");
        }

}

}
