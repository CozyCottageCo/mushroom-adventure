 	using Godot;
using System;
using SieniPeli;


public partial class Täplä : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
private SceneTree _mainMenuSceneTree = null;
private AnimationPlayer _animationPlayer;

private Sieni _sieni;
private Touch _touch = null;
	public override void _Ready()
	{
		base._Ready();
	_mainMenuSceneTree = GetTree();
	_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	_sieni = GetNode<Sieni>("/root/Node2D/Sieni");
	if (_mainMenuSceneTree == null) {
		GD.PrintErr("Ei löy'y scenetree");
	}
		_touch = GetNode<Touch>("/root/Node2D"); // root node
		GetNode<Area2D>("Area2D").Connect("area_entered", new Callable(this, nameof(OnCollected)));
	}

    public override void _Process(double delta)
    {

		_animationPlayer.Play("hover");
    }

    private async void OnCollected(Area2D area)
    {
        if (area.GetParent() is Sieni player)  // aktivoituu ku Sieni-luokan olento osuu
        {
            GD.Print("Täplä löydetty!");
			_sieni.Celebrate();
			await ToSignal(GetTree().CreateTimer(5f), "timeout");
			_touch.Voitto(); // voittometodi kutsu
			GetTree().Paused = true;

}
	}
}
