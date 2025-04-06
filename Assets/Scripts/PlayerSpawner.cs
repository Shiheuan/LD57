using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SpawnPlayer(transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
