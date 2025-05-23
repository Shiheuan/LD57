using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var levelMgr = LevelManager.Instance;
            levelMgr.nextLevel = ELevelType.Level1;
            levelMgr.StartLevel();
        }
    }
}
