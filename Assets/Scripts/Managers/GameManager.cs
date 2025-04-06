using System;
using System.Collections;
using System.Collections.Generic;
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
#endregion
}
