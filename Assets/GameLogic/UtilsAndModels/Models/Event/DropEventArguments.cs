using Assets.Enums;
using UnityEngine;

public class DropEventArguments : System.EventArgs
{
    public Dropper Dropper;
    public Vector2 DropPosition;
    public Vector2 DropForce;

    public DropEventArguments(Dropper dropper, Vector2 dropPosition, Vector2 dropForce)
    {
        Dropper = dropper;
        DropPosition = dropPosition;
        DropForce = dropForce;
    }
}