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
            
        
           // Debug.Log("PredictedForce : " + newForce);
            
            
            float distance = 0;
            if(j>1) {
                distance = Vector3.Distance(currentPositions[index],
                newPosition);
             
            }

            
            
            //Update values
            if ((index * maxValues + j) < predictedPositions.Length)
            {
           
                predictedPositions[index*maxValues + j] = newPosition;
                distanceOfLine[index * maxValues + j] = distance;
            }
            
            if (j < predictedTimes.Length) predictedTimes[j] = newTime;
            
            
            var newForce =  CalculatePredictedGravityPullAtPosition( GetBodiesPredictedPositionsArray(j, currentPositions.Length, maxValues),  currentVelocities, masses, currentForces, influences , index, timeStep);

            currentPosition = newPosition;
            currentVelocity = newVelocity;
            currentForce = newForce;
            currentTime = newTime;
        }
        
    }
    
    private Vector3 CalculatePredictedGravityPullAtPosition(NativeArray<Vector3> currentPositions,  NativeArray<Vector3> currentVelocities, NativeArray<float> masses, NativeArray<Vector3> currentForces,NativeArray<float> influences ,int index, float timeStep)
    {
        
        int numberOfBodies = currentPositions.Length;

        //update all positions - then recaluculate force for each of those new positions
        NativeArray<Vector3> predictedPositions = new NativeArray<Vector3>(numberOfBodies, Allocator.Temp);
        NativeArray<Vector3> predictedVelocities = new NativeArray<Vector3>(numberOfBodies, Allocator.Temp);
        NativeArray<Vector3> predictedForces = new NativeArray<Vector3>(numberOfBodies, Allocator.Temp);
        
        // Update predicted positions, velocities, forces, and times
        for(int i = 0; i< currentPositions.Length ; i++)
        {
            var mass = masses[i];
            var predictedForce = currentForces[i];
            var predictedPosition = currentPositions[i];
            var predictedVelocity = currentVelocities[i];
            
            var acceleration = predictedForce / mass;
            var newPosition = predictedPosition + predictedVelocity * timeStep;
            var newVelocity = predictedVelocity + acceleration * timeStep;

            predictedPositions[i] = newPosition;
            predictedVelocities[i] = newVelocity;
            
            for (int j = 0; j < numberOfBodies; j++)
            {
                if (j != i)
                {
                    predictedForces[i] += CalculateGravityPullJob(gConstant, newPosition, mass, predictedPositions[j], masses[j])* influences[j];
                }
            }
           
        }
    

        return  predictedForces[index] * directGravityMultiplier *  influences[index];
    }
    
    public static Vector3 CalculateGravityPullJob(double G, Vector3 thisPosition, float thisMass, Vector3 otherPosition, float otherMass)
    {
        
        Vector3 direction = otherPosition - thisPosition;
        float distance = direction.magnitude;
        float forceMagnitude =  (float)(G* thisMass * otherMass / (distance * distance));
        
        Vector3 force = direction.normalized * forceMagnitude ;
      //  Debug.Log("Force: " + forceMagnitude + " : this position : " + thisPosition + " : other position : "+ otherPosition +" : distance : " + distance + ": G:  " + G + " : thisMass : " + thisMass + "otherMass : " + otherMass );
            
        return force;
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

    public NativeArray<Vector3> GetBodiesPredictedPositionsArray(int t, int numBodies, int maxValues)
    {
        // get the position at predicted time t of all the bodies. 
        NativeArray<Vector3> newArray = new NativeArray<Vector3>(numBodies, Allocator.Temp);


        for (int i = 0; i < numBodies; i++)
        {

            var index = i * maxValues; 
            if (index >= predictedPositions.Length) break;
            newArray[i] = predictedPositions[index];
        }


        
        return newArray;
    }

    public NativeArray<Vector3> GetPredictedPositionsArray( int index)
    {
        var startIndex = index * maxValues;
        var endIndex = startIndex + maxValues;
         
        NativeArray<Vector3> newArray = new NativeArray<Vector3>(endIndex - startIndex, Allocator.Temp);
        for (int i = startIndex; i < endIndex; i++)
        {
            if(i - startIndex < newArray.Length && i < predictedPositions.Length) newArray[i - startIndex] = predictedPositions[i];
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
            if(i - startIndex < newArray.Length && i < distanceOfLine.Length) newArray[i - startIndex] = distanceOfLine[i];
        }
        return newArray;
    }

  

    
}
