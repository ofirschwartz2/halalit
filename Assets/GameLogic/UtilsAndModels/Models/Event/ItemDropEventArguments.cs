using Assets.Enums;
using UnityEngine;

public class ItemDropEventArguments : System.EventArgs
{
    public Dropper Dropper;
    public Vector2 DropPosition;
    public Vector2 DropForce;

    public ItemDropEventArguments(Dropper dropper, Vector2 dropPosition, Vector2 dropForce)
    {
        Dropper = dropper;
        DropPosition = dropPosition;
        DropForce = dropForce;
    }
}