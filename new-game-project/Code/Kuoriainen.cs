using Godot;
using System;

namespace SieniPeli {
    public partial class Kuoriainen : Ötökkä {
        [Export] private float maxSpeedKuoriainen = 100f;
        [Export] private float kiihdytysKuoriainen = 60f;
        [Export] private float jarrutusKuoriainen = 80f;
		[Export] protected float minRollingSpeedKuoriainen = 60f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedKuoriainen;
            kiihdytysSpeed = kiihdytysKuoriainen;
            jarrutusSpeed = jarrutusKuoriainen;
        }
    }
}
