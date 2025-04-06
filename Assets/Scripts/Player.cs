using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using MCommon.Unity.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public struct MovementSettings
{
    public float moveSpeed;
    public float moveSpeedInAir;
    public float turnSpeed;
    public float gravity;
    public float jumpForce;
    public uint jumpCount;
    public float maxDownwardSpeed;
    public Vector3 ShotPositionOffset;
    public float maxFallingDistance;
    public float maxLandingDistance;
    public float CroakMinTriggerTime;
    public float CroakProp;
}

public class Player : MonoBehaviour
{
    CharacterController controller;
    public MovementSettings settings;
    private CinemachineFreeLook freeLookCamera;
    private Camera _camera;
    private Animator animator;
    public Transform LookAtTarget;
    public Transform FollowTarget;
    public bool IsDie;
    public float croakTimer;
    public Vector2 InputDir;
    
    void Awake()
    {
        InitDebugInfo();
        controller = GetComponent<CharacterController>();
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        _camera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        
        BindCamera();
    }

    public void BindCamera()
    {
        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }
        freeLookCamera.LookAt = LookAtTarget;
        freeLookCamera.Follow = FollowTarget;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        getMoveDirection();
        InputDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (FreezeControl)
        {
            InputDir = Vector2.zero;
        }
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        // check every update
        var deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
        
        // todo: fix can't set position when spawn
        if (IsDie == false && FreezeControl == false)
            updateMovement(deltaTime);

        if (InputDir == Vector2.zero)
        {
            croakTimer += deltaTime;
            if (croakTimer >= settings.CroakMinTriggerTime &&
                Random.Range(0f, 1f) > settings.CroakProp)
            {

                croakCts = new CancellationTokenSource();
                Croak(croakCts).Forget();
                croakTimer = 0;
            }
        }
        else
        {
            croakTimer = 0;
        }
    }

    private CancellationTokenSource croakCts;
    private async UniTaskVoid Croak(CancellationTokenSource cts)
    {
        animator.SetTrigger("croak");
        // show something
        await UniTask.Delay(1000, cancellationToken:cts.Token);
        var index = Random.Range(1, 5);
        AudioManager.Instance.PlaySound($"frog{index}");
        
    }
    
    bool checkLandingDie()
    {
        return currentFallingDistance <= settings.maxLandingDistance;
    }
    
    bool checkFallingDie()
    {
        return currentFallingDistance <= settings.maxFallingDistance;
    }

    public void Die()
    {
        gameObject.SetGameObjectActive(false);
        FreezeControl = true;
        IsDie = true;
        currentFallingDistance = 0;
        GameManager.Instance.SpawnDieFx(transform.position+Vector3.up, transform.rotation);
        GameManager.Instance.GameOver();
    }
#region Presentation
    public ParticleSystem sparkjet;
    public GameObject GunRoot;
    public int GetDepth()
    {
        return (int)transform.position.y;
    }

    public void ShowGun(bool enable)
    {
        GunRoot.SetGameObjectActive(enable);
    }
#endregion
#region Movement

    public bool FreezeControl = false;
    private bool jumpFlag = false;
    private float vSpeed;
    private Vector3 currentMotion;
    private Vector3 dtMotion;
    private Vector3 moveForwardDirection;
    
    private uint currentJumpCount;
    private float currentFallingDistance;
    [SerializeField]
    private ParticleSystem moveParticle;
    void updateMovement(float deltaTime)
    {
        var hm = _getHorizontalMotion(deltaTime);
        vSpeed = _getVerticalMotion(vSpeed, deltaTime);
        currentMotion = new Vector3(hm.x, vSpeed, hm.y);
        var rot = Quaternion.Euler(0, _camera.transform.rotation.eulerAngles.y, 0);
        currentMotion = rot * currentMotion;
        dtMotion = currentMotion * deltaTime;
        // reset jump flag
        jumpFlag = false;
        var lastGrounded = controller.isGrounded;
        var pos_old = transform.position;
        var flags = controller.Move(dtMotion);
        var pos_new = transform.position;
        if (controller.isGrounded)
        {
            if (checkLandingDie())
            {
                Debug.Log("Landing Died!");
                // destroy frog and restart game
                Die();
            }
            vSpeed = 0;
            currentJumpCount = 0;
            currentFallingDistance = 0;
            if (lastGrounded == false)
                moveParticle.Play();
        }
        else
        {
            if (checkFallingDie())
            {
                Debug.Log("Falling Died!");
                // destroy frog and restart game¬
                Die();
            }
            currentFallingDistance += pos_new.y - pos_old.y;
            if (lastGrounded)
                moveParticle.Stop();
        }
        //Debug.Log($"curr position: {transform.position}");
    }

    void getMoveDirection()
    {
        moveForwardDirection = this.transform.position - freeLookCamera.transform.position;
        moveForwardDirection.y = 0;
        moveForwardDirection.Normalize();
    }

    Vector2 _getHorizontalMotion(float deltaTime)
    {
        var speed = controller.isGrounded?settings.moveSpeed:settings.moveSpeedInAir;
        var hMotion = InputDir*speed;
        if (hMotion != Vector2.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveForwardDirection, settings.turnSpeed * deltaTime);
        }
        return hMotion;
    }

    float _getVerticalMotion(float vspeed, float deltaTime)
    {
        var speed = vspeed;
        if (jumpFlag)
        {
            speed += settings.jumpForce;
        }
        else
        {
            speed -= settings.gravity*deltaTime;
            if (Mathf.Abs(speed) > settings.maxDownwardSpeed)
            {
                speed = -settings.maxDownwardSpeed;
            }
        }

        return speed;
    }

    public void Jump()
    {
        if (currentJumpCount < settings.jumpCount)
        {
            currentFallingDistance = 0;
            jumpFlag = true;
            currentJumpCount++;
            if (sparkjet != null)
            {
                sparkjet.Play();
            }
            GameManager.Instance.SpawnBullet(transform.position + settings.ShotPositionOffset, Quaternion.LookRotation(Vector3.down));
            
            AudioManager.Instance.PlaySound("gun");
        }
    }

    public int GetLeftJumpCount()
    {
        var countLeft = settings.jumpCount - (uint)currentJumpCount;
        return (int)countLeft;
    }

    private CancellationTokenSource freezeCts;
    public void SpawnFreeze(float duration)
    {
        moveParticle.Play();
        freezeCts = new CancellationTokenSource();
        FreezeDuration(duration, freezeCts).Forget();
    }
    
    async UniTaskVoid FreezeDuration(float duration, CancellationTokenSource cts)
    {
        FreezeControl = true;
        await UniTask.WaitForSeconds(duration, cancellationToken: cts.Token);
        FreezeControl = false;
        
    }
#endregion

#region DebugInfo

private GUIStyle _sharedStyle = new GUIStyle();
private List<string> _logs = new List<string>();

private void InitDebugInfo()
{
    _sharedStyle.fontSize = 24; 
    _sharedStyle.normal.textColor = Color.white; 
}
private void OnGUI()
{
    // _logs.Add($"[IsGrounded] {controller.isGrounded} [CurrentJumpCount] {currentJumpCount} [CurrentFallingDis] {currentFallingDistance}");
    // _logs.Add($"[currentMotion] {currentMotion} [dtMotion] {dtMotion}");
    // _logs.Add($"[freelookcam forward] {freeLookCamera.transform.forward} [dir] {moveForwardDirection}");
    // for (int i = 0; i < _logs.Count; i++)
    // {
    //     GUI.Label(new Rect(10, 60 + i * 20, 500, 20), _logs[i], _sharedStyle);
    // }
    // _logs.Clear();
}
#endregion
}
