 	using Godot;
using System;
using SieniPeli;


public partial class Täplä : Area2D
{
	// Called when the node enters the scene tree for the first time.
private SceneTree _mainMenuSceneTree = null;

private Touch _touch = null;
	public override void _Ready()
	{
		base._Ready();
	_mainMenuSceneTree = GetTree();
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
		_touch = GetNode<Touch>("/root/Node2D"); // root node
		Connect("area_entered", new Callable(this, nameof(OnCollected))); // sisäänastumisen connecti
	}

	private void OnCollected(Area2D area)
    {
        if (area.GetParent() is Sieni player)  // aktivoituu ku Sieni-luokan olento osuu
        {
            GD.Print("Täplä löydetty!");
			_touch.Voitto(); // voittometodi kutsu

}
	}
}
