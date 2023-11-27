using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AstralBodyEditor : MonoBehaviour
{
    [SerializeField]
    private static AstralBodyEditor _instance;
    public static AstralBodyEditor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AstralBodyEditor>();

                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("AstralBodiesManager");
                    _instance = singletonGO.AddComponent<AstralBodyEditor>();
                }
            }
            return _instance;
        }
    }

    [Header("Controllers")]
    [SerializeField] ControllerHand _leftHand;
    public ControllerHand LeftHand => _leftHand;

    [SerializeField] ControllerHand _rightHand;
    public ControllerHand RightHand => _rightHand;

    [Header("Grabbing Hand")]
    [SerializeField] HandSide _grabbingHandSide;
    public HandSide GrabbingHandSide => _grabbingHandSide;
    public ControllerHand grabbingHand;


    [Header("Editing Body")]
    [SerializeField] private AstralBodyHandler _grabbedBodyHandler;
    public AstralBodyHandler grabbedBodyHandler { get => _grabbedBodyHandler; set => _grabbedBodyHandler = value; }

    [SerializeField] private AstralBody _editingBody;
    public AstralBody grabbedBody { get => _editingBody; set => _editingBody = value; }

    [Header("Mass injection")]
    public float massInjectionMultiplicator = 100;
    private float startMassInjectionMultiplicator;

    [Header("Debug")]
    public bool isGrabbing;
    public bool editModeOn;
   
    private bool injectionStarted;
    private GeneratedBody predictedBody;
  

    public Action<bool, AstralBodyHandler> OnBodyEdit => EventBus.OnBodyEdit;
    public Action<AstralBodyHandler, float> OnInjectMass => EventBus.OnInjectMass;
    public Action<AstralBodyHandler, GeneratedBody> OnPredictBody => EventBus.OnPredictBody;

    #region Grab/Release
    public void StartGrab(SelectEnterEventArgs arg)
    {

        var grabbedBody = arg.interactableObject.transform.GetComponent<AstralBodyHandler>();
        if (!grabbedBody) return;

        Debug.Log("[Body Editor] "+ arg.interactorObject + " Start Grab : " + grabbedBody);

        GrabBody(grabbedBody, GetInteractorHand(arg.interactorObject));
    }

    private void GrabBody(AstralBodyHandler body, ControllerHand hand) 
    {
        if (!body) return;
        _grabbedBodyHandler = body;
        

        if (hand == null) return;
        if (body.isGrabbed) return;

        grabbingHand = hand;
        _grabbedBodyHandler.isGrabbed = isGrabbing = true;
    }

    public void EndGrab(SelectExitEventArgs arg)
    {
        if (!_grabbedBodyHandler) return;
  
        if (arg.interactableObject.interactorsSelecting.Count != 0) return; // we are still holding object

        ReleaseBody();
        
    }
   
    private void ReleaseBody()
    {
        _grabbedBodyHandler.isGrabbed = isGrabbing = false;

        grabbingHand = new ControllerHand();

        ToggleEditMode(false);

        _grabbedBodyHandler = null;

    }

    private void DoubleGrabBody(bool state, XRGrabInteractable interactable)
    {
        if (!interactable) return;
        //true = started double grab / false = just released second hand
        Debug.Log("Started double grab on :" + interactable + " : " + state);

        if (!state) ChangeGrabbingHand(interactable); //we released , do we need to change hand ? 
    }
    #endregion

    #region GrabbingHands
    private void ChangeGrabbingHand(XRGrabInteractable interactable)
    {
        var interactor = interactable.interactorsSelecting[0];
        ControllerHand currentHand = GetInteractorHand(interactor);

        ChangeGrabbingHand(currentHand);
    }

    public void ChangeGrabbingHand(ControllerHand hand)
    {
        if (hand == null) return;

        if (grabbingHand.handSide == hand.handSide) return;
        Debug.Log("Changing grabbing hand : " + hand);

        grabbingHand = hand;

    }

    private ControllerHand GetInteractorHand(IXRSelectInteractor interactorObject)
    {
        XRRayInteractor interactorRay = interactorObject as XRRayInteractor;
        if (!interactorRay)
        {
            Debug.Log("[Body Editor] interactor not a XRRayInteractor ");
            return null;
        }



        if (interactorRay == LeftHand.InteractorRay) return LeftHand;
        else if (interactorRay == RightHand.InteractorRay) return RightHand;

        return null;

    }

    private void GetControllerHands(PlayerController player)
    {
        if (!player) return;
        _leftHand = player.LeftHand;
        _rightHand = player.RightHand;
    }
    #endregion
  
    #region Injection 
    private void StartInjection()
    {
        if (!_grabbedBodyHandler) return;
        FXManager.Instance.ToggleFX(FXCategory.BodyEditing, FXElement.All, "inject", _grabbedBodyHandler.transform.position, _grabbedBodyHandler.transform.rotation, _grabbedBodyHandler.transform, true, true);
    }

    private void StopInjection()
    {
        massInjectionMultiplicator = startMassInjectionMultiplicator;

        if (!_grabbedBodyHandler) return;
        FXManager.Instance.ToggleOffFX(_grabbedBodyHandler.transform, "inject");
    }

    public void InjectMass(AstralBodyHandler bodyHandler, double delta, float multiplier)
    {
        if (bodyHandler == null) return;
        Debug.Log("delta : " + delta);

        delta = delta*multiplier > 5 ? 5 : delta * multiplier;
        delta = delta < 0 ? -1 / delta : delta; //if we are removing mass 

        bodyHandler.UpdateMass(delta);
        OnInjectMass?.Invoke(bodyHandler, (float)delta);
    }

    public void InjectMass(float input)
    {
        //Debug.Log($"[{(typeof)}] Injecting mass : " + input );
        massInjectionMultiplicator *= 1.005f;

        InjectMass(_grabbedBodyHandler, input, massInjectionMultiplicator);
    }

    public void InjectElement(AstralBody body, CelestialBodyElement element, double delta, bool remove)
    {
        if (remove) delta = -delta;

        var composition = body.BodyComposition;


        if (composition.Count > 0)
        {
            foreach (var chemical in composition)
            {
                if (chemical._element == element)
                {
                    if (chemical._percentage + delta <= 0)
                    {
                        body.BodyComposition.Remove(chemical);
                        return;
                    }


                    ReajustBodyComposition(composition, (float)(1 + (delta / chemical._percentage)));
                    //chemical._percentage += (float)delta;


                    return;
                }
            }
        }

        body.BodyComposition.Add(new ChemicalBodyCompositionElement(element, (float)delta));

    }
    #endregion
 
    #region Editmode 
    public void ToggleEditMode() 
    {
        ToggleEditMode(!editModeOn);
    }

    public void ToggleEditMode(bool state)
    {
        Debug.Log("[Body Editor] Toggle Edit mode : " + state);
        if (!_grabbedBodyHandler) return;
       
        editModeOn = state;

        _editingBody = state ? _grabbedBodyHandler.body : null;

        if (state) 
        {
            // start listen to edit buttons
            EventBus.OnInjectInput += InjectMass;
            
            /*play fx*/
            FXManager.Instance.ToggleFX(FXCategory.BodyEditing, FXElement.All, "startEdit", _grabbedBodyHandler.transform.position, _grabbedBodyHandler.transform.rotation, _grabbedBodyHandler.transform, true, true);
        }

        else 
        {
            // stop listen to edit buttons
            StopInjection();
            EventBus.OnInjectInput -= InjectMass;

            /*stop fx*/
            FXManager.Instance.ToggleOffFX(_grabbedBodyHandler.transform, "startEdit");
        }

        OnBodyEdit?.Invoke(state, _grabbedBodyHandler);
    }


    public void EditBody(AstralBody body) 
    {
        

    }
    #endregion

    public void CreateProtoBody() 
    {
        Debug.Log("[Editor]Create proto ");
        if (_grabbedBodyHandler) return; 

        HandSide handSide = HandSide.Left;
        var hand = handSide == HandSide.Left ? LeftHand : RightHand;

        var newProto = BodyGenerator.Instance.GenerateBody(AstralBodyType.ProtoBody, hand.BodySpawnPoint.transform.position + hand.Controller.transform.forward*-5f, Quaternion.identity);
        if (!newProto) return;
        var handler = newProto.GetComponent<AstralBodyHandler>();

        newProto.transform.localScale *= .01f;
        handler.ScaleBody(false);

        var grabHelper = newProto.GetComponent<GrabHelper>();
        if (grabHelper) grabHelper.Grab(hand);
       


        GrabBody(handler, hand);
        ToggleEditMode(true);

        FXManager.Instance.ToggleFX(FXCategory.BodyEditing, FXElement.All, "create", newProto.transform.position, newProto.transform.rotation, newProto.transform, true, true);

    }
    private List<ChemicalBodyCompositionElement> ReajustBodyComposition(List<ChemicalBodyCompositionElement> bodyComposition, float percentageFactor)
    {

        if (bodyComposition.Count == 0) return null;

        foreach (var chemical in bodyComposition)
        {
            chemical._percentage *= percentageFactor;
        }

        return bodyComposition;
    }
    private void PredictNewBody(AstralBody body)
    {
        if (Time.time % 5 == 0) return;
        AstralBodyType bodyType = BodyGenerator.Instance.PredictBodyTypeFromCharacteristic(body);

        //OnPredictBody?.Invoke(_grabbedBodyHandler, predictedBody);  
        Debug.Log("[Body Handler] Possiblebody : " + bodyType.ToString());
    }

    public void ScaleGrabbed(XRGrabInteractable grabbedInteractable)
    {
        if (!editModeOn) return;

        Debug.Log("[Body Editor] Scale Grabbed : " + grabbedInteractable + "/ Scale :" + grabbedInteractable.transform.localScale);
        if (_grabbedBodyHandler)
        {
            _grabbedBodyHandler.ScaleBody(true);
        }
    }



    private void Awake()
    {
        grabbingHand.handSide = HandSide.Unasigned;
    }

    private void Start()
    {
        startMassInjectionMultiplicator = massInjectionMultiplicator;
    }

    private void OnEnable()
    {
        EventBus.OnObjectGrabbed += StartGrab;
        EventBus.OnObjectReleased += EndGrab;
        EventBus.OnGrabbableProcess += ScaleGrabbed;
        EventBus.OnBodyUpdated += PredictNewBody;
        EventBus.OnToggleEditModeInput += ToggleEditMode;
        EventBus.OnInjectionStarted += StartInjection;
        EventBus.OnInjectionStopped += StopInjection;
        EventBus.OnPlayerStart += GetControllerHands;
        EventBus.OnCreateProtoBody += CreateProtoBody;
        EventBus.OnDoubleGrab += DoubleGrabBody;
    }

    

    private void OnDisable()
    {
        EventBus.OnObjectGrabbed -= StartGrab;
        EventBus.OnObjectReleased -= EndGrab;
        EventBus.OnGrabbableProcess -= ScaleGrabbed;
        EventBus.OnBodyUpdated -= PredictNewBody;
        EventBus.OnToggleEditModeInput -= ToggleEditMode;
        EventBus.OnInjectionStarted -= StartInjection;
        EventBus.OnInjectionStopped -= StopInjection;
        EventBus.OnPlayerStart -= GetControllerHands;
        EventBus.OnCreateProtoBody -= CreateProtoBody;
        EventBus.OnDoubleGrab -= DoubleGrabBody;
    }

   

    private void Update()
    {
      
    }

    private void OnDestroy()
    {
        
    }


}
