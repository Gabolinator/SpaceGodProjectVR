using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

    [BurstCompile]
    public struct GravityCalculationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        [ReadOnly] public NativeArray<float> masses;
        [ReadOnly] public NativeArray<float> influenceStrength;
        public NativeArray<Vector3> totalForces;
        public float directGravityMultiplier;
        public double gConstant;
        
        public void Execute(int index)
        {
            Vector3 totalPull = Vector3.zero;
        
            for (int i = 0; i < positions.Length; i++)
            {
                if (i != index)
                {
                    totalPull += CalculateGravityPullJob(gConstant, positions[index], masses[index], positions[i], masses[i])* influenceStrength[i];
                }
            }

            totalForces[index] = totalPull * directGravityMultiplier *  influenceStrength[index];
        }
    
    
    
        public static Vector3 CalculateGravityPullJob(double G, Vector3 thisPosition, float thisMass, Vector3 otherPosition, float otherMass)
        {
        
            Vector3 direction = otherPosition - thisPosition;
            float distance = direction.magnitude;
            float forceMagnitude =  (float)(G* thisMass * otherMass / (distance * distance));
        
            Vector3 force = direction.normalized * forceMagnitude ;
            //Debug.Log("Force: " + forceMagnitude + " : distance : " + distance + ": G:  " + G + " : thisMass : " + thisMass + "otherMass : " + otherMass );
            
            return force;
        }
    
    }

