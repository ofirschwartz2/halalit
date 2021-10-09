using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{
    public Transform firePoint;
    public GameObject shotPrefab;
    void Update()
    {
        if (Input.GetButtonDown("Fire1")){
            shoot();
        }
    }
    void shoot() {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
    }
}
