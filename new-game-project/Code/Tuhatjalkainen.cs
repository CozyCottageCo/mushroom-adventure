using Godot;
using System;

namespace SieniPeli {
    public partial class Tuhatjalkainen: Ötökkä {
        [Export] private float maxSpeedTuhatjalkainen= 60f;
        [Export] private float kiihdytysTuhatjalkainen= 60f;
        [Export] private float jarrutusTuhatjalkainen= 30f;
		[Export] protected float minRollingSpeedTuhatjalkainen= 20f;

        public override void _Ready() {
			AddToGroup("Ötökkä");
            base._Ready();
            maxSpeed = maxSpeedTuhatjalkainen;
            kiihdytysSpeed = kiihdytysTuhatjalkainen;
            jarrutusSpeed = jarrutusTuhatjalkainen;
        }
    }
}
