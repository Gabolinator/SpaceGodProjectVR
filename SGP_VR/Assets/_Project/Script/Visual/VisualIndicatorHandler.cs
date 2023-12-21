using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VisualIndicatorHandler : MonoBehaviour
{
    public VisualIndicatorManager _indicatorManager; 

    public bool ShowTotaPullLine => _indicatorManager.ShowTotalPullLine && !_thisBody.isGrabbed;
    
    public bool ShowIndividualLine => _indicatorManager.ShowIndividualPullLine && !_thisBody.isGrabbed;

    public GameObject linePrefab;

    public List<VectorLine> _objectPullLines = new ();
    public VectorLine _totalPullLine;
    
    public GameObject trajectoryPrefab;
    public GameObject trailPrefab;

    public TrajectoryDrawer _trajectoryDrawer;
    public Color _trajectoryColor;
    public Color _trailColor;
    public float trajectoryRefresh => _indicatorManager._refreshRate;

    public bool ShouldShowTrajectory => _indicatorManager.ShowTrajectory && !_thisBody.isGrabbed && !forceDisableTrajectory &&  _thisBody.IsInView && _thisBody.IsWithinDistance;
    public float _duration = 1;
    public bool forceDisableTrajectory;
    
    public bool ShouldShowTrail => _indicatorManager.ShowTrail && !_thisBody.isGrabbed && !forceDisableTrail && _thisBody.IsInView  && _thisBody.IsWithinDistance;
    public bool forceDisableTrail;
   
    public AstralBodyHandler _thisBody;
    public Vector3 _totalPull = Vector3.zero;
    public List<TrajectoryPoint> predictedPoints ; 


    private void Start()
    {
        //_indicatorManager =  VisualIndicatorManager.Instance;
        _thisBody = GetComponent<AstralBodyHandler> ();
        
        _totalPull = _thisBody.totalForceOnObject != null ? _thisBody.totalForceOnObject : Vector3.one;
        
        linePrefab = _indicatorManager.LinePrefab;

        trajectoryPrefab = _indicatorManager.TrajectoryPrefab;
        _trajectoryColor = _indicatorManager.TrajectoryColor;
        _trailColor = _indicatorManager.TrailColor;


        HandleTrajectory();
        HandlePullLines();
        
    }

    private IEnumerator HandleIndicators(float delay)
    {
        do 
        {
            
            HandleTrajectory();
            HandlePullLines();
        
            yield return new WaitForSeconds(delay);



        } while (true);
    }

    private void HandlePullLines()
    {
        if (ShowTotaPullLine)
        {

            if (_totalPullLine == null)
            {
                _totalPullLine = CreateVectorLine(new Vector3(1, 1, 1));
                _totalPullLine.UpdateLineColor(_indicatorManager.TotalPullColor);
            }
        }

        else
        {
            if (_totalPullLine != null) Destroy(_totalPullLine.gameObject);
        }
    }

    public void HandleTrajectory()
    {
        if (ShouldShowTrajectory)
        {
            if (_trajectoryDrawer == null)
            {
                _trajectoryDrawer = CreateTrajectoryDrawer();
            }

            _trajectoryDrawer.duration = _duration;
            if (_trajectoryDrawer != null) _trajectoryDrawer.trajectoryLineRenderer.enabled = true;
            _trajectoryDrawer.UpdateLineColor(_trajectoryColor);
        }

        else
        {
            if (_trajectoryDrawer != null) _trajectoryDrawer.trajectoryLineRenderer.enabled = false;
        }

        if (ShouldShowTrail)
        {
            if (_trajectoryDrawer == null)
            {
                _trajectoryDrawer = CreateTrajectoryDrawer();
            }

            _trajectoryDrawer.duration = _duration;
            if (_trajectoryDrawer != null) _trajectoryDrawer.trailLineRenderer.enabled = true;
            UpdateTrailColor(_trailColor);
        }

        else
        {
            if (_trajectoryDrawer != null) _trajectoryDrawer.trailLineRenderer.enabled = false;
           
        }

    }

    

  

    private VectorLine CreateVectorLine(Vector3 vector)
    {
      
        if (!linePrefab || vector == null) return null;

        var linePrefabClone = Instantiate(linePrefab, this.transform.position, Quaternion.identity);
        linePrefabClone.transform.parent = this.transform;
       
        var line = linePrefabClone.GetComponent<VectorLine>();
        if(!line) linePrefabClone.AddComponent<VectorLine>();

        line.UpdateLine(vector, (float)_thisBody.Radius, this.transform.position);

        return line;
    }

    public void UpdateLine()
    {
        UpdateLine(_totalPullLine, _thisBody.totalForceOnObject.normalized * (float)_thisBody.CurrentRadiusOfTrajectory);
    }

    private void UpdateLine(VectorLine line, Vector3 vector) 
    {
        if(!line) return;

        var start = this.transform.position;

        line.UpdateLine(vector, (float)_thisBody.Radius, start);
    }

    private TrajectoryDrawer CreateTrajectoryDrawer()
    {
        if (!trajectoryPrefab) return null;

        var trajectoryPrefabClone = Instantiate(trajectoryPrefab, this.transform.position, Quaternion.identity);
        trajectoryPrefabClone.transform.parent = this.transform;

        var trajectoryDrawer = trajectoryPrefabClone.GetComponent<TrajectoryDrawer>();
        if (!trajectoryDrawer) trajectoryPrefabClone.AddComponent<TrajectoryDrawer>();

        trajectoryDrawer.astralBody = _thisBody;
        trajectoryDrawer.rb = _thisBody.GetComponent<Rigidbody>();
        trajectoryDrawer.delay = trajectoryRefresh;
        trajectoryDrawer.thisVisualHandler = this;

        return trajectoryDrawer;
    }

    public void DestroyTotalPull() 
    {
        if (_totalPullLine) Destroy(_totalPullLine); 
    }

    public void DestroyTrajectory() 
    {
       
        if (_trajectoryDrawer) Destroy(_trajectoryDrawer.gameObject);
        
        _trajectoryDrawer = null; 
    }

    public void DestroyTrajectoryDrawer()
    {
        DestroyTrajectory();
    }

    public void DestroyTrajectoryDrawer(SelectEnterEventArgs obj)
    {
        if (obj == null) return;
        var body = obj.interactableObject.transform.GetComponent<AstralBodyHandler>();

        DestroyTrajectoryDrawer(true, body);
    }

    public void DestroyTrajectoryDrawer(bool state, AstralBodyHandler body) 
    {
        if(!state) return;
        if (!body) return;
        if (body.ID != _thisBody.ID) return;
        
        DestroyTrajectory();

    }
    public void DestroyIndividualPull()
    {
        //TODO : implement 
        throw new NotImplementedException();
    }

    private void ResetTrailPoints(SelectExitEventArgs obj) 
    {
        if (obj == null) return;

        var releasedBody = obj.interactableObject.transform.GetComponent<AstralBodyHandler>();
        ResetTrailPoints(releasedBody);
    }

    private void ResetTrailPoints(AstralBodyHandler body) 
    {
        if (!body) return;
        if (body.ID != _thisBody.ID) return;

        ResetTrailPoints();
    }
    private void ResetTrailPoints(SelectEnterEventArgs obj)
    {
        if(obj == null) return;
       
        var grabbedBody = obj.interactableObject.transform.GetComponent<AstralBodyHandler>();
        ResetTrailPoints(grabbedBody);
 
    }

    private void ResetTrailPoints() => _trajectoryDrawer.ResetTrailPoint();
 

    // private void OnEnable()
    // {
    //     EventBus.OnObjectGrabbed += DestroyTrajectoryDrawer;
    //     EventBus.OnBodyEdit += DestroyTrajectoryDrawer;
    //     //EventBus.OnObjectReleased += ResetTrailPoints;
    // }
    //
    //
    //
    // private void OnDisable()
    // {
    //     EventBus.OnObjectGrabbed -= DestroyTrajectoryDrawer;
    //     //EventBus.OnObjectReleased -= ResetTrailPoints;
    // }
    private void LateUpdate()
    {
      //  UpdateLine(_totalPullLine, _thisBody.totalForceOnObject.normalized * (float)_thisBody.currentRadiusOfTrajectory);

    }

    public void ShowTrajectory()
    {
        if (_trajectoryDrawer == null)
        {
            _trajectoryDrawer = CreateTrajectoryDrawer();
            UpdateTrajectoryColor(_trajectoryColor);

        }

        _trajectoryDrawer.duration = _duration;
        if (_trajectoryDrawer != null) _trajectoryDrawer.trajectoryLineRenderer.enabled = true;
    }

    public void HideTrajectory()
    {
        if (!ShouldShowTrail)
        {
            DestroyTrajectory(); 
            return;
        }
        
        if (_trajectoryDrawer != null) _trajectoryDrawer.trajectoryLineRenderer.enabled = false;
    }

    public void HideTrail()
    {
        
        if (!ShouldShowTrajectory)
        {
            DestroyTrajectory(); 
            return;
        }
        if (_trajectoryDrawer != null) _trajectoryDrawer.trailLineRenderer.enabled = false;
    }

    public void ShowTrail()
    {
        if (_trajectoryDrawer == null)
        {
            _trajectoryDrawer = CreateTrajectoryDrawer();
            UpdateTrailColor(_trailColor);

        }

        _trajectoryDrawer.duration = _duration;
        if (_trajectoryDrawer != null) _trajectoryDrawer.trailLineRenderer.enabled = true;
    }


    public void UpdateTrailColor(Color color)
    {
        if (_trajectoryDrawer == null) return;
        Debug.Log("Update trail color");
        _trajectoryDrawer.UpdateLineColor(_trajectoryDrawer.trailLineRenderer, new Color(color.r, color.g, color.b,0), new Color(color.r, color.g, color.b,color.a == 0 ? .1f: color.a) );
    }
    public void UpdateTrajectoryColor(Color color)
    {
        if (_trajectoryDrawer == null) return;
        Debug.Log("Update trajectory color");
        
        _trajectoryDrawer.UpdateLineColor(_trajectoryDrawer.trajectoryLineRenderer, new Color(color.r, color.g, color.b,0),  new Color(color.r, color.g, color.b,0) );
       // _trajectoryDrawer.UpdateLineColor(_trajectoryColor = color);
      
    }

}
