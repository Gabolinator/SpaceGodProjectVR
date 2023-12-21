using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Star : AstralBody
{
    [SerializeField] private StarType _starType = StarType.MainSequenceStar;
    public StarType StrType { get => _starType; set => _starType = value; }

    [SerializeField] private StarSpectralType _spectralType = StarSpectralType.none;
    public StarSpectralType SpectralType { get => _spectralType; set => _spectralType = value; }

    [SerializeField]
    private float _radiation;
    public float Radiation { get => _radiation; set => _radiation = value; }

    public Star(double mass, double density, Vector3 velocity, Vector3 angularVelocity) : base(mass, density, velocity, angularVelocity, AstralBodyType.Star) 
    {
        if (ShowDebugLog) Debug.Log("[Star] Creating new star : " + _id + " of mass :" + mass + " of density : " + density + "  of velocity " + velocity);
    }
    
    public Star(BodyPhysicalCharacteristics physicalCharacteristics, Vector3 velocity, Vector3 angularVelocity) : base(physicalCharacteristics, velocity, angularVelocity, AstralBodyType.Star) 
    {
        if (ShowDebugLog) Debug.Log("[Star] Creating new star : " + _id + " of mass :" + physicalCharacteristics._mass + " of density : " +physicalCharacteristics._density + "  of velocity " + velocity);
    }

    public Star(AstralBody astralBody) : base(astralBody) 
    {
        Debug.Log("[Star] Constructor (AstralBody astralBody) based on astral body");
    }

    public Star(Star star) :base(star._physicalCharacteristics, star.StartVelocity, star.StartAngularVelocity)
    {
        Debug.Log("[Star] Constructor (Star star) ");
        BodyType = star.BodyType;
        StrType = star.StrType;
        SpectralType = star.SpectralType;

        SubType = star.StrType.ToString();
        if (star.StrType == StarType.MainSequenceStar ) SubType += " - " + star.SpectralType;
        
        orbitingData = star.orbitingData;
        satellitesData = star.satellitesData;
        ringsData = star.ringsData;
        
    }

    public Star(double mass, double density, Vector3 velocity, Vector3 angularVelocity, StarType starType) : this(mass, density, velocity, angularVelocity)
    {
       StrType = starType;
    }

    public Star(double mass, double density, Vector3 velocity, Vector3 angularVelocity, StarSpectralType spectralType) : this(mass, density, velocity,angularVelocity ,StarType.MainSequenceStar)
    {
        SpectralType = spectralType;
        
    }

    public Star() : base() 
    {
        Debug.Log("[Star] Constructor () based on astral body");
    }

    
   
    
    public override void SetCanHaveRing()
    {
        ringsData.canHaveRings = false;
    }

}

