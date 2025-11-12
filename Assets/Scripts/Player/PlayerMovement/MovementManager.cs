using System;
using UnityEngine;

/**
 * This script manages the player's movement state, and
 * switching between moving the mouse and the mech.
 **/
public class MovementManager : PlayerMovementManager
{
    [NonSerialized] public CameraManager CameraManager;

    [NonSerialized] public static MovementManager Instance;
    [NonSerialized] public MechAIController MechAIController;

    private GameObject Mouse;
    private GameObject Mech;

    public bool isLockedMovement;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CameraManager = CameraManager.Instance;
        MechAIController = MechAIController.Instance;

        Mouse = PlayerMouse.Instance.gameObject;
        Mech = PlayerMech.Instance.gameObject;

        MovementConfig MouseConfig = new(Mouse, Config.MOUSE_MOVE_SPEED, Config.MOUSE_JUMP_FORCE, 
            Config.MOUSE_DASH_SPEED);
        MovementConfig MechConfig = new(Mech, Config.MECH_MOVE_SPEED, Config.MECH_JUMP_FORCE, 
            Config.MECH_DASH_SPEED);

        MouseMovementState = new MovementState();
        MechMovementState = new MovementState();

        MouseMovementState.EnterState(this, MouseConfig);
        MechMovementState.EnterState(this, MechConfig);

        ToggleMouse(false);
    }

    void Update()
    {
        Debug.Log(IsMouseActive);
        CameraManager.UpdateCamera();

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 camForward = CameraManager.Cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = CameraManager.Cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

        if (isLockedMovement) return;

        if (IsMouseActive)
        {
            MouseMovementState.UpdateState(this, true, moveDir);
            if (Input.GetButton("SummonMecha"))
            {
                MechAIController.SetTarget(Mouse.gameObject);
            }
            else
            {
                MechAIController.SetTarget(null);
            }

        }
        else
        {
            MechMovementState.UpdateState(this, !IsMouseActive, moveDir);

            if (MechAIController.Target == Mouse.gameObject)
            {
                MechAIController.SetTarget(null);
            }
        }

        if (Input.GetButtonDown("MountKey"))
        {
            ToggleMouse(!IsMouseActive);
        }
    }
    
    public override void ToggleMouse(bool toggle)
    {
        IsMouseActive = toggle;

        Rigidbody mouseRb = Mouse.GetComponent<Rigidbody>();
        Rigidbody mechRb = Mech.GetComponent<Rigidbody>();

        if (IsMouseActive)
        {
            if (mechRb != null)
            {
                mechRb.constraints = RigidbodyConstraints.FreezePositionX |
                                     RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezeRotation;
            }
            if (mouseRb != null) mouseRb.constraints = RigidbodyConstraints.FreezeRotation;

            if (MechAIController != null && MechAIController.Agent != null)
            {
                MechAIController.Agent.isStopped = true;
                MechAIController.Agent.ResetPath();
                MechAIController.Agent.velocity = Vector3.zero;
            }

            MechMovementState.Reset();
            CameraManager.SetFollowEntity(Mouse, Config.MOUSE_MAX_ZOOM);
            MechMovementState.UpdateJoyStick(Constant.JOY_RIGHT);
        }
        else
        {
            if (mouseRb != null)
            {
                mouseRb.constraints = RigidbodyConstraints.FreezePositionX |
                                      RigidbodyConstraints.FreezePositionZ |
                                      RigidbodyConstraints.FreezeRotation;
            }
            if (mechRb != null) mechRb.constraints = RigidbodyConstraints.FreezeRotation;
            
            if (MechAIController != null && MechAIController.Agent != null)
            {
                MechAIController.Agent.isStopped = false;
                MechAIController.Agent.velocity = Vector3.zero;
                MechAIController.Agent.ResetPath();
            }
            
            MouseMovementState.Reset();
            CameraManager.SetFollowEntity(Mech, Config.MECH_MAX_ZOOM);
            MechMovementState.UpdateJoyStick(Constant.JOY_LEFT);
        }        
    }

    public override void Reset()
    {
        MouseMovementState.Reset();
        MechMovementState.Reset();
    }
}
