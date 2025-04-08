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
        private string currentSong = ""; // This will track the current playing song

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            PlayMusicForCurrentScene();
        }

        public void PlayMusicForCurrentScene()
        {
            string scenePath = GetTree().CurrentScene.SceneFilePath;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            GD.Print(sceneName);

            if (sceneName.StartsWith("Level"))
            {
                sceneName = GetSeasonFromLevel(sceneName);
            }

            GD.Print(sceneName + " current scene for music");

            // Try to get the music associated with the scene
            if (sceneMusicMap.TryGetValue(sceneName, out string playerName))
            {
                // If BackgroundMusic is already playing, skip playing it again
                if (playerName == "BackgroundMusic" && currentSong == "BackgroundMusic")
                {
                    GD.Print("BackgroundMusic already playing, no need to start again.");
                    return; // Skip playing the music if it's already playing
                }

                // Stop the previous music if it's playing
                if (currentPlayer != null && currentPlayer.Playing)
                {
                    currentPlayer.Stop();
                }

                // Get the player node and start the music
                currentPlayer = GetNode<AudioStreamPlayer2D>(playerName);
                if (currentPlayer != null && !currentPlayer.Playing)
                {
                    currentPlayer.Play();
                    currentSong = playerName; // Track the current playing music
                }
            }
            else
            {
                currentPlayer = null; // No music for this scene
                currentSong = ""; // Clear the current track
            }
        }

        private string GetSeasonFromLevel(string sceneName)
        {
            if (sceneName.StartsWith("Level"))
            {
                // Extract the number from the scene name
                string numberPart = sceneName.Substring(5); // after "Level"
                if (int.TryParse(numberPart, out int levelNum))
                {
                    int seasonIndex = (levelNum - 1) / 4; // every 4 levels is a season
                    string[] seasons = { "Winter", "Spring", "Summer", "Autumn" };
                    return seasons[seasonIndex % seasons.Length];
                }
            }

            return sceneName; // If not a level scene, just return the name as-is
        }

        public override void _Process(double delta)
        {
        }
    }
}
