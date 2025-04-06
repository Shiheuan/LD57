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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#region Spawner

    public GameObject BulletPool;
    public GameObject BulletPrefab;
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

    public GameObject PlayerPrefab;
    [HideInInspector]
    public Player CurrentPlayer;

    public GameObject DieFxPool;
    public GameObject DieFxPrefab;

    public Player SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        if (CurrentPlayer == null)
        {
            CurrentPlayer = Instantiate(PlayerPrefab, position, rotation).GetComponent<Player>();
            // bind camera
            return CurrentPlayer;
        }
        
        CurrentPlayer.transform.SetPositionAndRotation(position, rotation);
        CurrentPlayer.gameObject.SetGameObjectActive(true);
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
    #endregion
}
