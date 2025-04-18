using Godot;
using System;

public partial class TutorialScene : TextureRect
{

	[Export] Button _okButton = null;
	[Export] public Label _tutorialabel1 = null;
	[Export] public Label _tutorialabel2 = null;
	[Export] public Label _tutorialabel3 = null;
	[Export] public Label _tutorialabel4 = null;

	[Export] public Panel tutorial1 = null;
	[Export] public VideoStreamPlayer tutorial1Player = null;

	[Export] public Panel tutorial2 = null;
	[Export] public VideoStreamPlayer tutorial2Player = null;

	[Export] public Panel tutorial3 = null;
	[Export] public VideoStreamPlayer tutorial3Player = null;

	[Export]public Panel tutorial4 = null;
	[Export] public VideoStreamPlayer tutorial4Player = null;
	[Export] AudioStreamPlayer2D _okAudio = null;

	public override void _Ready()
	{
		UpdateUIText();

		if (tutorial1 != null) {
			tutorial1.Visible = false;
		}
		if (tutorial2 != null) {
			tutorial2.Visible = false;
		}
		if (tutorial3 != null) {
			tutorial3.Visible = false;
		}
		if (tutorial4 != null) {
			tutorial4.Visible = false;
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
		if (number == 4) {
			this.Visible = true;
			tutorial4.Visible = true;
			tutorial4Player.Play();
		}
	}

	private async void OnOkPressed() {
		_okAudio.Play();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		tutorial1.Visible = false;
		tutorial1Player.Stop();
		tutorial2.Visible = false;
		tutorial2Player.Stop();
		tutorial3.Visible = false;
		tutorial3Player.Stop();
		tutorial4.Visible = false;
		tutorial4Player.Stop();
		this.Visible = false;
		GetTree().Paused = false;

	}

	private void UpdateUIText()
    {
    	_tutorialabel1.Text = Tr("tutorial1");
		_tutorialabel2.Text = Tr("tutorial2");
		_tutorialabel3.Text = Tr("tutorial3");
		_tutorialabel4.Text = Tr("tutorial4");
    }
	public override void _Process(double delta)
	{
	}
}
