using Godot;
using System;
using SieniPeli;

namespace SieniPeli {
public partial class GameClear : TextureRect
{

	[Export] Button _confirm = null;
	[Export] AudioStreamPlayer2D buttonSound = null;
	public override void _Ready()
	{
		_confirm.Connect(Button.SignalName.Pressed, new Callable(this, nameof(ConfirmPressed)));
	}

	private async void ConfirmPressed() {
		buttonSound.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		this.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
}
