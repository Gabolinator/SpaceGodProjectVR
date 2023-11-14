using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EventBus : MonoBehaviour
{
    #region Player

    public static Action<PlayerController> OnPlayerStart;

    #region LocomotionEvent

    public static Action<InputAction.CallbackContext> OnLeftControllerInput;
    public static Action<InputAction.CallbackContext> OnRightControllerInput;
    public static Action<bool> OnPlayerMoving;

    public static Action<bool> OnPlayerStoppedMoving;

    public static Action<float> OnPlayerMovementSpeedChange;

    #endregion




    #endregion

    #region AstralBody events

    public static Action<AstralBodyHandler> OnAstralBodyStartToExist;
    public static Action<AstralBodyHandler> OnAstralBodyDestroyed;

    public static Action<AstralBodyHandler> OnAstralBodyRayHit;

    public static Action<AstralBodyHandler, float> OnAstralBodyProximityRange;

    public static Action<AstralBodyHandler, Vector3, Vector3> OnAstralBodyAnyVelocitiesChanged;
    public static Action<AstralBodyHandler, Vector3> OnAstralBodyAngularVelocityChange;

    #endregion

    #region AstralBody Edit events

    public static Action<AstralBody> OnBodyUpdated;
    public static Action<AstralBodyDescriptor> OnBodyDescriptorUpdated;
    public static Action<bool, AstralBodyHandler> OnBodyEdit;
    public static Action OnToggleEditModeInput;
    public static Action<float> OnInjectInput;
    public static Action<AstralBodyHandler, float> OnInjectMass;
    public static Action OnInjectionStopped;
    public static Action OnInjectionStarted;
    public static Action<AstralBodyHandler, GeneratedBody> OnPredictBody;
    public static Action OnCreateProtoBody;

    #endregion

    #region GrabEvents

    public static Action<SelectEnterEventArgs> OnObjectGrabbed;
    public static Action<SelectExitEventArgs> OnObjectReleased;
    public static Action<XRGrabInteractable> OnGrabbableProcess; // i.e. scaling
    public static Action<bool, XRGrabInteractable> OnDoubleGrab;



    #endregion

    #region VisualIndicators event

    //not hooked yet
    public static Action<Color> OnTrajectoryColorChange;
    public static Action<Color> OnPathColorChange;
    public static Action<float> OnTrajectoryMaxLengthChange;





    #endregion

    #region GUIEvents

    public static Action OnToggleMainMenu;

    #endregion
}