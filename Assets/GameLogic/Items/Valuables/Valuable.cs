using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public abstract class Valuable : MonoBehaviour
{
    protected ValuableName _valuableName;

    private void OnTriggerEnter2D(Collider2D other) // TODO: Move to a Collectable Common class?
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerValuablePickedUp(this, new(_valuableName));
            Destroy(gameObject);
        }
    }

    protected void OnPlayerValuablePickedUp(object initiator, ValuableEventArguments arguments)
    {
        ValuableEvent.Invoke(EventName.PLAYER_VALUABLE_PICKUP, initiator, arguments);
    }

#if UNITY_EDITOR
    internal ValuableName GetValuableName()
    {
        return _valuableName;
    }
#endif
}
