using Assets.Enums;

public class ItemsBankEventArguments : System.EventArgs
{
    public ItemName ItemName;

    public ItemsBankEventArguments(ItemName itemName)
    {
        ItemName = itemName;
    }
}