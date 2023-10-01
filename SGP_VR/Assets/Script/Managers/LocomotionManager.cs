using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocomotionManager : MonoBehaviour

{
    private static LocomotionManager _instance;
    public static LocomotionManager Instance => _instance;


    [SerializeField] DynamicMoveProvider _locomotionController;

    public DynamicMoveProvider LocomotionController => _locomotionController;

    [SerializeField] float _moveAcceleration = 1.05f;

    public float MoveAcceleration => _moveAcceleration;

    [SerializeField] private bool _clampSpeed;
    public bool ClampSpeed => _clampSpeed;

    [SerializeField] float _maxSpeed = 2f;

    public float MaxSpeed => _maxSpeed;

    private float _initialMoveSpeed;

    private bool _isMoving;
    public bool IsMoving => _isMoving;
    private float _currentSpeed;
   

    public float CurrentSpeed => _currentSpeed;

    [SerializeField]InputActionReference _yaw;
    private InputAction _inputYaw;
    public float rotationSpeed = 5f;
    public GameObject _player;
    private bool enableRotation;
    private bool enableLocomotion;

    public static Action<InputAction.CallbackContext> OnLeftControllerInput => EventBus.OnLeftControllerInput;
    public static Action<InputAction.CallbackContext> OnRightControllerInput => EventBus.OnRightControllerInput;

    public static Action<float> OnMovementSpeedChange => EventBus.OnPlayerMovementSpeedChange;

    public static Action<bool> OnPlayerMoving => EventBus.OnPlayerMoving;
    public static Action<bool> OnPlayerStopped => EventBus.OnPlayerStoppedMoving;


    public void Accelerate(DynamicMoveProvider locomotionController, float moveAcceleration, float maxSpeed, bool clampSpeed) 
    {
        if (!locomotionController) return;

        float speed;

        if(clampSpeed && locomotionController.moveSpeed * moveAcceleration >= maxSpeed) speed = locomotionController.moveSpeed = maxSpeed;
        else speed = locomotionController.moveSpeed *= moveAcceleration;

        OnMovementSpeedChange?.Invoke(speed);

       // Debug.Log("[Locomotion Manager] Accelerate");
    }

    private IEnumerator Accelerate(float delay)
    {
        do
        {
            Accelerate(LocomotionController, MoveAcceleration, MaxSpeed, ClampSpeed);

            yield return new WaitForSeconds(delay);

        } while (true);


    }

    public void ResetSpeed(DynamicMoveProvider locomotionController, float initialSpeed) 
    {
        if (!locomotionController) return;

        if(locomotionController.moveSpeed == initialSpeed) return;
        locomotionController.moveSpeed = initialSpeed;
     
        //Debug.Log("[Locomotion Manager] Reset Speed");
    }


    public void OnLocomotionStop(LocomotionSystem locomotionSystem) 
    {
        //Debug.Log("[Locomotion Manager] Locomotion Stops : " +locomotionSystem.busy);
       
    }

    public void OnLocomotionStart(LocomotionSystem locomotionSystem) 
    {
       
      
    }



    private void OnRightHandInput(InputAction.CallbackContext obj)
    {
        
        OnRightControllerInput?.Invoke(obj);
    }


    private void OnLeftHandInput(InputAction.CallbackContext obj)
    {
        //Debug.Log("[Locomotion Manager] Left hand input : " + obj.performed);
        Accelerate(LocomotionController, MoveAcceleration, MaxSpeed, ClampSpeed);
        _currentSpeed = GetMoveSpeed(LocomotionController);
        OnPlayerMoving?.Invoke(IsMoving);

        OnLeftControllerInput?.Invoke(obj);
        
    }

    private void OnRigthHandYaw(InputAction.CallbackContext obj) 
    {
        Debug.Log("[Locomotion Manager] Right hand input");
        Vector2 input = obj.ReadValue<Vector2>();

        Debug.Log("[Locomotion Manager] Right hand input :" + input);
        RotatePlayer(input, _player);

        OnRightControllerInput?.Invoke(obj);
    }

    private float GetMoveSpeed(DynamicMoveProvider locomotionController)
    {
        return locomotionController ? locomotionController.moveSpeed : 0f;
    }

    //from  ActionBasedControllerManager
    InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
    }

    private void RotatePlayer(Vector2 input, GameObject player, bool enable = true)
    {

        if (!enable) return;

        float yawInput = input.y;
        float pitchInput = input.x;

        // Calculate rotation angles based on input.
        float yawRotation = yawInput * rotationSpeed*Time.deltaTime;
        float pitchRotation = pitchInput * rotationSpeed*Time.deltaTime;

        player.transform.eulerAngles += new Vector3(yawRotation, pitchRotation, 0);
    }

    private void ToggleLocomotion(bool state, AstralBodyHandler body)
    {
        LocomotionController.enabled =  enableLocomotion = enableRotation = !state;
    }

    private void Awake()
    {
        _inputYaw = GetInputAction(_yaw);
    }

    private void Start()
    {
        if (LocomotionController) 
        {
            _initialMoveSpeed = LocomotionController.moveSpeed;
        }

     

    }

  

    private void OnEnable()
    {
        //LocomotionController.endLocomotion += OnLocomotionStop;
        //LocomotionController.beginLocomotion += OnLocomotionStart;
        LocomotionController.leftHandMoveAction.action.performed += OnLeftHandInput;
        LocomotionController.rightHandMoveAction.action.performed += OnRightHandInput;
        EventBus.OnBodyEdit += ToggleLocomotion;


    }

    private void OnDisable()
    {
        //LocomotionController.endLocomotion -= OnLocomotionStop;
        //LocomotionController.beginLocomotion -= OnLocomotionStart;
        LocomotionController.leftHandMoveAction.action.performed -= OnLeftHandInput;
        LocomotionController.rightHandMoveAction.action.performed -= OnRightHandInput;
        EventBus.OnBodyEdit -= ToggleLocomotion;

    }


    private void Update()
    {
        if (LocomotionController) if (!(_isMoving = LocomotionController.leftHandMoveAction.action.IsPressed()))
        {
                ResetSpeed(LocomotionController, _initialMoveSpeed);
                _currentSpeed = GetMoveSpeed(LocomotionController);
                OnPlayerStopped?.Invoke(IsMoving);
        }
    }

    private void FixedUpdate()
    {
        if (_inputYaw.IsPressed()) RotatePlayer(_inputYaw.ReadValue<Vector2>(), _player, enableRotation);
    }
}
