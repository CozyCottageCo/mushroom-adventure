using Godot;

public partial class Darkener : ColorRect
{
    public override void _Ready()
    {
        // ööäh turha, oli yritys korjata mobiilikosketus
        MouseFilter = MouseFilterEnum.Ignore;
    }

}
