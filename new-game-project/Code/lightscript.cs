using Godot;
using System;

public partial class lightscript : Node2D
{
    [Export] private ColorRect _darknessOverlay; // itse "pimeys" laatikko
    [Export] private Node2D _lightSource; // mistä valo tulee
    [Export] private float _radius = 0.2f; // valon alue

    public override void _Process(double delta)
    {
        if (_darknessOverlay?.Material is ShaderMaterial shader && _lightSource != null) // katotaa et shader asetettu
        {
            Vector2 screenSize = GetViewport().GetVisibleRect().Size; // ruudun koko
            Vector2 lightPos = _lightSource.GlobalPosition / screenSize; // valon positio normalisoituna (eli 0-1 välil)

			float aspect_ratio = (float)(640.0 / 360.0); // aspect ratio 1,7777778 jtn

			Vector2 lightPosFix = new Vector2(lightPos.X * aspect_ratio, lightPos.Y); // korjataa valon positio (olettaa neliön, ei oo)

            shader.SetShaderParameter("light_position", lightPosFix); // välitetää shaderille tiedot, hoitaa laskelmat shaderissa
            shader.SetShaderParameter("light_radius", _radius);
			shader.SetShaderParameter("screen_size", screenSize);
        }
    }
}
