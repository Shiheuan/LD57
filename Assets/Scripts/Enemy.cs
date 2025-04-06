using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MCommon.Unity.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class EnemyState
{
    public EEnemyState type;
    protected Enemy enemy;
    protected CancellationTokenSource cts = new CancellationTokenSource();
    public EnemyState(Enemy e) => enemy = e;
    public abstract UniTask EnterState();
    public virtual void UpdateState(float deltaTime){}

    public virtual async UniTask ExitState()
    {
        await UniTask.Yield();
        cts?.Cancel();
        cts = null;
    }
}

public class PatrolState : EnemyState
{
    public PatrolState(Enemy e) : base(e)
    {
        type = EEnemyState.Patrol;
    }
    
    public override async UniTask EnterState()
    {
        cts = new CancellationTokenSource();
        var v2 = enemy.settings.PatrolPoints[enemy.currentPointIndex];
        var pos = enemy.SetDestination(v2);
        enemy.moving = true;

        await UniTask.WaitUntil(() => 
            Vector3.Distance(enemy.transform.position, pos) < enemy.settings.ReachTolerance, cancellationToken: cts.Token);

        enemy.currentPointIndex = (enemy.currentPointIndex + 1) % enemy.settings.PatrolPoints.Length;
        await SwitchNextPoint();
    }
    
    private async UniTask SwitchNextPoint()
    {
        cts = new CancellationTokenSource();
        enemy.moving = false;
        await UniTask.Delay(2000, cancellationToken: cts.Token);
        await EnterState();
    }

    public override void UpdateState(float deltaTime)
    {
        if (enemy != null)
        {
            enemy.checkPlayerDistance();
        }
    }
}

public class ChaseState : EnemyState
{
    public ChaseState(Enemy e) : base(e)
    {
        type = EEnemyState.Chase;
    }
    
    public override async UniTask EnterState()
    {
        cts = new CancellationTokenSource();
        var p = GameManager.Instance.CurrentPlayer;
        if (p == null) enemy.SwitchState(EEnemyState.Patrol);
        enemy.SetChaseTarget(p.transform);
        enemy.moving = true;
        await UniTask.WaitUntil(() =>
            p != null && enemy != null && (Vector3.Distance(p.transform.position, enemy.transform.position) >
                        enemy.settings.PlayerDetectRange)
        , cancellationToken: cts.Token);

        enemy.moving = false;
        enemy.SwitchState(EEnemyState.Patrol);
    }

    public override UniTask ExitState()
    {
        enemy.SetChaseTarget(null);
        enemy.moving = false;
        return base.ExitState();
    }
}

public class DeadState : EnemyState
{
    public DeadState(Enemy e) : base(e)
    {
        type = EEnemyState.Dead;
    }

    public override async UniTask EnterState()
    {
        cts = new CancellationTokenSource();
        enemy.isDie = true;
        // await enemy.animator.PlayAsync("Death", cancellationToken: cts.Token);
        
        await UniTask.Delay(3000, cancellationToken: cts.Token);
        enemy.gameObject.SetGameObjectActive(false);
        enemy.EndAI();
    }
}

[Serializable]
public struct EnemySettings
{
    public Vector2[] PatrolPoints;
    public float PlayerDetectRange;
    public float ReachTolerance;
    public float PatrolSpeed;
    public float ChaseSpeed;
    public float TurnSpeed;
    public int MaxHP;
}

public enum EEnemyState {
    Patrol,
    Chase,
    Dead,
}
public class Enemy : MonoBehaviour
{
    public EnemySettings settings;
    public Vector3 Destination;
    public bool isDie;
    private EnemyState currentState;
    public bool moving = false;
    public int currentHP;
    public int currentPointIndex;
    
    [SerializeField]
    private HazardTrigger hazardTrigger;
    [SerializeField]
    private GameObject groundSphere;
    [SerializeField]
    private GameObject RenderRoot;
    
    private Dictionary<EEnemyState, EnemyState> states = new Dictionary<EEnemyState, EnemyState>();

    private void Awake()
    {
        states.Add(EEnemyState.Patrol, new PatrolState(this));
        states.Add(EEnemyState.Chase, new ChaseState(this));
        states.Add(EEnemyState.Dead, new DeadState(this));
    }

    // Start is called before the first frame update
    void Start()
    {
        StartAI();
    }

    public void StartAI()
    {
        SwitchState(states[EEnemyState.Patrol]).Forget();
    }

    public void EndAI()
    {
        if (currentState != null)
        {
            currentState.ExitState().Forget();
            currentState = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(Time.deltaTime);

            if (moving)
            {
                if (ChaseTarget != null)
                {
                    SetDestination(ChaseTarget.transform.position);
                }
                UpdateMovement(Time.deltaTime);
            }
        }
    }

    private void OnDestroy()
    {
        if (currentState != null)
        {
            currentState.ExitState().Forget();
            currentState = null;
        }
    }

    public void SwitchState(EEnemyState state)
    {
        SwitchState(states[state]).Forget();
    }
    
    public void Die()
    {
        SwitchState(states[EEnemyState.Dead]).Forget();
        RefreshDeadState();
    }

    private void RefreshDeadState()
    {
        RenderRoot.SetGameObjectActive(isDie == false);
        groundSphere.SetGameObjectActive(isDie == true);
        hazardTrigger.gameObject.SetGameObjectActive(isDie == false);
    }

    public void TakeDamageOnce()
    {
        currentHP--;
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    async UniTask SwitchState(EnemyState newState)
    {
        if (currentState != null)
        {
            Debug.Log($"exit state: {currentState.type}");
            await currentState.ExitState();
        }
        currentState = newState;
        Debug.Log($"enter state: {currentState.type}");
        await currentState.EnterState();
    }

    public void checkPlayerDistance()
    {
        var player = GameManager.Instance.CurrentPlayer;
        if (player == null) return;
        
        if(Vector3.Distance(transform.position, player.transform.position) < settings.PlayerDetectRange)
        {
            if(currentState is not ChaseState)
                SwitchState(states[EEnemyState.Chase]).Forget();
        }
    }
    
    #region Movement

    public Vector3 SetDestination(Vector2 destination)
    {
        var targetY = transform.position.y;
        Destination = new Vector3(destination.x, targetY, destination.y);
        //Debug.Log($"set target {Destination}");
        return Destination;
    }
    
    public Vector3 SetDestination(Vector3 destination)
    {
        Destination = destination;
        //Debug.Log($"set target {Destination}");
        return Destination;
    }

    public Transform ChaseTarget;
    public void SetChaseTarget(Transform target)
    {
        ChaseTarget = target;
    }

    public void UpdateMovement(float deltaTime)
    {
        var dir = Destination - transform.position;
        //Debug.Log($"{Destination} - {transform.position} = {dir}");
        if (dir.magnitude < 0.1f) return;
        var motion = Vector3.forward * settings.PatrolSpeed * deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, dir, settings.TurnSpeed * deltaTime);
        transform.Translate(motion);
    }
    #endregion

    public void Init(EnemySpawner spawner)
    {
        EndAI();
        moving = false;
        settings = spawner.OverrideSettings;
        transform.position = spawner.transform.position;
        transform.rotation = spawner.transform.rotation;
        currentHP = settings.MaxHP;
        currentPointIndex = 0;
        SetChaseTarget(null);
        gameObject.SetGameObjectActive(true);
        isDie = false;
        RefreshDeadState();
        StartAI();
    }
}
