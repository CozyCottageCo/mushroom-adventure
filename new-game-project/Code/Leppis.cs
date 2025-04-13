using Godot;
using System;

namespace SieniPeli {
    public partial class Leppis: Ötökkä {
        [Export] private float maxSpeedLeppis= 40f;
        [Export] private float kiihdytysLeppis= 20f;
        [Export] private float jarrutusLeppis= 30f;
		[Export] protected float minRollingSpeedLeppis= 10f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedLeppis;
            kiihdytysSpeed = kiihdytysLeppis;
            jarrutusSpeed = jarrutusLeppis;
        }
    }
}
