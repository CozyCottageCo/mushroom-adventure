using Godot;
using System;
namespace SieniPeli;
public partial class MainMenuController : Control
{
[Export] private Button _pelaaButton = null;
[Export] private Button _asetuksetButton = null;
[Export]private Button _poistuButton = null;

private SceneTree _mainMenuSceneTree = null;

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

	_pelaaButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPelaaPressed)));
	_asetuksetButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnAsetuksetPressed)));
	_poistuButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnPoistuPressed)));
	// kuuntelee napin painallusta, laukasee metodin _pelaaButton.Pressed += OnNewGamePressed();
	// pitäs sit myös lopettaa, muute jää roskat latinkiin
}
}

private void OnPelaaPressed()
{
	_mainMenuSceneTree.ChangeSceneToFile("res://Level/LevelSelect1.tscn");
	GD.Print("Pelaa pressed");
}

private void OnKokoelmaPressed()
{
	GD.Print("Kokoelma pressed");
}

private void OnAsetuksetPressed()
{
	_mainMenuSceneTree.ChangeSceneToFile("res://Level/Settings.tscn");
	GD.Print("Asetukset opened");
}

private void OnPoistuPressed()
{
	GD.Print("Nytlähettäis pressed");
	_mainMenuSceneTree.Quit();
}

private void UpdateUIText()
{
    _pelaaButton.Text = Tr("play");
    _asetuksetButton.Text = Tr("settings");
    _poistuButton.Text = Tr("quit");
}


}
