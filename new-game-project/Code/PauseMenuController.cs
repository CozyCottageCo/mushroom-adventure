using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class PauseMenuController : Control
{
[Export] private Button _palaaPääValikkoonButton = null;
[Export] private Button _jatkaButton = null;
[Export] private Button _peliLäpiButton = null;

private SceneTree _mainMenuSceneTree = null;
private LevelSelectionController levelSelectionController;

private Touch _touch = null; // tää on vaa touch.cs alustus
public override void _Ready() {
{
	ConfigFile config = new ConfigFile();
        if (config.Load("user://settings.cfg") == Error.Ok)
        {
            int savedLanguage = (int)config.GetValue("Settings", "Language", 0);
            string locale = savedLanguage == 0 ? "en" : "fi";
            TranslationServer.SetLocale(locale);
        }

        // Update UI text translations
        UpdateUIText();

	base._Ready();

	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
	_touch = GetNode<Touch>("/root/Node2D"); // root node, nappeja kii
	_palaaPääValikkoonButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPääValikkoonPressed)));
	_jatkaButton.Connect(Button.SignalName.Pressed, new Callable(_touch, nameof(Touch.OnMenuButtonPressed)));
	// suoraan touch.cs lainattu metodi mikä vaa flippas sen paneelin näkyvyyden

	_peliLäpiButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(PeliLäpiPressed)));


}
}

    private void OnPääValikkoonPressed() { // menuun paluu
            string mainMenuPath = "res://Level/MainMenu.tscn";
            _mainMenuSceneTree.ChangeSceneToFile(mainMenuPath);
    }


	private void PeliLäpiPressed() {
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
		string levelSelectionPath = $"res://Level/LevelSelect{levelSelectNumber}.tscn";
		GD.Print(levelSelectionPath);
		_mainMenuSceneTree.ChangeSceneToFile(levelSelectionPath);

	}
	}

	private void UpdateUIText()
        {
            _palaaPääValikkoonButton.Text = Tr("mainmenu");
			_peliLäpiButton.Text = Tr("level");
			_jatkaButton.Text = Tr("continue");
        }

}

}
