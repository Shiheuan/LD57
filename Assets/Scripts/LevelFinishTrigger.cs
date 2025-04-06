using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinishTrigger : MonoBehaviour
{
    public ELevelType nextLevel;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // how to show
            GameManager.Instance.ShowEndScreen(nextLevel);
            //LevelManager.Instance.nextLevel = nextLevel;
            //LevelManager.Instance.StartLevel();
        }
    }
}
