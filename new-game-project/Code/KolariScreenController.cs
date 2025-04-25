using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class KolariScreenController : Control
{
    [Export] public Button _reTryButton = null;
    [Export] public Label _whoopsLabel = null;
    [Export] public Label _hintLabel = null;
    [Export] private AudioStreamPlayer2D _nappiAudio = null;
    private string _currentCrashType = "";
    private Touch _touch = null; // tää on vaa touch.cs alustus
    public override void _Ready()
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


    public async void ReTryPressed() {
        _nappiAudio.Play();
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        GD.Print("pressed");
        _touch._kolariActive = false;

        SceneTransition sceneTransition = GetNode<SceneTransition>("/root/SceneTransition");
        sceneTransition.FadeToCurrentScene(); // taso uusiks
        }

    //kaks alla olevaa metodia määrittää touch.cs avulla kolarin tyypin ja sen mukaan kääntää tekstin oikein
    public void SetCrashType(string crashType)
    {
        _currentCrashType = crashType;
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        _reTryButton.Text = Tr("retry");

        if (_hintLabel != null)
        {
            switch (_currentCrashType)
            {
                case "Tie":
                    _hintLabel.Text = Tr("hint");
                    break;
                case "Vesi":
                    _hintLabel.Text = Tr("swim");
                    break;
                case "Ötökkä":
                    _hintLabel.Text = Tr("hint2");
                    break;
                case "Vajaa":
                    _hintLabel.Text = Tr("vajaa");
                    break;
            }
        }
        if (_whoopsLabel != null)
        {
            switch (_currentCrashType)
            {
                case "Tie":
                    _whoopsLabel.Text = Tr("whoops");
                    break;
                case "Vesi":
                    _whoopsLabel.Text = Tr("");
                    break;
                case "Ötökkä":
                    _whoopsLabel.Text = Tr("crash");
                    break;
                case "Vajaa":
                    _whoopsLabel.Text = Tr("");
                    break;
            }
        }
    }
}
}