
using Godot;
using System;
using SieniPeli;

namespace SieniPeli {
    public partial class Ötökkä : PathFollow2D {
        [Export] protected float maxSpeed = 100f;
        [Export] protected float kiihdytysSpeed = 40f;
        [Export] protected float jarrutusSpeed = 80f;
        [Export] protected float risteysSpeed = 50f;
        [Export] protected float minRollingSpeed = 20f;

        private CrossWalkManager CrossWalkManager;

        private float currentSpeed = 0f;
        protected bool isStopped = false;
        private bool isOnCrossWalk = false;
        private bool isOnLights = false;
        private string sieniSuojaTie = "";
        private string detectedCrossWalk = "";

        private bool isBlocked = false;
        private bool approachingRisteys = false;
        private bool inRisteys = false;

        private string blockedBy = "";
        private string blockedByFront = "";
        private bool alreadyBlockedFront = false;
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
        private Vector2 _turnDirection;
        private bool printed = false;

        private Area2D detectionAreaLong = null;
        private Area2D detectionAreaShort = null;
        private Area2D detectionAreaFront = null;
        private Area2D collisionArea = null;

        public bool blockedByLight = false;

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
               // GD.Print(this.Name + "is turning");
            }
            if (isTurning) {
                //GD.Print($"{this.Name} is turning {firstPoint} {endPosition}");
               _turnDirection = SetTurnDirection(firstPoint, endPosition);
               //GD.Print(_turnDirection);


            }

            SetInitialDirection(firstPoint, startPosition);
            _direction = _initialDirection;
            _initialDirectionSaved = _initialDirection;

             //GD.Print($"{this.Name} Initial Direction: ", _initialDirectionSaved);

            detectionAreaLong = GetNode<Area2D>("DetectionArea2DLong");
            detectionAreaShort = GetNode<Area2D>("DetectionArea2DShort");
            detectionAreaFront = GetNode<Area2D>("DetectionArea2DFront");
            collisionArea = GetNode<Area2D>("CollisionArea2D");
            // Signal connections for area detection
            detectionAreaLong.AreaEntered += OnLongRangeEntered;
            detectionAreaLong.AreaExited += OnLongRangeExited;
            detectionAreaLong.BodyEntered += OnLongRangeEntered;
            detectionAreaLong.BodyExited += OnLongRangeExited;
            detectionAreaShort.AreaEntered += OnShortRangeEntered;
            detectionAreaShort.AreaExited += OnShortRangeExited;
            detectionAreaShort.BodyEntered += OnShortRangeEntered;
            detectionAreaShort.BodyExited += OnFrontRangeExited;
             detectionAreaFront.AreaEntered += OnFrontRangeEntered;
            detectionAreaFront.AreaExited += OnFrontRangeExited;
            collisionArea.AreaEntered += OnCrossWalkEntered;
            collisionArea.AreaExited += OnCrossWalkExited;
            collisionArea.BodyEntered += OnCrossWalkEntered;
            collisionArea.BodyExited += OnCrossWalkExited;

        }

        public override void _Process(double delta) {


            if (blockedByFront != "") {
           // GD.Print($"{this.Name} is blockedby {blockedByFront}");
            }
            if (Mathf.Abs(ProgressRatio) > 0.95f) {
                blockedBy = "";
                }

         if (MathF.Abs(ProgressRatio) > 0.001f) {
            _direction = _initialDirectionSaved;
        }
        if (MathF.Abs(ProgressRatio) > 0.01f) {

            _direction = SetDirection(Position, previousPosition);

        }
        if (Mathf.Abs(ProgressRatio) > 0.1f) {
        var frontRange = GetNode<Area2D>("DetectionArea2DFront");
            foreach (Area2D overlapping in frontRange.GetOverlappingAreas())
            {
                OnFrontRangeEntered(overlapping); // reuse your existing logic
            }
        }

         previousPosition = Position;


         PlayAnimation(GetDirectionAsString(_direction), currentSpeed);
            if (isStopped || blockedBy != "" || blockedByFront != "") {
                stopTime += (float)delta;

                // If the car has been stopped for more than the max stop time, resume
                if (stopTime >= maxStopTime) {
                    GD.Print($"{this.Name} has been stopped for 10 seconds, resuming.");
                    isStopped = false;
                    blockedBy = "";
                    blockedByFront = "";
                    Resume();
                }
                return;
            }
            stopTime = 0f;
            Move((float)delta);

            if (this.Name == "WRONG") {
                GD.Print(_direction);
            }


        }

        protected virtual void Move(float delta) {
            if (Mathf.Abs(ProgressRatio) < 0.1f) {
                blockedByFront = "";
            }
            if (blockedByFront != "") {
                return;
            }
            if (approachingRisteys) {
                currentSpeed =  Mathf.MoveToward(currentSpeed, risteysSpeed, jarrutusSpeed * delta);
            } else if (inRisteys) {
                currentSpeed = risteysSpeed;
            }
            else if (!isBlocked) {
                currentSpeed = Mathf.MoveToward(currentSpeed, maxSpeed, kiihdytysSpeed * delta);
            } else {
                currentSpeed = Mathf.MoveToward(currentSpeed, minRollingSpeed, jarrutusSpeed * delta);
            }
            if (isOnLights) {
                currentSpeed = Mathf.MoveToward(currentSpeed, maxSpeed, kiihdytysSpeed * delta);
            }
            Progress += currentSpeed * delta;
        }

        public virtual void Stop() {
            if (isOnCrossWalk || isOnLights) {
                return;
            }
            isStopped = true;
            currentSpeed = 0f;
            stopTime = 0f;
        }

        public string GetBlocked() {
            return blockedByFront;
        }

        public virtual void Resume() {
            if (!isStopped || blockedByLight) return; // Prevent unnecessary calls

           // GD.Print($"{this.Name} resumes moving.");
            isStopped = false;
            isBlocked = false; // <- Ensure movement can continue

            // Gradually accelerate back to normal speed
            if (currentSpeed < maxSpeed) {
                currentSpeed = Mathf.Max(currentSpeed, kiihdytysSpeed);
            }
        stopTime = 0f;
        }

        private void PlayAnimation(string direction, float speed) {

            AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            sprite.FlipH = false;
            sprite.FlipV = false;
            sprite.RotationDegrees = 0;
            string targetAnimation = "";
               this.Scale = Vector2.One;
           // GD.Print($"PlayAnimation: {direction}, Speed: {speed}, CurrentAnim: {sprite.Animation}, {blockedByLight}");

            if (blockedByLight || currentSpeed == 0)
                {
                    // Play the stop animation based on the direction
                    switch (direction)
                    {
                        case "Right":
                            targetAnimation = "Stopright";
                            sprite.FlipH = true;
                            break;
                        case "Left":
                            targetAnimation = "Stopleft";
                            sprite.FlipH = true;
                            this.Scale = new Vector2(1,-1);
                            break;
                        case "Down":
                           targetAnimation = "Stopdown";
                           sprite.FlipV = true;
                            sprite.RotationDegrees = 90;
                            break;
                        case "Up":
                            targetAnimation = "Stopup";
                            sprite.RotationDegrees = -90;
                            break;
                    }
                }
                else if (!blockedByLight && currentSpeed > 0)
                {
                   // GD.Print(blockedByLight, currentSpeed);
                    // Play the walk animation based on the direction
                    switch (direction)
                    {
                        case "Right":
                            targetAnimation = "Walkright";
                            sprite.FlipH = true;
                            break;
                        case "Left":
                           targetAnimation = "Walkleft";
                            sprite.FlipH = true;
                           this.Scale = new Vector2(1,-1); // flip koko homma et collisionshapet siirtyy kans
                            break;
                        case "Down":
                           targetAnimation = "Walkdown";
                            sprite.FlipV = true;
                            sprite.RotationDegrees = 90;
                            break;
                        case "Up":
                            targetAnimation = "Walkup";
                            sprite.RotationDegrees = -90;
                            break;
                    }
                }
                 if (sprite.Animation != targetAnimation)
                {
                    sprite.Stop();
                    sprite.Play(targetAnimation);
                }

                if (this.Name == "WRONG") {
                GD.Print(targetAnimation);
            }
            }

        private void OnLongRangeEntered(Node body) {
            string areaName = body.Name;
            if (body is Area2D area)  // Ensure it's an Area2D (which would be another car or object)
            {
                if (areaName == "Risteys" && isTurning) {
                    approachingRisteys = true;
                }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        bool otherTurned = otherÖtökkä.HasTurned();
                        bool otherRisteys = false;
                        if (otherTurning) {
                            otherRisteys = otherÖtökkä.GetRisteys();
                            if(!otherRisteys) {
                            other_direction = otherÖtökkä.GetTurnDirection();
                            }
                        }
                        if (ShouldYieldWithTurning(_direction, other_direction, isTurning, otherTurning, otherRisteys, otherÖtökkä.Name, otherTurned)) {
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
                  //  GD.Print($"{body.GetInstanceId().ToString()} isnt {blockedBy}");
                return;
                }
            }


                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        bool otherTurned = otherÖtökkä.HasTurned();
                        bool otherRisteys = false;
                        if (otherTurning) {
                            otherRisteys = otherÖtökkä.GetRisteys();
                            if(!otherRisteys) {
                            other_direction = otherÖtökkä.GetTurnDirection();
                            }
                        }
                        if (ShouldYieldWithTurning(_direction, other_direction, isTurning, otherTurning, otherRisteys, otherÖtökkä.Name, otherTurned)) {
                            //GD.Print($"{this.Name} blocked by{parentNode.Name}");
                            if (String.IsNullOrEmpty(blockedBy)) {
                          //  GD.Print($"Somehow empty {blockedBy}");
                            blockedBy = body.GetInstanceId().ToString();
                            alreadyBlocked = true;
                            }
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
                 //   GD.Print($"{body.GetInstanceId().ToString()} isnt {blockedBy}");
                return;
                }

            }
                // Ensure it's not the same as this instance's collision area
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    if (body.GetInstanceId().ToString() == blockedBy) {

                      //  GD.Print($"{this.Name} Stopped being blocked by {parentNode.Name}");
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

        private void OnFrontRangeEntered(Node body) {
            string areaName = body.Name;
            if (body is Area2D area)  // Ensure it's an Area2D (which would be another car or object)
            {
                if (areaName == "Valotie") {
                    blockedByLight = true;
                    Stop();
                }
                else if (blockedByFront != "") {
                if (body.GetInstanceId().ToString() != blockedByFront || alreadyBlockedFront) {
                return;
                }
            }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        string otherBlocked = otherÖtökkä.GetBlocked();
                        if (GetDirectionAsString(_direction) == GetDirectionAsString(other_direction)){
                            GD.Print("We get here when stuck");
                           GD.Print($"[{this.Name}] {otherÖtökkä.Name} blockedByFront: {otherBlocked}, This ID: {this.GetInstanceId()}");
                        if (otherBlocked == this.GetInstanceId().ToString()) { // verrataan onko toisen ötökän blokin id tämän id -> etteivät blokkaa toisiaan ja jumita
                            GD.Print("other bug already blocked by this, reverting");
                            blockedByFront = "";
                            alreadyBlockedFront = false;
                            return;
                        }
                        blockedByFront = parentNode.GetInstanceId().ToString();
                        alreadyBlockedFront = true;
                      //  GD.Print($"{this.Name} now blocked by {blockedByFront}");
                            }
                        }
                    }
            }
        }

         private void OnFrontRangeExited(Node body) {
            string areaName = body.Name;
            if (body is Area2D area) {

                if (areaName == "Valotie" && body != this) {
                    GD.Print("Set to false here");
                    blockedByLight = false;
                    Resume();

                }

                // Ensure it's not the same as this instance's collision area
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {

                    if (parentNode.GetInstanceId().ToString() == blockedByFront) {

                       // GD.Print($"{this.Name} Stopped being blocked by {parentNode.Name}");
                    blockedByFront = "";
                    alreadyBlockedFront = false;
                    Resume();
                    }
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
            if (areaName == "Valotie") {
                isOnLights = true;
                blockedByLight = false;
                Resume();

            }
            if (areaName == "Risteys" && isTurning) {
                approachingRisteys = false;
                inRisteys = true;
            } else {
                approachingRisteys = false;
            }
        }
        private void OnCrossWalkExited(Node Area2D) {
            string areaName = Area2D.Name;

            if (isOnCrossWalk) {
                isOnCrossWalk = false;
              //int($"{this.Name} has left crosswalk");
                CheckCrossWalkStatus();
            }
            if (areaName == "Risteys") {
                inRisteys = false;
                //GD.Print("left risteys");
            }
            if (areaName == "Valotie") {
                isOnLights = false;
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

            public bool GetRisteys() {
                return inRisteys;
            }
            private bool IsSame_direction(Vector2 other_direction) {
                float dotProduct = _direction.Dot(other_direction);
                return dotProduct > 0f;
            }

            private bool IsOppositeDirection(Vector2 otherBugDirection) {
                string thisDirectionString = GetDirectionAsString(_direction);
               string otherBugDirectionString = GetDirectionAsString(otherBugDirection);

                switch (thisDirectionString, otherBugDirectionString) {
                    case ("Up", "Down"):
                    return true;
                    case ("Down", "Up"):
                    return true;
                    case ("Left", "Right"):
                    return true;
                    case ("Right", "Left"):
                    return true;
                    default:
                    return false;
                }
            }

            private bool ShouldGiveWay (Vector2 otherBugDirection, bool otherTurning) {
                // opposite direction kusee varmaa jotain
                // tajusin et vasemmalle kääntyvien pitää väistää kaikkee abaut
                // oikeelle kääntyvien pitäs väistää vaa vasemmalta tulevaa suoraa liikennettä
                if (!isTurning) {
                    return false;
                /*}
                else if(IsOppositeDirection(otherBugDirection)) {
                    return false;;*/

                } if (isTurning && (GetCardinalDirection(otherBugDirection) != new Vector2(0, -1)) && (SetDirection(endPosition, firstPoint) != new Vector2(0,1))) {
                    return false; // ei toimi, koska ylhäältä alas tuleva oikeel kääntyvä tekee oikeestaan vasemman käännöksen ,mut tää kattoo sen oikeeks: pitää vertaa alkuperäsee directionii
                } else if (isTurning && !otherTurning) {
                            // näkee liia kauas ni stoppaa liia aikasi
                    return true;
                }

                Vector2 roundedDirection = GetCardinalDirection(_direction);
                Vector2 roundedOtherDirection = GetCardinalDirection(otherBugDirection);


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
    const float THRESHOLD = 0.01f;  // Small values treated as 0

    if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y) + THRESHOLD) {
        return new Vector2(Mathf.Sign(direction.X), 0);  // Left or Right
    }
    else if (Mathf.Abs(direction.Y) > Mathf.Abs(direction.X) + THRESHOLD) {
        return new Vector2(0, Mathf.Sign(direction.Y));  // Up or Down
    }
    else {
        return new Vector2(0, 0);  // Unknown or too small movement
    }
}


// Helper function to convert the direction to a human-readable string (e.g., "Left", "Right")


private bool IsTurning(Vector2 start, Vector2 middle, Vector2 end) {
    Vector2 dir1 = (middle - start).Normalized();
    Vector2 dir2 = (end - middle).Normalized();

    float angleChange = dir1.AngleTo(dir2); // in radians

    const float TURN_ANGLE_THRESHOLD = 0.3f; // ~17 degrees (adjust as needed)

    return Mathf.Abs(angleChange) > TURN_ANGLE_THRESHOLD;
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
                Vector2 direction = (position - lastposition);
                Vector2 cardinalDirection = GetCardinalDirection(direction);

               if (cardinalDirection == Vector2.Zero || (Mathf.Abs(cardinalDirection.X) > 0 && Mathf.Abs(cardinalDirection.Y) > 0)) {
                    if (!isTurning || isTurning && !inRisteys) {
                    cardinalDirection = _initialDirectionSaved;
                    return cardinalDirection;
                    } else if (inRisteys) {
                        cardinalDirection = GetTurnDirection();
                        return cardinalDirection;
                    }
                }
           return cardinalDirection;
}

              /*  if (cardinalDirection == Vector2.Zero || (Mathf.Abs(cardinalDirection.X) > 0 && Mathf.Abs(cardinalDirection.Y) > 0)) {
                    if (!isTurning || isTurning && !inRisteys) {
                    cardinalDirection = _initialDirectionSaved;
                    } else if (inRisteys) {
                        direction = GetTurnDirection(); // vaiha takas getturn tjsp
                    }
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
}*/


    private string GetDirectionAsString(Vector2 direction) {
    // Now we only check if X or Y is positive or negative
    if (direction == new Vector2(1,0)) return "Right";
    if (direction == new Vector2(-1,0)) return "Left";
    if (direction == new Vector2(0,1)) return "Down";
    if (direction == new Vector2(0,-1)) return "Up";


    return "Unknown";
}

private Vector2 SetTurnDirection(Vector2 point1, Vector2 point2)
{
    const float THRESHOLD = 0.1f;

    // Calculate the direction vector based on the difference between the points
    Vector2 direction = (point2 - point1).Normalized();

    // Apply the threshold logic to determine the direction
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

    // Return the final direction as a Vector2
    return direction;
}


public Vector2 GetTurnDirection() {
    return _turnDirection;
}

private bool RightTurn() {
    string turnDirectionString = GetDirectionAsString(_turnDirection);
    string currentDirectionString = GetDirectionAsString(_initialDirectionSaved);
   // GD.Print(this.Name, currentDirectionString, turnDirectionString);
    switch (currentDirectionString, turnDirectionString){
        case ("Up", "Right"):
        return true;
        case ("Left", "Up"):
        return true;
        case ("Down", "Left"):
        return true;
        case ("Right", "Down"):
        return true;
        default:
        return false;
    }
}

public bool HasTurned() {
    string initialDirection = GetDirectionAsString(_initialDirectionSaved);
    string currentDirection = GetDirectionAsString(_direction);

    if (initialDirection != currentDirection) {
        return true;
    }
    else {
        return false;
    }

}


private bool ShouldYieldWithTurning(Vector2 thisDirection, Vector2 otherDirection, bool isTurning, bool otherTurning, bool otherRisteys, string otherÖtökkä, bool otherTurned)
{
    string thisBugDirection = GetDirectionAsString(thisDirection);
    string otherBugDirection = GetDirectionAsString(otherDirection);
    if (thisBugDirection == "Unknown") {
      //  GD.Print(thisDirection);
        if(!isTurning) {
        thisBugDirection = GetDirectionAsString(_initialDirectionSaved);
        }
        else {
            thisBugDirection = GetDirectionAsString(_turnDirection);
        }
    }

    //GD.Print($"Checking yield: {this.Name} ({thisBugDirection}, Turning: {isTurning}), {otherÖtökkä}({otherBugDirection}, Turning: {otherTurning}, InIntersection: {otherRisteys})");

    if (isTurning) {
        thisBugDirection = GetDirectionAsString(_turnDirection);
        //GD.Print($"{this.Name} bug is turning, using turn direction: {thisBugDirection}");
    }



    // Case 1: If both cars are not turning (both are going straight), apply the straight yield rules.
    if (!isTurning && !otherTurning)
    {
       // GD.Print("Both cars are going straight, checking straight yield rules...");
        if (thisBugDirection == "Up" && otherBugDirection == "Left") {
           // GD.Print("Car going Up yields to car going Left.");
            return true;
        }
        if (thisBugDirection == "Down" && otherBugDirection == "Right") {
          //  GD.Print("Car going Down yields to car going Right.");
            return true;
        }
        if (thisBugDirection == "Left" && otherBugDirection == "Down") {
            //GD.Print("Car going Left yields to car going Down.");
            return true;
        }
        if (thisBugDirection == "Right" && otherBugDirection == "Up") {
           // GD.Print("Car going Right yields to car going Up.");
            return true;
        }
    }

    // Case 2: If the current car is turning and the other car is not, the current car yields.
    if (isTurning && !otherTurning && !IsOppositeDirection(otherDirection))
    {
        if(RightTurn()){
            return false;
        } else {
       // GD.Print("This bug is turning while the other bug is not. Yielding...");
        return true;
        }

    }

  // Case 3: If the other car is turning and the current car is not, the other car yields.
    if (!isTurning && otherTurning)
    {

       //GD.Print($"Other bug is turning while {this.Name} bug is not.");
        if (otherRisteys && !IsOppositeDirection(otherDirection)) {
            //GD.Print(thisBugDirection, otherBugDirection); // tää ei toimiiiii. Pitäs kattoo initial direction tjsp.
            return true;
        }
       // GD.Print("Other bug is turning but not in front. No yield needed.");
        return false;
    }

    // Case 4: Both cars are turning
    if (isTurning && otherTurning)
    {

       // GD.Print("Both bugs are turning, checking crossing conditions...");
        if (thisBugDirection == "Right" && otherBugDirection == "Right")
        {
          //  GD.Print("Both bugs are turning right, no yield needed.");
            return false;
        }
        if (thisBugDirection == "Left" && otherBugDirection == "Left")
        {
            //GD.Print("Both bugs are turning left, they yield to each other.");
            return true;
        }
        if ((thisBugDirection == "Left" && otherBugDirection == "Right") ||
            (thisBugDirection == "Right" && otherBugDirection == "Left"))
        {
            //GD.Print("One bug is turning left and the other is turning right, they yield to each other.");
            return true;
        }
    }

   // GD.Print("No specific yield condition met. No yield needed.");
    return false;
}
    }
}