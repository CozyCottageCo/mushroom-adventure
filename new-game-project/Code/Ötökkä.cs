using Godot;
using System;
using SieniPeli;

namespace SieniPeli {
    public partial class Ötökkä : PathFollow2D {
        [Export] protected float maxSpeed = 100f;
        [Export] protected float kiihdytysSpeed = 40f;
        [Export] protected float jarrutusSpeed = 80f;
        [Export] protected float minRollingSpeed = 20f;

        private CrossWalkManager CrossWalkManager;

        private float currentSpeed = 0f;
        protected bool isStopped = false;
        private bool isOnCrossWalk = false;
        private string sieniSuojaTie = "";
        private string detectedCrossWalk = "";

        private bool isBlocked = false;
        private string blockedBy = "";

        private float stopTime = 0f;
        private const float maxStopTime = 8f;

        public override void _Ready() {
            AddToGroup("Ötökkä");
            CrossWalkManager = GetNode<CrossWalkManager>("/root/Node2D/CrossWalkManager");
            currentSpeed = maxSpeed;

            var detectionAreaLong = GetNode<Area2D>("DetectionArea2DLong");
            var detectionAreaShort = GetNode<Area2D>("DetectionArea2DShort");
            var collisionArea = GetNode<Area2D>("CollisionArea2D");
            // Signal connections for area detection
            detectionAreaLong.AreaEntered += OnLongRangeEntered;
            detectionAreaLong.AreaExited += OnLongRangeExited;
            detectionAreaLong.BodyEntered += OnLongRangeEntered;
            detectionAreaLong.BodyExited += OnLongRangeExited;
            detectionAreaShort.AreaEntered += OnShortRangeEntered;
            detectionAreaShort.AreaExited += OnShortRangeExited;
            detectionAreaShort.BodyEntered += OnShortRangeEntered;
            detectionAreaShort.BodyExited += OnShortRangeExited;
            collisionArea.BodyEntered += OnCrossWalkEntered;
            collisionArea.BodyExited += OnCrossWalkExited;

        }

        public override void _Process(double delta) {
            if (isStopped || blockedBy != "") {
                stopTime += (float)delta;

                // If the car has been stopped for more than the max stop time, resume
                if (stopTime >= maxStopTime) {
                    GD.Print($"{this.Name} has been stopped for 10 seconds, resuming.");
                    blockedBy = "";
                    Resume();
                }
                return;
            }
            stopTime = 0f;
            Move((float)delta);
        }

        protected virtual void Move(float delta) {
            if (blockedBy != "") {
                return;
            }
            if (!isBlocked) {
                currentSpeed = Mathf.MoveToward(currentSpeed, maxSpeed, kiihdytysSpeed * delta);
            } else {
                currentSpeed = Mathf.MoveToward(currentSpeed, minRollingSpeed, jarrutusSpeed * delta);
            }
            Progress += currentSpeed * delta;
        }

        public virtual void Stop() {
            if (isOnCrossWalk) {
                return;
            }
            isStopped = true;
            currentSpeed = 0f;
            stopTime = 0f;
        }

        public virtual void Resume() {
            if (!isStopped) return; // Prevent unnecessary calls

           // GD.Print($"{this.Name} resumes moving.");
            isStopped = false;
            isBlocked = false; // <- Ensure movement can continue

            // Gradually accelerate back to normal speed
            if (currentSpeed < maxSpeed) {
                currentSpeed = Mathf.Max(currentSpeed, kiihdytysSpeed);
            }
        stopTime = 0f;
        }

        private void OnLongRangeEntered(Node body) {
            string areaName = body.Name;
            if (body is Area2D area)  // Ensure it's an Area2D (which would be another car or object)
            {
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    // Block the movement to simulate slowing down
                    isBlocked = true;
                }
            }
            else if (body is TileMapLayer tileMapLayer)
            {
                if (areaName.StartsWith("Suojatie"))
                {

                        detectedCrossWalk = areaName;
                        if(CrossWalkManager.CrossWalkOccupied &&
                        CrossWalkManager.CurrentCrossWalk == detectedCrossWalk) {
                            isBlocked = true;
                        }
                }

            }
        }

        private void OnLongRangeExited(Node body) {
            string areaName = body.Name;
            if (body is Area2D area) {
                // Ensure it's not the same as this instance's collision area
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {

                    // Unblock the movement to simulate resuming speed
                    isBlocked = false;
                }
            }
            else if (body is TileMapLayer tileMapLayer)
                {
                    if (areaName.StartsWith("Suojatie"))
                    {
                        isBlocked = false;
                        detectedCrossWalk = "";
                        CheckCrossWalkStatus();
                    }
                }
        }

        private void OnShortRangeEntered(Node body) {
            string areaName = body.Name;
            if (body is Area2D area)  // Ensure it's an Area2D (which would be another car or object)
            {
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    if (!isStopped) {
                    // Block the movement to simulate slowing down
                    blockedBy = body.GetInstanceId().ToString();
                    Stop();}
                    CheckCrossWalkStatus();
                }
            }
            else if (body is TileMapLayer tileMapLayer)
            {
                if (areaName.StartsWith("Suojatie"))
                {
                        detectedCrossWalk = areaName;
                        if(CrossWalkManager.CrossWalkOccupied &&
                        CrossWalkManager.CurrentCrossWalk == detectedCrossWalk) {
                            Stop();
                            CheckCrossWalkStatus();
                        }
                }

            }
        }

        private void OnShortRangeExited(Node body) {
            string areaName = body.Name;
            if (body is Area2D area) {
                // Ensure it's not the same as this instance's collision area
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    if (body.GetInstanceId().ToString() == blockedBy) {
                    blockedBy = "";
                    Resume();
                    }
                }
            }
            else if (body is TileMapLayer tileMapLayer)
                {
                    if (areaName.StartsWith("Suojatie"))
                    {
                        Resume();
                        detectedCrossWalk = "";
                        CheckCrossWalkStatus();
                    }
                }
        }

         private void OnCrossWalkEntered(Node body) {
            string areaName = body.Name;
            if (body is TileMapLayer tileMapLayer)
            {

              //  GD.Print($"{this.Name} on {areaName}");
                 if (!isOnCrossWalk)
                {
                    isOnCrossWalk = true; // Set the flag to true when entering a crosswalk
                    // Prevent stopping or slowing down when on crosswalk
                    Stop(); // Ensure the Toukka does not stop on the crosswalk
                }

            }
        }
        private void OnCrossWalkExited(Node Area2D) {
            if (isOnCrossWalk) {
                isOnCrossWalk = false;
              //int($"{this.Name} has left crosswalk");
                CheckCrossWalkStatus();
            }
        }

        private void CheckCrossWalkStatus()
        {
            sieniSuojaTie = CrossWalkManager.CurrentCrossWalk;

           if (!string.IsNullOrEmpty(detectedCrossWalk) && CrossWalkManager.CrossWalkOccupied &&
            sieniSuojaTie == detectedCrossWalk)
            {
                Stop();
               // GD.Print($"{this.Name} stops for crosswalk: {detectedCrossWalk}");
            }
            else
            {
                Resume();
            }
            }
    }
}
