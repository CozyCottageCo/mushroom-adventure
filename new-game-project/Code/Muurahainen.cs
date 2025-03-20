using Godot;
using System;

namespace SieniPeli {
    public partial class Muurahainen : Ötökkä {
        [Export] private float maxSpeedMuurahainen = 100f;
        [Export] private float kiihdytysMuurahainen = 40f;
        [Export] private float jarrutusMuurahainen = 80f;
		[Export] protected float minRollingSpeedMuurahainen = 20f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedMuurahainen;
            kiihdytysSpeed = kiihdytysMuurahainen;
            jarrutusSpeed = jarrutusMuurahainen;
        }
    }
}
