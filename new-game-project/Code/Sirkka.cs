using Godot;
using System;

namespace SieniPeli {
    public partial class Sirkka : Ötökkä {
        [Export] private float maxSpeedSirkka = 120f;
        [Export] private float kiihdytysSirkka = 60f;
        [Export] private float jarrutusSirkka = 80f;
		[Export] protected float minRollingSpeedSirkka = 40f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedSirkka;
            kiihdytysSpeed = kiihdytysSirkka;
            jarrutusSpeed = jarrutusSirkka;
        }
    }
}
