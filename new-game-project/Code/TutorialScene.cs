using Godot;
using System;

public partial class TutorialScene : TextureRect
{

	[Export] Button _okButton = null;

	[Export] public Panel tutorial1 = null;
	[Export] public VideoStreamPlayer tutorial1Player = null;

	[Export] public Panel tutorial2 = null;
	[Export] public VideoStreamPlayer tutorial2Player = null;

	[Export] public Panel tutorial3 = null;
	[Export] public VideoStreamPlayer tutorial3Player = null;

	public override void _Ready()
	{
		if (tutorial1 != null) {
			tutorial1.Visible = false;
		}
		if (tutorial2 != null) {
			tutorial2.Visible = false;
		}
		if (tutorial3 != null) {
			tutorial3.Visible = false;
		}

		_okButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnOkPressed)));
	}


	public void TutorialActivated(int number) {
		GetTree().Paused = true;
		if (number == 1) {
			this.Visible = true;
			tutorial1.Visible = true;
			tutorial1Player.Play();
		}
		if (number == 2) {
			this.Visible = true;
			tutorial2.Visible = true;
			tutorial2Player.Play();
		}
		if (number == 3) {
			this.Visible = true;
			tutorial3.Visible = true;
			tutorial3Player.Play();
		}
	}

	private void OnOkPressed() {
		tutorial1.Visible = false;
		tutorial1Player.Stop();
		tutorial2.Visible = false;
		tutorial2Player.Stop();
		tutorial3.Visible = false;
		tutorial3Player.Stop();
		this.Visible = false;
		GetTree().Paused = false;

	}
	public override void _Process(double delta)
	{
	}
}
