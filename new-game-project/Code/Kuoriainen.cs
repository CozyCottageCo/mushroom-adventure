using Godot;
using System;

namespace SieniPeli {
    public partial class Kuoriainen : Ötökkä {
        [Export] private float maxSpeedKuoriainen = 100f;
        [Export] private float kiihdytysKuoriainen = 60f;
        [Export] private float jarrutusKuoriainen = 80f;
		[Export] protected float minRollingSpeedKuoriainen = 60f;

        [Export] PointLight2D Red = null;

        [Export] PointLight2D Blue = null;

        private float lightToggle = 0.2f;

        private float lightTimer = 0f;

        private bool redOn = false;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedKuoriainen;
            kiihdytysSpeed = kiihdytysKuoriainen;
            jarrutusSpeed = jarrutusKuoriainen;

            UpdateLights();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

             lightTimer += (float)delta;

            if (lightTimer >= lightToggle) {
                lightTimer = 0f;
                redOn = !redOn;
                UpdateLights();
            }
        }

        private void UpdateLights() {
            if (Red != null) Red.Visible = redOn;
            if (Blue != null) Blue.Visible = !redOn;
        }
    }
}
