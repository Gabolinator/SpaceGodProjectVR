using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum GuiLocation 
{
    OnObject,
    OnWrist,
    BothWristAndObject,
    none
}

public enum GuiBehaviour
{
    FollowPlayer,
    DestroyIfPlayerTooFar,
    Sticky,
    DoNothing

}

public enum GuiSpawnCondition
{
    RayPointAtObject,
    LookAt,
    DistanceFromPlayer,
    DoNothing

}



public class GUIManager : MonoBehaviour
{
    private static GUIManager _instance;
    public static GUIManager Instance => _instance;

    [SerializeField] private GuiBehaviour _defaultGuiBehaviour;
    public GuiBehaviour DefaultGuiBehaviour => _defaultGuiBehaviour;
    [SerializeField] private float _distanceToDestroy;
    public float DistanceToDestroy => _distanceToDestroy;

    [Header("AstralBody Gui")]
    [SerializeField] private GameObject _bodyGuiPopUp;
    public GameObject BodyGuiPopUp => _bodyGuiPopUp;

    [SerializeField] private GuiLocation _bodyGuiLocation;
    public GuiLocation BodyGuiLocation => _bodyGuiLocation;

    [SerializeField] private GuiSpawnCondition _bodyGuiSpawnCondition;
    public GuiSpawnCondition BodyGuiSpawnCondition => _bodyGuiSpawnCondition;
    public float _maxDetectionRange = 10f;
    private GuiSpawnCondition _chachedGuiSpawnCondition;

    [Header("Main Menu")]
    [SerializeField] private GameObject _mainMenu;
    public GameObject MainMenu => _mainMenu;


    [Header("Fade In/Out")]
    [SerializeField] private bool _fadeIn;
    public bool FadeIn => _fadeIn;

    [SerializeField] private bool _fadeOut;
    public bool FadeOut=> _fadeOut;
    [SerializeField] private float _fadeDuration;
    public float FadeDuration => _fadeDuration;

    [SerializeField] private float _delayFade;
    public float DelayFade => _delayFade;

    [Header("All Active UIs")]
    [SerializeField] private GameObject _uisContainer;
    public GameObject UIsContainer => _uisContainer;
    public List<GameObject> activeGuis = new List<GameObject>();
    public List<GameObject> objectWithGuisAttached = new List<GameObject>();

    /*wrist gui*/
    public GUIWristScrollController _wristMenu;
    private GameObject _wristGui;
    public GameObject WristGui
    {
        get { return _wristMenu ? _wristMenu.CurrentGui : _wristGui; }
        set 
        { 
            if (_wristMenu) 
            {
                _wristGui = value;
                _wristMenu.CurrentGui = value;
            } 
            else _wristGui = value;
        }
    }
   

    //[Header("Debug")]
   
    //public bool _attachToWrist = true;
    
   
    private Transform _mainCamera;
    

    //public Dictionary<GameObject, GameObject> objectWithGui = new Dictionary<GameObject, GameObject>();


    #region Spawning 
    public GameObject SpawnGui(GameObject gui, Vector3 position, Quaternion rotation, GameObject parent = null )
    {
        if (gui == null) return null;
        GameObject guiClone;

      

        if (parent != null) guiClone = Instantiate(gui, position, rotation, parent.transform);

        else guiClone = Instantiate(gui, position, rotation);

        if (guiClone == null) return null;

        RegisterGui(guiClone, activeGuis);

        return guiClone;
    }

  
    public GameObject SpawnGui(GameObject gui,Transform mount, GameObject parent = null)
    {
        if(mount == null) return null;

       
    

        return SpawnGui(gui, mount.position, mount.rotation, parent);
    }

    public GameObject SpawnGui(GameObject gui, Vector3 position, Vector3 rotationEuler, GameObject parent = null)
    {
        return SpawnGui(gui, position, Quaternion.Euler(rotationEuler), parent);
    }

    public GameObject SpawnGui(GameObject gui, GameObject go, GameObject parent = null)
    {
        if(!go) return null;

        return SpawnGui(gui, go.transform, parent);
    }


    public GameObject AddGuiToWristScrollMenu(GameObject gui, GUIWristScrollController wristMenu, GameObject parent = null) 
    {
        if (!wristMenu) return null;
        var go = wristMenu.gameObject;


        var newGui = SpawnGui(gui, go.transform, parent);
        newGui.transform.localPosition = Vector3.zero;
        newGui.transform.localRotation = Quaternion.identity;

        wristMenu.AddGuiToWristMenu(newGui, true);

        return newGui;
    }

    private bool AlreadySpawned(GameObject gui, List<GameObject> activeGuis)
    { 
       
        if(activeGuis.Count == 0 ) return false;


        foreach (var activeGui in activeGuis) 
        {
          if(activeGui == gui) return true;
        }

        return false;
    }

  

    #endregion

    #region Register/Unregister
    public void RegisterGui(GameObject gui, List<GameObject> guiList)
    {
        if (gui == null) return;
        guiList.Add(gui);
    }

    public void RegisterGuis(List<GameObject> guis)
    {
        if (guis.Count == 0) return;

        foreach (var gui in guis) 
        {
            if(!guis.Contains(gui)) RegisterGui(gui, guis);
        }
    }

    private bool UnRegisterGui(GameObject gui, List<GameObject> guiList, bool instant = false)
    {
        if (gui == null || guiList.Count == 0) return false;

        if (!guiList.Contains(gui)) return false;

        var guiBehaviour = gui.GetComponent<GUIBehaviour>();
        if (guiBehaviour != null)
        {
            if (guiBehaviour.FadeOut && !instant)
            {
                guiBehaviour.FadeGuiOut();
                StartCoroutine(UnRegisterGuiCoroutine(gui, guiList));
                return true;
            }
        }
        
        guiList.Remove(gui);
        Destroy(gui);

        return true;
    }

    private IEnumerator UnRegisterGuiCoroutine(GameObject gui, List<GameObject> guiList)
    {
        var guiBehaviour = gui.GetComponent<GUIBehaviour>();
        do 
        { 
            
            yield return null;

        } while (guiBehaviour.isFading);

        guiList.Remove(gui);
        Destroy(gui);
        
    }
    #endregion
    
    private void AssignGuiToBody(AstralBodyHandler bodyHandler, bool overideLocation = false,GuiLocation newLocation = GuiLocation.none) 
    {
        GuiLocation guiLocation = BodyGuiLocation;
        if (overideLocation) guiLocation = newLocation;
        if (guiLocation == GuiLocation.none) return;


        Debug.Log("[Gui Manager] " + _bodyGuiSpawnCondition.ToString() + " :  Assign gui to : " + bodyHandler.ID);

        List<GameObject> guiMounts = new List<GameObject>();


        if (guiLocation != GuiLocation.OnObject)
        {
            /*attach to wrist */
            GameObject wristMount = _wristMenu.gameObject;
            if(!wristMount) wristMount = GetGuiMount(bodyHandler.gameObject, true, false);
            if (!wristMount) 
            { // instantiate  wrist menu prefab
            }

            if (CanSpawnGUI(wristMount))
            { 
                guiMounts.Add(wristMount);
            }
        }


        if (guiLocation != GuiLocation.OnWrist)
        {
            GameObject goMount = GetGuiMount(bodyHandler.gameObject, false, false);

            if (CanSpawnGUI(goMount))
            {
                guiMounts.Add(goMount);
            }
        }

        var index = 0;
        foreach (var guiMount in guiMounts)
        {

            var gui = SpawnGui(BodyGuiPopUp, bodyHandler.gameObject, UIsContainer);

            var guiContainer = guiMount.GetComponent<GuiContainer>();
            //Debug.Log("guiMount.transform.parent : " + guiMount.transform.parent);
            if (!guiContainer) guiContainer = guiMount.transform.parent.GetComponent<GuiContainer>();
            if (!guiContainer) guiContainer = guiMount.transform.parent.parent.GetComponent<GuiContainer>();
            if (!guiContainer) continue;
                
            if (guiContainer.CurrentGui == gui) 
            {
                DestroyGui(bodyHandler);
                    
                continue;
            }

            guiContainer.CurrentGui = gui;


            /*only if not wrist*/
            bool addScript = _wristMenu ? guiMount != _wristMenu.gameObject : true;
            if(addScript) AddTransformScripts(gui, guiMount, newLocation);
            

            UpdateGuiText(gui, bodyHandler);
           

            objectWithGuisAttached.Add(bodyHandler.gameObject);



            if (index == 0 && guiLocation != GuiLocation.OnObject)
            {
                WristGui = gui;
                WristGui.transform.localScale = new Vector3(.002f, .002f, .002f);
            }

            index++;
        }

    }

    private void AddTransformScripts(GameObject gui, GameObject guiMount, GuiLocation newLocation)
    {
        var followTransform = gui.GetComponent<FollowTransform>();
        if (!followTransform) followTransform = gui.AddComponent<FollowTransform>();
        followTransform.FollowTarget = guiMount.transform;
        followTransform.MatchRotation = newLocation != GuiLocation.OnObject;

        var faceCamera = gui.GetComponent<FaceCamera>();
        if (!faceCamera) faceCamera = gui.AddComponent<FaceCamera>();
        faceCamera.ignoreTilt = false;
        faceCamera.enabled = true;
    }

    private void UpdateGuiText(GameObject gui, AstralBodyHandler bodyHandler)
    {
        if(!gui) return;
        if (!bodyHandler) return;
        var configurator = gui.GetComponent<BodyDescriptorGUI>();
        if (!configurator) configurator = gui.AddComponent<BodyDescriptorGUI>();

        configurator.UpdateGUIText(bodyHandler.bodyDescriptor);
    }

    private void AssignGuiToBody(AstralBodyHandler bodyHandler)
    {
       
        AssignGuiToBody(bodyHandler, false);
    }

    private void AssignGuiToBodyOnRayPoint(AstralBodyHandler bodyHandler) 
    {

        if (BodyGuiSpawnCondition != GuiSpawnCondition.RayPointAtObject) return;
        AssignGuiToBody(bodyHandler, false);
    }

    public void SpawnGuisInProximityRange()
    {
        if(_mainCamera == null) FindMainCamera();
        if(_mainCamera == null) return;



        var colliders = CheckWhatIsInProximityRange();
        if (colliders.Length == 0) return;

        foreach (Collider collider in colliders) 
        {
            var bodyHandler = collider.gameObject.GetComponent<AstralBodyHandler>();
            if(bodyHandler == null) continue;

            AssignGuiToBody(bodyHandler);

        }

    }

    private Collider[] CheckWhatIsInProximityRange()
    {
        if (_mainCamera == null) return null;

        var center = _mainCamera.transform.position;

        Collider[] colliderInRange = Physics.OverlapSphere(center, _maxDetectionRange);

        return colliderInRange;
    }


    private GameObject GetGuiMount(GameObject go, bool attachToWrist, bool leftHand)
    {

        GameObject mount = go;

        if (attachToWrist)
        {
            var localPlayer = GameManager.Instance.localPlayer;
            if (localPlayer) mount = leftHand ? localPlayer.LeftController : localPlayer.RightController;
            
        }

        var guiContainer = mount.gameObject.GetComponent<GuiContainer>();
        if (!guiContainer) guiContainer = mount.gameObject.GetComponentInChildren<GuiContainer>();

        if (guiContainer) mount = guiContainer.GuiMount != null ? guiContainer.GuiMount : mount;

        return mount;
    }

    private bool CanSpawnGUI(GameObject obj)
    {
        if (!obj) return false;

        var guiContainer = obj.GetComponent<GuiContainer>();
        //Debug.Log(obj +" gui transform.parent : " + obj.transform.parent);
        if (!guiContainer) guiContainer = obj.transform.parent.GetComponent<GuiContainer>();
        if (!guiContainer) guiContainer = obj.transform.parent.parent.GetComponent<GuiContainer>();

        if (!guiContainer) return false;

        var wristGui = guiContainer as GUIWristScrollController;
        if (wristGui) return true;

        return guiContainer.CurrentGui == null ? true : false;
    }

  

    private void ToggleGuisWithPlayerMovement(bool isPlayerMoving)
    {
        //ToggleGuis(!isPlayerMoving);
        ToggleGui(_wristGui, !isPlayerMoving);
    }

    public void ToggleGui(GameObject gui , bool state)
    {
        if (gui == null) return;

        gui.SetActive(state);
        
    }

    public void ToggleGui(AstralBodyHandler body, bool state) 
    {
        if (!body) return;
        if (activeGuis.Count == 0) return;

        var bodyGO = body.gameObject;
        var index = 0;

        var objectsWithGuiClone = new List<GameObject>(objectWithGuisAttached);

        foreach (var obj in objectsWithGuiClone)
        {
            if (obj == body.gameObject)
            {
                ToggleGui(activeGuis[index], state);
               
            }
            index++;
        }
    }

    public void ToggleGuis(bool state) 
    {
        if (activeGuis.Count == 0) return;

        foreach (var gui in activeGuis)
        {
            gui.SetActive(state);
        }
    }

    private void DestroyGui(AstralBodyHandler body)
    {
        if (!body) return;
        if (activeGuis.Count == 0) return;

        var bodyGO = body.gameObject;
        var index = 0;

        var objectsWithGuiClone = new List<GameObject>(objectWithGuisAttached);

        foreach (var obj in objectsWithGuiClone)
        {
            if (obj == body.gameObject)
            {
                if(UnRegisterGui(activeGuis[index], activeGuis, true)) objectWithGuisAttached.Remove(obj);
            }
        }
    }

    private void DestroyGuiIfTooFar(GameObject gui, float distanceToDestroy) 
    {

        if (DistanceFromPlayer(gui) <= distanceToDestroy) return;

        int index = activeGuis.IndexOf(gui);
        if(index < objectWithGuisAttached.Count && index >= 0 ) objectWithGuisAttached.RemoveAt(index);
        UnRegisterGui(gui, activeGuis);


    }

    private float DistanceFromPlayer(GameObject obj) 
    {
     if(!obj) return -1;
    var player = GameManager.Instance.localPlayer;
     if (player == null) return -1;
       // Debug.Log("Distance: " + Vector3.Distance(gui.transform.position, player.transform.position));
        return Vector3.Distance(obj.transform.position, player.transform.position);
    }


    public void FindMainCamera()
    {
        if (_mainCamera == null)
        {
            // Find By Tag instead of Camera.main as the camera could be disabled
            if (GameObject.FindGameObjectWithTag("MainCamera") != null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
            }
        }
    }

    private void CheckGuisForDestroy(float distanceToDestroy, List<GameObject> activeGuis)
    {
        if (activeGuis.Count == 0) return;

        var cloneList = new List<GameObject>(activeGuis);
        foreach (var gui in cloneList)
        {
            DestroyGuiIfTooFar(gui, distanceToDestroy);
        }
    }



    public void MoveGuiToWrist(bool state, AstralBodyHandler body)
    {
        _bodyGuiSpawnCondition = GuiSpawnCondition.DoNothing;
       // DestroyGui(body);

        if (state) 
        {

            AssignGuiToBody(body, true, GuiLocation.OnWrist);
            var bodyDescriptor = WristGui.GetComponent<BodyDescriptorGUI>();
            if (bodyDescriptor != null) bodyDescriptor.SetEditMode(true);

        }
        else
        {
            //if (_wristMenu) _wristMenu.DestroyWristGui(WristGui); 
            //else Destroy(_wristGui);
            //_wristGui = null;
            _bodyGuiSpawnCondition = _chachedGuiSpawnCondition;
        }
    }


    public void MoveGuiToWrist(bool state, GameObject gui)
    {
       
    }


    private IEnumerator CheckWhatToDoWithGuis(float delay)
    {
        do 
        {
            /*spawn*/
            if (BodyGuiSpawnCondition == GuiSpawnCondition.DistanceFromPlayer) 
            {
                SpawnGuisInProximityRange();
            }


            /*destroy*/
            if (_defaultGuiBehaviour == GuiBehaviour.DestroyIfPlayerTooFar) 
            {
                CheckGuisForDestroy(DistanceToDestroy, activeGuis);

            }

            yield return new WaitForSeconds(delay);

        }while (true);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _chachedGuiSpawnCondition = _bodyGuiSpawnCondition;
        StartCoroutine(CheckWhatToDoWithGuis(1f));
        FindMainCamera();

    }

    private void Update()
    {
        Debug.Log("wrist gui: " + WristGui);
    }

    private void OnEnable()
    {
     
        if (_bodyGuiSpawnCondition == GuiSpawnCondition.RayPointAtObject) EventBus.OnAstralBodyRayHit += AssignGuiToBodyOnRayPoint;
        EventBus.OnPlayerMoving += ToggleGuisWithPlayerMovement;
        EventBus.OnPlayerStoppedMoving += ToggleGuisWithPlayerMovement;
        EventBus.OnAstralBodyDestroyed += DestroyGui;
        EventBus.OnBodyEdit += MoveGuiToWrist;
        //EventBus.OnAstralBodyStartToExist += AssignGuiToBody;
    }

    

    private void OnDisable()
    {
        if (_bodyGuiSpawnCondition == GuiSpawnCondition.RayPointAtObject) EventBus.OnAstralBodyRayHit -= AssignGuiToBodyOnRayPoint;
        EventBus.OnPlayerMoving -= ToggleGuisWithPlayerMovement;
        EventBus.OnPlayerStoppedMoving -= ToggleGuisWithPlayerMovement;
        EventBus.OnAstralBodyDestroyed -= DestroyGui;
        EventBus.OnBodyEdit -= MoveGuiToWrist;
        //EventBus.OnAstralBodyStartToExist -= AssignGuiToBody;
    }
}
