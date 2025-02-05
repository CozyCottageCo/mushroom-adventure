using Godot;
using SieniPeli;
using System;

public partial class Sieni : Sprite2D
{

	[Export] private string _gridScenePath = "res://Level/Grid.tscn";
	[Export] private float _speed = 0; // speed setting in editor (does nothing now rly)
			private Vector2 _direction = Vector2.Zero; // movement initialized at zero (no movement)
			private Grid grid; //initialize grid
	public override void _Ready()
	{
		PackedScene gridScene = ResourceLoader.Load<PackedScene>(_gridScenePath);
		grid = (Grid)gridScene.Instantiate(); // load&instantiate grid, place snake
		GlobalPosition = new Vector2(4,4);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}



	public void Move(Vector2[] path)
    {
		if (_direction != Vector2.Zero) { // if not staying still, add direction to current position on grid
        Vector2I targetPosition = (Vector2I)(GlobalPosition / grid._cellSize) + (Vector2I)_direction; //esim 112,64 -> 7,4 + 0,1 -> 7,5 ruutu, toimis toistenkipäi I guess (+ 0, 16 = 112,82)
        Vector2 worldPosition; // initiate worldposition

        // Call GetWorldPosition to get the world position for the target (and check its legal)
        if (grid.GetWorldPosition(targetPosition, out worldPosition))
        {
            GlobalPosition = worldPosition; // assign new position based on above
        }
    }
	}

	public void RotateHead(int times){
		RotationDegrees = 90 * times; //rotate head 90 degrees times direction specific int. (-1 left, 1 right, 2 down, 0 up)
	}
}
