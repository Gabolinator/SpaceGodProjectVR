using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public struct GenerationPrefs
{
    public bool generateEverythingAtRandom;
    public bool randomPhysicalCharacteristic;
    public bool randomVelocity;
    public bool randomAngularVelocity;
    public bool generateSatellites;
    public bool generateRandomNumberOfSatellites;
    public int numberOfSatellites;
    public int minNumberOfSatellites;
    public int maxNumberOfSatellites;
    public bool generateRingsAtRandom;
}

/// <summary>
/// This class will change the object in scene to the specific astral body selected
/// </summary>
public class BodyShapeShifter : MonoBehaviour
{

    [SerializeField] protected AstralBody _body;
    public AstralBody Body { get => _body; set => _body = value; }
        
    
    [ShowIf("@this.Body.BodyType == AstralBodyType.Planet")] [SerializeField] protected PlanetType _planetType = PlanetType.none;
    public PlanetType PltType => _planetType;

    [ShowIf("@this.Body.BodyType == AstralBodyType.Star")] [SerializeField] protected StarType _starType = StarType.none;
    public StarType StrType => _starType;
    [ShowIf("@this.StrType == StarType.MainSequenceStar")] [SerializeField] protected StarSpectralType _starSpectralType = StarSpectralType.none;
    public StarSpectralType SpectralType => _starSpectralType;

    public GenerationPrefs prefs;

    public bool GenerateEverythingAtRandom => prefs.generateEverythingAtRandom;

    public bool RandomPhysicalCharacteristic => prefs.randomPhysicalCharacteristic;
    public bool RandomVelocity => prefs.randomVelocity;
    public bool RandomAngularVelocity => prefs.randomAngularVelocity;

    protected void GenerateBody(Vector3 position, AstralBody body, GenerationPrefs prefs) => AstralBodiesManager.Instance.GenerateBody(position, body,prefs);
    
    protected void GenerateBody(Vector3 position, AstralBody body, bool generateAtRandom, bool generateRandomPhysicalCharacteristic,  bool randomVelocity, bool randomAngularVelocity) => AstralBodiesManager.Instance.GenerateBody(position, body, generateAtRandom, generateRandomPhysicalCharacteristic, randomVelocity, randomAngularVelocity) ;


    protected AstralBody CreateBody(AstralBody body, PlanetType planetType, StarType starType, StarSpectralType starSpectralType) => AstralBodiesManager.Instance.CreateAstralBody(body, planetType, starType,starSpectralType);


    protected void UnregisterSelf(AstralBodyHandler handler) => AstralBodiesManager.Instance.DestroyBody(handler);

    public void Awake()
    {
        gameObject.AddComponent<AstralBodyHandler>();
    }

    public void Start()
    {
       
        GenerateBody(transform.position, CreateBody(Body, PltType, StrType, SpectralType), prefs);
        this.gameObject.SetActive(false);

        var handler = gameObject.GetComponent<AstralBodyHandler>();
        UnregisterSelf(handler);

        
    }

  
   
}
