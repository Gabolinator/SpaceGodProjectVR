using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.UI;


public enum HandSide 
{
 Left,
 Right,
 Unasigned
}

[System.Serializable]
public class ControllerHand
{
    public HandSide handSide = HandSide.Unasigned;
    [SerializeField] XRRayInteractor _interactorRay;
    public XRRayInteractor InteractorRay => _interactorRay;

    [SerializeField] GameObject _controller;
    public GameObject Controller => _controller;

    [SerializeField] GameObject _bodySpawnPoint;
    public GameObject BodySpawnPoint => _bodySpawnPoint;

    public ControllerHand(ControllerHand hand) 
    {
        handSide = hand.handSide;
        _interactorRay = hand._interactorRay;
        _controller = hand._controller;
        _bodySpawnPoint = hand._bodySpawnPoint;
    }

    public ControllerHand() { }

}

public class PlayerController : MonoBehaviour
{


    public float playerScale = 1;
    private Vector3 _initialPlayerScale;

    [Header("Controllers")]
    [SerializeField] ControllerHand _leftHand;
    public ControllerHand LeftHand => _leftHand;

    [SerializeField] ControllerHand _rightHand;
    public ControllerHand RightHand => _rightHand;

    [Header("Locomotion")]
    [SerializeField] LocomotionManager _locomotionManager;
    public LocomotionManager LocomotionManager => _locomotionManager;

    [Header("Interactor Ray")]
    [SerializeField] HandSide _interactorHandSide;
    public HandSide InteractorHandSide => _interactorHandSide;

    public XRRayInteractor InteractorRay => _interactorHandSide == HandSide.Left ? LeftHand.InteractorRay : RightHand.InteractorRay;

    //[SerializeField] GameObject _leftController;
    public GameObject LeftController => LeftHand.Controller;

    //[SerializeField] GameObject _rightController;
    public GameObject RightController => RightHand.Controller;


    public Action<float> OnScalingPlayer;






    private IEnumerator CheckRayCastHit(float delay)
    {
        do
        {
            CheckRayCastHitInternal();

            yield return new WaitForSeconds(delay);

        }
        while (true);
    }

    private void CheckRayCastHitInternal()
    {
        if (!InteractorRay)
        {
            Debug.LogWarning("[Player Controller] No Interactor Ray Set");
            return;
        }

        RaycastHit hit;

        InteractorRay.TryGetCurrent3DRaycastHit(out hit);
        // Debug.Log("Ray Hit : " + hit);
        if (!hit.collider) return;

        //Debug.Log("Ray Hit : " + hit.collider);
        var bodyHandler = hit.collider.gameObject.GetComponent<AstralBodyHandler>();
        if (bodyHandler)
        {
            Debug.Log("Ray Hit body: " + bodyHandler);
            
            EventBus.OnAstralBodyRayHit?.Invoke(bodyHandler);
        }

        var gui = hit.transform.gameObject.GetComponent<SGPScreen>();
        if (gui)
        {
            Debug.Log("Ray Hit gui: " + gui);
            //EventBus.OnPointingGui?.Invoke(true);
        }

        //else EventBus.OnPointingGui?.Invoke(false);
    }

    public void ScalePlayer(float scale,  float minScale, float maxScale, bool clampScale = false) 
    {
        if (scale <= 0) return;
        if (clampScale) 
        {
            if (scale < minScale) scale = minScale;
            if (scale > maxScale) scale = maxScale;
        }

        ScalePlayer(Vector3.one * scale);
    }

    public void ScalePlayer(float scale)
    {

        ScalePlayer(scale, .165f, 100, true);
    }

    private void ScalePlayer(Vector3 scale)
    {
        transform.localScale = scale;
        playerScale = scale.x;

        OnScalingPlayer?.Invoke(scale.x);
    }

    private void ResetPlayerScale()
    {
        ScalePlayer(_initialPlayerScale); 
    }

    private void ScaleHandController(ControllerHand controller, float scaleFactor) 
    {
        if(scaleFactor <= 0) return ;

        /*scale line renderer*/
        var renderer = controller.InteractorRay.GetComponent<LineRenderer>();

        renderer.startWidth *= scaleFactor;
        renderer.endWidth *= scaleFactor;

    }

    private void OnStoppedPointingObject(HoverExitEventArgs arg)
    {
        Debug.Log("[Player] Ray hover stops ");
    }
    
    private void OnPointingObject(HoverEnterEventArgs arg)
    {
        //Debug.Log("[Player] Ray hover starts " + arg.interactableObject);
       
        var bodyHandler = arg.interactableObject.transform.GetComponent<AstralBodyHandler>();
        
        if (bodyHandler)
        {
            Debug.Log("[Player] Ray hover body: " + bodyHandler);
            
            EventBus.OnAstralBodyRayHit?.Invoke(bodyHandler);
        }
    }

    
    private void OnPointingGui(UIHoverEventArgs arg)
    {
       // Debug.Log("[Player] Ray hover ui starts " + arg.uiObject);
        EventBus.OnPointingGui?.Invoke(true);
    }
    private void OnStoppedPointingGui(UIHoverEventArgs arg)
    {
       // Debug.Log("[Player] Ray hover ui stops ");
        EventBus.OnPointingGui?.Invoke(false);
    }
    public void Start()
    {
        _initialPlayerScale = transform.localScale;

        GameManager.Instance.localPlayer = this;

        EventBus.OnPlayerStart?.Invoke(this);

        
       // StartCoroutine(CheckRayCastHit(1.0f));
    }
    private void Update()
    {
       
    }

    private void OnEnable()
    {
        if (!InteractorRay)
        {
            Debug.Log("[Player] No interactor ray set");
            return;
        }

        InteractorRay.uiHoverEntered.AddListener(OnPointingGui);
        InteractorRay.uiHoverExited.AddListener(OnStoppedPointingGui);
        InteractorRay.hoverEntered.AddListener(OnPointingObject);
        InteractorRay.hoverExited.AddListener(OnStoppedPointingObject);
    }

 

    

    private void OnDisable()
    {
        if (!InteractorRay)
        {
            Debug.Log("[Player] No interactor ray set");
            return;
        }
        InteractorRay.uiHoverEntered.RemoveListener(OnPointingGui);
        InteractorRay.uiHoverExited.RemoveListener(OnStoppedPointingGui);
        InteractorRay.hoverEntered.RemoveListener(OnPointingObject);
        InteractorRay.hoverExited.RemoveListener(OnStoppedPointingObject);
    }
}
