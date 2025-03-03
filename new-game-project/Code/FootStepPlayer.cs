using Godot;

public partial class FootStepPlayer : AudioStreamPlayer2D
{
    public void PlaySound()
    {
        GD.Print("Footstep sound triggered!"); // Debugging
        Play(); // Play the footstep sound
    }
}
