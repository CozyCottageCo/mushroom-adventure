using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class KolariScreenController : Control
{
[Export] public Button _reTryButton = null;



private Touch _touch = null; // tää on vaa touch.cs alustus
public override void _Ready() {
{
   base._Ready();
    _touch = GetNode<Touch>("/root/Node2D");

    GD.Print("KolariScreenController Ready");

    if (_reTryButton != null)
    {
        GD.Print("Retry Button exists.");
    }
    else
    {
        GD.PrintErr("Error: Retry Button is NULL.");
    }

    _reTryButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(ReTryPressed)));
}
}


	public void ReTryPressed() {
GetTree().Paused = false; // paussi pois, resetataan taso
		GD.Print("pressed");
		_touch._kolariActive = false;

		GetTree().ReloadCurrentScene();
	}


	}
}
