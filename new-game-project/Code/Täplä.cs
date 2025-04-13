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
[Export] AudioStreamPlayer2D _voittoAudio = null;
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
			_sieni.atTäplä = true;
			_sieni.controlSpeed(0);
			MusicPlayer musicplayer = GetNode<MusicPlayer>("/root/MusicPlayer");
			musicplayer.currentPlayer.StreamPaused = true;
			_sieni.Celebrate();
			_voittoAudio.Play();
			_voittoAudio.Finished += () => {
				GD.Print("Victory audio finished signal received.");
			musicplayer.currentPlayer.StreamPaused = false;
			};
			string currentLevelPath = GetTree().CurrentScene.SceneFilePath;
			string currentLevel = currentLevelPath.GetFile().GetBaseName(); //katotaan tason nimi missä ollaan
			var saveManager = GetNode<SaveManager>("/root/SaveManager");
			saveManager.MarkLevelCompleted(currentLevel); // savemanageriin tason nimi tallennusta varten tehdyksi
			await ToSignal(GetTree().CreateTimer(5f), "timeout");
			_touch.Voitto(); // voittometodi kutsu
			GetTree().Paused = true;

}
	}
}
