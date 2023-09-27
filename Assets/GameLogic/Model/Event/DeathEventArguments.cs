public class DeathEventArguments : System.EventArgs
{
    public float Scale;

    public DeathEventArguments(float scale)
    {
        Scale = scale;
    }
}
