using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUIWristScrollController : GuiContainer
{

    private float _startAngleOffset;
    private float _angleOffset;
    public float _radius = 1;
    private float _angleTolerance;
    public float neighbourGuiAlpha = .3f;
   
    [Range(0.1f, 10f)]
    public float _scrollSpeed;
    
    [Header("FallBack Gui")]
    [SerializeField] private GameObject _fallBackPrefab;
    [SerializeField] private GameObject _fallBackGui;
    
    public GameObject FallBackGui
    {
        get
        {
            if (!_fallBackGui)
            {
                if (_fallBackPrefab)
                {
                    Debug.Log("[WristMenu] instantiating FallBack GUI ");
                    return _fallBackGui = Instantiate(_fallBackPrefab, this.transform);
                }

                
                return null;
            }

            return _fallBackGui;

        }

        set => _fallBackGui = value;
    }

    [Header("Debug")]
    public bool testMode = false;
    public bool useIndex;
    public int indexToFront;

    private GameObject _nextGui;
    private GameObject _previousGui;
    private int _indexCurrentGui;
    private float _angle;
    private float _cachedAngleOffset;
    

    [SerializeField] private List<GameObject> _guis;
    private int numberOfTurn = 0;
    public List<GameObject> Guis { get => _guis; set => _guis = value; }

    public override GameObject CurrentGui {

        get
        {
            if(base.CurrentGui == null) return _previousGui ? _previousGui : FallBackGui;
            
            return base.CurrentGui;
        }
        set
        {
            if (CurrentGui == value && value != null) return;

            if (value != null)
            {
                UpdateGuisPosition(_angleOffset);
                MoveGuiToCentralPosition(value);
            }

            else
            {
                base.CurrentGui = _previousGui ? _previousGui : FallBackGui;
                UpdateGuisPosition(_angleOffset);
                MoveGuiToCentralPosition(base.CurrentGui);
                
            }
        }
        
    }



    public int GetIndexOfGui(GameObject gui, List<GameObject> guis)
    {
        int index = -1;
        if (guis.Count == 0) return index;
        if (!guis.Contains(gui)) return index;


        index = guis.IndexOf(gui);
        return index;
    }


    public int GetIndexOfNeighbouringGui(GameObject currentCui, List<GameObject> guis, bool preceding)
    {

        if (guis.Count == 1) return -1; //only one element , no neighbour

        int indexOfCurrentGui = GetIndexOfGui(currentCui, guis);
        if (indexOfCurrentGui < 0) return -1; // count find current gui in list

        if (preceding) return indexOfCurrentGui == 0 ? guis.Count - 1 : indexOfCurrentGui - 1; //are we first element , if yes, preceding guis is last index
        return indexOfCurrentGui == guis.Count - 1 ? 0 : indexOfCurrentGui + 1;  //are we last element , if yes, next guis is first
    }

    public int GetIndexOfNeighbouringGui(int currentIndex, List<GameObject> guis, bool preceding)
    {

        if (guis.Count == 1) return -1; //only one element , no neighbour


        if (preceding) return currentIndex == 0 ? guis.Count - 1 : currentIndex - 1; //are we first element , if yes, preceding guis is last index
        return currentIndex == guis.Count - 1 ? 0 : currentIndex + 1;  //are we last element , if yes, next guis is first
    }

    public GameObject GetGuiByIndex(List<GameObject> guis, int index)
    {
        if (guis.Count == 0) return null;
        if (index < 0) return null;

        if (index >= guis.Count) return null;
        return guis[index];
    }

    private void MakeCurrentGui(GameObject newGui)
    {

        if (!Guis.Contains(newGui))
        {
            AddGuiToWristMenu(newGui, true);
            return;
        }

        _currentGui = newGui;
        _indexCurrentGui = GetIndexOfGui(_currentGui, Guis);
       
        if (_currentGui) ToggleGui(_currentGui, true);
    }

    private GameObject GetCurrentGuiByPosition(float radius, float angleTolerance)
    {
        if (Guis.Count == 0) return null;

        
        foreach (var gui in Guis) 
        {
            if (!gui) continue;
        
            float z = gui.transform.localPosition.z;

            var angle = angleTolerance *Mathf.Deg2Rad;

            if (z >=  radius*Mathf.Cos(angle)) return gui;
        }

        return null;
    }

    private void UpdateNeighbouringGuis(bool useIndex = false)
    {   
        if(CurrentGui == null) 
        {
            ToggleGui(_previousGui, false, true);
            ToggleGui(_nextGui,false, true);

            _previousGui = null;
            _nextGui = null;
            return;
        }

        UpdateNextGuis(useIndex);
        UpdatePreviousGuis(useIndex);

        if(_previousGui.gameObject.activeSelf)  SetMaxGuiAlpha(_previousGui, neighbourGuiAlpha);
        if (_nextGui.gameObject.activeSelf) SetMaxGuiAlpha(_nextGui, neighbourGuiAlpha);

        ToggleGui(_previousGui, true, true);
        ToggleGui(_nextGui, true, true);
    }

    private void UpdatePreviousGuis(bool useIndex = false)
    {
        int indexPrevGui = -1;
        if (useIndex) indexPrevGui = GetIndexOfNeighbouringGui(_indexCurrentGui, Guis, true);

        else indexPrevGui = GetIndexOfNeighbouringGui(CurrentGui, Guis, true);
        //Debug.Log("index prev :" + indexPrevGui);
        if (indexPrevGui < 0) return;
        _previousGui = GetGuiByIndex(Guis, indexPrevGui);
        //_previousGuiIndex = indexPrevGui;
    }

    private void UpdateNextGuis(bool useIndex = false)
    {
        int indexNextGui = -1;
        if (useIndex) indexNextGui = GetIndexOfNeighbouringGui(_indexCurrentGui, Guis, false);

        else indexNextGui = GetIndexOfNeighbouringGui(CurrentGui, Guis, false);

       // Debug.Log("index next :" + indexNextGui);
        if (indexNextGui < 0) return;
        _nextGui = GetGuiByIndex(Guis, indexNextGui);
       // _nextGuiIndex = indexNextGui;

    }

    public void AddGuiToWristMenu(GameObject newGui, bool makeCurrent)
    {
        if (!newGui) return;
        /*check if already there ? */

        _guis.Add(newGui);

        newGui.transform.parent = this.transform;
        newGui.transform.localPosition = Vector3.zero;
        newGui.transform.localEulerAngles = new Vector3(0, 180, 0);

        _angle = CalculateAngle(_guis);

        if (!makeCurrent) return;

        MakeCurrentGui(newGui);
        UpdateNeighbouringGuis(true);
    }

    public void HideAllExcept(List<GameObject> allGuis, List<GameObject> guisToShow, bool show = false)
    {
        if (allGuis.Count == 0) return;

        if (!show)
        {
            foreach (var gui in allGuis)
            {

                if (guisToShow.Count == 0)
                {
                   ToggleGui(gui, false);
                }

                else
                {
                    if (!guisToShow.Contains(gui)) ToggleGui(gui, false);
                }
            }
        }

        else 
        {

            foreach (var gui in allGuis)
            {
               ToggleGui(gui, show);
            }

        }
    }

    private void HideAll()
    {
        if (_guis.Count == 0) return;

        foreach(var gui in _guis) 
        {
            if (!gui) continue;
            ToggleGui(gui, false);
        }
        
    }

    private void OrganizeGuis(List<GameObject> allGuis, float radius, float offset = 0, float spread = 0f)
    {
        if (allGuis.Count == 0) return;
        if (allGuis.Count == 1)
        {
            /*dont need to organise if just one element*/
            if (!allGuis[0])
            {
                allGuis[0] = CurrentGui; //make sure the element is not missing - > CurrentGui is gonna be fallback if null
                
                return;
            }

            
            if(allGuis[0].transform.localPosition != Vector3.zero) allGuis[0].transform.localPosition = Vector3.zero;
            return;
        }

        int numberOfElements = allGuis.Count;
        float angleStep = 360f / numberOfElements; 

        if (spread != 0)
        {
            angleStep += spread;
            numberOfElements = (int)(360 / angleStep);
        }

        if (_angle != angleStep)
        {
            _angle = angleStep;
            _startAngleOffset = angleStep / 2;
        }
       
        offset += _startAngleOffset;


        for (int i = 0; i < numberOfElements; i++)
        {
            SetGuiPositionInCircle(allGuis[i], radius ,i * angleStep , offset);
        }
    }


    public void SetGuiPositionInCircle(GameObject gui, float radius, float angleStep, float angleOffset) 
    {
        gui.transform.localPosition = Vector3.zero;

        float angle = angleStep * Mathf.Deg2Rad; // Convert angle to radians.
        angle += angleOffset * Mathf.Deg2Rad;

        // Calculate the position of the GUI element in the circle.
        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);

        Vector3 newPosition = gui.transform.localPosition + new Vector3(x, 0f, z);


        gui.transform.localPosition = newPosition;
    }

    private List<GameObject> GetGuisInTransform() 
    {
        var guis = GetComponentsInChildren<GUIBehaviour>();
        List<GameObject> guisList = new List<GameObject>();

        if(guis.Length== 0 ) return null;
        foreach (var gui in guis) 
        {
            if (!gui) continue;
            guisList.Add(gui.gameObject);
            
        }

        return guisList;    
    }

    private List<GameObject> GetGuisToShow()
    {
        List<GameObject> guistoShow = new List<GameObject>();
        guistoShow.Add(_previousGui);
        guistoShow.Add(_nextGui);
        guistoShow.Add(_currentGui);

        return guistoShow;
    }


    public void UpdateGuisPosition(float value) 
    {
        if (Guis.Count == 0) return ;
        
        OrganizeGuis(Guis, _radius,  _angleOffset = value);
        
        if (Guis.Count == 1) return;
            
        MakeCurrentGui(GetCurrentGuiByPosition(_radius, _angleTolerance = _startAngleOffset));
        
        SetMaxGuiAlpha(CurrentGui, 1f);

        UpdateNeighbouringGuis(true);
        HideAllExcept(Guis, GetGuisToShow());

    }

    public void MoveGuiToCentralPosition(int index)
    {
        if (_guis.Count == 0|| _guis.Count == 1) return;
        //if (_guis.Count == 1) index = 0;
        

        /* Make sure index is in range*/
        if (index > _guis.Count) index %= _guis.Count;
        else if( index < 0) index %= -_guis.Count;
 

        int destinationIndex = index;
        float offset = (_guis.Count -2) *_startAngleOffset/2; // to have the gui centered - else its a bit offset 
       
        
        //Debug.Log("Destination Index : " + destinationIndex);
        float destinationAngle = -(destinationIndex * _angle) + offset;
        
        /*Count the number of turn  - numberOfTurn > 0 : turn left : numberOfTurn < 0 turn right*/
        if (index - _indexCurrentGui == _guis.Count -1 )
        {
            numberOfTurn++;
        }
        
        else if (index - _indexCurrentGui == -(_guis.Count -1) )
        {
            numberOfTurn--;
        }
        
       
        //Debug.Log("[ScrollGUI] Addoffset to destination angle : " +angleOffset + "* numberofturn : " + numberOfTurn );
       
        /*Add 360 degres to number when we increment number of turn*/ 
        destinationAngle += numberOfTurn*360f;
        
        
        StartCoroutine(LerpGuisToAngle(_angleOffset, destinationAngle, 1/_scrollSpeed));
    }

    public void MoveGuiToCentralPosition(GameObject gui) 
    {
        if (!_guis.Contains(gui)) AddGuiToWristMenu(gui, true);

        int destinationIndex = _guis.IndexOf(gui);
        
        MoveGuiToCentralPosition(destinationIndex);
    }

    private float CalculateAngle(List<GameObject> allGuis)
    {
        int numberOfElements = allGuis.Count;
        float angleStep = 360f / numberOfElements;

         return angleStep;
       
    }

    public IEnumerator LerpGuisToAngle(float currentAngle, float angleDestination,float duration, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        float angle;

        float currentTime = 0;
      
        while (currentTime < duration)
        {
            
           _angleOffset = angle = Mathf.Lerp( currentAngle, angleDestination, currentTime / duration); 
            currentTime+=Time.deltaTime;
            yield return null;
        }
        _angleOffset = angleDestination;
        _cachedAngleOffset = _angleOffset;
       
    }


    public void NextGuiToCenter() 
    {
        if (!_nextGui) return;
       // MoveGuiToCentralPosition(_indexCurrentGui + 1);
        MoveGuiToCentralPosition(_nextGui);
    }

    public void PrevGuiToCenter()
    {
        if (!_previousGui) return;
        //MoveGuiToCentralPosition(_indexCurrentGui -1);
        MoveGuiToCentralPosition(_previousGui);
    }

    public void MoveClickedGuiToCenter(GameObject gui) 
    {
        if(!gui) return;
       
        if(!_guis.Contains(gui)) return;
        
        if(gui == CurrentGui) return;

        MoveGuiToCentralPosition(gui);
    }

    public void DestroyWristGui(GameObject gui)
    {
        if(!_guis.Contains(gui)) return ;
        _guis.Remove(gui);
        Destroy(gui);
        CurrentGui = null;
    }


    public void DestroyCurrentGUI() 
    {
        if (!CurrentGui || Guis.Count == 0)
        {
            Debug.Log("[WristScrollGUI] No gui to close");
            return;
        }

       
      
        
        if(CurrentGui == _fallBackGui) 
        {
            Debug.Log("[WristScrollGUI] wont close : is fall back gui ");
            return;
        }
        
        if (!GetGuiScript(CurrentGui).isClosable)
        {
            Debug.Log("[WristScrollGUI] Gui not Closable");
            return;
        }

       

        DestroyWristGui(CurrentGui);
        CurrentGui = null;
        
        if (Guis.Count == 1)
        {
           
           

            UpdateGuisPosition(_angleOffset = 0);
            SetMaxGuiAlpha(CurrentGui, 1, false);
        }

     

    }

    void Awake()
    {
        Guis = GetGuisInTransform();
    }


    private void Start()
    {
        
    }

    private void OnEnable()
    {
        UpdateGuisPosition(_angleOffset);
        HideAllExcept(Guis, GetGuisToShow());
    }

    private void OnDisable()
    {
        HideAll();
    }

    

    void Update()
    {  
        if (useIndex && indexToFront != _indexCurrentGui) MoveGuiToCentralPosition(indexToFront);
          
        if ( _angleOffset != _cachedAngleOffset) UpdateGuisPosition(_angleOffset); //TODO: move to a coroutine ? 
    }

    
}
