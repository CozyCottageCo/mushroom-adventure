 	using Godot;
using System;
using SieniPeli;


public partial class Heijastin : Sprite2D
{
    private bool pickedUp = false;

    private Sieni _sieni;
    public override void _Ready()
    {
        _sieni = GetNode<Sieni>("/root/Node2D/Sieni");
        GetNode<Area2D>("Area2D").Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));
    }

    private void OnAreaEntered(Area2D area)
    {
        if (pickedUp) return; // ei turhia toistoja

        if (area.GetParent() is Sieni player)
        {
            pickedUp = true;
            _sieni._hasHeijastin = true;
            this.GetParent().RemoveChild(this); // sienelle heijastin true + lapseksi että pysyy mukana
            area.AddChild(this);
            this.Scale = new Vector2((float)0.1, (float)0.1);

            Position = new Vector2(-10, -40); //heijastin pieneksi ja pään päälle
        }
    }
}
