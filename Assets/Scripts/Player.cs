using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct MovementSettings
{
    public float moveSpeed;
    public float gravity;
    public float jumpForce;
    public float maxDownwardSpeed;
}

public class Player : MonoBehaviour
{
    CharacterController controller;
    public MovementSettings settings;
    
    void Awake()
    {
        InitDebugInfo();
        controller = GetComponent<CharacterController>();
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        // check every update
        var deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
        updateMovement(deltaTime);
    }

#region Movement
    private bool jumpFlag = false;
    private float vSpeed;
    private Vector3 currentMotion;
    private Vector3 dtMotion;
    void updateMovement(float deltaTime)
    {
        var hm = _getHorizontalMotion();
        vSpeed = _getVerticalMotion(vSpeed);
        currentMotion = new Vector3(hm.x, vSpeed, hm.y);
        dtMotion = currentMotion * deltaTime;
        // reset jump flag
        jumpFlag = false;
        var flags = controller.Move(dtMotion);
        // if (controller.isGrounded)
        // {
        //     vSpeed = 0;
        // }
    }

    Vector2 _getHorizontalMotion()
    {
        var speed = settings.moveSpeed;
        var hMotion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))*speed;
        return hMotion;
    }

    float _getVerticalMotion(float vspeed)
    {
        var speed = vspeed;
        if (jumpFlag)
        {
            speed = settings.jumpForce;
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
        // todo: inAir can't jump again
        jumpFlag = true;
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
    _logs.Add($"[IsGrounded] {controller.isGrounded}");
    _logs.Add($"[currentMotion] {currentMotion} [dtMotion] {dtMotion}");
    for (int i = 0; i < _logs.Count; i++)
    {
        GUI.Label(new Rect(10, 60 + i * 20, 500, 20), _logs[i], _sharedStyle);
    }
    _logs.Clear();
}

#endregion
}
