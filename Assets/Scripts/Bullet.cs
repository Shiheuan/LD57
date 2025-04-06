using System.Threading;
using Cysharp.Threading.Tasks;
using MCommon.Unity.Utils;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem TailFx;

    public ParticleSystem ExplosionFx;
    public float lifetime;
    private CancellationTokenSource lifetimeCts;
    private CancellationTokenSource explosionCts;
    private bool died = true;

    public void Init(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetGameObjectActive(true);
        died = false;
        TailFx.gameObject.SetGameObjectActive(true);
        TailFx.Play();
        
        lifetimeCts = new CancellationTokenSource();
        DelayDestroy(lifetime, lifetimeCts).Forget();
    }
    
    async UniTaskVoid DelayDestroy(float lifetime, CancellationTokenSource cts)
    {
        await UniTask.WaitForSeconds(lifetime, cancellationToken: cts.Token);
        died = true;
        gameObject.SetGameObjectActive(false);
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

    private void OnDestroy()
    {
        lifetimeCts?.Cancel();
        explosionCts?.Cancel();
    }

    private RaycastHit[] hits = new RaycastHit[10];
    public LayerMask hitLayerMask;
    private void CheckCollision()
    {
        var count = Physics.SphereCastNonAlloc(
            transform.position,
            0.5f, 
            transform.TransformDirection(Vector3.forward), 
            hits, 
            0.5f, 
            hitLayerMask);
        if (count > 0)
        {
            var hitted = false;
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    hitted = true;
                }
                else if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    //Debug.Log(hit.collider.gameObject.name);
                    hitted = true;
                    var enemy = hit.collider.GetComponentInParent<Enemy>();
                    enemy.TakeDamageOnce();
                }

                if (hitted)
                {
                    ExplosionFx.gameObject.SetGameObjectActive(true);
                    ExplosionFx.Play();
                    TailFx.gameObject.SetGameObjectActive(false);
                    died = true; // stop movement
                    lifetimeCts?.Cancel();
                    explosionCts = new CancellationTokenSource();
                    DelayDestroy(5f, explosionCts).Forget();
                    break;
                }
            }
        }
    }
}
