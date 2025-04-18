using Godot;
using SieniPeli;
using System;
using System.Collections.Generic;

namespace SieniPeli {
	public partial class Sieni : AnimatedSprite2D // Change base class to AnimatedSprite2D
	{
		[Export] private string _gridScenePath = "res://Level/grid.tscn";

		private Vector2 currentPosition;
		private Vector2 initialPosition;

		[Signal] public delegate void PysähtynytSuojaTielleEventHandler(string suojaTieName, bool onSuojaTiellä);
		[Signal] public delegate void PoistuuSuojaTieltäEventHandler(string suojaTieName, bool onSuojaTiellä);
		public bool onSuojaTiellä = false;
		private string suojaTieNimi = "";

		private Timer suojaTieTimer;
		private Timer suojaTieDelayTimer;
		private bool canEmitSignal = true;
		private const float suojaTieAika = 2.0f;
		private const float suojaTieDelayAika = 10.0f;

		private Vector2 _direction = Vector2.Zero;
		private Grid grid;
		private List<Vector2> path = new List<Vector2>();
		private int currentTargetIndex = 0;
		public bool isMoving = false;
		private float _currentSpeed = 75f;

		public bool atTäplä = false;
		private bool celebrationTime = false;
		private Vector2 _lastDirection = Vector2.Zero; // vika suunta taltee animaatioil

		public bool _hasHeijastin = false;
		public override void _Ready() {
			PackedScene gridScene = ResourceLoader.Load<PackedScene>(_gridScenePath);
			grid = (Grid)gridScene.Instantiate();
			initialPosition = Position;
			currentPosition = initialPosition;
			Position = initialPosition;
			GlobalPosition = initialPosition;

			// Katotaa animaatiot
			if (SpriteFrames == null) {
				GD.PrintErr("AnimatedSprite2D missing SpriteFrames!");
			}
			Play("idledown");

			GetNode<Area2D>("Area2D").BodyEntered += OnBodyEntered;
			GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;
			GetNode<Area2D>("Area2D").AreaEntered += OnAreaEntered;

			suojaTieTimer = new Timer { WaitTime = suojaTieAika, OneShot = true };
			suojaTieTimer.Timeout += OnSuojaTieTimeout;
			AddChild(suojaTieTimer);

			suojaTieDelayTimer = new Timer { WaitTime = suojaTieDelayAika, OneShot = true };
			suojaTieDelayTimer.Timeout += OnDelayTimeOut;
			AddChild(suojaTieDelayTimer);
		}

		public override void _Process(double delta) {
			if (!isMoving || path.Count == 0 || currentTargetIndex >= path.Count)
				return;

			Vector2 targetPosition = path[currentTargetIndex];
			Vector2 direction = (targetPosition - GlobalPosition).Normalized();
			float distance = _currentSpeed * (float)delta;
			GlobalPosition = GlobalPosition.MoveToward(targetPosition, distance);

			RunAnimation(direction); // Oikee animaatio tulil
			if (_currentSpeed > 0) {
				AudioStreamPlayer2D footStepPlayer = GetNode<AudioStreamPlayer2D>("FootStepPlayer");
				if (!footStepPlayer.Playing) { // Jalanjälkiäänet (kun ei oo aiempi menos)
					footStepPlayer.Play();
				}
			} else {
				AudioStreamPlayer2D footStepPlayer = GetNode<AudioStreamPlayer2D>("FootStepPlayer");
				footStepPlayer.Stop(); // Stop ku stop
			}


			if (GlobalPosition.DistanceTo(targetPosition) < distance) {
				GlobalPosition = targetPosition;
				currentTargetIndex++;

				if (currentTargetIndex >= path.Count) {
					isMoving = false;
					if (!atTäplä) {
						GameOver(); // jos ei saavuta täplää ku reitti loppuu, häviö
					}
			}
			}
		}

		public void Move(Vector2I[] inputPath) {
			if (inputPath.Length < 2) return; // jos reitti alle 2, ei mennä

			path.Clear();
			foreach (var coord in inputPath)
				path.Add(new Vector2(coord.X * 32, coord.Y * 32));

			currentTargetIndex = 0;
			isMoving = true;
		}

		public void controlSpeed(float speed) {
			_currentSpeed = speed;
		}

		private void RunAnimation(Vector2 direction) {
			if (celebrationTime) {
				Play("celebrate");
				return;
			}
			if (_currentSpeed == 0) { // Idle animation
				switch ((_lastDirection.X, _lastDirection.Y)) {
					case (0, 1): Play("idledown"); break;
					case (0, -1): Play("idleup"); break;
					case (-1, 0): Play("idleleft"); break;
					case (1, 0): Play("idleright"); break;
					default: Play("idledown"); break;
				}
				return;
			}

			_lastDirection = direction; // Vika suunta idlee varten



			switch ((direction.X, direction.Y)) {
				case (0, 1): Play("walk"); break;
				case (0, -1): Play("walkup"); break;
				case (-1, 0): Play("walkleft"); break;
				case (1, 0): Play("walkright"); break;
			}

		}

		public void Celebrate() {
			celebrationTime = true; // called ku täplää osuu
		}

		private void OnBodyEntered(Node body) {
			GD.Print($"current body: {body.Name}");
			string bodyName = body.Name;

			if (body is TileMapLayer tileMapLayer) {
				GD.Print($"Current tilemaplayer: {bodyName}");

				if (bodyName.StartsWith("Suojatie")) {
					GD.Print("Pelaaja suojatiellä", bodyName);
					onSuojaTiellä = true;
					suojaTieNimi = bodyName;
					suojaTieTimer.Start();
					return;
				}

				if (tileMapLayer.Name == "Tie") {
					GD.Print("Kolariiii!");
					Touch touch = GetNode<Touch>("/root/Node2D");
					touch.Kolari(bodyName);
				} else if (tileMapLayer.Name == "Vesi") {
					GD.Print("Hukutaaaa");
					Touch touch = GetNode<Touch>("/root/Node2D");
					touch.Kolari(bodyName);
				} else {
					GD.Print("Mis vitus me ollaan");
				}
			}
		}

		private void OnBodyExited(Node body) {
			GD.Print($"Exiting body: {body.Name}");
			string bodyName = body.Name.ToString();

			if (body is TileMapLayer tileMapLayer) {
				if (bodyName.StartsWith("Suojatie")) {
					GD.Print($"Sieni is leaving the crosswalk: {bodyName}");
					onSuojaTiellä = false;
					EmitSignal(nameof(PoistuuSuojaTieltä), suojaTieNimi, onSuojaTiellä);
					suojaTieNimi = "";
				}
			}
		}

		private void OnAreaEntered(Area2D area) {
			string areaName = area.Name;
			GD.Print($"Sieni collided with: {area.Name}");

			if (area.Name == "CollisionArea2D") {
				GD.Print("Sieni touched Ötökkä!");
				Touch touch = GetNode<Touch>("/root/Node2D");
				touch.Kolari(areaName);
			}
			if (area.Name == "PunanenTie") {
				Touch touch = GetNode<Touch>("/root/Node2D");
				touch.Kolari("Tie");
			}
		}

		private void OnSuojaTieTimeout() {
			GD.Print("Timeout triggered, checking conditions...");

			if (onSuojaTiellä && _currentSpeed == 0 && canEmitSignal) {
				GD.Print($"Sieni has been on the crosswalk for {suojaTieAika} seconds. Stopping Toukka.");
				EmitSignal(nameof(PysähtynytSuojaTielle), suojaTieNimi, onSuojaTiellä);
				canEmitSignal = false;
				suojaTieDelayTimer.Start();
			} else {
				GD.Print($"Conditions not met: onSuojaTiellä = {onSuojaTiellä}, _currentSpeed = {_currentSpeed}");
			}
		}

		private void OnDelayTimeOut() {
			GD.Print("Delay passed");
			canEmitSignal = true;
			suojaTieTimer.Start();
		}

		private void GameOver() {
			GD.Print("Called");
			Touch touch = GetNode<Touch>("/root/Node2D");
			touch.Kolari("Vajaa");
		}
	}
}
