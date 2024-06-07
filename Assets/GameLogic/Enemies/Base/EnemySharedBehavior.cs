using Assets.Utils;

public class EnemySharedBehavior : SeedfulRandomGeneratorUser
{
    private void FixedUpdate()
    {
        if (!Utils.IsCenterInsideTheWorld(gameObject) && !Utils.IsCenterInExternalSafeIsland(transform.position))
        {
            Destroy(gameObject); 
        }
    }
}