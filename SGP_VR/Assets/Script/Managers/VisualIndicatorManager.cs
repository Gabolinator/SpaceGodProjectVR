using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Object = System.Object;

public class VisualIndicatorManager : MonoBehaviour
{
    static VisualIndicatorManager _instance;
    public static VisualIndicatorManager Instance => _instance;

    public float _refreshRate = 1f;

    [Header("Visualization Parameters")]
 
    [SerializeField] [BoxGroup("Parameter")] protected GameObject _linePrefab;
    public GameObject LinePrefab => _linePrefab;
    [BoxGroup("Parameter")] public int maxNumberOfPoints = 50;
    
    [BoxGroup("Parameter")] public bool useMaxDistance = false;
    [BoxGroup("Parameter")] [ShowIf("useMaxDistance")] public float maxLineDistance = 20;

    [SerializeField] [BoxGroup("Total Pull")] protected bool _showTotalPullLine;
    public bool ShowTotalPullLine => _showTotalPullLine;

  

    [SerializeField] [BoxGroup("Total Pull")] [ShowIf("_showTotalPullLine")] protected Color _totalPullColor;
    [BoxGroup("Total Pull")] public Color TotalPullColor => _totalPullColor;

   
    [BoxGroup("Individual forces")] [SerializeField] protected bool _showIndividualPullLine;
    public bool ShowIndividualPullLine => _showIndividualPullLine;
    [SerializeField] [ShowIf("_showIndividualPullLine")] protected Color _indivitualPullColor;
    [BoxGroup("Individual forces")] public Color IndividualPullColor => _indivitualPullColor;

    [SerializeField] [BoxGroup("Individual forces")] [ShowIf("_showIndividualPullLine")] protected int _maxNumberOfLines;
    public int MaxNumberOfLines => _maxNumberOfLines;
    
    [SerializeField]
    private bool usingJob;
    
    
    [BoxGroup("Trajectory")] 
    [SerializeField]private bool _showTrajectory;
    
    public bool ShowTrajectory
    {
        get {return _showTrajectory; }
        set
        {
            OnShowTrajectoryChanged(value);
            _showTrajectory = value;
           
        }
    }

    private void OnShowTrajectoryChanged(bool state)
    {

       // Debug.Log("On show trajectory changed: " + state);
        if (state) ShowAllTrajectory(allVisHandlers);
        else HideAllTrajectory(allVisHandlers);
    }
    
    [SerializeField][BoxGroup("Trajectory")] private bool _showTrail;

    public bool ShowTrail
    {
        get { return _showTrail; }

        set { 
            OnShowTrailChanged(value); 
            _showTrajectory = value;
            
        }
    }

    private void OnShowTrailChanged(bool state)
    {
       // Debug.Log("On show trail changed: " + state);
        if (state) ShowAllTrails(allVisHandlers);
        else HideAllTrails(allVisHandlers);
    }

   

    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("@this.ShowTrajectory || this.ShowTrail")] protected GameObject _trajectoryPrefab;
    public GameObject TrajectoryPrefab => _trajectoryPrefab;

    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("ShowTrajectory")] protected Color _trajectoryColor;

    public Color TrajectoryColor
    {
        get { return _trajectoryColor; }
        set
        {
            _trajectoryColor = value;
            OnTrajectoryColorChanged(value);
        }
    }
    

    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("ShowTrail")] protected Color _trailColor;
    public Color TrailColor {
        get { return _trailColor; }
        set
        {
            _trailColor = value;
            OnTrailColorChanged(value);
        }
    }

    public List<VisualIndicatorHandler> allVisHandlers = new List<VisualIndicatorHandler>();
    public List<TrajectoryDrawer> allTrajectoryDrawers = new List<TrajectoryDrawer>();
    //public List<TrajectoryDrawer>
    
    [Header("Debug")]
    public List<AstralBodyHandler> allAstralBodies = new List<AstralBodyHandler>();
    public bool showDebugLog = true;

    

    private IEnumerator AddIndicatorHandlerCoroutine(float delay)
    {
        do 
        {
            if (showDebugLog) Debug.Log("[Visual Indicator Manager] Checking to add indicator");
            yield return new WaitForSeconds(delay);
            if(AstralBodiesManager.Instance) AddIndicatorHandler(allAstralBodies = AstralBodiesManager.Instance._allBodies);

        } while (true);

    }
    

    private void DestroyAllTrajectoryDrawers(List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        if(visualIndicatorHandlers.Count == 0) return;
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.DestroyTrajectoryDrawer();
        }
      
    }

   

    
    
    private void HideAllTrajectory(List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        if(visualIndicatorHandlers.Count == 0) return;
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.HideTrajectory();
        }
    }

    private void ShowAllTrajectory(List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        if(visualIndicatorHandlers.Count == 0) return;
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.ShowTrajectory();
        }
    }
    
    private void HideAllTrails(List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        if(visualIndicatorHandlers.Count == 0) return;
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.HideTrail();
        }
    }

    private void ShowAllTrails(List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        if(visualIndicatorHandlers.Count == 0) return;
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.ShowTrail();
        }
    }
    
    private void AddIndicatorHandler(List<AstralBodyHandler> allAstralBodies)
    {
        if (allAstralBodies.Count == 0) return;

        foreach (AstralBodyHandler body in allAstralBodies)
        {
            AddIndicator(body);
        }
    }

    public void OnTrailColorChanged(Color color)
    {
        
        if(!ShowTrail) return;
        UpdateTrailsColor(color, allVisHandlers);
    }

    public void OnTrajectoryColorChanged(Color color)
    {
        if(!ShowTrajectory) return;
        UpdateTrajectoriesColor(color, allVisHandlers);
    }
    public void UpdateTrailsColor(Color color, List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        Debug.Log("Update trails color");
        if(visualIndicatorHandlers.Count == 0) return;
        
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.UpdateTrailColor(color);
        }
    }
    
    public void UpdateTrajectoriesColor(Color color, List<VisualIndicatorHandler> visualIndicatorHandlers)
    {
        Debug.Log("Update trajectories color");
        if(visualIndicatorHandlers.Count == 0 ) return;
        
        foreach (var visualIndicatorHandler in visualIndicatorHandlers)
        {
            visualIndicatorHandler.UpdateTrajectoryColor(color);
        }
    }

    public void AddIndicator(AstralBodyHandler body) 
    {
        if(!body ) return;

        VisualIndicatorHandler indicatorHandler = body.GetComponent<VisualIndicatorHandler>();
        if (!indicatorHandler) indicatorHandler = body.gameObject.AddComponent<VisualIndicatorHandler>();
        if (showDebugLog) Debug.Log("[Visual Indicator Manager] adding indicator to : " + body);
        indicatorHandler._indicatorManager = this;
        RegisterIndicatorHandler(indicatorHandler);
    }

    public void RegisterIndicatorHandler(VisualIndicatorHandler visualIndicatorHandler)
    {
        if(!visualIndicatorHandler) return;
        if (allVisHandlers.Count != 0)
        {
            if(allVisHandlers.Contains(visualIndicatorHandler)) return;
        }

        allVisHandlers.Add(visualIndicatorHandler);
        
    }

    public void RegisterTrajectoryDrawer(VisualIndicatorHandler visualIndicatorHandler)
    {
        if(!visualIndicatorHandler) return;

        RegisterTrajectoryDrawer(visualIndicatorHandler._trajectoryDrawer);
        
    }

    public void RegisterTrajectoryDrawer(TrajectoryDrawer trajectoryDrawer)
    {
        if(!trajectoryDrawer) return;
        if (allTrajectoryDrawers.Count != 0)
        {
            if(allTrajectoryDrawers.Contains(trajectoryDrawer)) return;
        }

        allTrajectoryDrawers.Add(trajectoryDrawer);

    }

    public void UnRegisterTrajectoryDrawer(TrajectoryDrawer trajectoryDrawer)
    {
        if(!trajectoryDrawer) return;
        if (allTrajectoryDrawers.Count == 0) return;
        if(!allTrajectoryDrawers.Contains(trajectoryDrawer)) return;
        

        allTrajectoryDrawers.Remove(trajectoryDrawer);
        
    }
    
    public void UnRegisterTrajectoryDrawer(VisualIndicatorHandler visualIndicatorHandler)
    {
        if(!visualIndicatorHandler) return;

        UnRegisterTrajectoryDrawer(visualIndicatorHandler._trajectoryDrawer);
    }
    
    public void UnRegisterIndicatorHandler(VisualIndicatorHandler visualIndicatorHandler)
    {
        if(!visualIndicatorHandler) return;
        if (allVisHandlers.Count == 0) return; 
        if( !allVisHandlers.Contains(visualIndicatorHandler)) return;
        
        allVisHandlers.Remove(visualIndicatorHandler);
    }

    public void UnRegisterIndicatorHandler(AstralBodyHandler body)
    {
        var visIndicator = body.GetComponent<VisualIndicatorHandler>();
        UnRegisterIndicatorHandler(visIndicator);
    }

    private void AddTrajectoryDrawer(SelectExitEventArgs obj)
    {
        if (obj == null) return;
        var visHandler = obj.interactableObject.transform.GetComponent<VisualIndicatorHandler>();
        if(!visHandler) return;

        visHandler.HandleTrajectory();
    }

    
    private void DestroyTrajectoryDrawer(SelectEnterEventArgs obj)
    {
        if (obj == null) return;
        var visHandler = obj.interactableObject.transform.GetComponent<VisualIndicatorHandler>();
        if(!visHandler) return;
        
        visHandler.DestroyTrajectoryDrawer(obj);
        
    }

    private void DestroyTrajectoryDrawer(bool state, AstralBodyHandler body) 
    {
        var visHandler = body.GetComponent<VisualIndicatorHandler>();
        if(!visHandler) return;

        visHandler.DestroyTrajectoryDrawer(state, body);
    }
    
    
    private void ManageTrajectories(List<TrajectoryDrawer> trajectoryDrawers)
    {
        if(trajectoryDrawers.Count == 0) return;
        foreach (var trajectoryDrawer in trajectoryDrawers)
        {
            ManageTrajectory(trajectoryDrawer);
        }
      
       
    }

    private void ManageTrajectory(TrajectoryDrawer trajectoryDrawer)
    {
        if(!trajectoryDrawer) return;
       
       if(ShowTrajectory) trajectoryDrawer.DrawTrajectory();
       if(ShowTrail) trajectoryDrawer.DrawTrail();
       
    }
    
    private void PredictTrajectories(List<TrajectoryDrawer> trajectoryDrawers)
    {
        if(trajectoryDrawers.Count == 0) return;
        foreach (var trajectoryDrawer in trajectoryDrawers)
        {
            if (trajectoryDrawer)
                trajectoryDrawer.trajectoryPredictor.PredictTrajectory(5, .2f, trajectoryDrawer.astralBody);
        }
    }

    private void PredictTrajectoriesJob(List<TrajectoryDrawer> trajectoryDrawers)
    {
        if(trajectoryDrawers.Count == 0) return;
        
        //Debug.Log("PredictTrajectory job");
        int numberOfDrawers = trajectoryDrawers.Count;
        
        /*Set duration of prediction (sec) - every timestep we re-calculate - we calculate until the number of points reaches maxValues or duration is reached*/
        float duration = 10;
        float timeStep = 0.2f;
        int maxValues = (int)(duration / timeStep);
      //  Debug.Log("PredictTrajectory job max value : " +maxValues +"/max number of points :" +  maxNumberOfPoints );
        maxValues = maxNumberOfPoints < maxValues ? maxNumberOfPoints : maxValues;
        
        /* Initialize NativeArray to be used by job*/
        NativeArray<Vector3> currentPositionsArray = new NativeArray<Vector3>(numberOfDrawers, Allocator.TempJob);
        NativeArray<Vector3> currentVelocitiesArray = new NativeArray<Vector3>(numberOfDrawers, Allocator.TempJob);
        NativeArray<Vector3> currentForcesArray = new NativeArray<Vector3>(numberOfDrawers, Allocator.TempJob);
        NativeArray<float> massesArray = new NativeArray<float>(numberOfDrawers, Allocator.TempJob);
        NativeArray<float> influencesStrengths = new NativeArray<float>(numberOfDrawers, Allocator.TempJob);
        
        
        /* Initialize NativeArrays for predicted positions, velocities, forces, and times- all predicted values per categories will be in one array*/
        NativeArray<Vector3> predictedPositionsArrays = new NativeArray<Vector3>((maxValues*numberOfDrawers), Allocator.TempJob); 
        NativeArray<float> distancesArray = new NativeArray<float>((maxValues*numberOfDrawers), Allocator.TempJob); 
        NativeArray<float> predictedTimesArrays = new NativeArray<float>(maxValues, Allocator.TempJob); //same array for all bodies
        
        /*For each body set values in array */
        for (int i =0; i <numberOfDrawers; i++)
        {
            var body = trajectoryDrawers[i].astralBody;
            if(!body) continue;

           // Debug.Log("PredictTrajectory job : " + body );
            
            /*Initialize values for each body*/
            currentPositionsArray[i] = body.transform.position;
            currentVelocitiesArray[i] = body.Velocity;
            currentForcesArray[i] = body.totalForceOnObject;
            massesArray[i] = (float)body.Mass;
            influencesStrengths[i] = body.InfluenceStrength;

        }
        
        /* Create the trajectory prediction job - one job for all bodies - initialize its variables */
        TrajectoryPredictionJob trajectoryJob = new TrajectoryPredictionJob
        {
            directGravityMultiplier = UniverseManager.Instance.PhysicsProperties.DirectGravityPullMultiplier,
            gConstant = UniverseManager.Instance.PhysicsProperties.GravitationnalConstant* UniverseManager.Instance.PhysicsProperties.GravitationnalConstantFactor,
            
            currentPositions = currentPositionsArray,
            currentVelocities = currentVelocitiesArray,
            currentForces = currentForcesArray,
            masses = massesArray,
            influences = influencesStrengths,
            
            predictedPositions = predictedPositionsArrays,
            predictedTimes = predictedTimesArrays,
            distanceOfLine = distancesArray,
            currentTime = Time.time,
            
            duration = duration,
            timeStep = timeStep,
            maxValues = maxValues
        };

      // Debug.Log("PredictTrajectory job handle predictedPositions.lenght : " +  trajectoryJob.predictedPositions.Length);
      //  Debug.Log("PredictTrajectory job handle predictedTimes.lenght : " +  trajectoryJob.predictedTimes.Length);
        
        /*Schedule the job */
        JobHandle jobHandle = trajectoryJob.Schedule(numberOfDrawers, 64); // Adjust the batch size (64 is an example) 

        List<NativeArray<float>> floatArrays = new List<NativeArray<float>>();
        List<NativeArray<Vector3>> v3Arrays = new List<NativeArray<Vector3>>();
        
        
        floatArrays.Add(massesArray);
        floatArrays.Add(influencesStrengths);
        floatArrays.Add(predictedTimesArrays);
        v3Arrays.Add(currentPositionsArray);
        v3Arrays.Add(currentVelocitiesArray);
        v3Arrays.Add(currentForcesArray);
        v3Arrays.Add(predictedPositionsArrays);

       StartCoroutine(WaitThenCompletePredictionJob(.1f, jobHandle, trajectoryDrawers, trajectoryJob, floatArrays, v3Arrays));
    }

    private IEnumerator WaitThenCompletePredictionJob(float delay, JobHandle jobHandle, List<TrajectoryDrawer> trajectoryDrawers, TrajectoryPredictionJob trajectoryJob, List<NativeArray<float>> floatArrays,  List<NativeArray<Vector3>> v3Arrays)
    {

        yield return new WaitForSeconds(delay);
       
        jobHandle.Complete();

       if (trajectoryDrawers.Count == 0) yield return null;
        
        for (int i = 0; i < trajectoryDrawers.Count; i++)
        {
            var drawer = trajectoryDrawers[i];
            var body = drawer.astralBody;
          
            List<TrajectoryPoint> trajectoryPoints = new List<TrajectoryPoint>();

            NativeArray<Vector3> predictedPositions = new NativeArray<Vector3>(trajectoryJob.predictedTimes.Length, Allocator.TempJob);
            predictedPositions =trajectoryJob.GetPredictedPositionsArray(i);
            NativeArray<float> distances = new NativeArray<float>(trajectoryJob.predictedTimes.Length, Allocator.TempJob);
            distances =trajectoryJob.GetDistanceArray(i);
            NativeArray<float> predictedTimes = new NativeArray<float>(trajectoryJob.predictedTimes.Length, Allocator.TempJob);
           
            predictedTimes = trajectoryJob.predictedTimes;

           
            float  distance = 0;
            
            for (int j = 0; j < predictedTimes.Length; j++)
            {
                var trajectoryPoint = new TrajectoryPoint( j == 0 ? body.transform.position : predictedPositions[j], predictedTimes[j], Quaternion.identity );
                distance = distances[j];
                
                if (useMaxDistance && distance > maxLineDistance) break;
                
                trajectoryPoints.Add(trajectoryPoint);
            }

           
            if (distance > body.Radius)
            {
                drawer.trajectoryPredictor.SetPredictedTrajectoryPoints(trajectoryPoints);
                drawer.thisVisualHandler.predictedPoints = trajectoryPoints;
            }


            else
            {
                Debug.Log("Line not visible dont draw");
                trajectoryPoints.Clear();
                drawer.trajectoryPredictor.SetPredictedTrajectoryPoints(trajectoryPoints);
            }


        }
        
        /*Dispose of all natives arrays*/
        if(floatArrays.Count !=0)
            foreach (var floatArray in floatArrays)
            {
                floatArray.Dispose();
            }
        if(v3Arrays.Count !=0)
            foreach (var v3Array in v3Arrays)
            {
                v3Array.Dispose();
            }

       
    }

    private void PrintArray(NativeArray<Vector3> array)
    {
        int i = 0;
        Debug.Log("number of Element :" + array.Length  );
        foreach (var element in array)
        {
           Debug.Log("Element at index :" + i+ "is :" + element  );
            i++;
        }
    }


    private void PredictTrajectories(bool useJob = false)
    {
        if(!ShowTrajectory) return;
        if(useJob) PredictTrajectoriesJob(allTrajectoryDrawers);
        else PredictTrajectories(allTrajectoryDrawers);
       

    }

    private void UpdateTrajectories(List<TrajectoryDrawer> trajectoryDrawers)
    {
        //todo remove - its just to test 
        if(trajectoryDrawers.Count ==0) return;
        foreach (var drawer in trajectoryDrawers)
        {
            drawer.thisVisualHandler.predictedPoints = drawer.trajectoryPredictor.GetPredictedTrajectoryPoints();
        }
      
    }


#if UNITY_EDITOR
   
    private void OnValidate()
    {
        OnShowTrajectoryChanged(_showTrajectory);
        OnShowTrailChanged(_showTrail);
        OnTrailColorChanged(_trailColor);
        OnTrajectoryColorChanged(_trajectoryColor);
    }
#endif
    
    private void Awake()
    {
        _instance = this;
    }

 

    private void Start()
    {
        

    //    StartCoroutine(AddIndicatorHandlerCoroutine(_refreshRate));


    }

    private void Update()
    {
        PredictTrajectories(usingJob);
      
        
    }
    

    private void LateUpdate()
    {
        ManageTrajectories(allTrajectoryDrawers);
    }

  

    private void OnEnable()
    {
        EventBus.OnAstralBodyStartToExist += AddIndicator;
        EventBus.OnAstralBodyDestroyed += UnRegisterIndicatorHandler;
        EventBus.OnObjectGrabbed += DestroyTrajectoryDrawer;
        EventBus.OnBodyEdit += DestroyTrajectoryDrawer;
        EventBus.OnObjectReleased += AddTrajectoryDrawer; 
    }


    private void OnDisable()
    {
        EventBus.OnAstralBodyStartToExist -= AddIndicator;
        EventBus.OnAstralBodyDestroyed -= UnRegisterIndicatorHandler;
        EventBus.OnObjectGrabbed -= DestroyTrajectoryDrawer;
        EventBus.OnBodyEdit -= DestroyTrajectoryDrawer;
        
    }

  
}

