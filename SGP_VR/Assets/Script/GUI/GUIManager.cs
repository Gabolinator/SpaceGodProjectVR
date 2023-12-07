using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public enum GuiLocation 
{
    OnObject,
    OnWrist,
    BothWristAndObject,
    InFrontOfPlayer,
    none
}

public enum EGuiBehaviour
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
    Select,
    DoNothing

}

 [System.Serializable]
public struct GuiScreen
{
    public GameObject screenPrefab;
    public string screenName;
}


public class GUIManager : MonoBehaviour
{
    private static GUIManager _instance;
    public static GUIManager Instance => _instance;

    [SerializeField] private EGuiBehaviour _defaultGuiBehaviour;
    public EGuiBehaviour DefaultGuiBehaviour => _defaultGuiBehaviour;
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

    [Header("Screens")]
    [SerializeField] private List<GuiScreen> _screens = new List<GuiScreen>();
    public List<GuiScreen> Screens => _screens;
    
    
    
    [Header("Main Menu")]
    [SerializeField] private GameObject _mainMenu;
    public GameObject MainMenu
    {
        get { return _mainMenu ? _mainMenu : WristMenu.gameObject; }
        set => _mainMenu = value;
    }
    
    
    
    /*wrist gui*/
    [Header("Wrist Menu")]
    [SerializeField] private GUIWristScrollController _wristMenu;
    public GUIWristScrollController WristMenu => _wristMenu;

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
    public List<GameObject> guisInWord = new List<GameObject>();
    public List<GameObject> objectWithGuisAttached = new List<GameObject>();
    public List<GameObject> allGuis = new List<GameObject>();
    
    
    private Transform _mainCamera;
    [FormerlySerializedAs("keepMainMenuOn")] public bool forceMainMenuOn = true;

    #region Spawning 
    public GameObject SpawnGui(GameObject gui, Vector3 position, Quaternion rotation, GameObject parent = null )
    {
        if (gui == null) return null;
        GameObject guiClone;

        if (parent != null) guiClone = Instantiate(gui, position, rotation, parent.transform);

        else guiClone = Instantiate(gui, position, rotation);

        if (guiClone == null) return null;

        RegisterGui(guiClone, guisInWord);

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


  
    private bool AlreadySpawned(GameObject gui, List<GameObject> activeGuis)
    { 
       
        if(activeGuis.Count == 0 ) return false;


        foreach (var activeGui in activeGuis) 
        {
          if(activeGui == gui) return true;
        }

        return false;
    }

    private void AssignGuiToBody(AstralBodyHandler bodyHandler, bool overideLocation = false, GuiLocation newLocation = GuiLocation.none)
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
            if (!wristMount) wristMount = GetGuiMount(bodyHandler.gameObject, true, false);
            if (!wristMount)
            { // instantiate  wrist menu prefab
            }


            if (CanSpawnGUI(wristMount, bodyHandler))
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

        if (guiMounts.Count == 0) return;

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
            if (addScript) AddTransformScripts(gui, guiMount, newLocation);



            UpdateGuiDescriptor(gui, bodyHandler, guiMount);

            objectWithGuisAttached.Add(bodyHandler.gameObject);



            if (index == 0 && guiLocation != GuiLocation.OnObject)
            {
                WristGui = gui;
                WristGui.transform.localScale = new Vector3(.002f, .002f, .002f);
            }

            index++;
        }

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
        if (_mainCamera == null) FindMainCamera();
        if (_mainCamera == null) return;



        var colliders = CheckWhatIsInProximityRange();
        if (colliders.Length == 0) return;

        foreach (Collider collider in colliders)
        {
            var bodyHandler = collider.gameObject.GetComponent<AstralBodyHandler>();
            if (bodyHandler == null) continue;

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
    private bool CanSpawnGUI(GameObject obj, AstralBodyHandler handler)
    {

        if (!obj) return false;

        var guiContainer = obj.GetComponent<GuiContainer>();

        if (!guiContainer) guiContainer = obj.transform.parent.GetComponent<GuiContainer>();
        if (!guiContainer) guiContainer = obj.transform.parent.parent.GetComponent<GuiContainer>();

        if (!guiContainer) return false;

        var wristGui = guiContainer as GUIWristScrollController;
        if (!wristGui)
        {
            return CanSpawnGUI(obj);
        }

        if (wristGui.Guis.Count == 0) return true;

        foreach (var gui in wristGui.Guis)
        {
            if (gui.GetComponent<BodyDescriptorGUI>())
            {
                if (gui.GetComponent<BodyDescriptorGUI>()._descriptor._id == handler.ID) return false;
            }
        }

        return true;

    }
    private bool CanSpawnGUI(GameObject obj)
    {
        if (!obj) return false;

        var guiContainer = obj.GetComponent<GuiContainer>();
        //Debug.Log(obj +" gui transform.parent : " + obj.transform.parent);
        if (!guiContainer) guiContainer = obj.transform.parent.GetComponent<GuiContainer>();
        if (!guiContainer) guiContainer = obj.transform.parent.parent.GetComponent<GuiContainer>();

        if (!guiContainer) return false;

        return guiContainer.CurrentGui == null ? true : false;
    }

    #endregion

    #region Register/Unregister
    public void RegisterGui(GameObject gui, List<GameObject> guiList)
    {
        if (gui == null) return;
        guiList.Add(gui);
    }
    public void RegisterGui(GameObject gui)
    {
        RegisterGui(gui, allGuis);
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

    public bool UnRegisterGui(GameObject gui, bool instant = false)
    {
        return UnRegisterGui(gui, allGuis, instant);
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

    #region Gui configuration 
    private void UpdateGuiDescriptor(GameObject gui, AstralBodyHandler bodyHandler, GameObject guiMount)
    {
        if (!gui) return;
        if (!bodyHandler) return;
        
        UpdateGuiText(gui, bodyHandler);
        
       
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

    #endregion

    #region Toggle/Destroy Gui
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
        if (guisInWord.Count == 0) return;

        var bodyGO = body.gameObject;
        var index = 0;

        var objectsWithGuiClone = new List<GameObject>(objectWithGuisAttached);

        foreach (var obj in objectsWithGuiClone)
        {
            if (obj == body.gameObject)
            {
                ToggleGui(guisInWord[index], state);
               
            }
            index++;
        }
    }

    public void ToggleGuis(bool state) 
    {
        if (guisInWord.Count == 0) return;

        foreach (var gui in guisInWord)
        {
            gui.SetActive(state);
        }
    }

    private void DestroyGui(AstralBodyHandler body)
    {
        if (!body) return;
        if (guisInWord.Count == 0) return;

        var bodyGO = body.gameObject;
        var index = 0;

        var objectsWithGuiClone = new List<GameObject>(objectWithGuisAttached);

        foreach (var obj in objectsWithGuiClone)
        {
            if (obj == body.gameObject)
            {
                if(UnRegisterGui(guisInWord[index], guisInWord, true)) objectWithGuisAttached.Remove(obj);
            }
        }
    }

    private void DestroyGuiIfTooFar(GameObject gui, float distanceToDestroy) 
    {

        if (DistanceFromPlayer(gui) <= distanceToDestroy) return;

        int index = guisInWord.IndexOf(gui);
        if(index < objectWithGuisAttached.Count && index >= 0 ) objectWithGuisAttached.RemoveAt(index);
        UnRegisterGui(gui, guisInWord);


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
            if (_defaultGuiBehaviour == EGuiBehaviour.DestroyIfPlayerTooFar)
            {
                CheckGuisForDestroy(DistanceToDestroy, guisInWord);

            }

            yield return new WaitForSeconds(delay);

        } while (true);
    }
    
    #endregion

    #region WristMenu 
    public void ToggleWristMenu() 
    {
       if(!WristMenu) return;
        bool state = WristMenu.gameObject.activeInHierarchy;

        ToggleGui(WristMenu.gameObject, !state);
    }

    public void ToggleWristMenu(bool state)
    {
        if (!WristMenu) return;
       
        ToggleGui(WristMenu.gameObject, state);
    }

    public void MoveGuiToWrist(bool state, AstralBodyHandler body)
    {
        _bodyGuiSpawnCondition = GuiSpawnCondition.DoNothing;
        if(_bodyGuiLocation != GuiLocation.OnWrist ) DestroyGui(body);

        if (state) 
        {

            AssignGuiToBody(body, true, GuiLocation.OnWrist);
            var bodyDescriptor = WristGui.GetComponent<BodyDescriptorGUI>();
            if (bodyDescriptor != null) bodyDescriptor.SetEditMode(true);

        }
        else
        {

            _bodyGuiSpawnCondition = _chachedGuiSpawnCondition;
            
            if (WristGui == null) return;
            if (!WristGui.GetComponent<BodyDescriptorGUI>()) return;

            if (_wristMenu )         
            {
                _wristMenu.DestroyWristGui(WristGui);
                   
                WristGui = null; 
            }
                
            else Destroy(WristGui);

        }
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


    #endregion

    #region Other
    private float DistanceFromPlayer(GameObject obj)
    {
        if (!obj) return -1;
        var player = GameManager.Instance.localPlayer;
        if (player == null) return -1;

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

    #endregion

    public GameObject GetScreenPrefab(string screenName)
    {
        if (Screens.Count == 0) return null;
        if (string.IsNullOrEmpty(screenName)) return null;
        
        foreach (var screen in Screens)
        {
            if (screen.screenName == screenName) return screen.screenPrefab;
        }

        Debug.Log("[GuiManager] No screen found by name : " + screenName);
        return null;
    }

    public bool AreFromSamePrefab(GameObject obj1, GameObject obj2)
    {
        GameObject prefab1 = PrefabUtility.GetCorrespondingObjectFromSource(obj1);
        GameObject prefab2 = PrefabUtility.GetCorrespondingObjectFromSource(obj2);

        return prefab1 != null && prefab1 == prefab2;
    }

    // Check if a GameObject is instantiated from a specific prefab
    public bool IsInstantiatedFromPrefab(GameObject obj, GameObject prefab)
    {
        GameObject prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        Debug.Log("PrefabObj :" + prefabObj);
        return prefabObj != null && prefabObj == prefab;
    }
    
    public void ToggleMainMenu()
    {
        if(forceMainMenuOn) return;
        bool currentState = MainMenu.activeSelf;
        ToggleGui(MainMenu, !currentState);
        
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
        if (_wristMenu)
        {
            RegisterGuis(_wristMenu.Guis);
            ToggleWristMenu(true);
        }

        if (UIsContainer) guisInWord = GetGuisInContainer(UIsContainer);
    }

    private List<GameObject> GetGuisInContainer(GameObject container)
    {
        List<GameObject> guiGos = new List<GameObject>();
        if (!container) return guiGos;

        var guis = container.GetComponentsInChildren<GUIBehaviour>();
        if(guis.Length == 0) return guiGos;
        foreach (var gui in guis)
        {
            guiGos.Add(gui.gameObject);
        }

        return guiGos;
    }

     public GameObject GuiAlreadyPresent(GameObject screen)
    {
        if (allGuis.Count == 0) return null;
        var isPrefab = PrefabUtility.IsPartOfAnyPrefab(screen);
        
        List<GameObject> allObjectFromPrebab = new List<GameObject>();
        /*Is a prefab check all object that have been instantiated from it*/
        if (isPrefab)
        {
            foreach (var gui in allGuis)
            {
                var guiB = gui.GetComponent<GUIBehaviour>();
                if(!guiB) continue;
                 if(guiB.RootPrefab != null && guiB.RootPrefab == screen) allObjectFromPrebab.Add(gui);
            }
        }

        /*Is instantiated - get all object that have the same prefab*/
        else
        {
            foreach (var gui in allGuis)
            {
                if(AreFromSamePrefab(gui, screen)) allObjectFromPrebab.Add(gui);
            }
        }

        return allObjectFromPrebab.Count == 0 ? null : allObjectFromPrebab[0]; //returning first object for now
    }

    public void OpenScreen(string screenName) //TODO- for now only on wristMenu - eventually add option to open elsewhere than wristmenu 
    {
        if (screenName == "All")
        {
         OpenAllScreens();
         return;
        }
        
            var screen = GetScreenPrefab(screenName);
            if (!screen) return;

            var guiAlreadyPresent = GuiAlreadyPresent(screen);
            if (guiAlreadyPresent)
            {
                WristGui = guiAlreadyPresent;
                return;
            }

            var newScreen = Instantiate(screen);
            if (!newScreen) return;

            var gui = newScreen.GetComponent<GUIBehaviour>();
            if (gui) gui.RootPrefab = screen;
            WristGui = newScreen;
        
        
    }

    private void OpenAllScreens()
    {
        if (Screens.Count == 0) return;
      
        foreach (var screen in Screens)
        {
            OpenScreen(screen.screenName);
        }

        
    }
    

    private void OnEnable()
    {
     
        if (_bodyGuiSpawnCondition == GuiSpawnCondition.RayPointAtObject) EventBus.OnAstralBodyRayHit += AssignGuiToBodyOnRayPoint;
        EventBus.OnPlayerMoving += ToggleGuisWithPlayerMovement;
        EventBus.OnPlayerStoppedMoving += ToggleGuisWithPlayerMovement;
        EventBus.OnAstralBodyDestroyed += DestroyGui;
        EventBus.OnBodyEdit += MoveGuiToWrist;
        EventBus.OnToggleMainMenu += ToggleMainMenu;
        //EventBus.OnAstralBodyStartToExist += AssignGuiToBody;
    }

    

    private void OnDisable()
    {
        if (_bodyGuiSpawnCondition == GuiSpawnCondition.RayPointAtObject) EventBus.OnAstralBodyRayHit -= AssignGuiToBodyOnRayPoint;
        EventBus.OnPlayerMoving -= ToggleGuisWithPlayerMovement;
        EventBus.OnPlayerStoppedMoving -= ToggleGuisWithPlayerMovement;
        EventBus.OnAstralBodyDestroyed -= DestroyGui;
        EventBus.OnBodyEdit -= MoveGuiToWrist;
        EventBus.OnToggleMainMenu -= ToggleMainMenu;
        
        //EventBus.OnAstralBodyStartToExist -= AssignGuiToBody;
    }

   
}
