using UnityEngine;

public class SpeedLimiter : MonoBehaviour
{
    [SerializeField]
    private float _maxSpeed;

    private static float _staticMaxSpeed;

    void Awake()
    {
        _staticMaxSpeed = _maxSpeed;
    }

    public static void LimitSpeed(Rigidbody2D rigidbody)
    {
        if (rigidbody.isKinematic)
        {
            LimitKinematicSpeed(rigidbody);
        }
        else
        {
            LimitDinamicSpeed(rigidbody);
        }
    }

    private static void LimitKinematicSpeed(Rigidbody2D rigidbody)
    {
        KinematicMovement kinematicMovement = rigidbody.gameObject.GetComponent<KinematicMovement>();

        if (kinematicMovement != null)
        {
            float KinematicSpeed = kinematicMovement.GetSpeed();

            if (KinematicSpeed > 0 && KinematicSpeed > _staticMaxSpeed)
            {
                kinematicMovement.SetSpeed(_staticMaxSpeed);
            }

            if (KinematicSpeed < 0 && KinematicSpeed < _staticMaxSpeed)
            {
                kinematicMovement.SetSpeed(-_staticMaxSpeed);
            }
        }
    }

    private static void LimitDinamicSpeed(Rigidbody2D rigidbody)
    {
        if (rigidbody.velocity.magnitude > 0 && rigidbody.velocity.magnitude > _staticMaxSpeed)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * _staticMaxSpeed;
        }

        if (rigidbody.velocity.magnitude < 0 && rigidbody.velocity.magnitude < _staticMaxSpeed)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * -_staticMaxSpeed;
        }
    }

    public static bool IsAllowedSpeed(float speed)
    {
        return speed <= _staticMaxSpeed;
    }

}
