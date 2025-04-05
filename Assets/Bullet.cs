using System.Collections;
using System.Collections.Generic;
using MCommon.Unity.Utils;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem TailFx;

    public ParticleSystem ExplosionFx;

    private bool died = false;
    // Start is called before the first frame update
    void Start()
    {
        died = false;
        TailFx.gameObject.SetGameObjectActive(true);
        TailFx.Play();
    }

    public float speed;
    // Update is called once per frame
    void Update()
    {
        if (died == false)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            CheckCollision();
        }
        
    }

    private RaycastHit[] hits = new RaycastHit[10];
    private void CheckCollision()
    {
        var count = Physics.SphereCastNonAlloc(transform.position,0.5f, transform.TransformDirection(Vector3.forward), hits, 0.5f, LayerMask.GetMask("Ground"));
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    ExplosionFx.gameObject.SetGameObjectActive(true);
                    ExplosionFx.Play();
                    TailFx.gameObject.SetGameObjectActive(false);
                    died = true;
                }
            }
        }
    }
}
