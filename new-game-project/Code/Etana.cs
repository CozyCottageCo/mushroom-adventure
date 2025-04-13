using Godot;
using System;

namespace SieniPeli {
    public partial class Etana : Ötökkä {
        [Export] private float maxSpeedEtana = 80f;
        [Export] private float kiihdytysEtana = 30f;
        [Export] private float jarrutusEtana = 50f;
		[Export] protected float minRollingSpeedEtana = 10f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedEtana;
            kiihdytysSpeed = kiihdytysEtana;
            jarrutusSpeed = jarrutusEtana;
        }
    }
}
