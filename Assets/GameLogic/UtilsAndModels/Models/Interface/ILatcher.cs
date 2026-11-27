namespace Assets.Models
{
    // Something that latches onto the Halalit (Leech, #215). Its touch does no harm while it can latch or is latched.
    public interface ILatcher
    {
        bool IsHarmlessTouch();
    }
}
