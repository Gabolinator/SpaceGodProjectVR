using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormulaLibrary
{

    #region Gravity Related


    public static double G => UniverseManager.Instance.PhysicsProperties.GravitationnalConstant* UniverseManager.Instance.PhysicsProperties.GravitationnalConstantFactor; //10e-11 //m3 kg-1 s-2


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
        if (volume == 0) return 0;
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


    public static double CalculateRadius(double mass, double density)
    {
        double volume = CalculateVolume(mass, density);
        return CalculateRadius(volume);
    }

    public static double CalculateVolume(double mass, double density)
    {
        if (density == 0) return 0;
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


    public static double EstimateMeshVolume(Mesh mesh)
    {
        if (!mesh) return -1;
        
        if (!mesh.isReadable)
        {
            // Make the mesh readable
            mesh.MarkDynamic();
        }
        
        float volume = 0f;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        if (vertices.Length == 0 || triangles.Length == 0) return -1;
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];

            volume += Vector3.Dot(Vector3.Cross(p1, p2), p3) / 6f;
        }

        return Mathf.Abs(volume); // Volume should be positive
    }


    public static double CalculateImpactEnergy(CollidingBody target, CollidingBody projectile)
    {
     
        double ef = UniverseManager.Instance.PhysicsProperties.EnergyFactor;
        
        //impact energy relative to center of mass  eq.1
        double u = CalculateReducedMass(target._body.Mass, projectile._body.Mass);

        var vi = CalculateImpactRelativeVelocity(target, projectile);

        return
            .5f * u * vi * vi /
            (target._body.Mass+ projectile._body.Mass)*ef; //CalculateKineticEnergy(target._body.Mass, target._bodyImpactVelocity) +  CalculateKineticEnergy(projectile._body.Mass, projectile._bodyImpactVelocity); 
    }

    private static double CalculateReducedMass(double targetMass, double projectileMass)
    {
        return (targetMass * projectileMass) / (targetMass + projectileMass); 
    }

    private static Vector3 CalculateCenterOfMassVelocity(Vector3 v1, float m1, Vector3 v2, float m2)
    {
        Vector3 centerOfMassVelocity = (m1 * v1 + m2 * v2) / (m1 + m2);

        return centerOfMassVelocity;
    }

    
    private static Vector3 CalculateRelativeVelocity(Vector3 v, Vector3 centerOfMassVelocity)
    {
        Vector3 relativeVelocity = v - centerOfMassVelocity;

        return relativeVelocity;
    }

    
    public static double CalculateEnergyToDisperseHalf(CollidingBody target, CollidingBody projectile)
    {
        throw new System.NotImplementedException();
    }

    public static double CalculateErosionEnergy(double superCatEnergy, CollidingBody target, CollidingBody projectile)
    {
       return 2 * (superCatEnergy) * target._body.Mass / (target._body.Mass + projectile._body.Mass);
    }

    public static double CalculateVelocity(double impactEnergy, CollidingBody target, CollidingBody projectile)
    {
        double reducedMass = CalculateReducedMass(target._body.Mass, projectile._body.Mass);
        
        return Mathf.Sqrt(2 * (float)impactEnergy * (float)(target._body.Mass + projectile._body.Mass) / (float)reducedMass);
    }

    public static double CalculateCriticalImpactEnergy(CollidingBody target, CollidingBody projectile)
    {
        
        /**/
       
        // The parameter c is a measure of the dissipation of energy withinthe target.
        //  c∗ = 5 ± 2 and u = 0.37 ± 0.01 for
        // small bodies with a wide variety of material characteristics and
        // c = 1.9 ± 0.3 and u = 0.36 ± 0.01 for the hydrodynamic
        // planet-size bodies.
         
        //eq.23 + eq 28
     
        var c = target._body.c; 
        var  u = target._body.u; 
       
        double ef = UniverseManager.Instance.PhysicsProperties.EnergyFactor;
        
        float rho = 1000;
        double Rc = CalculateRadius(projectile._body.Mass + target._body.Mass, 1000) ; //Radius of a sphere of total mass projectile._body.Mass + target._body.Mass but of density 1000
        float y = (float)((projectile._body.Mass + target._body.Mass)/target._body.Mass); // y=total mass /mass target -1 from eq. 21
        
        double part1 = c * 4 / 5 * Mathf.PI * rho * G * Rc; //QrdStary=1 eq. 28
        
        float insidePow = (.25f * (Mathf.Pow(y + 1, 2)) / y); 
        
        float exponent = 2 / (3 * u) - 1;
       
        double part2 = Mathf.Pow(insidePow, exponent);
        
        return part1 * part2 * ef;
        

        //return c * (target._body.Density * Mathf.Pow((float)target._body.Radius, 3) * s * Mathf.Pow((float)(projectile._body.Mass / target._body.Mass), u)) / ((projectile._body.Mass / target._body.Mass)) ;
    }

    public static float CalculateEscapeVelocity(CollidingBody target, CollidingBody projectile, double interactinMass)
    {
         //double df = UniverseManager.Instance.PhysicsProperties.DistanceFactor;
         double mf = UniverseManager.Instance.PhysicsProperties.MassFactor;
         //double gf = UniverseManager.Instance.PhysicsProperties.GravitationnalConstantFactor;
         
        double Mprime = (target._body.Mass + interactinMass);
        double Rprime = Mathf.Pow((3 * (float)Mprime) / (4 * Mathf.PI * (float)CalculateBulkDensity(target, projectile)), 1f / 3f);
        return Mathf.Sqrt(2 * (float)G * (float)(Mprime / Rprime));
    }

    public static double CalculateBulkDensity(CollidingBody target, CollidingBody projectile)
    {
        return (target._body.Mass + projectile._body.Mass) / (target._body.Volume + projectile._body.Volume);
    }

    public static double CalculateInteractingMass(CollidingBody projectile, CollidingBody target, float impactAngle)
    {
        var b = Mathf.Sin(impactAngle);
        
       float l = (float)(target._body.Radius + projectile._body.Radius) * (1 - b);

        // Calculate the interacting mass of the projectile during impact eq.2
        float alpha = Mathf.Abs((3 * (float)projectile._body.Radius) * Mathf.Pow(l, 2) - Mathf.Pow(l, 3)) /
                      (4 * Mathf.Pow((float)projectile._body.Radius, 3));

        return alpha * projectile._body.Mass;

    }


    public static float CalculateImpactAngle(Vector3 projectileBodyImpactVelocity, Vector3 contactPointNormal)
    {
        var impactNormal = contactPointNormal.normalized;
        var projectileVelocity = projectileBodyImpactVelocity.normalized;
        
        float dotProduct = Vector3.Dot(impactNormal, projectileVelocity);
        
        return Mathf.Acos(dotProduct); // in radian 
    }

    public static double CalculateImpactRelativeVelocity(CollidingBody target, CollidingBody projectile)
    {
        Vector3 Vcm = CalculateCenterOfMassVelocity(target._bodyImpactVelocity, (float)(target._body.Mass),
            projectile._bodyImpactVelocity, (float)(projectile._body.Mass));
        
        return CalculateRelativeVelocity(target._bodyImpactVelocity, Vcm).magnitude +
                    CalculateRelativeVelocity(projectile._bodyImpactVelocity, Vcm).magnitude;
        
    }

    public static double CalculateLargestRemnantMass(double totalMass, double Qr, double Qrd)
    {
        return (.5f * (Qr / Qrd - 1) + .5f) * totalMass;
    }
}
