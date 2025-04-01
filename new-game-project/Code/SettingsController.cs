using Godot;
using System;

namespace SieniPeli {

public partial class SettingsController : Control
{
	private string configPath = "user://settings.cfg";
	private ConfigFile config = new ConfigFile();

	[Export] private OptionButton _languageOption = null;
	[Export] private HSlider _volumeSlider = null;
	[Export] private CheckButton _movementToggle = null;
	[Export] private Button _backButton = null;

	[Export] private Button _resetProgress = null;
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
		// Yritetetään config filen latausta
		if (config.Load(configPath) != Error.Ok)
		{
			GD.Print("No settings file found, creating a new one...");

			// Jos config filua ei löydy luodaan uusi default arvoilla
			config.SetValue("Settings", "Language", 0);
			config.SetValue("Settings", "Volume", 1.0);
			config.SetValue("Settings", "AlternativeMovement", false);

			// tallennetaan congif file
			config.Save(configPath);
		}

		// Ladataan arvot congif filulta
		_languageOption.Selected = (int)config.GetValue("Settings", "Language", 0);
		_volumeSlider.Value = (float)(double)config.GetValue("Settings", "Volume", 1.0);
		_movementToggle.ButtonPressed = (bool)config.GetValue("Settings", "AlternativeMovement", false);

		// Volume muuttuu heti
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb((float)_volumeSlider.Value));
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
		SaveSettings();
		GD.Print("Language changed to " + index);
		// Tähänn pitäisi keksiä kielenvaihto tsydeemi
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
}
}