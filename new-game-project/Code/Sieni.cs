	using Godot;
	using SieniPeli;
	using System;
	using System.Collections.Generic;
	using System.Security;

	namespace SieniPeli {
	public partial class Sieni : Sprite2D
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

				private Vector2 _direction = Vector2.Zero; // movement initialized at zero (no movement)
				private Grid grid; //initialize grid

				private List<Vector2> path = new List<Vector2>(); // alustetaan reittikordinaatti-lista
				private int currentTargetIndex = 0; // alustetaan tänhetkinen kohdekordinaatti-indeksi
				public bool isMoving = false; // alustetaan ettei liikuta

				private float _currentSpeed = 50f; // alustettu defaultnopeus

				private AnimationPlayer _animationPlayer;
				private Vector2 _lastDirection = Vector2.Zero;
		public override void _Ready()
			{
			PackedScene gridScene = ResourceLoader.Load<PackedScene>(_gridScenePath);
			grid = (Grid)gridScene.Instantiate(); // load&instantiate grid, place snake
			initialPosition = Position;
       		currentPosition = initialPosition;
			Position = initialPosition;
			GlobalPosition = initialPosition;
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer"); // alustetaa animationplayer
			GD.Print(GlobalPosition);


			GetNode<Area2D>("Area2D").BodyEntered += OnBodyEntered; // tää monitoroi millon kollisio tapahtuu tilemaplayeri kans
			GetNode<Area2D>("Area2D").BodyExited += OnBodyExited;
			GetNode<Area2D>("Area2D").AreaEntered += OnAreaEntered; // tää area2d (töl hetkel toukka)

			suojaTieTimer = new Timer();
        	suojaTieTimer.WaitTime = suojaTieAika;
       		suojaTieTimer.OneShot = true;
        	suojaTieTimer.Timeout += OnSuojaTieTimeout;
        	AddChild(suojaTieTimer);

			suojaTieDelayTimer = new Timer();
        	suojaTieDelayTimer.WaitTime = suojaTieDelayAika;
       		suojaTieDelayTimer.OneShot = true;
        	suojaTieDelayTimer.Timeout += OnDelayTimeOut;
        	AddChild(suojaTieDelayTimer);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			// jos ei liiku tai ei reittiä tai kohdeindeksi > reitin koko, älä tee mitään
			if (!isMoving || path.Count == 0 || currentTargetIndex >= path.Count)
			return;

			// kohdekoordinaatti = reittiarrayn tänhetkinen indeksi koordinaatti
			Vector2 targetPosition = path[currentTargetIndex];
			Vector2 direction = (targetPosition - GlobalPosition).Normalized(); // suunta näiden erotus
			float distance = _currentSpeed * (float)delta; // kuinka pal liikutaan (freimiä sekunnissa x framet tjsp)
			GlobalPosition = GlobalPosition.MoveToward(targetPosition, distance); // positio liikkuu suunnan x etäisyyden
			RunAnimation(direction); // ajetaan liikkues animaatioo

			if (GlobalPosition.DistanceTo(targetPosition) < distance) { // jos tarpeeks lähel kohdetta, asetetaan et ollaan perillä (ettei junnaa)
				GlobalPosition = targetPosition;
				currentTargetIndex++; // kohdeindeksi kasvaa kun aiempi kohde saavutettu

				if (currentTargetIndex >= path.Count) { // jos indeksi isompi tai sama kuin reittilistan kohdekoordinaattien määrä
					isMoving = false; // ei liikuta ennöö
				}
			}

		}



		public void Move(Vector2I[] inputPath) // sienen liikutusmetodi, saa vectoriarrayn
		{
			if (inputPath.Length <2) return; // ei tee mitn jos array on vaan 1 koordinaatti

			path.Clear(); // tyhjentää reitin
			foreach (var coord in inputPath) // lisää jokaisen annetun reittiarrayn reittikoordinaatin uuteen reittiin
				path.Add(new Vector2(coord.X * 32, coord.Y * 32));

			currentTargetIndex = 0; // nollaa kohdekoordinaatti-indeksin että alotetaan alusta
			isMoving = true; // lähtee liikkuu

		}

		public void controlSpeed (float speed) { // metodi jolle laitetaan kosketuksesta nopeus jotta hallitaan sitä täällä
			_currentSpeed = speed;
		GD.Print($"controlSpeed(): Speed set to {_currentSpeed}");

		}



		private void OnBodyEntered(Node body) // träkkää collisioneita (täplä träkkää itse)
	{
		GD.Print($"current body: {body.Name}");
		string bodyName = body.Name.ToString();

		if (body is TileMapLayer tileMapLayer) // jos kollisio tapahtuu tilemaplayerin kanssa..
		{
			GD.Print($"Current tilemaplayer: {bodyName}"); // mikä sellanen

			if (bodyName.StartsWith("Suojatie")) // jos suojatie, sillä selvä
			{
				GD.Print("Pelaaja suojatiellä", bodyName);
				onSuojaTiellä = true;
				suojaTieNimi = bodyName;

				suojaTieTimer.Start();


				return;  // tän pitäs lopettaa tsekkaus mut jos oli päällekkäin otti silti sen tien sieltä alta.
			}

			if (tileMapLayer.Name == "Tie") // tän pitäs tsekkaa vaan jos suojatie ei blokannu tätä, mut tapahtu silti
			{
				GD.Print("Kolariiii!");
				Touch touch = GetNode<Touch>("/root/Node2D"); // öäh main nodesta taas otetaan ratkasu
				touch.Kolari();  // kolarikutsu
			}
			else
			{
				GD.Print("Mis vitus me ollaan"); // out of bounds
			}
		}
	}

	private void OnBodyExited(Node body) {
		GD.Print($"Exiting body: {body.Name}");
    	string bodyName = body.Name.ToString();

    if (body is TileMapLayer tileMapLayer) // Check if it's a TileMapLayer
    {
        if (bodyName.StartsWith("Suojatie")) // Check if it's a crosswalk
        {
            GD.Print($"Sieni is leaving the crosswalk: {bodyName}");
            onSuojaTiellä = false; // Sieni is no longer on the crosswalk
			EmitSignal(nameof(PoistuuSuojaTieltä), suojaTieNimi, onSuojaTiellä);
            suojaTieNimi = ""; // Clear crosswalk name

        }
    }
}

	private void OnAreaEntered(Area2D area)
	{
		GD.Print($"Sieni collided with: {area.Name}");

		if (area.GetType().Name == "CollisionArea2D") // purkkaratkasu
		{
			GD.Print("Sieni touched Ötökkä!");
			Touch touch = GetNode<Touch>("/root/Node2D");
			touch.Kolari();
		}
	}

	private void OnSuojaTieTimeout() {
    GD.Print("Timeout triggered, checking conditions...");

    if (onSuojaTiellä && _currentSpeed == 0 && canEmitSignal)
    {
        GD.Print($"Sieni has been on the crosswalk for {suojaTieAika} seconds. Stopping Toukka.");

            // Emit the signal after the timeout period (timer reset)
            EmitSignal(nameof(PysähtynytSuojaTielle), suojaTieNimi, onSuojaTiellä);
			canEmitSignal = false;
			suojaTieDelayTimer.Start();
             // Restart the timer to ensure it doesn't keep firing too quickly


    }
    else
    {
        GD.Print($"Conditions not met: onSuojaTiellä = {onSuojaTiellä}, _currentSpeed = {_currentSpeed}");
    }
}

	private void OnDelayTimeOut() {
		GD.Print("Delay passed");
		canEmitSignal = true;
		suojaTieTimer.Start();
	}




	private void RunAnimation(Vector2 direction) { // animaatiokoodi
		if (_currentSpeed == 0) { // jos nopeus = 0 eli ollaan paikallaan, odotellaan suojatiellä tjsp
			switch ((_lastDirection.X, _lastDirection.Y)) { // huom viimesen liikkeen suunta
				case (0, 1):
					_animationPlayer.Play("idledown"); // jokaseen oma idle"animaatio" per suunta
					break;
				case (0, -1):
					_animationPlayer.Play("idleup");
					break;

				case (-1, 0):
					_animationPlayer.Play("idleleft");
					break;

				case (1, 0):
					_animationPlayer.Play("idleright");
					break;
				default:
					_animationPlayer.Play("idledown"); // Default idle ales
					break;
			}
			return;
		}

		_lastDirection = direction; // täsä kohtaa (viimesen liikkeen jälkee niiq) asetetaan viimenen suunta idlelle

		switch ((direction.X, direction.Y)) { // suoraan direction-vectorista otetaa vaa suunta, ja sen mukanen animaatio tulille
			case (0, 1):
				_animationPlayer.Play("walk");
				break;
			case (0, -1):
				_animationPlayer.Play("walkup");
				break;
			case (-1, 0):
				_animationPlayer.Play("walkleft");
				break;
			case (1, 0):
				_animationPlayer.Play("walkright");
				break;
		}
	}

    }
	}
