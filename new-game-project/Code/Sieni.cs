using Godot;
using SieniPeli;
using System;
using System.Collections.Generic;

public partial class Sieni : Sprite2D
{

	[Export] private string _gridScenePath = "res://Level/Grid.tscn";
	[Export] private float _speed = 100f; // speed setting in editor (does nothing now rly)
			private Vector2 _direction = Vector2.Zero; // movement initialized at zero (no movement)
			private Grid grid; //initialize grid

			private List<Vector2> path = new List<Vector2>(); // alustetaan reittikordinaatti-lista
			private int currentTargetIndex = 0; // alustetaan tänhetkinen kohdekordinaatti-indeksi
			private bool isMoving = false; // alustetaan ettei liikuta
	public override void _Ready()
	{
		PackedScene gridScene = ResourceLoader.Load<PackedScene>(_gridScenePath);
		grid = (Grid)gridScene.Instantiate(); // load&instantiate grid, place snake
		GlobalPosition = new Vector2(4,4); // örh alotuskoordinaatti ainakin marginaalien verran nurkasta (emt onko tällä nii väliä)
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
		float distance = _speed * (float)delta; // kuinka pal liikutaan (freimiä sekunnissa x framet tjsp)

		GlobalPosition += direction * distance; // positio liikkuu suunnan x etäisyyden

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
			path.Add(new Vector2(coord.X * 24, coord.Y * 24));

		currentTargetIndex = 0; // nollaa kohdekoordinaatti-indeksin että alotetaan alusta
		isMoving = true; // lähtee liikkuu

    }

	public void RotateHead(int times){
		RotationDegrees = 90 * times; //rotate head 90 degrees times direction specific int. (-1 left, 1 right, 2 down, 0 up)
	}
}
