using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct TrajectoryPredictionJob : IJobParallelFor
{
    /*all bodies to predict on*/
    [ReadOnly] public NativeArray<Vector3> currentPositions;
    [ReadOnly] public NativeArray<Vector3> currentVelocities;
    [ReadOnly]public NativeArray<Vector3> currentForces;
    [ReadOnly] public NativeArray<double> masses;
    
    [ReadOnly] public NativeArray<float> influenceStrength;
    public float directGravityMultiplier;
    public double gConstant;
    
    public float currentTime;
    
    public float duration; //duration of prediction
    public float timeStep; //we look values every timeStep sec
    
    /*for the body we predict for*/
    public List<NativeArray<Vector3>> predictedPositions;
    public List<NativeArray<Vector3>> predictedVelocities;
    public List<NativeArray<Vector3>> predictedForces;
    public List<NativeArray<Vector3>> predictedTimes; 
    public List<List<TrajectoryPoint>> predictedTrajectoryPoints;
    

    public void Execute(int index)
    {
        
        
   
        
        
        
    }
}
