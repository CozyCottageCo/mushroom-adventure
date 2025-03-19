using Godot;
using System;
using System.Collections.Generic;
using SieniPeli;

namespace SieniPeli {
public partial class CrossWalkManager : Node
{
    // Movement constants
     // normalSpeed at which the car moves (pixels per second)

    private Sieni sieni = null;
 	private string sieniSuojaTie = "";
    private bool sieniSuojaTiellä = false;

	public bool CrossWalkOccupied => sieniSuojaTiellä;
	public string CurrentCrossWalk => sieniSuojaTie;

    public override void _Ready()
    {

        CallDeferred(nameof(InitializeSieniNode));

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
		//GD.Print(CrossWalkOccupied, CurrentCrossWalk);

    }



        private void OnSieniStoppedAtCrosswalk(string suojatienimi, bool onSuojaTiellä)
        {
            sieniSuojaTie = suojatienimi;
            sieniSuojaTiellä = onSuojaTiellä;
            GD.Print($"Sieni stopped at crosswalk: {suojatienimi}, Is at crosswalk: {onSuojaTiellä}");
			GetTree().CallGroup("Toukka", "CheckCrossWalkStatus");
        }


    private void OnSieniLeftCrossWalk (string suojatienimi, bool onSuojaTiellä) {
            sieniSuojaTiellä = onSuojaTiellä;
            GD.Print($"Sieni left crosswalk {suojatienimi}, current {sieniSuojaTiellä}");
            sieniSuojaTie = "";

			 if (!sieniSuojaTiellä)
        {
            GD.Print("Sieni left crosswalk, Toukka can move.");
            GetTree().CallGroup("Toukka", "CheckCrossWalkStatus");
		}
        }
    }
}