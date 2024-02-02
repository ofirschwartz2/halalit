using UnityEngine;

public abstract class ConsecutiveAttack : MonoBehaviour
{
    public abstract void StartConsecitiveAttack(Vector2 position, Quaternion rotation);
    public abstract void UpdateConsecitiveAttack(Vector2 position, Quaternion rotation);
    public abstract void StopConsecitiveAttack();
}
