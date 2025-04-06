using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MCommon.Unity.Utils;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("[S]GameManager");
                    instance = obj.AddComponent<GameManager>();
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

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
        
        LevelManager.Instance.StartLevel();
    }

    void Update()
    {
        if ((isGameOver && Input.GetKeyDown(KeyCode.R)) || 
            (waitOperation && Input.GetKeyDown(KeyCode.Return)))
        {
            LevelManager.Instance.StartLevel();
            FindObjectOfType<ShowInfo>().HideTips();
        }
    }
    
    public bool isGameOver = false;
    public int DepthBaseline = 0; // level
    
    public void GameOver()
    {
        string tip = String.Empty;
        if (CurrentPlayer != null && CurrentPlayer.GunRoot.activeSelf == false)
        {
            tip = ConstString.GameOverWithoutGun;
            LevelManager.Instance.nextLevel = ELevelType.Intro;
        }
        else
        {
            tip = ConstString.GameOver;
        }
        isGameOver = true;
        // show some tip
        FindObjectOfType<ShowInfo>().ShowTips(tip, false);
    }

    public bool waitOperation;
    public async UniTaskVoid ShowEndScreen(ELevelType nextLevel)
    {
        HideAllEnemies();
        bool hasGun = CurrentPlayer.GunRoot.activeSelf;
        string e1 = string.Empty, e2 = string.Empty;
        switch (LevelManager.Instance.nextLevel)
        {
            case ELevelType.Level1:
                e1 = ConstString.Level1Ending;
                e2 = ConstString.Level1EndingWithOperation;
                if (hasGun)
                {
                    CurrentPlayer.settings.jumpCount = 3;
                }
                break;
            case ELevelType.Level2:
                e1 = ConstString.Level2Ending;
                e2 = ConstString.Level2EndingWithOperation;
                if (hasGun)
                {
                    CurrentPlayer.settings.jumpCount = 5;
                }
                break;
            case ELevelType.Level3:
                e1 = ConstString.Level3Ending;
                e2 = ConstString.Level3EndingWithOperation;
                if (hasGun)
                {
                    // back to intro
                    CurrentPlayer.settings.jumpCount = 0;
                }
                break;
        }
        
        FindObjectOfType<ShowInfo>().ShowTips(e1, true);
        await UniTask.WaitForSeconds(3f);
        FindObjectOfType<ShowInfo>().ShowTips(e2, true);
        LevelManager.Instance.nextLevel = nextLevel;
        waitOperation = true;
    }

    public void HideAllEnemies()
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            e.gameObject.SetGameObjectActive(false);
        }
    }
    
    public void RespawnAllEnemies()
    {
        // hide enemies
        HideAllEnemies();
        // respawn them
        var spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var s in spawners)
        {
            SpawnEnemy(s);
        }
    }
#region Spawner

    public GameObject BulletPool;
    public GameObject BulletPrefab;
    public GameObject PlayerPrefab;
    [HideInInspector]
    public Player CurrentPlayer;

    public GameObject DieFxPool;
    public GameObject DieFxPrefab;

    public GameObject EnemyPrefab;
    public GameObject EnemyPool;
    
    public Bullet SpawnBullet(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < BulletPool.transform.childCount; i++)
        {
            Bullet bullet = BulletPool.transform.GetChild(i).GetComponent<Bullet>();
            if (bullet.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            bullet.Init(position, rotation);
            return bullet;
        }
        
        Bullet newBullet = Instantiate(BulletPrefab, BulletPool.transform).GetComponent<Bullet>();
        newBullet.Init(position, rotation);
        return newBullet;
    }

    public Player SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        if (CurrentPlayer == null)
        {
            CurrentPlayer = Instantiate(PlayerPrefab, position, rotation).GetComponent<Player>();
            // bind camera
        }
        // todo: init()
        CurrentPlayer.SpawnFreeze(.5f);
        Debug.Log($"set position: {position}");
        CurrentPlayer.transform.SetPositionAndRotation(position, rotation);
        CurrentPlayer.gameObject.SetGameObjectActive(true);
        CurrentPlayer.IsDie = false;
        // change vc
        return CurrentPlayer;
    }

    public void SpawnDieFx(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < DieFxPool.transform.childCount; i++)
        {
            var fx = DieFxPool.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (fx.gameObject.activeInHierarchy)
            {
                continue;
            }
            fx.transform.SetPositionAndRotation(position, rotation);
            fx.gameObject.SetGameObjectActive(true);
            fx.Play();
            DelayHide(8f,fx.gameObject).Forget();
            return;
        }
        
        if (DieFxPrefab != null)
        {
            var fx = Instantiate(DieFxPrefab, position, rotation, DieFxPool.transform);
            fx.SetGameObjectActive(true);
            DelayHide(8f,fx).Forget();
        }
    }

    async UniTaskVoid DelayHide(float duration, GameObject obj)
    {
        await UniTask.WaitForSeconds(duration);
        obj.SetGameObjectActive(false);
    }


    public Enemy SpawnEnemy(EnemySpawner spawner)
    {
        for (int i = 0; i < EnemyPool.transform.childCount; i++)
        {
            Enemy enemy = EnemyPool.transform.GetChild(i).GetComponent<Enemy>();
            if (enemy.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            enemy.Init(spawner);
            return enemy;
        }
        
        Enemy newEnemy = Instantiate(EnemyPrefab, EnemyPool.transform).GetComponent<Enemy>();
        newEnemy.Init(spawner);
        return newEnemy;
    }
    #endregion
}
