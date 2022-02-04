using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRateItemController : MonoBehaviour
{

    public void LoadItem(GameObject gun)
    {
        gun.SendMessage("FasterCooldownInterval");
    }
}
