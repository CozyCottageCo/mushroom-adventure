using Godot;
using System;
using System.Collections.Generic;

namespace SieniPeli
{
    public partial class TrafficLight : Node2D
    {

        [Export] public CollisionShape2D valoTie; // CollisionShape2D
        [Export] public CollisionShape2D punanenTie;

        private Timer toggleTimer;
        private bool isRed = false;

        private List<PointLight2D> lights; // lista valoista

        public override void _Ready()
        {
            // timer luonti ja tulille
            toggleTimer = new Timer();
            AddChild(toggleTimer);
            toggleTimer.WaitTime = 6.0f; // 6 sekuntia per sykli
            toggleTimer.OneShot = false;
            toggleTimer.Timeout += OnToggleTimerTimeout;
            toggleTimer.Start();

            // Murkut "näkee" suojatien ku !isRed
            SetCrosswalkVisibility(isRed);

            lights = GetChildrenLights(); // liikennevalojen valot
            GD.Print($"[TrafficLight:{Name}] Found {lights.Count} light(s).");

            UpdateLightColors(); // alkuväri valoille
            PlayTrafficLightAnimations(); // liikennevalojen animaatio päälle
        }

        private void OnToggleTimerTimeout()
        {
            // valo red / green
            isRed = !isRed;

            GD.Print($"Traffic lights are {(isRed ? "red" : "green")}");

            // Vaihetaan collisionshape2d = disabled isRed mukaan
            SetCrosswalkVisibility(isRed);
            UpdateLightColors(); // ja valot vaihtuu
        }

        private void SetCrosswalkVisibility(bool isRed)
        {
            if (valoTie != null && punanenTie != null)
            {
                // Jos valo = punanen, suojatie disabled ja toisinpäin; eli vihree valo SIENELLE
                valoTie.Disabled = isRed;
                punanenTie.Disabled = !isRed;
            }
            else
            {
                GD.Print("No CollisionShape2D found");
            }
        }

         private void UpdateLightColors() // valojen vaihtaminen
        {
              Color targetColor = isRed ? Colors.Red : Colors.Green;
            GD.Print($"Setting color to: {targetColor}");

            foreach (var light in lights)
            {
                if (light != null)
                {
                    GD.Print($"Updating color to: {targetColor}");
                    light.Color = targetColor;
                }
            }
        }

        private void PlayTrafficLightAnimations() // iha vaa animatedsprit2d päälle
        {
            foreach (var child in GetChildren())
            {
                if (child is AnimatedSprite2D animatedSprite)
                {
                    // Ensure the animation exists and play it
                    if (!string.IsNullOrEmpty(animatedSprite.Animation))
                    {
                        animatedSprite.Play();
                    }
                    else
                    {
                        GD.Print("Error: No animation");
                    }
                }
            }
        }

         private List<PointLight2D> GetChildrenLights()
        { // haetaa valot nodelistasta
            var foundLights = new List<PointLight2D>();

            // Loop through all the children
            foreach (var child in GetChildren())
            {
                if (child is AnimatedSprite2D animatedSprite)
                {
                    // Find PointLight2D children inside each AnimatedSprite2D node
                    foreach (var subChild in animatedSprite.GetChildren())
                    {
                        if (subChild is PointLight2D pointLight)
                        {
                            foundLights.Add(pointLight);
                        }
                    }
                }
            }

            return foundLights;
        }
    }
}
