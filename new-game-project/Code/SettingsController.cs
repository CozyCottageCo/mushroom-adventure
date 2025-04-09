using Godot;
using System;

namespace SieniPeli {

public partial class SettingsController : Control
{
	private string configPath = "user://settings.cfg";
	private ConfigFile config = new ConfigFile();
	[Export] private OptionButton _languageOption = null;
	[Export] private HSlider _volumeSlider = null;
	[Export] private CheckButton _movementToggle = null; // toggle on/off
	[Export] private Button _backButton = null;
	[Export] private Button _resetProgress = null;

	[Export] private Button _olenVarma = null;

	[Export] private Button _enOleVarma = null;
	[Export] private Label _volumeLabel = null;
	[Export] private Label _movementLabel = null;
	[Export] private Label _languageLabel = null;
	[Export] private Label _settingsLabel = null;

	[Export] TextureRect confirmPanel = null;

	[Export] TextureButton _openTutorial = null;
	[Export] TextureRect _tutorialPanel = null;
	[Export] Button _tutorial1 = null;
	[Export] Button _tutorial2 = null;
	[Export] Button _tutorial3 = null;
	[Export] Button _closeTutorial = null;

	[Export] AudioStreamPlayer2D _nappi1 = null;
	[Export] AudioStreamPlayer2D _nappi2 = null;
	[Export] AudioStreamPlayer2D _nappi3 = null;

	public override void _Ready()
	{
		LoadSettings();
		confirmPanel.Visible = false;
		_tutorialPanel.Visible = false;
		Node currentScene = GetTree().CurrentScene;



		// Connect signals
		_languageOption.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(OnLanguageChanged)));
		_volumeSlider.Connect(HSlider.SignalName.ValueChanged, new Callable(this, nameof(OnVolumeChanged)));
		_movementToggle.Connect(CheckButton.SignalName.Toggled, new Callable(this, nameof(OnMovementToggled)));
		_backButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnBackPressed)));
		_resetProgress.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnResetPressed)));
		_olenVarma.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnResetConfirmPressed)));
		_enOleVarma.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnResetCanceled)));
		_openTutorial.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTutorialOpenPressed)));
		_tutorial1.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTutorial1Pressed)));
		_tutorial2.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTutorial2Pressed)));
		_tutorial3.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTutorial3Pressed)));
		_closeTutorial.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnTutorialClosePressed)));

	}

	private void LoadSettings()
	{
		if (config.Load(configPath) != Error.Ok)
		{
			GD.Print("No settings file found, creating a new one...");
			config.SetValue("Settings", "Language", 0);
			config.SetValue("Settings", "Volume", 1.0);
			config.SetValue("Settings", "AlternativeMovement", false);
			config.Save(configPath);
		}

		// Load saved language
		int savedLanguage = (int)config.GetValue("Settings", "Language", 0);
		_languageOption.Selected = savedLanguage;
		TranslationServer.SetLocale(savedLanguage == 0 ? "en" : "fi");

		// Load other settings
		_volumeSlider.Value = (float)(double)config.GetValue("Settings", "Volume", 1.0);
		_movementToggle.ButtonPressed = (bool)config.GetValue("Settings", "AlternativeMovement", false);

		// Update UI text
		UpdateUIText();
	}


	private void SaveSettings()
	{
		config.SetValue("Settings", "Language", _languageOption.Selected);
		config.SetValue("Settings", "Volume", _volumeSlider.Value);
		config.SetValue("Settings", "AlternativeMovement", _movementToggle.ButtonPressed);

		config.Save(configPath);
		GD.Print("Settings saved to " + configPath);
	}

	private void OnLanguageChanged(int index)
	{
		_nappi1.Play();
		string selectedLanguage = index == 0 ? "en" : "fi";

		// Change the locale
		TranslationServer.SetLocale(selectedLanguage);

		// Save the language setting
		config.SetValue("Settings", "Language", index);
		config.Save(configPath);

		GD.Print("Language changed to: " + selectedLanguage);
		SaveSettings();
	}



	private void OnVolumeChanged(double value)
	{
		_nappi1.Play();
		float volume = (float)value;  // Ensure it's a float
		// Clamp the volume to a range of 0.0 to 1.0 to prevent clipping
		volume = Mathf.Clamp(volume, 0.0f, 1.0f);
		SaveSettings();
		GD.Print("Volume changed to: " + volume);

		// Set volume in dB with linear-to-dB conversion
		float dbValue = Mathf.LinearToDb(volume);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), dbValue);
	}

	private void OnMovementToggled(bool toggled)
	{
		if (toggled) {
			_nappi2.Play();
		}
		else if (!toggled) {
			_nappi3.Play();
		}
		//await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		SaveSettings();
		GD.Print("Movement mode set to: " + toggled);
		// tähän pitää yhdistää sienen pysähtymismetodi
	}

	private async void OnBackPressed()
	{
		_nappi3.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		LoadSettings();
		_tutorialPanel.Visible = false;
		this.Visible = false; // Hide the settings menu instead of changing the scene
    GD.Print("Settings menu hidden.");
	}

	private void OnResetPressed() { // mahdollisuus resetoida tallennus (ainakin pelikehityksen ajaksi kiva olla)
		if (confirmPanel != null) {
			_nappi1.Play();
			confirmPanel.Visible = true;
		}
	}

	private async void OnResetConfirmPressed() {
		_nappi2.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager");
		saveManager.Reset();
		confirmPanel.Visible = false;


		 Node currentScene = GetTree().CurrentScene;
		string scenePath = currentScene.SceneFilePath;
		string sceneName = scenePath.GetFile().Replace(".tscn", "");
		GD.Print("Current scene name: " + sceneName);

        if (sceneName.StartsWith("Level"))
    {
        GD.Print("Level scene detected, switching to the main menu.");
        // Change to the main menu scene
		GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Level/MainMenu.tscn");

    }
	}

	private void OnResetCanceled() {
		_nappi3.Play();
		confirmPanel.Visible = false;
	}

	private void UpdateUIText()
	{
		_backButton.Text = Tr("back");
		_resetProgress.Text = Tr("reset");
		_volumeLabel.Text = Tr("volume");
		_movementLabel.Text = Tr("movement");
		_languageLabel.Text = Tr("language");
		_settingsLabel.Text = Tr("settings");
	}

	public bool GetMovementToggle() { // tarkistetaan mikä asetuksissa on tallennettu movement toggle likkkumiselle

		 if (config.Load(configPath) != Error.Ok)
            {
                GD.Print("Settings file not found or failed to load.");
                return false; // Default false jos ei lataa (tää taitaa itteasias bugittaa mut toimii just oikein :D)
            }

            // katotaa value
            bool isMovementToggled = (bool)config.GetValue("Settings", "AlternativeMovement", false);
            GD.Print($"Movement toggle state: {isMovementToggled}");

            return isMovementToggled; // palautetaa liikkeelle
	}

	private void OnTutorialOpenPressed() {
		_nappi1.Play();
		_tutorialPanel.Visible = !_tutorialPanel.Visible;
	}
	private async void OnTutorialClosePressed() {
		_nappi3.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		_tutorialPanel.Visible = false;
	}

	private async void OnTutorial1Pressed() {
		_nappi2.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		var tutorial = GetNode<TutorialScene>("TutorialScene");
            tutorial.TutorialActivated(1);
	}
	private async void OnTutorial2Pressed() {
		_nappi2.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		var tutorial = GetNode<TutorialScene>("TutorialScene");
            tutorial.TutorialActivated(2);
	}
	private async void OnTutorial3Pressed() {
		_nappi2.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		var tutorial = GetNode<TutorialScene>("TutorialScene");
            tutorial.TutorialActivated(3);
	}

}
}
