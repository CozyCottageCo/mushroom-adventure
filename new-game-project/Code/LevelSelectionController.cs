using Godot;
using System;
using SieniPeli;

namespace SieniPeli
{
    public partial class LevelSelectionController : Control
    {
        [Export] private TextureButton _taso1Button = null;
        [Export] private TextureButton _taso2Button = null;
        [Export] private TextureButton _taso3Button = null;
        [Export] private TextureButton _taso4Button = null;

        [Export] private TextureButton _extraButton = null;
        [Export] private TextureButton _eteenpäinButton = null;
        [Export] private TextureButton _takaisinpäinButton = null;
		[Export] private TextureButton _mainMenuButton = null;
        [Export] private AudioStreamPlayer2D _nappi1 = null;
        [Export] private AudioStreamPlayer2D _nappi2 = null;
        [Export] private AudioStreamPlayer2D _nappi3 = null;
        [Export] private TextureRect _gameClearPanel = null;

        private SceneTree _levelSelectSceneTree = null;
        public string currentScreen = "";

        [Export] private Label TäpläCount; // tehtyjen tasojen täplien laskuri

        SaveManager saveManager = null; // savemanager alustus (autoload kyl)

        private Texture2D _sieniTäplä = (Texture2D)GD.Load("res://Art/Spritet/Käyttöliittymä/Levelselection/Levelselect_sienet_täplällä.png"); // valmiit texturet ladattuna käyttöö
        private Texture2D _sieniIlmanTäplä = (Texture2D)GD.Load("res://Art/Spritet/Käyttöliittymä/Levelselection/Levelselect_sienet_ilmantäplä.png");

        public override void _Ready()
        {

                var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
                musicPlayer.PlayMusicForCurrentScene();
                // Levelselect musalataus


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
          //  _extraButton.Pressed += () => OnButtonPressed(_extraButton);

            if (_eteenpäinButton != null)
                _eteenpäinButton.Pressed += () => OnButtonPressed(_eteenpäinButton);

            if (_takaisinpäinButton != null)
                _takaisinpäinButton.Pressed += () => OnButtonPressed(_takaisinpäinButton);

			_mainMenuButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(MainMenuPressed)));
            saveManager = GetNode<SaveManager>("/root/SaveManager"); // haetaa savemanager (autoloadi)
            CheckLevelButtonTextures(); // tarkistetaan aina levelscreeninauetessa mitkä auki
            TäpläCount.Text = saveManager.GetLevelsCompleted() + " / 18"; // päivitetää tehyt täplät

            _gameClearPanel.Visible = false;
            if (saveManager.GetLevelsCompleted() == 18) {
                _gameClearPanel.Visible = true;
            }
        }

        private async void OnButtonPressed(TextureButton button) // saa parametrinä sen tietyn napin nimen nääs
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
                _nappi1.Play();
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
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
                _nappi3.Play();
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
				currentScreen = this.Name; // katsotaan tämän scenen nimi
				string screenNumber = currentScreen.Replace("LevelSelect", ""); // kaikki paitsi nro pois (LevelSelect1) -> 1
				if (int.TryParse(screenNumber, out int thisScreenNumber)) { // intiksi
                	LoadSelectionScreen(thisScreenNumber + 1); // metodille tämän scenen nro +1
				}
            }
            else if (buttonName == "Takaisinpäin") // sama takaisinpäin (1. scenessä ei ole takaisinpäin nappia, ei ongelmaa)
            {
                _nappi3.Play();
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
				currentScreen = this.Name;
				string screenNumber = currentScreen.Replace("LevelSelect", "");
				if (int.TryParse(screenNumber, out int thisScreenNumber)) {
                	LoadSelectionScreen(thisScreenNumber - 1);
                    GD.Print(thisScreenNumber);
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

		private async void MainMenuPressed() { // tietysti main menuun takaisin
            _nappi2.Play();
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			string mainMenuPath = "res://Level/MainMenu.tscn";
			_levelSelectSceneTree.ChangeSceneToFile(mainMenuPath);
		}

        public string GetCurrentLevelSelect() {
            return currentScreen;
        }

       private void CheckLevelButtonTextures() // tarkistetaan mikä texture pitäs olla
{
        string currentLevelSelect= GetTree().CurrentScene.SceneFilePath;
        string currentLevelSelectName = currentLevelSelect.GetFile().GetBaseName(); // current level selection nimi

        if (currentLevelSelectName == "LevelSelect1")
        {
        bool level1Completed = saveManager.IsLevelCompleted("Level1");
        bool level2Completed = saveManager.IsLevelCompleted("Level2");
        bool level3Completed = saveManager.IsLevelCompleted("Level3");
        bool level4Completed = saveManager.IsLevelCompleted("Level4");

        // asetetaa texture sen mukaa onko true vai false savefilussa

        _taso1Button.TextureNormal = level1Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso1Button.Disabled = false;  // Onko nappi käytettävissä: eka taso on aina ofc
        _taso1Button.Modulate = new Color(1, 1, 1, 1); // eikä oo moduloitu hämyseks

        _taso2Button.TextureNormal = level2Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso2Button.Disabled = !level1Completed;  // Tarkistetaan onko aina aiempi taso tehty, disabloidaan nappi jos ei
        _taso2Button.Modulate = level1Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f); // ja moduloidaan hämyiseksi

        _taso3Button.TextureNormal = level3Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso3Button.Disabled = !level2Completed;
        _taso3Button.Modulate = level2Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso4Button.TextureNormal = level4Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso4Button.Disabled = !level3Completed;
        _taso4Button.Modulate = level3Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        if (level4Completed) {
            saveManager.SetLevelsDone(1); // kun level selectin screenin kaikki (ei secret) tasot tehty, asetetaan savemanageriin tehdyksi
        }
    }

    if (currentLevelSelectName == "LevelSelect2")
    {
        bool level5Completed = saveManager.IsLevelCompleted("Level5");
        bool level6Completed = saveManager.IsLevelCompleted("Level6");
        bool level7Completed = saveManager.IsLevelCompleted("Level7");
        bool level8Completed = saveManager.IsLevelCompleted("Level8");

        _taso1Button.TextureNormal = level5Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso1Button.Disabled = !saveManager.firstLevelsDone;  // Uuden level selectin screenin eka tarkistaa onko aiempi screeni asetettu tehdyksi
        _taso1Button.Modulate = saveManager.firstLevelsDone ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);


        _taso2Button.TextureNormal = level6Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso2Button.Disabled = !level5Completed;
        _taso2Button.Modulate = level5Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);


        _taso3Button.TextureNormal = level7Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso3Button.Disabled = !level6Completed;
        _taso3Button.Modulate = level6Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso4Button.TextureNormal = level8Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso4Button.Disabled = !level7Completed;
        _taso4Button.Modulate = level7Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        if (level8Completed) {
            saveManager.SetLevelsDone(2); // ja taas screen tehdyksi
        }
    }

    if (currentLevelSelectName == "LevelSelect3")
    {
        bool level9Completed = saveManager.IsLevelCompleted("Level9");
        bool level10Completed = saveManager.IsLevelCompleted("Level10");
        bool level11Completed = saveManager.IsLevelCompleted("Level11");
        bool level12Completed = saveManager.IsLevelCompleted("Level12");
        bool levelHuussiCompleted = saveManager.IsLevelCompleted("Levelhuussi"); //todo: näille oma viel. 17 ja 18?


        _taso1Button.TextureNormal = level9Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso1Button.Disabled = !saveManager.secondLevelsDone;
        _taso1Button.Modulate = saveManager.secondLevelsDone ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso2Button.TextureNormal = level10Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso2Button.Disabled = !level9Completed;
        _taso2Button.Modulate = level9Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso3Button.TextureNormal = level11Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso3Button.Disabled = !level10Completed;
        _taso3Button.Modulate = level10Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso4Button.TextureNormal = level12Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso4Button.Disabled = !level11Completed;
        _taso4Button.Modulate = level11Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        if (level12Completed) {
            saveManager.SetLevelsDone(3);
    }
    }

    if (currentLevelSelectName == "LevelSelect4")
    {
        bool level13Completed = saveManager.IsLevelCompleted("Level13");
        bool level14Completed = saveManager.IsLevelCompleted("Level14");
        bool level15Completed = saveManager.IsLevelCompleted("Level15");
        bool level16Completed = saveManager.IsLevelCompleted("Level16");
        bool levelMökkiCompleted = saveManager.IsLevelCompleted("Levelmökki"); // todo: oma viel


        _taso1Button.TextureNormal = level13Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso1Button.Disabled = !saveManager.thirdLevelsDone;
        _taso1Button.Modulate = saveManager.thirdLevelsDone ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso2Button.TextureNormal = level14Completed ? _sieniTäplä : _sieniIlmanTäplä;
        _taso2Button.Disabled = !level13Completed;
        _taso2Button.Modulate = level13Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso3Button.TextureNormal = level15Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso3Button.Disabled = !level14Completed;
        _taso3Button.Modulate = level14Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

        _taso4Button.TextureNormal = level16Completed ? _sieniTäplä : _sieniIlmanTäplä;
         _taso4Button.Disabled = !level15Completed;
        _taso4Button.Modulate = level15Completed ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);

    }
    }

    }
}
