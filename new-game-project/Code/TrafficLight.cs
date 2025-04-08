using Godot;
using System;
using System.Collections.Generic;

namespace SieniPeli {
    public partial class TrafficLight : Node2D
    {
        [Export] public string CrosswalkName = "";

        private Timer toggleTimer;
        private bool isRed = false;

        private List<PointLight2D> lights;

        public override void _Ready()
        {
            // Initialize and start timer
            toggleTimer = new Timer();
            AddChild(toggleTimer);
            toggleTimer.WaitTime = 6.0f;
            toggleTimer.OneShot = false;
            toggleTimer.Timeout += OnToggleTimerTimeout;
            toggleTimer.Start();

            // Find all PointLight2D children
            lights = GetChildrenLights();
            GD.Print($"[TrafficLight:{Name}] Found {lights.Count} light(s).");

            UpdateLightColors(); // Set initial color
            PlayTrafficLightAnimations();
        }

         private void PlayTrafficLightAnimations()
        {
            foreach (var child in GetChildren())
            {
                if (child is AnimatedSprite2D animatedSprite)
                {
                    // Ensure the animation exists
                    if (!string.IsNullOrEmpty(animatedSprite.Animation))
                    {
                        // Play animation
                        animatedSprite.Play();
                    }
                    else
                    {
                        GD.Print("Error: No animation");
                    }
                }
            }
        }

        private void OnToggleTimerTimeout()
        {
            isRed = !isRed;
            GD.Print($"Traffic light at {CrosswalkName} is green for pedestrians: {isRed}");

            UpdateLightColors();

            var manager = GetNodeOrNull<CrossWalkManager>("/root/Node2D/CrossWalkManager");
            if (manager != null)
            {
                if (isRed)
                    manager.Call("OnSieniStoppedAtCrosswalk", CrosswalkName, true);
                else
                    manager.Call("OnSieniLeftCrossWalk", CrosswalkName, false);
            }
        }

        private void UpdateLightColors()
        {
            Color targetColor = isRed ? Colors.Red : Colors.Green;
            foreach (var light in lights)
            {
                light.Color = targetColor;
            }
        }

        private List<PointLight2D> GetChildrenLights()
        {
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
