using Godot;
using System;
using System.Collections.Generic;
using SieniPeli;

namespace SieniPeli {
public partial class Toukka : Sprite2D
{
    // Movement constants
    [Export] float Speed = 100f; // Speed at which the car moves (pixels per second)

	[Export] float stopDistance = 40f;
    private const float TurnChanceRight = 0.4f; // 40% chance to turn right
    private const float TurnChanceLeft = 0.2f; // 20% chance to turn left


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
	private bool nearCrossWalk = false;
	private bool isWaiting = false;

	private int risteysCounter = 0;

	private Vector2 initialPosition;
	private float initialRotation;

	private string sieniSuojaTie = "";

    public override void _Ready()
    {

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
                    if (enteredArea == GetNode<Area2D>("Area2D")) // Only trigger if this Toukka's Area2D entered
                    {
                        OnIntersectionEntered(intersectionArea);

            }
        };
    }
		}
		var sieni = GetNode<Sieni>("/root/Node2D/Sieni"); // Ensure you reference the correct node
            sieni.PysähtynytSuojaTielle += OnSieniStoppedAtCrosswalk;
	}

    public override void _Process(double delta)
    {
        /**if (isMoving) {

			if(nearCrossWalk && Sieni.onSuojaTiellä) {
				Speed = 0;
				GD.Print("Stop for sieni");

			} else {*/

				// Move the car
				currentPosition += currentDirection * Speed * (float)delta;

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

			if (isWaiting) {
				GD.Print("Waiting at crosswalk");
				return;
			}

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
            if (!string.IsNullOrEmpty(suojatienimi))
            {
                GD.Print($"Sieni stopped at the {suojatienimi} Toukka is now waiting.");
				sieniSuojaTie = suojatienimi;
                isWaiting = true; // Toukka will stop
            }
            else
            {
                GD.Print("Sieni moved past the crosswalk. Toukka can move again.");
				sieniSuojaTie = "";
                isWaiting = false; // Toukka can resume movement
            }
        }
}
}