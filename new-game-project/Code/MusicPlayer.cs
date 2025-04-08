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
            { "Autumn", "Autumn" }
        };

        private AudioStreamPlayer2D currentPlayer = null;
        private string currentSong = ""; // Träkkää current biisin (ettei bgmusic ala alusta mainmenu/level select välillä)

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
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

            GD.Print(sceneName + " current scene for music");

            // Verrataan dictionaryyn tallennettuihin mikä player lyödään päälle
            if (sceneMusicMap.TryGetValue(sceneName, out string playerName))
            {
                // Tarkistetaa ettei se yritä laittaa BG music uudelleen, skipataa jos niin
                if (playerName == "BackgroundMusic" && currentSong == "BackgroundMusic")
                {
                    GD.Print("BackgroundMusic already playing, no need to start again.");
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

        private string GetSeasonFromLevel(string sceneName)
        {
            if (sceneName.StartsWith("Level"))
            {
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

        public override void _Process(double delta)
        {
        }
    }
}
