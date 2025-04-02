using Godot;
using System;
using SieniPeli;
namespace SieniPeli {

public partial class KolariScreenController : Control
{
    [Export] public Button _reTryButton = null;
    [Export] public Label _whoopsLabel = null;
    [Export] public Label _hintLabel = null;
    [Export] public Label _hint2Label = null;
    [Export] public Label _crashLabel = null;


    private Touch _touch = null; // tää on vaa touch.cs alustus
    public override void _Ready()
    {
        ConfigFile config = new ConfigFile();
        if (config.Load("user://settings.cfg") == Error.Ok)
        {
            int savedLanguage = (int)config.GetValue("Settings", "Language", 0);
            string locale = savedLanguage == 0 ? "en" : "fi";
            TranslationServer.SetLocale(locale);
        }

        // Update UI text translations
        UpdateUIText();

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


    public void ReTryPressed() {
        GetTree().Paused = false; // paussi pois, resetataan taso
        GD.Print("pressed");
        _touch._kolariActive = false;

        GetTree().ReloadCurrentScene();
        }
        private void UpdateUIText()
        {
            _reTryButton.Text = Tr("retry");
            _whoopsLabel.Text = Tr("whoops");
            _hintLabel.Text = Tr("hint");
            _crashLabel.Text = Tr("crash");
            _hint2Label.Text = Tr("hint2");
        }
    }
}
