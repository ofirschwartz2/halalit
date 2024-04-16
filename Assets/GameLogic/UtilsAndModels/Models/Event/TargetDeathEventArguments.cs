public class TargetDeathEventArguments : System.EventArgs
{
    public float Scale;

    public TargetDeathEventArguments(float scale)
    {
        Scale = scale;
    }
}
