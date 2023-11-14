using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputManager : MonoBehaviour
{
    [SerializeField] InputActionReference _toggleEditModeRef;
    private InputAction _editModeInput;

    [SerializeField] InputActionReference _injectMassRef;
    private InputAction _injectMassInput;

    [SerializeField] InputActionReference _translateAnchorRef;
    private InputAction _translateAnchorInput;

    [SerializeField] InputActionReference _rotateAnchorRef;
    private InputAction _rotateAnchorInput;

    [SerializeField] InputActionReference _createProtoBodyRef;
    private InputAction _createProtoBodyInput;

    [SerializeField] InputActionReference _toggleMainMenuRef;
    private InputAction _toggleMainMenuInput;
    
    Action OnToggleEditMode => EventBus.OnToggleEditModeInput;
    Action<float> OnInjectMassInput => EventBus.OnInjectInput;

    Action OnInjectStarted => EventBus.OnInjectionStarted;
    Action OnInjectStopped => EventBus.OnInjectionStopped;

    Action OnCreateProtoBody => EventBus.OnCreateProtoBody;
    
    Action OnToggleMainMenu => EventBus.OnToggleMainMenu;

    //from  ActionBasedControllerManager
    InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
    }

    public void ToggleAction(InputAction action, bool state) 
    {
        if (action == null) return;
        Debug.Log("[Input Manager] Toggle Action : " + action.name +"/ state: " + state);
       
        if(state) action.Enable();
        else action.Disable();
    }


    public void ToggleMainMenu(InputAction.CallbackContext obj)
    {
        OnToggleMainMenu?.Invoke();
    }


    private void ToggleEditMode(InputAction.CallbackContext obj)
    {
        if(obj.action.IsPressed()) OnToggleEditMode?.Invoke();
    }

   

    private void CreateProtoBody(InputAction.CallbackContext obj)
    {
        
        Debug.Log("Input create proto");
        OnCreateProtoBody?.Invoke();
    }

    private void InjectMass(float input)
    {
        OnInjectMassInput?.Invoke(input);
    }
    
    private void InjectMass(InputAction.CallbackContext obj) => InjectMass( obj.ReadValue<Vector2>().y);

    
    private void InjectionStarted(InputAction.CallbackContext obj)
    {
        OnInjectStarted?.Invoke();
    }
    private void InjectionStopped()
    {
        OnInjectStopped?.Invoke();
    }

    private void InjectionStopped(InputAction.CallbackContext obj) => InjectionStopped();
    
    private void DisableAnchorTransform(bool state)
    {
        
        ToggleAction(_translateAnchorInput, !state);
        ToggleAction(_rotateAnchorInput, !state);
    }

    private void DisableAnchorTransform(bool state, AstralBodyHandler arg2) => DisableAnchorTransform(state);
   
   
    
    public void Awake()
    {
        _editModeInput = GetInputAction(_toggleEditModeRef);
        _injectMassInput = GetInputAction(_injectMassRef);
        _translateAnchorInput = GetInputAction(_translateAnchorRef);
        _rotateAnchorInput = GetInputAction(_rotateAnchorRef);
        _createProtoBodyInput = GetInputAction(_createProtoBodyRef);
        _toggleMainMenuInput = GetInputAction(_toggleMainMenuRef);
        
        DisableAnchorTransform(false);
    }

 

    private void OnEnable()
    {

        _editModeInput.performed += ToggleEditMode;
        _injectMassInput.started += InjectionStarted;
        _injectMassInput.performed += InjectMass;
        _injectMassInput.canceled += InjectionStopped;
        
        _createProtoBodyInput.performed += CreateProtoBody;
        _toggleMainMenuInput.performed += ToggleMainMenu;
       
        EventBus.OnBodyEdit += DisableAnchorTransform;
    }

   

   

    private void OnDisable()
    {
        _editModeInput.performed -= ToggleEditMode;
        EventBus.OnBodyEdit -= DisableAnchorTransform;
        _injectMassInput.performed -= InjectionStarted;
        _createProtoBodyInput.performed -= CreateProtoBody;
        _toggleMainMenuInput.performed -= ToggleMainMenu;
    }


    // private void Update()
    // {
    //  //   if (_injectMassInput.IsPressed()) InjectMass(_injectMassInput.ReadValue<Vector2>().y);
    //    // if (_injectMassInput.WasReleasedThisFrame()) InjectionStopped();
    // }

  
}
