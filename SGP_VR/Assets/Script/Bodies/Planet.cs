using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Planet : AstralBody
{
    [SerializeField] private PlanetType _planetType = PlanetType.Terrestrial;
    public PlanetType PltType { get => _planetType; set => _planetType = value; }

    public Planet(double mass, double density, Vector3 velocity, Vector3 angularVelocity) : base(mass, density, velocity, angularVelocity, AstralBodyType.Planet) 
    {
        if (ShowDebugLog) Debug.Log("[Planet] Creating new planet :" + _id + " of mass :" + mass + " of density : " + density + "  of velocity " + velocity);


    }

    public Planet(AstralBody astralBody) : base(astralBody) 
    {
        Debug.Log("[Planet] Constructor (AstralBody astralBody) based on astral body");
    }

    public Planet(Planet planet) : base(planet.Mass, planet.Density, planet.StartVelocity, planet.StartAngularVelocity)
    {
        Debug.Log("[Planet] Constructor (Planet planet)  ");
        BodyType = planet.BodyType;
        PltType = planet.PltType;

        SubType = planet.PltType.ToString();
        

   

    }

    public Planet(double mass, double density, Vector3 velocity, Vector3 angularVelocity, PlanetType subType) : this(mass, density, velocity, angularVelocity) 
    {
        PltType = subType;
    }

    public Planet() : base()
    {
        Debug.Log("[Planet] Constructor () based on astral body");
    }


}
