using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
public class AstralBody
{

    [SerializeField] protected string _id;
    public string ID { get => _id; set => _id = value; }

    [SerializeField] protected AstralBodyType _bodyType = AstralBodyType.Uninitialized;
    public AstralBodyType BodyType { get => _bodyType; set => _bodyType = value; }

    [SerializeField] protected double _mass;
    public double Mass { get => _mass; set => _mass = value; }

    [SerializeField] protected Vector3 _startVelocity;
    public Vector3 StartVelocity { get => _startVelocity; set => _startVelocity = value; }

    [SerializeField] protected Vector3 _startAngularVelocity;
    public Vector3 StartAngularVelocity { get => _startAngularVelocity; set => _startAngularVelocity = value; }

    [SerializeField] protected double _density;
    public double Density { get => _density; set => _density = value; }

    [SerializeField] protected double _radius;
    public double Radius { get => _radius; set => _radius = value; }

    [SerializeField] protected double _volume;
    public double Volume { get => _volume; set => _volume = value; }

    [SerializeField] protected string _subType = "-";
    public string SubType { get => _subType; set => _subType = value; }

    [SerializeField]
    private List<ChemicalBodyCompositionElement> _bodyComposition = new();
    public List<ChemicalBodyCompositionElement> BodyComposition { get => _bodyComposition; set => _bodyComposition = value; }

    [SerializeField]
    private float _temperature; 
    public float Temperature { get => _temperature; set => _temperature = value; }
    
    [SerializeField]
    private float _internalResistance; //how well does the structure hold together
    public float InternalResistance { get => _internalResistance; set => _internalResistance = value; }

    public bool ShowDebugLog => AstralBodiesManager.Instance._showDebugLog;

    #region Constructor
    public AstralBody(double mass, double density, Vector3 velocity,  Vector3 angularVelocity, AstralBodyType type) : this(mass, density, velocity, angularVelocity)
    {
        BodyType = type;
    
    }


    public AstralBody(double mass, double density, Vector3 velocity, Vector3 angularVelocity, string id = "")
    {

        _mass = mass;
        _density = density;
        StartVelocity = velocity;
        StartAngularVelocity = angularVelocity;

        //_internalResistance = 0.001f; 
        
        _volume = CalculateVolume(mass, density);
        _radius = CalculateRadius(_volume);
        _id = id == "" ? AstralBodiesManager.Instance.GenerateName() : id;
        
        
        _bodyType = AstralBodyType.other;

        if (ShowDebugLog) Debug.Log("[AstralBody] Creating new body : " + _id + " of mass :" + mass + " of density : " + density + "  of velocity " + velocity);

    }

    public AstralBody(AstralBodyType type) : this(0, 0, Vector3.zero, Vector3.zero, type) { }

    public AstralBody() { }


    public AstralBody(AstralBody astralBody)
    {
        _mass = astralBody.Mass;
        _density = astralBody.Density;
        StartVelocity = astralBody.StartVelocity;
        _bodyType = astralBody._bodyType;
        StartAngularVelocity = astralBody.StartAngularVelocity;
        _id = astralBody._id;
        
        _internalResistance = astralBody._internalResistance;
        
        
        _volume = CalculateVolume(_mass, _density);
        _radius = CalculateRadius(_volume);
    }

    #endregion

    public float CalculateInternalGravity(double mass) => FormulaLibrairy.CalculateInternalGravity(mass);
   
    public double CalculateMass(double density, double volume) => FormulaLibrairy.CalculateMass(density, volume);

    public double CalculateDensity(double mass, double volume) => FormulaLibrairy.CalculateDensity(mass, volume);   

    public  virtual double CalculateVolume(double radius) => FormulaLibrairy.CalculateVolume(radius);

    public virtual double CalculateRadius(Vector3 scale) => FormulaLibrairy.CalculateRadius(scale);

    public  virtual double CalculateRadius(double volume) => FormulaLibrairy.CalculateRadius(volume);

    public double CalculateVolume(double mass, double density) => FormulaLibrairy.CalculateVolume(mass, density);

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
