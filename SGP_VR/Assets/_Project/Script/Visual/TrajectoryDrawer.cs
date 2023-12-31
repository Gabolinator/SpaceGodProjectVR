using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;


public class TrajectoryDrawer : LineDrawer
{
    public Rigidbody rb;
    public float delay;
    public LineRenderer trajectoryLineRenderer => lineRenderer;
    public LineRenderer trailLineRenderer;
    public VisualIndicatorHandler thisVisualHandler;

    public GameObject trailGO;
    public GameObject trajectoryGO;
    //public LineRenderer projectedTrajectoryLineRenderer;
    public AstralBodyHandler astralBody;

    public SplineContainer splineContainer; 

    public TrajectoryPredictor trajectoryPredictor;
    public float duration = .5f;
    public float trailRefreshRate = .1f;
    
    public List<TrajectoryPoint> debugList = new();
    public bool ShowTrajectory=> thisVisualHandler.ShouldShowTrajectory;
    public bool ShowTrail => thisVisualHandler.ShouldShowTrail;

    private void DrawTrajectory(LineRenderer renderer , List<TrajectoryPoint> trajectoryPoints)
    {
        if (trajectoryPoints == null) return;
        
        if (trajectoryPoints.Count == 0 || trajectoryLineRenderer == null) return;


        Vector3[] positions = new Vector3[trajectoryPoints.Count];
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            positions[i] = trajectoryPoints[i].Position;
        }

        renderer.positionCount = trajectoryPoints.Count;
        renderer.SetPositions(positions);
        //trajectoryLineRenderer.SetPosition(0, rb.position);
    }


    public void DrawTrajectory()
    {
        if (ShowTrajectory)
            DrawTrajectory(trajectoryLineRenderer, debugList = trajectoryPredictor.GetPredictedTrajectoryPoints());
        
        ToggleRenderer(trajectoryLineRenderer, ShowTrajectory);
    }


    public void DrawTrail()
    {
        if (ShowTrail)  DrawTrajectory(trailLineRenderer, trajectoryPredictor.GetPassedTrajectoryPoints());
        
        ToggleRenderer(trailLineRenderer, ShowTrajectory);
    }

    public void UpdateSpline(List<TrajectoryPoint> trajectoryPoints, bool clearData = true)
    {
        if (splineContainer == null) return;


        if (splineContainer.transform.parent != null) splineContainer.transform.parent = null;
        if (splineContainer.transform.position != Vector3.zero) splineContainer.transform.position = Vector3.zero;

        if (trajectoryPoints.Count == 0) return;

        if (clearData)
        {
            Spline spline = new Spline();
            splineContainer.RemoveSplineAt(0);

            foreach (var point in trajectoryPoints)
            {
                BezierKnot knot = new BezierKnot();
                knot.Position = point.Position;
                knot.Rotation = point.Rotation;
                spline.Add(knot);
            }

            splineContainer.AddSpline(spline);
        }

        else
        {

            if (splineContainer.Splines.Count == 0) splineContainer.AddSpline(new Spline());

           // splineContainer.Splines[0];
        }
    }


    public void ResetTrailPoint() 
    {
        ResetTrajectoryPoints(trailLineRenderer, trajectoryPredictor.GetPassedTrajectoryPoints());
    }

    public void ResetTrajectoryPoints(LineRenderer lineRenderer , List<TrajectoryPoint> trajectoryPoints)
    {
        lineRenderer.positionCount = 0; 
        trajectoryPoints.Clear();
    }

    public override void Start()
    {
        base.Start();

        if (!rb) rb = GetComponentInParent<Rigidbody>();
        if (!astralBody) astralBody = GetComponentInParent<AstralBodyHandler>();

        trajectoryPredictor = new TrajectoryPredictor(rb, astralBody, this);
       
        thisVisualHandler._indicatorManager.RegisterTrajectoryDrawer(this);
        
        if (astralBody && trajectoryPredictor != null)
        {
            /*populate trajectory predictiion points*/
         //   StartCoroutine(trajectoryPredictor.PredictTrajectoryCoroutine(delay, 5, .2f, astralBody)); //moved to visualisation manager 

            /*populate trail points*/
            StartCoroutine(trajectoryPredictor.AddTrajectoryPointCoroutine(trailRefreshRate, trajectoryPredictor.GetPassedTrajectoryPoints())); // TODO move to manager
        }
    }

    private void Update()
    {
        
        //TODO move to visualisation manager to reduce update calls 
        
        
        /*draw projected trajectory*/
        //if(showTrajectory) DrawTrajectory(trajectoryLineRenderer, debugList = trajectoryPredictor.GetPredictedTrajectoryPoints());

        //UpdateSpline(trajectoryPredictor.GetPredictedTrajectoryPoints());

        /*draw trail position */
        //if (showTrail)  DrawTrajectory(trailLineRenderer, trajectoryPredictor.GetPassedTrajectoryPoints());
        //UpdateSpline(trajectoryPredictor.GetPassedTrajectoryPoints());
    }

    public void OnDestroy()
    {
        thisVisualHandler._indicatorManager.UnRegisterTrajectoryDrawer(this);
    }
}
