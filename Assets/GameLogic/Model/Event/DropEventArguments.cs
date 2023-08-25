using Assets.Enums;
using UnityEngine;

public class DropEventArguments : System.EventArgs
{
    public DropType DropType;
    public Vector2 DropPosition;
    public Vector2 DropForce;
    public RangeAttribute Luck;

    public DropEventArguments(DropType dropType, Vector2 dropPosition, Vector2 dropForce, RangeAttribute luck)
    {
        DropType = dropType;
        DropPosition = dropPosition;
        DropForce = dropForce;
        Luck = luck;
    }
}