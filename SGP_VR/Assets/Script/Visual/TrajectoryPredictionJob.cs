using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct TrajectoryPredictionJob : IJobParallelFor
{
 
    [ReadOnly] public NativeArray<Vector3> currentPositions;
    [ReadOnly] public NativeArray<Vector3> currentVelocities;
    [ReadOnly] public NativeArray<Vector3> currentForces;
    [ReadOnly] public NativeArray<float> masses;
    [ReadOnly] public NativeArray<float> influences;
    [ReadOnly] public float directGravityMultiplier;
    [ReadOnly] public double gConstant;

    [ReadOnly] public float currentTime;

    [ReadOnly] public float duration;
    [ReadOnly] public float timeStep;

    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> predictedPositions; //packed array of predicted positions of ALL bodies
    [NativeDisableParallelForRestriction]  
    public NativeArray<float> distanceOfLine;  //packed array of distance from Last predicted position from first - for ALL bodies
    [NativeDisableParallelForRestriction]  
    public NativeArray<float> predictedTimes;
    [ReadOnly] public int maxValues;
    
    public void Execute(int index)
    {
        // values of body at index position
        Vector3 currentPosition = currentPositions[index];
        Vector3 currentVelocity = currentVelocities[index];
        Vector3 currentForce = currentForces[index];
        float mass = masses[index];
        
        predictedTimes[0] = currentTime; 
        
        //predicted index
        int j = 0;
        for (float t = 0; t < duration; t += timeStep, j++)
        {
            if(j > maxValues) break;
            
            // Update predicted positions, velocities, forces, and times
            var acceleration = currentForce / mass;
            var newPosition = currentPosition + currentVelocity * timeStep;
            var newVelocity = currentVelocity + acceleration * timeStep;
            var newTime = currentTime + t;

            var newForce = currentForce;   //TODO need to predict force at new position
            
            float distance = 0;
            if(j>1) distance = Vector3.Distance(currentPositions[index],
                newPosition);
            
            //Update values
            if ((index * maxValues + j) < predictedPositions.Length)
            {
           
                predictedPositions[index*maxValues + j] = newPosition;
                distanceOfLine[index * maxValues + j] = distance;
            }
            
            if (j < predictedTimes.Length) predictedTimes[j] = newTime;
            
            
            
            
            
            currentPosition = newPosition;
            currentVelocity = newVelocity;
            currentForce = newForce;
            currentTime = newTime;
        }
        
    }
    
    private void PrintArray(NativeArray<Vector3> array)
    {
        int i = 0;
       // Debug.Log("[Job] number of Element :" + array.Length  );
        foreach (var element in array)
        {
            Debug.Log("[Job] Element at index :" + i+ "is :" + element  );
            i++;
        }
    }
    
    
    public NativeArray<Vector3> GetPredictedPositionsArray( int index)
    {
        var startIndex = index * maxValues;
        var endIndex = startIndex + maxValues;
         
        NativeArray<Vector3> newArray = new NativeArray<Vector3>(endIndex - startIndex, Allocator.Temp);
        for (int i = startIndex; i < endIndex; i++)
        {
            newArray[i - startIndex] = predictedPositions[i];
        }
        return newArray;
    }
    public NativeArray<float> GetDistanceArray( int index)
    {
        var startIndex = index * maxValues;
        var endIndex = startIndex + maxValues;
         
        NativeArray<float> newArray = new NativeArray<float>(endIndex - startIndex, Allocator.Temp);
        for (int i = startIndex; i < endIndex; i++)
        {
            newArray[i - startIndex] = distanceOfLine[i];
        }
        return newArray;
    }
 
    private Vector3 CalculatePredictedGravityPullAtPosition(Vector3 newPosition, double mass, NativeArray<Vector3> currentPositions,  NativeArray<Vector3> currentVelocities, NativeArray<float> masses, NativeArray<Vector3> currentForces,NativeArray<float> influences ,int index, float f)
    {
        
        int numberOfBodies = currentPositions.Length;

        GravityCalculationJob gravityJob = new GravityCalculationJob
        {
            positions = currentPositions,
            masses = masses,
            totalForces = currentForces,
            influenceStrength = influences,
            directGravityMultiplier = directGravityMultiplier,
            gConstant = gConstant
        };

        JobHandle jobHandle = gravityJob.Schedule(numberOfBodies, 64); // Adjust the batch size (64 is an example)
        jobHandle.Complete();

        return gravityJob.totalForces[index];
    }
}
