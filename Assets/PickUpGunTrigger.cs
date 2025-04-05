using System;
using System.Collections;
using System.Collections.Generic;
using MCommon.Unity.Utils;
using UnityEngine;

public class PickUpGunTrigger : MonoBehaviour
{
    public Transform pickUp;
    public Light pickUpLight;

    public float RotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (pickUp != null)
        {
            pickUp.Rotate(Vector3.up, Time.deltaTime*RotateSpeed);
            pickUpLight.intensity = 2 * (pickUp.rotation.eulerAngles.y % 18f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            player.ShowGun(true);
            // update jump count
            player.settings.jumpCount = 2;
            
            gameObject.SetGameObjectActive(false);
        }
    }
}
