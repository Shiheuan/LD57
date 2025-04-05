using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MCommon.Unity.Utils;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct MovementSettings
{
    public float moveSpeed;
    public float gravity;
    public float jumpForce;
    public uint jumpCount;
    public float maxDownwardSpeed;
    public Vector3 ShotPositionOffset;
    public float maxFallingDistance;
    public float maxLandingDistance;
}

public class Player : MonoBehaviour
{
    CharacterController controller;
    public MovementSettings settings;
    private CinemachineFreeLook freeLookCamera;
    private Camera camera;
    private Animator animator;
    
    void Awake()
    {
        InitDebugInfo();
        controller = GetComponent<CharacterController>();
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        camera = Camera.main;
        animator = GetComponentInChildren<Animator>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        getMoveDirection();
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
            animator.SetTrigger("croak");
        }
        // check every update
        var deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
        updateMovement(deltaTime);
    }
    
    bool checkLandingDie()
    {
        return currentFallingDistance <= settings.maxLandingDistance;
    }
    
    bool checkFallingDie()
    {
        return currentFallingDistance <= settings.maxFallingDistance;
    }
#region Presentation
    public ParticleSystem sparkjet;
    public GameObject GunRoot;
#endregion
#region Movement
    private bool jumpFlag = false;
    private float vSpeed;
    private Vector3 currentMotion;
    private Vector3 dtMotion;
    private Vector3 moveForwardDirection;
    
    private uint currentJumpCount;
    private float currentFallingDistance;
    void updateMovement(float deltaTime)
    {
        var hm = _getHorizontalMotion();
        vSpeed = _getVerticalMotion(vSpeed);
        currentMotion = new Vector3(hm.x, vSpeed, hm.y);
        var rot = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);
        currentMotion = rot * currentMotion;
        dtMotion = currentMotion * deltaTime;
        // reset jump flag
        jumpFlag = false;
        var pos_old = transform.position;
        var flags = controller.Move(dtMotion);
        var pos_new = transform.position;
        if (controller.isGrounded)
        {
            if (checkLandingDie())
            {
                Debug.Log("Landing Died!");
                // destroy frog and restart game
            }
            vSpeed = 0;
            currentJumpCount = 0;
            currentFallingDistance = 0;
        }
        else
        {
            if (checkFallingDie())
            {
                Debug.Log("Falling Died!");
                // destroy frog and restart game¬
            }
            currentFallingDistance += pos_new.y - pos_old.y;
        }
    }

    void getMoveDirection()
    {
        moveForwardDirection = this.transform.position - freeLookCamera.transform.position;
        moveForwardDirection.y = 0;
        moveForwardDirection.Normalize();
        // lerp player forward
        // transform.forward = Vector3.Slerp(transform.forward, moveForwardDirection, settings.moveSpeed * Time.deltaTime);
    }

    Vector2 _getHorizontalMotion()
    {
        var speed = settings.moveSpeed;
        var hMotion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))*speed;
        if (hMotion != Vector2.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveForwardDirection, settings.moveSpeed * Time.deltaTime);
        }
        return hMotion;
    }

    float _getVerticalMotion(float vspeed)
    {
        var speed = vspeed;
        if (jumpFlag)
        {
            speed += settings.jumpForce;
        }
        else
        {
            speed -= settings.gravity*Time.deltaTime;
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
        }
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
    _logs.Add($"[IsGrounded] {controller.isGrounded} [CurrentJumpCount] {currentJumpCount} [CurrentFallingDis] {currentFallingDistance}");
    _logs.Add($"[currentMotion] {currentMotion} [dtMotion] {dtMotion}");
    _logs.Add($"[freelookcam forward] {freeLookCamera.transform.forward} [dir] {moveForwardDirection}");
    for (int i = 0; i < _logs.Count; i++)
    {
        GUI.Label(new Rect(10, 60 + i * 20, 500, 20), _logs[i], _sharedStyle);
    }
    _logs.Clear();
}

public int GetDepth()
{
    return (int)transform.position.y;
}

public void ShowGun(bool enable)
{
    GunRoot.SetGameObjectActive(enable);
}

#endregion
}
