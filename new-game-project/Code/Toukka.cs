using Godot;
using System;

namespace SieniPeli {
    public partial class Toukka : Ötökkä {
        [Export] private float maxSpeedToukka = 80f;
        [Export] private float kiihdytysToukka = 30f;
        [Export] private float jarrutusToukka = 40f;
		[Export] protected float minRollingSpeedToukka = 20f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedToukka;
            kiihdytysSpeed = kiihdytysToukka;
            jarrutusSpeed = jarrutusToukka;
        }
    }
}
