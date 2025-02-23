using Godot;
using System;

namespace SieniPeli
{
    public partial class LevelSelectionController : Control
    {
        [Export] private TextureButton _taso1Button = null;
        [Export] private TextureButton _taso2Button = null;
        [Export] private TextureButton _taso3Button = null;
        [Export] private TextureButton _taso4Button = null;
        [Export] private TextureButton _eteenpäinButton = null;
        [Export] private TextureButton _takaisinpäinButton = null;
		[Export] private TextureButton _mainMenuButton = null;

        private SceneTree _levelSelectSceneTree = null;
        string currentScreen = "";

        public override void _Ready()
        {
            base._Ready();
            _levelSelectSceneTree = GetTree(); // ladataan scenetree
            if (_levelSelectSceneTree == null)
            {
                GD.PrintErr("Ei löydy scenetree");
            }

            // lambdafunktiolla painetun napin "muuttuja" viittaus metodeille (hankala konsepti mut toimii :D)
            _taso1Button.Pressed += () => OnButtonPressed(_taso1Button);
            _taso2Button.Pressed += () => OnButtonPressed(_taso2Button);
            _taso3Button.Pressed += () => OnButtonPressed(_taso3Button);
            _taso4Button.Pressed += () => OnButtonPressed(_taso4Button);

            if (_eteenpäinButton != null)
                _eteenpäinButton.Pressed += () => OnButtonPressed(_eteenpäinButton);

            if (_takaisinpäinButton != null)
                _takaisinpäinButton.Pressed += () => OnButtonPressed(_takaisinpäinButton);

			_mainMenuButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(MainMenuPressed)));
        }

        private void OnButtonPressed(TextureButton button) // saa parametrinä sen tietyn napin nimen nääs
        {
            if (button == null)
            {
                GD.PrintErr("No button found");
                return;
            }

            string buttonName = button.Name;
            GD.Print($"Button {buttonName} pressed");

            if (buttonName.StartsWith("Taso")) // tarkistetaan mitä nappia painettu
            {
                string chosenLevel = buttonName.Replace("Taso", ""); //otetaan kaikki paitsi numero pois
                if (int.TryParse(chosenLevel, out int levelNumber)) // muutetaan intiksi
                {
                    LoadLevel(levelNumber); //levelin lataus metodille levelin numero
                }
                else
                {
                    GD.PrintErr($"No level {chosenLevel} found"); // jos joku kusee ni error
                }
            }
            else if (buttonName == "Eteenpäin") // jos nappi on eteenpäin
            {
				currentScreen = this.Name; // katsotaan tämän scenen nimi
				string screenNumber = currentScreen.Replace("LevelSelect", ""); // kaikki paitsi nro pois (LevelSelect1) -> 1
				if (int.TryParse(screenNumber, out int thisScreenNumber)) { // intiksi
                	LoadSelectionScreen(thisScreenNumber + 1); // metodille tämän scenen nro +1
				}
            }
            else if (buttonName == "Takaisinpäin") // sama takaisinpäin (1. scenessä ei ole takaisinpäin nappia, ei ongelmaa)
            {
				currentScreen = this.Name;
				string screenNumber = currentScreen.Replace("LevelSelect", "");
				if (int.TryParse(screenNumber, out int thisScreenNumber)) {
                	LoadSelectionScreen(thisScreenNumber - 1);
				}
                LoadSelectionScreen(thisScreenNumber - 1);
            }
        }

        private void LoadLevel(int levelNumber) // levelin latausmetodiin annettu nro
        {
            string levelPath = $"res://Level/Level{levelNumber}.tscn"; // scenen path vaihtuu sen mukaan
            GD.Print($"Loading level {levelNumber}"); // debug viestiä
            _levelSelectSceneTree.ChangeSceneToFile(levelPath); // ja vaihetaan scene
        }

        private void LoadSelectionScreen(int screen) // levelselectionin latausmetodi, nro parametrinä
        {
            string levelSelectPath = $"res://Level/LevelSelect{screen}.tscn"; // vaihtaa level selectionin reitin nro mukaan
            GD.Print($"Going to level select screen {screen}"); // debuggia
            _levelSelectSceneTree.ChangeSceneToFile(levelSelectPath); // ja skene vaihtuu
        }

		private void MainMenuPressed() { // tietysti main menuun takaisin
			string mainMenuPath = "res://Level/MainMenu.tscn";
			_levelSelectSceneTree.ChangeSceneToFile(mainMenuPath);
		}
    }
}
