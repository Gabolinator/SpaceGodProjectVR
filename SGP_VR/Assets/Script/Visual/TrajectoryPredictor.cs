using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TrajectoryPoint
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Time;

    public TrajectoryPoint(Vector3 position, float time, Quaternion rotation)
    {
        Position = position;
        Time = time;
        Rotation = rotation;
     }
}

public class TrajectoryPredictor
{
    private List<TrajectoryPoint> predictedTrajectoryPoints = new List<TrajectoryPoint>();
    private List<TrajectoryPoint> passedTrajectoryPoints = new List<TrajectoryPoint>();
    private Rigidbody rigidbody;
    private AstralBodyHandler thisBody;
    private TrajectoryDrawer trajectoryDrawer;
    public float lastTime;
    Vector3 currentPosition ;
    Vector3 currentVelocity ;
    Vector3 currentAcceleration;
    private Quaternion currentRotation;
    private float currentTime;
    private bool showTrajectory => trajectoryDrawer.thisVisualHandler.showTrajectory;

    public TrajectoryPredictor(Rigidbody rb, AstralBodyHandler body, TrajectoryDrawer drawer )
    {
        rigidbody = rb;
        thisBody = body;
        trajectoryDrawer = drawer;
    }

    public IEnumerator PredictTrajectoryCoroutine(float delay, float duration, float timestep, AstralBodyHandler body) 
    {
        do 
        {
   
            if(showTrajectory) PredictTrajectory(duration, timestep, body);
            yield return new WaitForSeconds(delay);
        }
        while (true);
    }


    public void PredictTrajectory(float duration, float timestep, AstralBodyHandler body)
    {

        ClearTrajectory(predictedTrajectoryPoints);
        float scaleOffset = body.transform.localScale.x;
        float distance = 0;
        currentVelocity = rigidbody.velocity;
        currentPosition = rigidbody.position + currentVelocity.normalized * scaleOffset;
        var maxNumberOfPoints = 50;
        var maxDistance = 20f;
        var useMaxDistance = false;

        if (VisualIndicatorManager.Instance !=null ) 
        {
            maxNumberOfPoints = VisualIndicatorManager.Instance.maxNumberOfPoints;
            maxDistance = VisualIndicatorManager.Instance.maxLineDistance;
            useMaxDistance = VisualIndicatorManager.Instance.useMaxDistance;
        }
       

        currentRotation = Quaternion.identity;
       

        currentTime = Time.time;
        lastTime = currentTime;

        
        currentAcceleration = GetAcceleration(body);

        var lastPosition = currentPosition;

        AddTrajectoryPoint(predictedTrajectoryPoints, new TrajectoryPoint(currentPosition, currentTime, currentRotation));

        if(timestep ==0) timestep = Time.fixedDeltaTime;

        for (float t = 0; t < duration; t += timestep)

        {
            lastPosition = currentPosition;

            float predictionTime = lastTime + t;

            currentPosition += currentVelocity * t + currentVelocity.normalized * scaleOffset;

            distance += Vector3.Distance(currentPosition,lastPosition);

            if (body.PredictCollisionAtPosition(currentPosition)) break; 
            
            AddTrajectoryPoint(predictedTrajectoryPoints, new TrajectoryPoint(currentPosition, predictionTime, currentRotation));

            currentAcceleration = GetAcceleration(body, currentPosition, timestep);

            //Debug.Log("[Trajectory Predictor] Predicted acceletation at : " + currentPosition + " is :" + currentAcceleration);

            currentVelocity += currentAcceleration * timestep;
            
            //Debug.Log("[Trajectory Predictor] Predicted velocity at : " + currentPosition + " is :" + currentVelocity);

            lastTime = predictionTime;
           
            if(predictedTrajectoryPoints.Count >= maxNumberOfPoints || (useMaxDistance && distance > maxDistance)) break;

        }
    }

  

    private Vector3 GetAcceleration(AstralBodyHandler body) => FormulaLibrairy.GetAcceleration(body);
 

    private Vector3 GetAcceleration(AstralBodyHandler body, Vector3 atPosition, float timeStep) => FormulaLibrairy.GetAcceleration(body, atPosition, timeStep);
  

    public void AddTrajectoryPoint(List<TrajectoryPoint> list, TrajectoryPoint point) 
    {
        if (list.Contains(point)) return;
        //Debug.Log("[Trajectory Predictor] Adding trajectory point ");
        list.Add(point);
    }


    public IEnumerator AddTrajectoryPointCoroutine(float delay,  List<TrajectoryPoint> list, TrajectoryPoint point) 
    {
        do 
        {
           yield return new WaitForEndOfFrame();
            AddTrajectoryPoint(list, point);

        } while (true);
    }


    public IEnumerator AddTrajectoryPointCoroutine(float delay, List<TrajectoryPoint> list)
    {
        do
        {
            int maxNumberInList = VisualIndicatorManager.Instance.maxNumberOfPoints;

            var point = new TrajectoryPoint(rigidbody.transform.position, Time.time, rigidbody.rotation);
            if(list.Count >= maxNumberInList ) 
                if(list[0] != null) list.RemoveAt(0);
            AddTrajectoryPoint(list, point);
            
            yield return new WaitForSeconds(delay);
           

        } while (true);
    }

    public List<TrajectoryPoint> GetPredictedTrajectoryPoints()
    {
        return predictedTrajectoryPoints;
    }

    public List<TrajectoryPoint> GetPassedTrajectoryPoints()
    {
        return passedTrajectoryPoints;
    }

    public List<TrajectoryPoint> GetAllTrajectoryPoints()
    {
        var list = new List<TrajectoryPoint>();
        list.AddRange(predictedTrajectoryPoints);
        list.AddRange(passedTrajectoryPoints);
        
        return list;
    }

    public void ClearTrajectory(List<TrajectoryPoint> list)
    {
        list.Clear();
    }
}
