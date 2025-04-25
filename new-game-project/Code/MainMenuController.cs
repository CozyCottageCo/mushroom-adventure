using Godot;
using System;
namespace SieniPeli;
public partial class MainMenuController : Control
{
[Export] private Button _pelaaButton = null;
[Export] private Button _asetuksetButton = null;
[Export]private Button _poistuButton = null;
[Export] AudioStreamPlayer2D _audioPuu = null;
[Export] AudioStreamPlayer2D _audioMuu = null;

[Export] Button _nettisivu = null;

private SceneTree _mainMenuSceneTree = null;

public override void _Ready() {
{
	ApplySettingsVolume();
	var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
		musicPlayer.PlayMusicForCurrentScene(); // musalataus

	ConfigFile config = new ConfigFile();
	if (config.Load("user://settings.cfg") == Error.Ok)
	{
		int savedLanguage = (int)config.GetValue("Settings", "Language", 0);
		string locale = savedLanguage == 0 ? "en" : "fi";
		TranslationServer.SetLocale(locale);
	}

	// Update UI text
	UpdateUIText();

	base._Ready();
	 Control settingsMenu = GetNode<Control>("Settings");
		if (settingsMenu != null) {
			settingsMenu.Visible = false; // Hide  alkuun
		} else {
			GD.PrintErr("Settings menu node not found!");
		}

	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}

	_pelaaButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPelaaPressed)));
	_asetuksetButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnAsetuksetPressed)));
	_poistuButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPoistuPressed)));
	_nettisivu.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnNettiSivuPressed)));
	// kuuntelee napin painallusta, laukasee metodin _pelaaButton.Pressed += OnNewGamePressed();
	// pitäs sit myös lopettaa, muute jää roskat latinkiin


}
}

private void ApplySettingsVolume() {
			ConfigFile config = new ConfigFile();
			string configPath = "user://settings.cfg";

			if (config.Load(configPath) == Error.Ok)
			{
				float volume = (float)(double)config.GetValue("Settings", "Volume", 1.0);  // Default 1.0 jos ei löydy
				float dbVolume = Mathf.LinearToDb(volume);


				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), dbVolume);
			}
}

private async void OnPelaaPressed()
{
	_audioPuu.Play();
	await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
	SceneTransition sceneTransition = GetNode<SceneTransition>("/root/SceneTransition");
	sceneTransition.FadeToScene("res://Level/LevelSelect1.tscn");
	GD.Print("Pelaa pressed"); // mennään levelselect 1
}


private void OnAsetuksetPressed()
{
	_audioPuu.Play();
	GD.Print("Asetukset pressed");
		 Control settingsMenu = GetNode<Control>("Settings"); // haetaa autoload settingsi
		if (settingsMenu != null) {
			settingsMenu.Visible = !settingsMenu.Visible; // näkyvii
		} else {
			GD.PrintErr("Settings menu node not found!");
		}
	}

private async void OnPoistuPressed()
{
	_audioPuu.Play();
	await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
	_mainMenuSceneTree.Quit(); // peli kii
}

private void OnNettiSivuPressed() {
string url = "https://webpages.tuni.fi/24tiko3f/index.html";
OS.ShellOpen(url); // cozycottage logosta linkki sivuille
}

public void UpdateUIText()
{
	_pelaaButton.Text = Tr("play");
	_asetuksetButton.Text = Tr("settings");
	_poistuButton.Text = Tr("quit");
}


}
