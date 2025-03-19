using Godot;
using System;
using System.Collections.Generic;
using SieniPeli;

namespace SieniPeli {
public partial class Toukka : Sprite2D
{
    // Movement constants
     // normalSpeed at which the car moves (pixels per second)

    private float _speed;
    [Export] float normalSpeed = 100f;

    private float slowSpeed = 100f;
    private float slowdownDuration = 1.0f;
    private float slowdownTimer = 0f;
    private float speedupTimer = 1.0f;

    private float speedupDuration = 1.0f;
    private float stopSpeed = 0f;
    private bool isSlowingDown = false;
    private bool isSpeedingUp = false;
	[Export] float stopDistance = 150;

    private float stopDelay = 3.5f;
    private float stopDelayTimer = 0f;
    private bool isStopped = false;
    private const float TurnChanceRight = 0.4f; // 40% chance to turn right
    private const float TurnChanceLeft = 0.2f; // 20% chance to turn left

    private string detectedCrossWalk = "";
    private bool isOnCrossWalk = false;
    private string sieniSuojaTie = "";



    // Directions: up, down, left, right

	public enum Direction {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	}

	[Export] public Direction StartDirection = Direction.Right;
    private Vector2[] Directions = new Vector2[]
    {
        new Vector2(0, -1), // Up
        new Vector2(0, 1),  // Down
        new Vector2(-1, 0), // Left
        new Vector2(1, 0)   // Right
    };

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Vector2 currentDirection; // Current direction of movement
    private Vector2 currentPosition;  // Current position of the car
    private bool isTurning = false;   // Flag to avoid turning while moving

	private bool isMoving = true;

	private int risteysCounter = 0;

	private Vector2 initialPosition;
	private float initialRotation;

    private CrossWalkManager CrossWalkManager;

    private bool ötökkäAhead = false;
    private bool signalsConnected = false;

    public override void _Ready()
    {
        AddToGroup("Toukka");
        CrossWalkManager = GetNode<CrossWalkManager>("/root/Node2D/CrossWalkManager");

        _speed = normalSpeed;
		rng.Seed = (ulong)GD.Randi(); // oma rng jokasel
        currentDirection = Directions[(int)StartDirection]; // Direction set in editor

        // currentPosition simply the position inside the editor
		initialPosition = Position;
        currentPosition = initialPosition;
		Position = initialPosition;
		initialRotation = RotationDegrees;

        var intersectionsNode = GetNode<Node2D>("/root/Node2D/Risteykset"); // Adjust the node name if needed
			 if (intersectionsNode == null)
			{
				GD.PrintErr("Risteykset node not found!");
				return;
			}


        foreach (Node child in intersectionsNode.GetChildren())
        {
            if (child is Area2D intersectionArea)
            {
				intersectionArea.AreaEntered += (enteredArea) =>
                {
                    // Check if this Toukka's Area2D is the one that entered
                    if (enteredArea == GetNode<Area2D>("MiddleArea2D")) // Only trigger if this Toukka's Area2D entered
                    {
                        OnIntersectionEntered(intersectionArea);

            }
        };
        var detectionArea = GetNode<Area2D>("DetectionArea2D");
        var collisionArea = GetNode<Area2D>("CollisionArea2D");

        // Only connect if not already connected

            if (!signalsConnected) {
            detectionArea.BodyEntered += OnDetectionAreaEntered;
            detectionArea.BodyExited += OnDetectionAreaExited;
            detectionArea.AreaEntered += OnDetectionAreaEntered;
            detectionArea.AreaExited += OnDetectionAreaExited;
            collisionArea.BodyEntered += OnCrossWalkEntered;
            collisionArea.BodyExited += OnCrossWalkExited;
            collisionArea.AreaEntered += OnCrossWalkEntered;
            collisionArea.AreaExited += OnCrossWalkExited;
            signalsConnected = true;
            GD.Print(signalsConnected);
            }



    }
		}

	}


        public override void _Process(double delta)
    {

        if (isStopped && stopDelayTimer > 0) //&& !CrossWalkManager.CrossWalkOccupied
        {
        stopDelayTimer -= (float)delta;
        if (stopDelayTimer <= 0 && !ötökkäAhead)
        {
            SpeedUp(); // Resume movement after delay
        }
        return; // Prevent further movement calculations while waiting
        }
            if (isSlowingDown && slowdownTimer > 0)
                {
                    slowdownTimer -= (float)delta;
                    _speed = Mathf.Lerp(normalSpeed, slowSpeed, (slowdownDuration - slowdownTimer) / slowdownDuration);
                   // GD.Print($"Slowing down: Speed = {_speed}");
                }
                else if (isSlowingDown)
                {
                    _speed = slowSpeed;
                }

            if (isSpeedingUp && speedupTimer > 0)
            {
                speedupTimer -= (float)delta;
                _speed = Mathf.Lerp(slowSpeed, normalSpeed, (speedupDuration - speedupTimer) / speedupDuration);
            }
            else if (isSpeedingUp)
            {
                _speed = normalSpeed;
                isSpeedingUp = false;
            }

            if (!isStopped) {
            currentPosition += currentDirection * _speed * (float)delta;
            Position = currentPosition;
            if (Position.X < -50 || Position.X > 690 || Position.Y < -50 || Position.Y > 410) {
					ResetPosition();
				}
            }
    }



	private void ResetPosition() {
		//GD.Print("Car is out of bounds, resetting position...");
        currentPosition = initialPosition; // Reset to the initial position
        Position = initialPosition;
        currentDirection = Directions[(int)StartDirection];
		risteysCounter = 0;
		RotationDegrees = initialRotation;
	}
    private void OnIntersectionEntered(Area2D area)
    {


		risteysCounter++;
        if (area is Area2D intersection) // Check if it's an intersection (Area2D)
        {

			if (risteysCounter == 1) {
				 float randomValue = rng.Randf();
				if (randomValue < TurnChanceRight)
				{
					// Turn right (90 degrees clockwise)
					TurnRight();
					//GD.Print("Turning Right at first intersection");
				}
				else
				{
					// Continue straight (no turn)
					ContinueStraight();
					//GD.Print("Continuing straight at first intersection");
				}
        }
            else if (risteysCounter == 2)
        {
            // Second intersection - random chance to turn left
            float randomValue = rng.Randf();
            if (randomValue < TurnChanceLeft)
            {
                // Turn left (90 degrees counterclockwise)
                TurnLeft();
                //GD.Print("Turning Left at second intersection");
            }
            else
            {
                // Continue straight (no turn)
                ContinueStraight();
                //GD.Print("Continuing straight at second intersection");
            }
        }

		isTurning = false;
		//GD.Print("Turning decision made");
		}
	}

    private void OnDetectionAreaEntered(Node body) {
         //GD.Print($"Toukka detected: {body.Name}");
         string areaName = body.Name.ToString();

        if (body is Area2D area)
        {
            if (area.GetParent() is Toukka otherToukka && otherToukka != this)
            {
              //  GD.Print($"{this.Name} detected another Toukka: {otherToukka.Name}");
                SlowDown(otherToukka);

            }
        }
        else if (body is TileMapLayer tileMapLayer)
        {
            if (areaName.StartsWith("Suojatie"))
            {
               // GD.Print($"Toukka detected a crosswalk: {tileMapLayer.Name}");
                detectedCrossWalk = areaName;
                CheckCrossWalkStatus();
            }
    }
}

        private void OnCrossWalkEntered(Node body) {
            string areaName = body.Name;
            if (body is TileMapLayer tileMapLayer)
            {

                GD.Print($"{this.Name} on {areaName}");
             if (!isOnCrossWalk)
                {
                    isOnCrossWalk = true; // Set the flag to true when entering a crosswalk
                    // Prevent stopping or slowing down when on crosswalk
                    StopToukka(); // Ensure the Toukka does not stop on the crosswalk
                }

            }
        }

        private void OnCrossWalkExited(Node Area2D) {
            if (isOnCrossWalk) {
                isOnCrossWalk = false;
                GD.Print($"{this.Name} has left crosswalk");
                CheckCrossWalkStatus();
            }
        }

        private void SlowDown(Toukka otherToukka)
        {
            float distance = Position.DistanceTo(otherToukka.Position);

            while (isOnCrossWalk) {
                return;
            }
            if (distance < stopDistance && !isOnCrossWalk) // Too close, stop immediately
            {
            _speed = stopSpeed;
            isStopped = true;
            stopDelayTimer = stopDelay;
            ötökkäAhead = true;
           // GD.Print("Toukka too close, stopping immediately!");
            }
            else if (!isOnCrossWalk)// Not too close yet, start slowing down
            {
            if (!isSlowingDown)
            {
                isSlowingDown = true;
                slowdownTimer = slowdownDuration;
               // GD.Print("Initiating gradual slowdown...");
            }
        }
    }

        private void OnDetectionAreaExited (Node body) {
         //GD.Print($"Toukka left area: {body.Name}");
         string areaName = body.Name.ToString();

                if (body is Area2D area)
                {
                    if (area.GetParent() is Toukka otherToukka && otherToukka != this)
                    {
                    //  GD.Print($"{this.Name} cano no longer detect {otherToukka.Name}");
                      ötökkäAhead = false;
                      SpeedUp();
                    }
                }
                else if (body is TileMapLayer tileMapLayer)
                {
                    if (tileMapLayer.Name == detectedCrossWalk)
                    {
                       // GD.Print($"Toukka left crosswalk: {tileMapLayer.Name}");
                        detectedCrossWalk = "";
                        CheckCrossWalkStatus();
                    }
            }

        }

        private void SpeedUp()
        {


                if (CrossWalkManager.CrossWalkOccupied && CrossWalkManager.CurrentCrossWalk == detectedCrossWalk)
                {
                    // Do not speed up if crosswalk is still occupied
                    GD.Print("Crosswalk is still occupied, cannot speed up.");
                    return;
                }



                isSlowingDown = false;
                isStopped = false;
                isSpeedingUp = true;
                speedupTimer = speedupDuration;
                _speed = normalSpeed;
               // GD.Print("Speeding up...");

        }

        private void TurnRight()
    {
        // Turn right (90 degrees clockwise)
        if (currentDirection == Directions[0]) // Up
            currentDirection = Directions[3]; // Right
        else if (currentDirection == Directions[1]) // Down
            currentDirection = Directions[2]; // Left
        else if (currentDirection == Directions[2]) // Left
            currentDirection = Directions[0]; // Up
        else if (currentDirection == Directions[3]) // Right
            currentDirection = Directions[1]; // Down

       // GD.Print("Turning Right");
		RotateSprite(90);
    }

    private void TurnLeft()
    {
        // Turn left (90 degrees counterclockwise)
        if (currentDirection == Directions[0]) // Up
            currentDirection = Directions[2]; // Left
        else if (currentDirection == Directions[1]) // Down
            currentDirection = Directions[3]; // Right
        else if (currentDirection == Directions[2]) // Left
            currentDirection = Directions[1]; // Down
        else if (currentDirection == Directions[3]) // Right
            currentDirection = Directions[0]; // Up

       // GD.Print("Turning Left");
        isTurning = false;
		RotateSprite(-90);
    }

    private void ContinueStraight()
    {
       // GD.Print("Continuing Straight");
        isTurning = false;
    }

	private void RotateSprite(float angle) {
		RotationDegrees += angle;
	}


	private void CheckCrossWalkStatus()
{
    if (CrossWalkManager.CrossWalkOccupied && CrossWalkManager.CurrentCrossWalk == detectedCrossWalk)
    {
        StopToukka();
       // GD.Print($"Stopping Crosswalk: {CrossWalkManager.CrossWalkOccupied}, tie: {CrossWalkManager.CurrentCrossWalk} : {detectedCrossWalk}");
    }
    else if (CrossWalkManager.CurrentCrossWalk != detectedCrossWalk)
    {
        SpeedUp();
       // GD.Print($"Moving Crosswalk: {CrossWalkManager.CrossWalkOccupied}, tie: {CrossWalkManager.CurrentCrossWalk} : {detectedCrossWalk}");
    }

}


    private void StopToukka() {
        while (isOnCrossWalk) {
            GD.Print("Cant stop wont stop");
            return;
        }
        _speed = stopSpeed;
        isStopped = true;
        stopDelayTimer = stopDelay;
    }
}
}