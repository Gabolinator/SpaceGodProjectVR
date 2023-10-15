using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormulaLibrairy
{

    #region Gravity Related


    public static double G => UniverseManager.Instance.PhysicsProperties.GravitationnalConstantFactor; //10e-11 //m3 kg-1 s-2


    public static float CalculateInternalGravity(double mass)
    {
        //TODO : implement 
        float internalGravity = 0;

        return internalGravity;
    }

    public static Vector3 CalculateGravityPull(AstralBodyHandler body, AstralBodyHandler otherBody, Vector3 position, float timeStep = 0)
    {
        if (otherBody == null) return Vector3.zero;

        float distanceFromOther = Vector3.Distance(position, otherBody.transform.position + timeStep * otherBody.Velocity);

        //Debug.Log("[AstralBody] Distance between " + this + " and " + otherBody + " : " + distanceFromOther);

        if (otherBody.InfluenceRange < distanceFromOther && otherBody.InfluenceRange > 0) return Vector3.zero;


        Vector3 direction = -1 * (position - otherBody.transform.position + timeStep * otherBody.Velocity).normalized;

        double m1 = otherBody.Mass;
        double m2 = body.Mass;
        //Debug.Log("[AstralBody] M1 before cast to float" + m1 + " and after" + (float)m1);
        //Debug.Log("[AstralBody] M1 before cast to float" + m2 + " and after" + (float)m2);
        //Debug.Log("[AstralBody] G : " + G );
        double pull = (G * m1 * m2 / Mathf.Pow(distanceFromOther, 2)) * otherBody.InfluenceStrength * body.InfluenceStrength;
        // if (showDebugLog) Debug.Log("[AstralBody] Gravitationnal Pull between" + this + " and " + otherBody + " : " + pull) ;
        return (float)pull * direction;
    }

    public static Vector3 CalculateTotalGravityPull(List<AstralBodyHandler> listOfBody, AstralBodyHandler thisBody ,Vector3 position, float timeStep = 0)
    {
        Vector3 totalPull = Vector3.zero;

        if (listOfBody.Count == 0) return totalPull;

        foreach (var body in listOfBody)
        {
            if (body) totalPull += CalculateGravityPull(thisBody, body,position, timeStep);
        }

        return totalPull;
    }


    #endregion

    #region Physical Propertie (Mass, volume etc)


    public static double CalculateMass(double density, double volume)
    {
        return density * volume;
    }

    public static double CalculateDensity(double mass, double volume)
    {
        return mass / volume;
    }

    public static double CalculateVolume(double radius)
    {
        return 4 * Mathf.PI / 3 * Mathf.Pow((float)radius, 3);
    }

    public static double CalculateRadius(Vector3 scale)
    {
        return scale.x / 2;
    }

    public static  double CalculateRadius(double volume)
    {
        double radius = Mathf.Pow(3 * (float)volume / (4 * Mathf.PI), 1.0f / 3.0f);

        return radius;
    }

    public static double CalculateVolume(double mass, double density)
    {
        return mass / density;
    }

    #endregion


    public static Vector3 CalculateAcceleration(double mass, Vector3 force)
    {
        Vector3 acceleration = Vector3.zero;

        float _mass = (float)mass;

        if (_mass > 0) acceleration = force / _mass;

        return acceleration;
    }


    public static Vector3 GetAcceleration(AstralBodyHandler body)
    {
        Vector3 acceleration = Vector3.zero;
        if (!body)
        {
            return acceleration;
        }


        if (body)
        {
            float mass = (float)body.Mass;
            if (mass > 0) acceleration = body.totalForceOnObject / mass;
        }
        return acceleration;
    }

    public static Vector3 GetAcceleration(AstralBodyHandler body, Vector3 atPosition, float timeStep)
    {
        Vector3 acceleration = Vector3.zero;
        if (!body)
        {
            return acceleration;
        }

        if (body)
        {
            float mass = (float)body.Mass;
            List<AstralBodyHandler> allBodyInRange = new List<AstralBodyHandler>();
            float range = body.InfluenceRange < 0 ? 50 : body.InfluenceRange;
            allBodyInRange = body.GetAllBodyInRange(50, allBodyInRange, atPosition);

            Vector3 totalPullAtLocation = body.CalculateTotalGravityPull(allBodyInRange, atPosition, timeStep);
            if (mass > 0) acceleration = totalPullAtLocation / mass;
        }
        
        return 2 * acceleration;
    }







    public static double CalculateKineticEnergy(double mass, Vector3 velocity)
    {
        double kyneticEnergy = .5f * mass * velocity.magnitude * velocity.magnitude;
     
        return kyneticEnergy;
    }

}