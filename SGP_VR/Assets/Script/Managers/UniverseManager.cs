using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PhysicsProperties
{
    [SerializeField] private const double _gravitationnalConstant = 6.6743;
   
    public double GravitationnalConstant => _gravitationnalConstant;

    [SerializeField] private const double _gravitationnalConstantFactor = 1E-11;
    public double GravitationnalConstantFactor => _gravitationnalConstantFactor;

    [SerializeField] private const double _massFactor = 1E21;
    public double MassFactor => _massFactor;

    [SerializeField] private const double _timeFactor =1;
    public double TimeFactor => _timeFactor;

    [SerializeField] private const double _densityFactor =1;
    public double DensityFactor => _densityFactor;

    [SerializeField] private const double _velocityFactor =1;
    public double VelocityFactor => _velocityFactor;

    [SerializeField] private const double _distanceFactor = 1.00068976040482E7;
    public double DistanceFactor => _distanceFactor;



    [SerializeField] private float _directGravityPullMultiplier = 1000;
    public float DirectGravityPullMultiplier => _directGravityPullMultiplier;



    [SerializeField] private const double _gravitationnalPullFactor = 9.98621905187072E16;

    public double GravitationnalPullFactor => _gravitationnalPullFactor;

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
    [SerializeField] private float _planetPercentage = 60;
    public float PlanetPercentage => _planetPercentage;

    [SerializeField] private float _starPercentage = 15;
    public float StarPercentage => _starPercentage;

    [SerializeField] private float _planetoidPercentage = 20;
    public float PlanetoidPercentage => _planetoidPercentage;

    [SerializeField] private float _blackHolePercentage = 5;
    public float BlackHolePercentage => _blackHolePercentage;

    [Header("Gravity Pull")]
    public bool enableGravity;
    [SerializeField] protected float _maxDetectionRange = 1000;
    public float MaxDetectionRange => _maxDetectionRange;

   

    [Header("Generate Random Bodies")]
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
        obj.SetInfluence(PhysicsProperties.DirectGravityPullMultiplier);
    }
}
