using Godot;
using System;

namespace SieniPeli {
    public partial class CrossWalkManager : Node
    {
        private Sieni sieni = null;
        private string sieniSuojaTie = "";
        private bool sieniSuojaTiellä = false;

        private Timer resetTimer;  //  sieniSuojaTie reset timeri
        private const float RESET_TIME = 8.0f;  // 8s odotus

        public bool CrossWalkOccupied => sieniSuojaTiellä;
        public string CurrentCrossWalk => sieniSuojaTie;

        public bool StoppedByLight { get; private set; } = false;

        public override void _Ready()
        {
            CallDeferred(nameof(InitializeSieniNode)); // varmistetaan et sieni eka skenes

            // Initialize the timer
            resetTimer = new Timer();
            AddChild(resetTimer);
            resetTimer.Timeout += OnResetTimerTimeout;  // timer pääl ja kii
        }

        private void InitializeSieniNode()
        {
            sieni = GetNode<Sieni>("/root/Node2D/Sieni");
            if (sieni != null)
            {
                GD.Print("Sieni node found.");
                sieni.Connect("PysähtynytSuojaTielle", new Callable(this, nameof(OnSieniStoppedAtCrosswalk)));
            sieni.Connect("PoistuuSuojaTieltä", new Callable(this, nameof(OnSieniLeftCrossWalk)));
            }
            else
            {
                GD.Print("Sieni node not found.");
            }
        }

        public override void _Process(double delta)
        {

        }

        private void OnSieniStoppedAtCrosswalk(string suojatienimi, bool onSuojaTiellä)
        {
            sieniSuojaTie = suojatienimi;
            sieniSuojaTiellä = onSuojaTiellä;

            // kun tulee signaali et sieni suojatiellä, timer päälle
            resetTimer.Start(RESET_TIME);

            GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus"); // ötököille käsky tsekkaa tilanne
        }

        private void OnSieniLeftCrossWalk(string suojatienimi, bool onSuojaTiellä)
        {
            // Timer pois ku sieni lähtee suojatieltä
            resetTimer.Stop();

            sieniSuojaTiellä = onSuojaTiellä;
         //   GD.Print($"Sieni left crosswalk {suojatienimi}, current {sieniSuojaTiellä}");
            sieniSuojaTie = "";

            if (!sieniSuojaTiellä)
            {
              //  GD.Print("Sieni left crosswalk, Toukka can move.");
                GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus"); // kehotus tsekkaa taas
            }
        }

        // Jos sieni ei 8s päästä oo lähteny suojatieltä, timer loppuu
        private void OnResetTimerTimeout()
        {
            GD.Print("Sieni didn't leave crosswalk in time. Resetting sieniSuojaTie...");
            sieniSuojaTie = ""; // Sienisuojatie resettaa (kunnes uusi signaali tulee)
            GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus"); // kattokaa ny
        }
    }
}
