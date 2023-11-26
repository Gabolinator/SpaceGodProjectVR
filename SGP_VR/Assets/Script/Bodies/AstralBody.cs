using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]

public enum CelestialBodyElement
{
    Hydrogen,
    Helium,
    Oxygen,
    Carbon,
    Nitrogen,
    Silicon,
    Iron,
    Sodium,
    Magnesium,
    Potassium,
    Calcium,
    Aluminum,
    Sulfur,
    Phosphorus,
    Chlorine,
    Argon,
    Neon,
    Xenon,
    Krypton,
    Radon,
    Gold,
    Silver,
    Uranium,
    Thorium,
    Titanium,
    Nickel,
    Chromium,
    Vanadium,
    Cobalt,
    Manganese,
    Zinc,
    Copper,
    Boron,
    Iodine,
    Platinum,
    Palladium,
    Rhodium,
    Tungsten,
    Rhenium,
    Lead,
    Bismuth,
    Tin,
    Mercury,
    Cadmium,
    Thallium,
    Barium,
    Strontium,
    Rubidium,
    Cesium,
    Beryllium,
    Lithium,
    Other // Use this for elements not in the list
}

[System.Serializable]
public class ChemicalBodyCompositionElement 
{
    public CelestialBodyElement _element;
    public float _percentage;

    public ChemicalBodyCompositionElement(CelestialBodyElement element, float percentage) 
    {
        _element = element;
        _percentage = percentage;
    }

}

[System.Serializable]
public class AstralBodyDescriptor 
{
    
    public string _id;

    public string _bodyType;

    public string _subType = "-";

    public double _mass;

    public double _density;

    public double _radius;

    public float _temperature;

    public List<ChemicalBodyCompositionElement> _bodyComposition = new ();

    public Vector3 _velocity;

    public Vector3 _angularVelocity;

   
    public AstralBodyDescriptor(AstralBody body) 
    {
        _id = body.ID;
        _bodyType = body.BodyType.ToString();
        _mass = body.Mass;// * UniverseManager.Instance.PhysicsProperties.MassFactor;
        _density = body.Density;// * UniverseManager.Instance.PhysicsProperties.DensityFactor;
        _radius = body.Radius;// *UniverseManager.Instance.PhysicsProperties.DistanceFactor;
        //_volume = body.Volume *UniverseManager.Instance.PhysicsProperties.vo;
        _subType = body.SubType;

    } 
    
    
}

[System.Serializable]
public struct BodyPhysicalCharacteristics
{

    public double _mass;
    public double _density;
    
    public List<ChemicalBodyCompositionElement> _bodyComposition;
    
    /*internal resistance*/
   
    public float _c; 
    public float _u;
    public float _s;
    public float _internalResistance; 
    
}

[System.Serializable]
public struct OrbitingData
{
    [Header("Orbit")] 
    public double currentRadiusOfTrajectory;
    public Transform _centerOfRotation;
    public bool IsOrbiting => (_centerOfRotation != null);
    public float distanceFromCenter;
    public bool isSatellite;
}

[System.Serializable]
public struct SatellitesData
{
    [Header("Satellites")] 
    public int maxNumberOfSatellites;
    public List<AstralBody> satellites;
    public bool canHaveSatellites;
    public bool HasSatellite => satellites.Count != 0;
}
[System.Serializable]
public struct RingsData
{
    [Header("Ring")] 
    public List<CelestialRing> rings;
    public bool canHaveRings;
   
    public bool HasRings => rings.Count != 0;
}

[System.Serializable]
public class AstralBody
{

    [SerializeField] protected string _id;

    public string ID
    {
        get => _id;
        set => _id = value;
    }

    [SerializeField] protected AstralBodyType _bodyType = AstralBodyType.Uninitialized;

    public AstralBodyType BodyType
    {
        get => _bodyType;
        set => _bodyType = value;
    }

    //[SerializeField] protected double _mass;

    public double Mass
    {
        get => _physicalCharacteristics._mass;
        set => _physicalCharacteristics._mass = value;
    }

    [SerializeField] protected Vector3 _startVelocity;

    public Vector3 StartVelocity
    {
        get => _startVelocity;
        set => _startVelocity = value;
    }

    [SerializeField] protected Vector3 _startAngularVelocity;

    public Vector3 StartAngularVelocity
    {
        get => _startAngularVelocity;
        set => _startAngularVelocity = value;
    }

    //[SerializeField] protected double _density;

    public double Density
    {
        get => _physicalCharacteristics._density;
        set => _physicalCharacteristics._density = value;
    }

    [SerializeField] protected double _radius;

    public double Radius
    {
        get => _radius;
        set => _radius = value;
    }

    [SerializeField] protected double _volume;

    public double Volume
    {
        get => _volume;
        set => _volume = value;
    }

    [SerializeField] protected string _subType = "-";

    public string SubType
    {
        get => _subType;
        set => _subType = value;
    }

    public BodyPhysicalCharacteristics _physicalCharacteristics = new BodyPhysicalCharacteristics();

    public List<ChemicalBodyCompositionElement> BodyComposition
    {
        get => _physicalCharacteristics._bodyComposition;
        set => _physicalCharacteristics._bodyComposition = value;
    }

    [SerializeField] private float _temperature;

    public float Temperature
    {
        get => _temperature;
        set => _temperature = value;
    }

    public float InternalResistance
    {
        get => _physicalCharacteristics._internalResistance;
        set => _physicalCharacteristics._internalResistance = value;
    }

    /*Internal resistance variables  -see page 9 */
    public float c
    {
        get => _physicalCharacteristics._c;
        set => _physicalCharacteristics._c = value;
    } //= 6.4f; 

    public float u
    {
        get => _physicalCharacteristics._u;
        set => _physicalCharacteristics._u = value;
    } // = .43f;

    public float s
    {
        get => _physicalCharacteristics._s;
        set => _physicalCharacteristics._s = value;
    } //= 1; 

    public OrbitingData orbitingData = new OrbitingData();
    
    public double CurrentRadiusOfTrajectory
    {
        get => orbitingData.currentRadiusOfTrajectory;
        set => orbitingData.currentRadiusOfTrajectory = value;
    }

    public Transform CenterOfRotation => orbitingData._centerOfRotation;
    public bool IsOrbiting => (CenterOfRotation != null);
    public float DistanceFromCenter=> orbitingData.distanceFromCenter;
    public bool IsSatellite => orbitingData.isSatellite;


    public SatellitesData satellitesData = new SatellitesData();
    
    public bool CanHaveSatellites => satellitesData.canHaveSatellites;
    public List<AstralBody> Satellites => satellitesData.satellites;
    public bool HasSatellite => satellitesData.HasSatellite;
  
    public RingsData ringsData = new RingsData();
    
    public List<CelestialRing> Rings => ringsData.rings;
    public bool CanHaveRings => ringsData.canHaveRings;
    public bool HasRings => ringsData.HasRings;
    
    public bool ShowDebugLog => AstralBodiesManager.Instance._showDebugLog;

    #region Constructor

    public AstralBody(double mass, double density, Vector3 velocity, Vector3 angularVelocity, AstralBodyType type) :
        this(mass, density, velocity, angularVelocity)
    {
        BodyType = type;
        SetCanHaveSatellites();
        SetCanHaveRing();

    }

    public AstralBody(BodyPhysicalCharacteristics physicalCharacteristics, Vector3 velocity, Vector3 angularVelocity, AstralBodyType type) :   this(physicalCharacteristics, velocity, angularVelocity)
    {
       
        BodyType = type;
        SetCanHaveSatellites();
        SetCanHaveRing();
    }

    public AstralBody(BodyPhysicalCharacteristics physicalCharacteristics, Vector3 velocity, Vector3 angularVelocity,
        string id = "")
    {
        _physicalCharacteristics = physicalCharacteristics;
        
        StartVelocity = velocity;
        StartAngularVelocity = angularVelocity;
        

        _volume = CalculateVolume(Mass, Density);
        _radius = CalculateRadius(_volume);
        _id = id == "" ? AstralBodiesManager.Instance.GenerateName() : id;


        _bodyType = AstralBodyType.other;
        SetCanHaveSatellites();
        SetCanHaveRing();
        
        if (ShowDebugLog)
            Debug.Log("[AstralBody] Creating new body : " + _id + " of mass :" + Mass + " of density : " + Density +
                      "  of velocity " + velocity);
    }

    public AstralBody(double mass, double density, Vector3 velocity, Vector3 angularVelocity, string id = "")
    {

        Mass = mass;
        Density = density;
        StartVelocity = velocity;
        StartAngularVelocity = angularVelocity;

        //_internalResistance = 0.001f; 

        _volume = CalculateVolume(mass, density);
        _radius = CalculateRadius(_volume);
        _id = id == "" ? AstralBodiesManager.Instance.GenerateName() : id;


        _bodyType = AstralBodyType.other;

        SetCanHaveSatellites();
        SetCanHaveRing();
        
        if (ShowDebugLog)
            Debug.Log("[AstralBody] Creating new body : " + _id + " of mass :" + mass + " of density : " + density +
                      "  of velocity " + velocity);

    }

    public AstralBody(AstralBodyType type) : this(0, 0, Vector3.zero, Vector3.zero, type)
    {
        
    }

    public AstralBody()
    {
    }


    public AstralBody(AstralBody astralBody)
    {
        _physicalCharacteristics = astralBody._physicalCharacteristics;
        StartVelocity = astralBody.StartVelocity;
        _bodyType = astralBody._bodyType;
        StartAngularVelocity = astralBody.StartAngularVelocity;
        _id = astralBody._id;

        _physicalCharacteristics = astralBody._physicalCharacteristics;

        orbitingData = astralBody.orbitingData;
        satellitesData = astralBody.satellitesData;
        ringsData = astralBody.ringsData;
       

        _volume = CalculateVolume(Mass, Density);
        _radius = CalculateRadius(_volume);
        
        SetCanHaveSatellites();
        SetCanHaveRing();
    }



    #endregion

    public void SetPhysicalCharateristics(List<ChemicalBodyCompositionElement> composition, float c, float u, float s, float internalResistance)
    {
        _physicalCharacteristics._bodyComposition = composition;
        _physicalCharacteristics._c = c;
        _physicalCharacteristics._u = u;
        _physicalCharacteristics._s = s;
        _physicalCharacteristics._internalResistance = internalResistance;
    }

    public void SetPhysicalCharateristics(BodyPhysicalCharacteristics characteristics)
    {
        _physicalCharacteristics = _physicalCharacteristics;
    }

    public virtual void SetCanHaveSatellites()
    {
        //TODO check if mass is large enough to keep satellites 
        satellitesData.canHaveSatellites = true;
    }
    
    public virtual void SetCanHaveRing()
    {
        if(BodyType == AstralBodyType.Fragment || BodyType == AstralBodyType.SmallBody) ringsData.canHaveRings = false || BodyType == AstralBodyType.BlackHole;
        else ringsData.canHaveRings = true;
    }

    public float CalculateInternalGravity(double mass) => FormulaLibrary.CalculateInternalGravity(mass);
   
    public double CalculateMass(double density, double volume) => FormulaLibrary.CalculateMass(density, volume);

    public double CalculateDensity(double mass, double volume) => FormulaLibrary.CalculateDensity(mass, volume);   

    public  virtual double CalculateVolume(double radius) => FormulaLibrary.CalculateVolume(radius);

    public virtual double CalculateRadius(Vector3 scale) => FormulaLibrary.CalculateRadius(scale);

    public  virtual double CalculateRadius(double volume) => FormulaLibrary.CalculateRadius(volume);

    public double CalculateVolume(double mass, double density) => FormulaLibrary.CalculateVolume(mass, density);

    public string GetBodyName() 
    {
        return GetBodyName(this);
    }

    public string GetBodyName(AstralBody body)
    {
        return body.BodyType.ToString() + ((body.SubType.ToString() == "-") ? "" : ("_" + body.SubType.ToString())) + "_" + body.ID;
    }

    public bool PredictCollisionAtPosition(AstralBody body, Vector3 position, float buffer = 0.5f)
    {
        var radius = body.Radius;
        if (body == null) Debug.LogWarning("[Astral Body] Trying to test collision at : " + position + "but body is null");
        if (radius == 0)
        {

            Debug.LogWarning("[Astral Body] Trying to test collision at : " + position + "but no body.Radius is set");
            //radius = 10;
        }

        Collider[] colliders = Physics.OverlapSphere(position, (float)radius + buffer);

        return !(colliders.Length == 0);
    }

    public bool PredictCollisionAtPosition(Vector3 position, float buffer = 0.5f)
    {
        return PredictCollisionAtPosition(this, position, buffer);
    }

   

}
