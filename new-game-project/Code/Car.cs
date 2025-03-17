/**using Godot;
using System;

namespace SieniPeli {
    public partial class Car : PathFollow2D
    {
        // Base movement variables (Set in child classes)
        protected float MaxSpeed;
        protected float Acceleration;
        protected float Deceleration;

        [Export] public float MaxSpeed = 100f;
        [Export] public float MinSpeed = 10f;
        [Export] public float Acceleration = 50f;
        [Export] public float Deceleration = 70f;
        [Export] public float StopDistance = 40f;

        private float currentSpeed = 0f;
        private float targetSpeed = 0f;
        private bool shouldStop = false;
        private bool shouldSlowDown = false;

        private bool isWaiting = false;
        private bool isApproachingCrosswalk = false;

        public override void _Ready()
        {
            currentSpeed = MaxSpeed;
            targetSpeed = MaxSpeed;
            SetupSieniListener();
        }

        public override void _Process(double delta)
        {
            UpdateSpeed(delta);
            MoveCar(delta);
        }

        // Setup listener for when the player (Sieni) stops at the crosswalk
        protected virtual void SetupSieniListener()
        {
            var sieni = GetNode<Sieni>("/root/Node2D/Sieni");
            sieni.PysähtynytSuojaTielle += OnSieniStoppedAtCrosswalk;
        }

        // Update the speed based on the situation
        protected virtual void UpdateSpeed(double delta)
        {
            // Determine the target speed based on conditions
            if (shouldStop)
            {
                targetSpeed = 0f;  // Stop if we need to
            }
            else if (shouldSlowDown)
            {
                targetSpeed = MinSpeed; // Slow down to a minimum speed if needed
            }
            else
            {
                targetSpeed = MaxSpeed;  // Default maximum speed
            }

            // Smoothly accelerate or decelerate to target speed
            if (currentSpeed < targetSpeed)
            {
                currentSpeed = Mathf.Min(currentSpeed + Acceleration * (float)delta, targetSpeed);
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed = Mathf.Max(currentSpeed - Deceleration * (float)delta, targetSpeed);
            }
        }

        // Move the car based on current speed
        protected virtual void MoveCar(double delta)
        {
            Progress += currentSpeed * (float)delta;

            // If the car reaches a point where it is too close to stop, slow down more
            if (currentSpeed <= 0f && !shouldStop)
            {
                // If we stop for some reason, wait before resuming normal speed
                currentSpeed = 0f;
            }
        }

        // Handle detection area logic: This can be for crosswalk detection or other cars
        protected virtual void OnDetectionAreaEntered(Node body)
        {
            if (body is TileMap)  // Crosswalk logic
            {
                shouldSlowDown = true;

                if (Sieni.onSuojaTiellä)  // If the player is on the crosswalk
                {
                    shouldStop = true;  // Stop the car if the player is blocking
                }
            }
            else if (body is Car)  // Logic for colliding with another car
            {
                shouldSlowDown = true;
            }
        }

        // Handle when leaving the detection area, resume normal speed if possible
        protected virtual void OnDetectionAreaExited(Node body)
        {
            if (body is TileMap || body is Car)
            {
                shouldSlowDown = false;
                shouldStop = Sieni.onSuojaTiellä;  // Stop only if the player is still at the crosswalk
            }
        }

        // Handle collision with other cars: If they get too close, slow down
        protected virtual void OnCollisionAreaEntered(Node body)
        {
            if (body is Car)  // Colliding with another car
            {
                shouldSlowDown = true; // Gradually slow down to avoid collision
            }
        }

        // Handle when the player (Sieni) stops or leaves the crosswalk
        protected virtual void OnSieniStoppedAtCrosswalk(object sender, string suojatienimi)
        {
            shouldStop = !string.IsNullOrEmpty(suojatienimi);  // Stop if the player is at the crosswalk
        }
    }
}
*/