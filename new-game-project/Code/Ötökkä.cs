
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
        private bool alreadyBlocked = false;

        private float blockedBySpeed = 0f;

        private float stopTime = 0f;
        private const float maxStopTime = 8f;

        private Vector2 _direction;
        private Vector2 _initialDirection;
        private Vector2 _initialDirectionSaved;
        private Vector2 previousPosition;
        private Path2D path = null;

        private Vector2 firstPoint;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private bool isTurning = false;
        private bool printed = false;

        public override void _Ready() {
            AddToGroup("Ötökkä");
            CrossWalkManager = GetNode<CrossWalkManager>("/root/Node2D/CrossWalkManager");
            currentSpeed = maxSpeed;
            previousPosition = Position;

            path = GetParent<Path2D>();
            startPosition = path.Curve.GetPointPosition(0);
            firstPoint = path.Curve.GetPointPosition(1);
            endPosition = path.Curve.GetPointPosition(2);
            isTurning = IsTurning(startPosition, firstPoint, endPosition);
            if (isTurning) {
                GD.Print($"{this.Name} is turning");
            }

            SetInitialDirection(firstPoint, startPosition);
            _direction = _initialDirection;
            _initialDirectionSaved = _initialDirection;

             GD.Print("Initial Direction: ", _initialDirection);

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


            if (Mathf.Abs(ProgressRatio) > 0.95f) {
                blockedBy = "";
                }

         if (MathF.Abs(ProgressRatio) > 0.001f) {
            _direction = _initialDirectionSaved;
            printed = false;
        }
        if (MathF.Abs(ProgressRatio) > 0.01f) {

            _direction = SetDirection(Position, previousPosition);
        }

        if ((this.Name == "Ylös" || this.Name == "Ylösoikea") && !printed){
       // GD.Print(_direction);
        printed = true;
        }





    // Flip the sprite vertically if the path moves down (negative y _direction)
GetNode<Sprite2D>("Sprite2D").FlipV = _direction.Y > 0;
GetNode<Sprite2D>("Sprite2D").FlipV = _direction.X < 0;

         previousPosition = Position;

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
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        if (IsSame_direction(other_direction) || ShouldGiveWay(other_direction, otherTurning)) {
                            isBlocked = true;
                            blockedBySpeed = otherÖtökkä.GetSpeed();
                        }
                    }
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
                if (blockedBy != "") {
                if (body.GetInstanceId().ToString() != blockedBy || alreadyBlocked) {
                    GD.Print($"{body.GetInstanceId().ToString()} isnt {blockedBy}");
                return;
                }
            }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        if (IsSame_direction(other_direction) || ShouldGiveWay(other_direction, otherTurning)) {
                            GD.Print($"{this.Name} blocked by{parentNode.Name}");
                            if (String.IsNullOrEmpty(blockedBy)) {}
                            GD.Print($"Somehow empty {blockedBy}");
                            blockedBy = body.GetInstanceId().ToString();
                            alreadyBlocked = true;
                            Stop();
                           // GD.Print($"Direction {GetDirectionAsString(_direction)} for {GetDirectionAsString(other_direction)}");
                        }
                    }

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

                 if (blockedBy != "") {
                if (body.GetInstanceId().ToString() != blockedBy) {
                    GD.Print($"{body.GetInstanceId().ToString()} isnt {blockedBy}");
                return;
                }

            }
                // Ensure it's not the same as this instance's collision area
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    if (body.GetInstanceId().ToString() == blockedBy) {

                        GD.Print($"{this.Name} Stopped being blocked by {parentNode.Name}");
                    blockedBy = "";
                    alreadyBlocked = false;
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


            public Vector2 GetDirection() {
                return _direction;
            }
            public bool GetTurning() {
                return isTurning;
            }

            public float GetSpeed() {
                return currentSpeed;
            }
            private bool IsSame_direction(Vector2 other_direction) {
                float dotProduct = _direction.Dot(other_direction);
                return dotProduct > 0f;
            }

            private bool IsOppositeDirection(Vector2 otherDirection) {
                if (GetCardinalDirection(_direction) == GetCardinalDirection(-otherDirection)) {
                return true;
                }
                return false;
            }

            private bool ShouldGiveWay (Vector2 otherDirection, bool otherTurning) {


                if(isTurning && !IsOppositeDirection(otherDirection) && !otherTurning) {
                    return true;
                } else if (!isTurning) {
                    return false;
                }

                Vector2 roundedDirection = GetCardinalDirection(_direction);
                Vector2 roundedOtherDirection = GetCardinalDirection(otherDirection);


                 if (roundedDirection == new Vector2(-1, 0) && roundedOtherDirection == new Vector2(0, 1)) {  // Left -> Down

        return true;
    }
    else if (roundedDirection == new Vector2(1, 0) && roundedOtherDirection == new Vector2(0, -1)) {  // Right -> Up
        GD.Print("[GiveWay Check] Going right, give way to up. moving");
        return true;
    }
    else if (roundedDirection == new Vector2(0, -1) && roundedOtherDirection == new Vector2(-1,0)) {  // Up -> Left
        GD.Print("[GiveWay Check] Going up, give way to left moving");
        return true;
    }
    else if (roundedDirection == new Vector2(0, 1) && roundedOtherDirection == new Vector2(1, 0)) {  // Down -> Right
        GD.Print("[GiveWay Check] Going down, give way to right moving");
        return true;
    }

    // No yield condition met, meaning no giving way

    return false; // Current object does not give way
}

// Helper function to round the direction to cardinal directions (-1, 0, 1)
private Vector2 GetCardinalDirection(Vector2 direction) {
    if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y)) {
        return new Vector2(Mathf.Sign(direction.X), 0);  // Left or Right
    }
    else if (Mathf.Abs(direction.Y) > Mathf.Abs(direction.X)) {
        return new Vector2(0, Mathf.Sign(direction.Y));  // Up or Down
    }
    else {
        return new Vector2(0, 0);  // Unknown direction if both X and Y are close to 0
    }

}

// Helper function to convert the direction to a human-readable string (e.g., "Left", "Right")
private string GetDirectionAsString(Vector2 direction) {
    // Now we only check if X or Y is positive or negative
    if (direction.X < 0) return "Left";
    if (direction.X > 0) return "Right";
    if (direction.Y < 0) return "Up";
    if (direction.Y > 0) return "Down";
    return "Unknown";
}

private bool IsTurning(Vector2 start, Vector2 middle, Vector2 end) {
       Vector2 startVector = start;
       Vector2 middleVector = middle;
       Vector2 endVector = end;


       // GD.Print($"{this.Name} start {startVector} middle {middleVector} end {endVector}");
       Vector2 toTurn = SetDirection(startVector, middleVector);
       Vector2 toEnd = SetDirection(middleVector, endVector);

        GD.Print($"{this.Name}, {toTurn}, {toEnd}");
       if (toTurn != toEnd) {
        return true;
       }
       return false;

}

private void SetInitialDirection (Vector2 firstPoint, Vector2 startPosition) {
      // Get the first curve point

                // Calculate the initial direction as the normalized vector from the start position to the first point
                _initialDirection = (firstPoint - startPosition).Normalized();
                 const float THRESHOLD = 0.1f;
            if (Mathf.Abs(_initialDirection.Y) < THRESHOLD) {
    _initialDirection.Y = 0f; // Small Y changes, treat as 0
}
else if (_initialDirection.Y > THRESHOLD) {
    _initialDirection.Y = 1f; // Positive Y, treat as down (1)
}
else if (_initialDirection.Y < -THRESHOLD) {
    _initialDirection.Y = -1f; // Negative Y, treat as up (-1)
}

// Handle X direction
if (Mathf.Abs(_initialDirection.X) < THRESHOLD) {
    _initialDirection.X = 0f; // Small X changes, treat as 0
}
else if (_initialDirection.X > THRESHOLD) {
    _initialDirection.X = 1f; // Positive X, treat as right (1)
}
else if (_initialDirection.X < -THRESHOLD) {
    _initialDirection.X = -1f; // Negative X, treat as left (-1)
}
            _direction = _initialDirection;
}

private Vector2 SetDirection (Vector2 position, Vector2 lastposition) {
                 const float THRESHOLD = 0.1f;
                Vector2 direction = (position - lastposition).Normalized();

                if (direction == Vector2.Zero) {
                    direction = _direction;
                }
            if (Mathf.Abs(direction.Y) < THRESHOLD) {
    direction.Y = 0f; // Small Y changes, treat as 0
}
else if (direction.Y > THRESHOLD) {
    direction.Y = 1f; // Positive Y, treat as down (1)
}
else if (direction.Y < -THRESHOLD) {
    direction.Y = -1f; // Negative Y, treat as up (-1)
}

// Handle X direction
if (Mathf.Abs(direction.X) < THRESHOLD) {
    direction.X = 0f; // Small X changes, treat as 0
}
else if (direction.X > THRESHOLD) {
    direction.X = 1f; // Positive X, treat as right (1)
}
else if (direction.X < -THRESHOLD) {
    direction.X = -1f; // Negative X, treat as left (-1)
}
           return direction;
}

    }
}