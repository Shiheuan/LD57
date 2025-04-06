using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var p = other.GetComponent<Player>();
            p.Die();
        }
    }
}
