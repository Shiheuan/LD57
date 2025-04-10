using System;
using System.Collections;
using System.Collections.Generic;
using MCommon.Unity.Utils;
using UnityEngine;

public enum ELevelType
{
    None = -1,
    Intro,
    Level1,
    Level2,
    Level3,
    Endless,
}
public class LevelManager : MonoBehaviour
{
    public ELevelType nextLevel = ELevelType.None;
    private static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LevelManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("[S]LevelManager");
                    instance = obj.AddComponent<LevelManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public GameObject Intro;
    public GameObject IntroSpawner;
    public GameObject Level_1;
    public GameObject Level_1_Spawner;
    public GameObject Level_2;
    public GameObject Level_2_Spawner;
    public GameObject Level_3;
    public GameObject Level_3_Spawner;
    public GameObject Endless;
    public GameObject Endless_Spawner;

    public void StartLevel()
    {
        if (nextLevel == ELevelType.None)
        {
            nextLevel = ELevelType.Intro;
        }
        
        AudioManager.Instance.StopAllSounds();
        Transform spawner = null;
        string bgm = string.Empty;
        var ui = FindObjectOfType<ShowInfo>();
        if (Intro != null)
            Intro.SetGameObjectActive(nextLevel == ELevelType.Intro);
        if (Level_1 != null)
            Level_1.SetGameObjectActive(nextLevel == ELevelType.Level1);
        if (Level_2 != null)
            Level_2.SetGameObjectActive(nextLevel == ELevelType.Level2);
        if (Level_3 != null)
            Level_3.SetGameObjectActive(nextLevel == ELevelType.Level3);
        if (Endless != null)
            Endless.SetGameObjectActive(nextLevel == ELevelType.Endless);
        
        ui.ScoreRoot.gameObject.SetGameObjectActive(nextLevel != ELevelType.Intro);
        
        GameManager.Instance.RespawnAllEnemies();
        
        switch (nextLevel)
        {
            case ELevelType.Intro:
                spawner = IntroSpawner.transform;
                bgm = "intro";
                GameManager.Instance.DepthBaseline = 40;
                break;
            case ELevelType.Level1:
                spawner = Level_1_Spawner.transform;
                GameManager.Instance.DepthBaseline = 40;
                bgm = "bgm";
                break;
            case ELevelType.Level2:
                spawner = Level_2_Spawner.transform;
                GameManager.Instance.DepthBaseline = 80;
                bgm = "intro";
                break;
            case ELevelType.Level3:
                spawner = Level_3_Spawner.transform;
                GameManager.Instance.DepthBaseline = 140;
                bgm = "bgm";
                break;
            case ELevelType.Endless:
                spawner = Endless_Spawner.transform;
                GameManager.Instance.DepthBaseline = 0;
                bgm = "intro";
                break;
            default:
                break;
        }
        

        if (spawner != null)
        {
            GameManager.Instance.SpawnPlayer(spawner.position, spawner.rotation);
        }
        
        var p = GameManager.Instance.CurrentPlayer;
        ui.AmmoRoot.SetGameObjectActive(p.settings.jumpCount != 0);
        AudioManager.Instance.PlaySound(bgm);
    }
    
}
