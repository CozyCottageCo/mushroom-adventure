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
	[Export] private Label _volumeLabel = null;
	[Export] private Label _movementLabel = null;
	[Export] private Label _languageLabel = null;
	[Export] private Label _settingsLabel = null;

	public override void _Ready()
	{
		LoadSettings();

		// Connect signals
		_languageOption.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(OnLanguageChanged)));
		_volumeSlider.Connect(HSlider.SignalName.ValueChanged, new Callable(this, nameof(OnVolumeChanged)));
		_movementToggle.Connect(CheckButton.SignalName.Toggled, new Callable(this, nameof(OnMovementToggled)));
		_backButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnBackPressed)));
		_resetProgress.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnResetPressed)));
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
		string selectedLanguage = index == 0 ? "en" : "fi";

		// Change the locale
		TranslationServer.SetLocale(selectedLanguage);

		// Save the language setting
		config.SetValue("Settings", "Language", index);
		config.Save(configPath);

		GD.Print("Language changed to: " + selectedLanguage);
	}



	private void OnVolumeChanged(double value)
	{
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
		SaveSettings();
		GD.Print("Movement mode set to: " + toggled);
		// tähän pitää yhdistää sienen pysähtymismetodi
	}

	private void OnBackPressed()
	{
		GetTree().ChangeSceneToFile("res://Level/MainMenu.tscn");
	}

	private void OnResetPressed() { // mahdollisuus resetoida tallennus (ainakin pelikehityksen ajaksi kiva olla)
		SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager");
		saveManager.Reset();
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

}
}
