using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

    [BoxGroup("Trajectory")] public bool showTrajectory = true;
    [SerializeField][BoxGroup("Trajectory")] public bool showTrail = true;
    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("@this.showTrajectory || this.showTrail")] protected GameObject _trajectoryPrefab;
    public GameObject TrajectoryPrefab => _trajectoryPrefab;

    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("showTrajectory")] protected Color _trajectoryColor;
    public Color TrajectoryColor => _trajectoryColor;

    [SerializeField] [BoxGroup("Trajectory")] [ShowIf("showTrail")] protected Color _trailColor;
    public Color TrailColor => _trailColor;



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

    private void AddIndicatorHandler(List<AstralBodyHandler> allAstralBodies)
    {
        if (allAstralBodies.Count == 0) return;

        foreach (AstralBodyHandler body in allAstralBodies)
        {
            AddIndicator(body);
        }
    }

    public void AddIndicator(AstralBodyHandler body) 
    {
        if(!body ) return;

        VisualIndicatorHandler indicatorHandler = body.GetComponent<VisualIndicatorHandler>();
        if (!indicatorHandler) indicatorHandler = body.gameObject.AddComponent<VisualIndicatorHandler>();
        if (showDebugLog) Debug.Log("[Visual Indicator Manager] adding indicator to : " + body);
        indicatorHandler._indicatorManager = this;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        

        StartCoroutine(AddIndicatorHandlerCoroutine(_refreshRate));


    }

    private void LateUpdate()
    {

    }

    private void OnEnable()
    {
       
    }

    private void OnDisable()
    {
        
    }
}

