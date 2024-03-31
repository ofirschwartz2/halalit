using Assets.Enums;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ValuableDropper : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ValuableName, GameObject>> _valuablesBank;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DropEvent.NewDrop += DropNewValuable;
    }
    #endregion


    #region Drop
    private void DropNewValuable(object initiator, DropEventArguments dropEventArguments)
    {
        var valuable = Instantiate(
            _valuablesBank.Find(a => a.Key == dropEventArguments.ValuableName).Value,
            dropEventArguments.DropPosition, Quaternion.identity);

        valuable.GetComponent<Rigidbody2D>().AddForce(dropEventArguments.DropForce, ForceMode2D.Impulse);
    }
    #endregion
}
