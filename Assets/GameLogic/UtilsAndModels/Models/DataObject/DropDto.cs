using Assets.Enums;
using System;

[Serializable]
public class DropDto
{
    public ItemName ItemName;
    public int OneOfChance;

    public float GetDropChance()
    {
        return 1f / OneOfChance;
    }
}
