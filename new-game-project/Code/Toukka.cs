using Godot;
using System;
using System.Collections.Generic;
using SieniPeli;

namespace SieniPeli {
public partial class Toukka : Sprite2D
{
    // Movement constants
     // normalSpeed at which the car moves (pixels per second)

    private Sieni sieni;
    private float _speed;
    [Export] float normalSpeed = 100f;

    private float slowSpeed = 50f;
    private float stopSpeed = 0f;
    private bool isSlowingDown = false;
	[Export] float stopDistance = 40f;
    private const float TurnChanceRight = 0.4f; // 40% chance to turn right
    private const float TurnChanceLeft = 0.2f; // 20% chance to turn left

    private string detectedCrossWalk = "";


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
    private bool atCrossWalk = false;
	private string sieniSuojaTie = "";

    public override void _Ready()
    {
        base._Ready();
        CallDeferred(nameof(InitializeSieniNode));

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
                    if (enteredArea == GetNode<Area2D>("CollisionArea2D")) // Only trigger if this Toukka's Area2D entered
                    {
                        OnIntersectionEntered(intersectionArea);

            }
        };

        GetNode<Area2D>("DetectionArea2D").BodyEntered += OnDetectionAreaEntered;
        GetNode<Area2D>("DetectionArea2D").BodyExited += OnDetectionAreaExited;
    }
		}

	}

        private void InitializeSieniNode()
        {
            sieni = GetNode<Sieni>("/root/Node2D/Sieni"); // Ensure you reference the correct node
            if (sieni != null)
        {
            GD.Print("Sieni node found.");
            sieni.PysähtynytSuojaTielle += OnSieniStoppedAtCrosswalk;
            sieni.PoistuuSuojaTieltä += OnSieniLeftCrossWalk;
        }
        else
        {
            GD.Print("Sieni node not found.");
        }
    }


        public override void _Process(double delta)
    {

            StopToukka();
				// Move the car
				currentPosition += currentDirection * _speed * (float)delta;

				// Update the car's position
				Position = currentPosition;

				if (Position.X < -50 || Position.X > 690 || Position.Y < -50 || Position.Y > 410) {
					ResetPosition();
				}


	}


	private void ResetPosition() {
		GD.Print("Car is out of bounds, resetting position...");
        currentPosition = initialPosition; // Reset to the initial position
        Position = initialPosition;
        currentDirection = Directions[(int)StartDirection];
		risteysCounter = 0;
		RotationDegrees = initialRotation;
	}
    private void OnIntersectionEntered(Area2D area)
    {
		GD.Print($"Collision detected with: {area.Name}");

		risteysCounter++;
        if (area is Area2D intersection) // Check if it's an intersection (Area2D)
        {
			GD.Print($"Toukka risteyksessä {intersection.Name}");


			if (risteysCounter == 1) {
				 float randomValue = rng.Randf();
				if (randomValue < TurnChanceRight)
				{
					// Turn right (90 degrees clockwise)
					TurnRight();
					GD.Print("Turning Right at first intersection");
				}
				else
				{
					// Continue straight (no turn)
					ContinueStraight();
					GD.Print("Continuing straight at first intersection");
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
                GD.Print("Turning Left at second intersection");
            }
            else
            {
                // Continue straight (no turn)
                ContinueStraight();
                GD.Print("Continuing straight at second intersection");
            }
        }

		isTurning = false;
		GD.Print("Turning decision made");
		}
	}

    private void OnDetectionAreaEntered(Node body) {
         GD.Print($"Toukka detected: {body.Name}");
         string areaName = body.Name.ToString();

        if (body is Area2D area)
        {
            if (area.GetParent() is Toukka otherToukka)
            {
                GD.Print($"Toukka detected another Toukka: {otherToukka.Name}");
                SlowDown();
            }
        }
        else if (body is TileMapLayer tileMapLayer)
        {
            if (areaName.StartsWith("Suojatie"))
            {
                GD.Print($"Toukka detected a crosswalk: {tileMapLayer.Name}");
                detectedCrossWalk = tileMapLayer.Name;
            }
    }
}
        private void SlowDown()
        {
            if (!isSlowingDown) {
                isSlowingDown = true;
                _speed = slowSpeed;
                GD.Print("Slowing down due to other toukka");

                if (currentPosition.DistanceTo(Position) < stopDistance) {
                    _speed = stopSpeed;
                    GD.Print("Toukka too close, stopping");
                }
            }
        }

        private void OnDetectionAreaExited (Node body) {
         GD.Print($"Toukka left area: {body.Name}");
         string areaName = body.Name.ToString();

                if (body is Area2D area)
                {
                    if (area.GetParent() is Toukka otherToukka)
                    {
                        GD.Print($"Toukka left detection area of another Toukka: {otherToukka.Name}");
                        SpeedUp();
                    }
                }
                else if (body is TileMapLayer tileMapLayer)
                {
                    if (tileMapLayer.Name == detectedCrossWalk)
                    {
                        GD.Print($"Toukka left crosswalk: {tileMapLayer.Name}");
                        detectedCrossWalk = "";
                    }
            }
        }

        private void SpeedUp()
        {
            if (isSlowingDown)
            {
                isSlowingDown = false;
                _speed = normalSpeed;
                GD.Print("Speeding up as the path is clear.");
            }
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

        GD.Print("Turning Right");
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

        GD.Print("Turning Left");
        isTurning = false;
		RotateSprite(-90);
    }

    private void ContinueStraight()
    {
        GD.Print("Continuing Straight");
        isTurning = false;
    }

	private void RotateSprite(float angle) {
		RotationDegrees += angle;
	}


	private void OnSieniStoppedAtCrosswalk(object sender, string suojatienimi)
        {
            sieniSuojaTie = suojatienimi;
            GD.Print($"detectedCrossWalk: {detectedCrossWalk}, suojatienimi: {suojatienimi}");
            GD.Print($"Toukka received the signal: Sieni stopped at {suojatienimi}");
            if (!string.IsNullOrEmpty(suojatienimi)) {
                atCrossWalk = true;
            }
        }


    private void OnSieniLeftCrossWalk (object sender, string suojatienimi) {
        if(detectedCrossWalk == suojatienimi) {
            GD.Print($"Sieni left crosswalk {suojatienimi}");
            atCrossWalk = false;
            _speed = normalSpeed;
            sieniSuojaTie = "";
        }
    }

    private void StopToukka() {
        if (atCrossWalk && detectedCrossWalk == sieniSuojaTie) {
            GD.Print($"Stopping for Sieni: {detectedCrossWalk} = {sieniSuojaTie}");
            _speed = stopSpeed;
        }
        else {
            _speed = normalSpeed;
        }
    }
}
}