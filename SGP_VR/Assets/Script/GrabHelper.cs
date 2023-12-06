using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class GrabHelper : MonoBehaviour
{
    public GameObject _grabbedObject;

    public XRGrabInteractable _xrGrabInteractable;

    public XRGeneralGrabTransformer _xrGrabTransformer;

    public Action<SelectEnterEventArgs> OnGrab => EventBus.OnObjectGrabbed;
    public Action<SelectExitEventArgs> OnRelease => EventBus.OnObjectReleased;

    public Action<SelectExitEventArgs> OnThisObjectRelease ;
    public Action<SelectEnterEventArgs> OnThisObjectGrab;

    private bool _isGrabbed;
    public bool IsGrabbed => _isGrabbed;
    
    private void ToggleTwoHandScale(bool state, AstralBodyHandler bodyHandler)
    {
        _xrGrabTransformer.allowTwoHandedScaling = state;
      
    }

    private void OnObjectGrabbed(SelectEnterEventArgs arg)
    {
        _isGrabbed = true;
        OnThisObjectGrab?.Invoke(arg);
        OnGrab?.Invoke(arg);
    }

    private void OnObjectReleased(SelectExitEventArgs arg)
    {
        _isGrabbed = false;
        OnThisObjectRelease?.Invoke(arg);
       OnRelease?.Invoke(arg);
    }

    public void Grab(ControllerHand hand)
    {
       
        var interactor = hand.InteractorRay;
        if(!interactor || !_xrGrabInteractable) return;

        bool selectPossible = hand.InteractorRay.interactionManager.IsSelectPossible(interactor, _xrGrabInteractable);
        if (selectPossible)
        {
            interactor.interactionManager.SelectEnter(interactor as IXRSelectInteractor, _xrGrabInteractable as IXRSelectInteractable);
        }
        else DestroyGrabbable();
                
           
    }

    private void DestroyGrabbable()
    {
        var BodyHandler = GetComponent<AstralBodyHandler>();
        if (BodyHandler == null) Destroy(this);
        else BodyHandler.DestroySelf();
    }

    public void Start()
    {
        if (!_grabbedObject) _grabbedObject = this.gameObject;
        if (!_xrGrabInteractable) _xrGrabInteractable = gameObject.GetComponent<XRGrabInteractable>();
        if (_xrGrabInteractable)
        {
            _xrGrabInteractable.selectEntered.AddListener(OnObjectGrabbed);
            _xrGrabInteractable.selectExited.AddListener(OnObjectReleased);
        }

        if (!_xrGrabTransformer) _xrGrabTransformer = gameObject.GetComponent<XRGeneralGrabTransformer>();
        if (_xrGrabTransformer)
        {
            // _xrGrabTransformer.
        }

        EventBus.OnBodyEdit += ToggleTwoHandScale;
    }

    public void OnDestroy()
    {
        if (!_xrGrabInteractable)
        {
            _xrGrabInteractable.selectEntered.RemoveListener(OnObjectGrabbed);
        }
    }
}
