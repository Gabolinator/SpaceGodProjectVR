using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Script.Physics
{
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
           // Debug.Log("totalPull: " + totalPull);
            for (int i = 0; i < positions.Length; i++)
            {
                if (i != index)
                {
                    totalPull += CalculateGravityPullJob(gConstant, positions[index], masses[index], positions[i], masses[i])* influenceStrength[i];
                }
            }

            totalForces[index] = totalPull * directGravityMultiplier *  influenceStrength[index];
        }
    
        // public static Vector3 CalculateTotalGravityPull(Vector3 thisPosition, float thisMass, Vector3 otherPosition, float otherMass)
        // {
        //     Vector3 totalPull = Vector3.zero;
        //
        //     if (listOfBody.Count == 0) return totalPull;
        //
        //     foreach (var body in listOfBody)
        //     {
        //         if (body) totalPull += CalculateGravityPull(thisBody, body,position, timeStep);
        //     }
        //
        //     return totalPull;
        // }
    
        public static Vector3 CalculateGravityPullJob(double G, Vector3 thisPosition, float thisMass, Vector3 otherPosition, float otherMass)
        {
            // if (otherBody == null) return Vector3.zero;

            //float distanceFromOther = Vector3.Distance(thisPosition, otherPosition);

       
            //
            // Vector3 direction = -1 * (position - otherBody.transform.position + timeStep * otherBody.Velocity).normalized;
            //
            // double m1 = otherBody.Mass;
            // double m2 = body.Mass;
            //
            // double pull = (G * m1 * m2 / Mathf.Pow(distanceFromOther, 2)) * otherBody.InfluenceStrength * body.InfluenceStrength;
            // // if (showDebugLog) Debug.Log("[AstralBody] Gravitationnal Pull between" + this + " and " + otherBody + " : " + pull) ;
            // return (float)pull * direction;
        
            Vector3 direction = otherPosition - thisPosition;
            float distance = direction.magnitude;
            float forceMagnitude =  (float)(G* thisMass * otherMass / (distance * distance));
        
            Vector3 force = direction.normalized * forceMagnitude ;
            //Debug.Log("Force: " + forceMagnitude + " : distance : " + distance + ": G:  " + G + " : thisMass : " + thisMass + "otherMass : " + otherMass );
            
            return force;
        }
    
    }
}
