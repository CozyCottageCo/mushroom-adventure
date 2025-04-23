using Godot;

public partial class Darkener : ColorRect
{
    public override void _Ready()
    {
        // Even if it's visually on top, mouse filter is set
        MouseFilter = MouseFilterEnum.Ignore;
    }

}
