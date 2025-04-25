using Godot;
using System;
using SieniPeli;
using System.Collections.Generic;

namespace SieniPeli {
    public partial class MusicPlayer : Node2D
    {
        private Dictionary<string, string> sceneMusicMap = new Dictionary<string, string>
        {
            { "MainMenu", "BackgroundMusic" },
            { "LevelSelect1", "BackgroundMusic" },
            { "LevelSelect2", "BackgroundMusic" },
            { "LevelSelect3", "BackgroundMusic" },
            { "LevelSelect4", "BackgroundMusic" },
            { "Winter", "Winter" },
            { "Spring", "Spring" },
            { "Summer", "Summer" },
            { "Autumn", "Autumn" },
            { "Huussi", "Huussi" },
            { "Mökki", "Mökki" } // dictionarysta katotaan mikä skenenimi -> mikä audiostreamplayeri pääl
        };

        public AudioStreamPlayer2D currentPlayer = null;
        public string currentSong = ""; // Träkkää current biisin (ettei bgmusic ala alusta mainmenu/level select välillä)

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ApplySavedVolume(); // iha eka katotaa settings volume, muute starttaa max volumel
            PlayMusicForCurrentScene();
        }

        public void PlayMusicForCurrentScene()
        {
            string scenePath = GetTree().CurrentScene.SceneFilePath;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            GD.Print(sceneName); // katotaan skenen nimi

            if (sceneName.StartsWith("Level")) // jos alkaa Level, haetaa vuodenaika (toimii vaikka LevelSelect on asia)
            {
                sceneName = GetSeasonFromLevel(sceneName);
            }

            //GD.Print(sceneName + " current scene for music");

            // Verrataan dictionaryyn tallennettuihin mikä player lyödään päälle
            if (sceneMusicMap.TryGetValue(sceneName, out string playerName))
            {
                // Tarkistetaa ettei se yritä laittaa BG music uudelleen, skipataa jos niin
                if (playerName == "BackgroundMusic" && currentSong == "BackgroundMusic")
                {
                    //GD.Print("BackgroundMusic already playing, no need to start again.");
                    return; //skippp
                }

                // Vanha musa seis
                if (currentPlayer != null && currentPlayer.Playing)
                {
                    currentPlayer.Stop();
                }

                // Playeri päälle
                currentPlayer = GetNode<AudioStreamPlayer2D>(playerName);
                if (currentPlayer != null && !currentPlayer.Playing)
                {
                    currentPlayer.Play();
                    currentSong = playerName; // Ja träkätää se current biisi
                }
            }
            else
            {
                currentPlayer = null; // Nullin tilantees ei mitn
                currentSong = "";
            }
        }

        private string GetSeasonFromLevel(string sceneName) // katotaa mikä levelin musa pits olla
        {
            if (sceneName.StartsWith("Level")) // huussil ja mökil omat ku ei oo numeroitu
            {
                if (sceneName == "Levelhuussi") {
                    return "Huussi";
                }
                if (sceneName == "Levelmökki") {
                    return "Mökki";
                }
                // Katotaa mikä nro ekan 5 kirjaime jälkee; ei haittaa vaikka LevelSelect olemas
                string numberPart = sceneName.Substring(5);
                if (int.TryParse(numberPart, out int levelNum))
                {
                    int seasonIndex = (levelNum - 1) / 4; // Katotaa joka 4 levelin nro jälkee eri season jne
                    string[] seasons = { "Winter", "Spring", "Summer", "Autumn" };
                    return seasons[seasonIndex % seasons.Length];
                }
            }

            return sceneName;
        }

        private void ApplySavedVolume()
        {
            ConfigFile config = new ConfigFile();
            string configPath = "user://settings.cfg";

            if (config.Load(configPath) != Error.Ok)
            {
                GD.Print("Could not load settings for volume.");
                return;
            }

            float volume = (float)(double)config.GetValue("Settings", "Volume", 1.0);
            volume = Mathf.Clamp(volume, 0.0f, 1.0f); // Safety

            float dbValue = Mathf.LinearToDb(volume);
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), dbValue);

            GD.Print($"Applied volume from settings: {volume} ({dbValue} dB)");
        }


        public override void _Process(double delta)
        {
        }
    }
}
