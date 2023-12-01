using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class PhysicsProperties
{
    private const double _gravitationnalConstant = 6.6743;
   
    public double GravitationnalConstant => _gravitationnalConstant;

   private const double _gravitationnalConstantFactor = 1E-11;
    public double GravitationnalConstantFactor => _gravitationnalConstantFactor;

    private const double _massFactor = 1E21;
    public double MassFactor => _massFactor;

    private const double _timeFactor =1;
    public double TimeFactor => _timeFactor;

    private const double _densityFactor =1;
    public double DensityFactor => _densityFactor;

     private const double _velocityFactor =100; 
    public double VelocityFactor => DistanceFactor/TimeFactor;

    [SerializeField] private const double _distanceFactor = 1.00068976040482E7;
    public double DistanceFactor => _distanceFactor;

    public double EnergyFactor => (.5f * MassFactor * VelocityFactor * VelocityFactor);


    [SerializeField] private float _bodiesDefaultInfluenceStrength = 1000;
    public float BodiesDefaultInfluenceStrength => _bodiesDefaultInfluenceStrength;

    [SerializeField] private float _directGravityPullMultiplier = 10;
    public float DirectGravityPullMultiplier => _directGravityPullMultiplier;


    [SerializeField] private const double _gravitationnalPullFactor = 9.98621905187072E16;

    public double GravitationnalPullFactor => _gravitationnalPullFactor;

}

[System.Serializable]
public struct UniverseComposition
{
    public float smallBodyPercentage;
    public float planetoidPercentage;
    public float planetPercentage;
    public float starPercentage;
    public float blackHolePercentage;

    public bool Empty =>
        (smallBodyPercentage + planetPercentage + planetoidPercentage + starPercentage + blackHolePercentage) == 0;
}


public class UniverseManager : MonoBehaviour
{

    private static UniverseManager _instance;


    public static UniverseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UniverseManager>();

                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("AstralBodiesManager");
                    _instance = singletonGO.AddComponent<UniverseManager>();
                }
            }
            return _instance;
        }
    }


    [SerializeField] private PhysicsProperties _physicsProperties = new PhysicsProperties();
    public PhysicsProperties PhysicsProperties { get => _physicsProperties; set => _physicsProperties = value; }

    [SerializeField]
    private GameObject _universeContainer;
    public GameObject UniverseContainer => _universeContainer;

    [Header("Scale")]
    [SerializeField] private float _maxAstralBodyScale = 900;
    public float MaxAstralBodyScale => _maxAstralBodyScale;

    [SerializeField] protected bool _clampScale;
    public bool ClampScale => _clampScale;

    [SerializeField] protected float _maxUniverseScale = 5;
    public float MaxUniverseScale => _maxUniverseScale;

    [SerializeField] protected float _scaleMultiplier = 1.005f;
    public float ScaleMultiplier => _scaleMultiplier;

    private float _initialUniverseScale;

    private float _universeScale => GetUniverseScale();
    public float UniverseScale => _universeScale;



    [Header("Universe Composition")] 
    public UniverseComposition universeComposition = new UniverseComposition();

    public float PlanetPercentage => universeComposition.planetPercentage;

    public float StarPercentage => universeComposition.starPercentage;
    
    public float PlanetoidPercentage => universeComposition.planetoidPercentage;
    
    public float SmallBodyPercentage => universeComposition.smallBodyPercentage;
    
    public float BlackHolePercentage => universeComposition.blackHolePercentage;

    [Header("Gravity Pull")]
    public bool enableGravity;
    [SerializeField] protected float _maxDetectionRange = 1000;
    public float MaxDetectionRange => _maxDetectionRange;

   

    [Header("Generate Random Bodies")]
    public GenerationPrefs generationPrefs = new GenerationPrefs();
    
    public bool generateRandomBodies;
    public int numberOfBodyToGenerate;
    public float spawnZoneMin;
    public float spawnZoneMax;


    public void ScaleUniverseWhenCloseToPlanet(AstralBodyHandler body, float distance) 
    {
        ScaleUniverse(UniverseContainer, ScaleMultiplier/distance, MaxUniverseScale, ClampScale);
    }

    public void ScaleUniverse(GameObject universeContainer, float scaleMultiplier, float maxScale, bool clampScale) 
    {
        if (!universeContainer || scaleMultiplier <= 0) return;

        if (clampScale) 
        {
            if(universeContainer.transform.localScale.x * scaleMultiplier >= maxScale ) universeContainer.transform.localScale = Vector3.one*maxScale;
        } 

        else universeContainer.transform.localScale *= scaleMultiplier;
    }

    private float GetUniverseScale()
    {
        return UniverseContainer ? UniverseContainer.transform.localScale.x : 0f;
    }

    private void Awake()
    {
        _instance = this;
        if(UniverseContainer) _initialUniverseScale = UniverseContainer.transform.localScale.x;
       
    }

    private void OnEnable()
    {
        EventBus.OnAstralBodyProximityRange += ScaleUniverseWhenCloseToPlanet;
        EventBus.OnAstralBodyStartToExist += UpdateInfluenceStrength; 
    }

    private void OnDisable()
    {
        EventBus.OnAstralBodyProximityRange -= ScaleUniverseWhenCloseToPlanet;
        EventBus.OnAstralBodyStartToExist -= UpdateInfluenceStrength;
    }

    private void UpdateInfluenceStrength(AstralBodyHandler obj)
    {
        if(obj.BodyType != AstralBodyType.Fragment)  obj.SetInfluence(PhysicsProperties.BodiesDefaultInfluenceStrength);
        else obj.SetInfluence(PhysicsProperties.BodiesDefaultInfluenceStrength/50);
    }
}
