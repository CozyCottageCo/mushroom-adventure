using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class PauseMenuController : Control
{
[Export] private Button _palaaTasoValikkoonButton = null;
[Export] private Button _jatkaButton = null;

[Export] private Button _asetuksetButton = null;

private SceneTree _mainMenuSceneTree = null;
private LevelSelectionController levelSelectionController;

private Touch _touch = null; // tää on vaa touch.cs alustus

private SettingsController settings = null;

[Export] AudioStreamPlayer2D _nappiAudio = null;
[Export] AudioStreamPlayer2D _nappiAudio2 = null;
public override void _Ready() {
{
	 settings = GetNode<SettingsController>("/root/SettingsController");
            if (settings == null) {
                GD.PrintErr("no find settings");
            }

	ConfigFile config = new ConfigFile();
		if (config.Load("user://settings.cfg") == Error.Ok)
		{
			GD.Print("Settings loaded successfully!"); // Debugging
			int savedLanguage = (int)config.GetValue("Settings", "Language", 0);
			GD.Print("Saved language index: " + savedLanguage); // Debugging
			string locale = savedLanguage == 0 ? "en" : "fi";
			TranslationServer.SetLocale(locale);
			GD.Print("Locale set to: " + locale); // Debugging
		}
		else
		{
			GD.PrintErr("Failed to load settings.cfg!");
		}

        // Update UI text translations
        UpdateUIText();

	base._Ready();

	  Control settingsMenu = GetNode<Control>("Settings");
		if (settingsMenu != null) {
			settingsMenu.Visible = false; // Hide at start
		} else {
			GD.PrintErr("Settings menu node not found!");
		}

	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
	_touch = GetNode<Touch>("/root/Node2D"); // root node, nappeja kii
	_palaaTasoValikkoonButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTasoValikkoonPressed)));
	_asetuksetButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnAsetuksetPressed)));
	_jatkaButton.Connect(Button.SignalName.Pressed, new Callable(_touch, nameof(Touch.OnMenuButtonPressed)));
	// suoraan touch.cs lainattu metodi mikä vaa flippas sen paneelin näkyvyyden



}
}



	private async void OnAsetuksetPressed() {
		_nappiAudio.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		GD.Print("Asetukset pressed");
		 Control settingsMenu = GetNode<Control>("Settings"); // Make sure the node path matches
		if (settingsMenu != null) {
			settingsMenu.Visible = !settingsMenu.Visible; // Toggle visibility
		} else {
			GD.PrintErr("Settings menu node not found!");
		}
	}


	private async void OnTasoValikkoonPressed() {
		_nappiAudio2.Play();
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
		string levelSelectionPath = $"res://Level/LevelSelect{levelSelectNumber}.tscn";
		GD.Print(levelSelectionPath);
		_mainMenuSceneTree.ChangeSceneToFile(levelSelectionPath);

	}
	}

	private void UpdateUIText()
        {
            _palaaTasoValikkoonButton.Text = Tr("level");
		    _asetuksetButton.Text = Tr("settings");
			_jatkaButton.Text = Tr("continue");
        }

}

}
