using Godot;
using System;
using SieniPeli;

namespace SieniPeli {
public partial class GameClear : TextureRect
{

	[Export] Button _confirm = null;
	[Export] AudioStreamPlayer2D buttonSound = null;
	[Export] Label _clearedLabel = null;
	public override void _Ready()
	{
		UpdateUIText();
		_confirm.Connect(Button.SignalName.Pressed, new Callable(this, nameof(ConfirmPressed)));
	}

	private async void ConfirmPressed() {
		buttonSound.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		this.Visible = false;
	}

	private void UpdateUIText()
    {
        _confirm.Text = Tr("thanks");
		_clearedLabel.Text = Tr("cleared");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
}
