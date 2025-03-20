using Godot;
using System;

namespace SieniPeli {
    public partial class CrossWalkManager : Node
    {
        private Sieni sieni = null;
        private string sieniSuojaTie = "";
        private bool sieniSuojaTiellä = false;

        private Timer resetTimer;  // Timer to reset sieniSuojaTie
        private const float RESET_TIME = 8.0f;  // 8 seconds timeout

        public bool CrossWalkOccupied => sieniSuojaTiellä;
        public string CurrentCrossWalk => sieniSuojaTie;

        public override void _Ready()
        {
            CallDeferred(nameof(InitializeSieniNode));

            // Initialize the timer
            resetTimer = new Timer();
            AddChild(resetTimer);
            resetTimer.Timeout += OnResetTimerTimeout;  // Connect the timeout signal to the method
        }

        private void InitializeSieniNode()
        {
            sieni = GetNode<Sieni>("/root/Node2D/Sieni"); // Ensure you reference the correct node
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
            // GD.Print(CrossWalkOccupied, CurrentCrossWalk);
        }

        private void OnSieniStoppedAtCrosswalk(string suojatienimi, bool onSuojaTiellä)
        {
            sieniSuojaTie = suojatienimi;
            sieniSuojaTiellä = onSuojaTiellä;
            GD.Print($"Sieni stopped at crosswalk: {suojatienimi}, Is at crosswalk: {onSuojaTiellä}");

            // Start the timer when Sieni stops at the crosswalk
            resetTimer.Start(RESET_TIME);

            GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus");
        }

        private void OnSieniLeftCrossWalk(string suojatienimi, bool onSuojaTiellä)
        {
            // Stop the timer when the Sieni leaves the crosswalk
            resetTimer.Stop();

            sieniSuojaTiellä = onSuojaTiellä;
            GD.Print($"Sieni left crosswalk {suojatienimi}, current {sieniSuojaTiellä}");
            sieniSuojaTie = "";

            if (!sieniSuojaTiellä)
            {
                GD.Print("Sieni left crosswalk, Toukka can move.");
                GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus");
            }
        }

        // Timer timeout method to reset the sieniSuojaTie after 8 seconds if not triggered
        private void OnResetTimerTimeout()
        {
            GD.Print("Sieni didn't leave crosswalk in time. Resetting sieniSuojaTie...");
            sieniSuojaTie = ""; // Reset the crosswalk tie after timeout
            GetTree().CallGroup("Ötökkä", "CheckCrossWalkStatus"); // Ensure the Ötökkä group checks the status
        }
    }
}
