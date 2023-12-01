using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Script.Physics;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// Takes care of the generation of the astral bodies (Spawing, getting right prefab, material, customizing etc)
/// </summary>
/// 



public class GeneratedBody
{
    public AstralBody astralBody;
    public GameObject prefab;

    public GeneratedBody(AstralBody body) 
    {
        astralBody = body;
    }


    public GeneratedBody() { }
}

[System.Serializable]
public class NameGenerator
{
    public bool _showDebugLog = true;
    private static System.Random random = new System.Random();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string digits = "0123456789";

    private string[] commonNames = {
        "Earth", "Mars", "Jupiter", "Venus", "Mercury", "Saturn", "Neptune", "Uranus",
        "Pluto", "Sun", "Moon", "Alpha Centauri", "Sirius", "Betelgeuse", "Polaris",
        "Andromeda", "Orion", "Cassiopeia", "Pleiades", "Hubble", "Chandra", "Kepler"
        // Add more common names as needed
    };
    
    
    
     public string GenerateRandomName()
    {
        int nameLength = random.Next(5, 11); // Generates a random length between 5 and 10 characters
        StringBuilder nameBuilder = new StringBuilder();

        if (random.Next(3) == 0)
        {
            int randomIndex = random.Next(commonNames.Length);
            string commonName = commonNames[randomIndex];
            nameBuilder.Append(commonName);

            if (random.Next(2) == 0)
            {
                int suffixLength = random.Next(1, 5); // Generates a random suffix length between 1 and 4 characters
                for (int i = 0; i < suffixLength; i++)
                {
                    if(i == random.Next(2)) nameBuilder.Append("-");


                    if (i < 3 && random.Next(2) == 0)
                    {
                        char randomDigit = digits[random.Next(digits.Length)];
                        nameBuilder.Append(randomDigit);
                    }
                    else
                    {
                        char randomChar = chars[random.Next(chars.Length)];
                        nameBuilder.Append(randomChar);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < nameLength; i++)
            {
                if (i < nameLength - 1 && i < 3 && random.Next(2) == 0)
                {
                    char randomDigit = digits[random.Next(digits.Length)];
                    nameBuilder.Append(randomDigit);
                }
                else
                {
                    char randomChar = chars[random.Next(chars.Length)];
                    nameBuilder.Append(randomChar);
                }

                // Add a '-' special character in the middle of the name
                if (i == random.Next(3,5))
                {
                    nameBuilder.Append("-");
                }
            }
        }

        if (_showDebugLog) Debug.Log("[Body Generator] Generated ID : " + nameBuilder.ToString());

        
        
        return nameBuilder.ToString();

    }

     public void LoadBodyNameData()
     {
         throw new NotImplementedException();
     }

     public void SaveBodyNameData()
     {
         throw new NotImplementedException();
     }
     

}


public class BodyGenerator : MonoBehaviour
{

    private static BodyGenerator _instance; 
    public static BodyGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BodyGenerator>();

                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("BodyGenerator");
                    _instance = singletonGO.AddComponent<BodyGenerator>();
                }
            }
            return _instance;
        }
    }
    
    private static System.Random random = new System.Random();
    
    public NameGenerator nameGenerator = new NameGenerator();
    
    [SerializeField] private List<AstralBodyDictionnary> _astralBodyDictionnary = new List<AstralBodyDictionnary>();
    [SerializeField] private List<PlanetDictionnary> _planetDictionnary = new List<PlanetDictionnary>();
    [SerializeField] private List<StarDictionnary> _starDictionnary = new List<StarDictionnary>();
    
    [SerializeField] private List<AstralBodyPhysicalCharacteristics> _astralBodyCharacteristics = new List<AstralBodyPhysicalCharacteristics>();
    public List<AstralBodyPhysicalCharacteristics> AstralBodyCharacteristics => _astralBodyCharacteristics;
    
    public bool _showDebugLog = true;
    
    public string GenerateRandomName() => nameGenerator.GenerateRandomName();

  
    #region BodyType / Subtype
    
    public string PredictSubtypeFromCharacteristic(AstralBody body, AstralBodyType bodyType) =>PredictSubtypeFromCharacteristic(AstralBodyCharacteristics, body, bodyType);

    public AstralBodyType PredictBodyTypeFromCharacteristic(AstralBody body) => PredictBodyTypeFromCharacteristic(AstralBodyCharacteristics, body);

    
    public string PredictSubtypeFromCharacteristic(List<AstralBodyPhysicalCharacteristics> astralBodyCharacteristics, AstralBody body, AstralBodyType bodyType)
    {
        //TODO : implement
        throw new NotImplementedException();
    }


    private AstralBodyType GetBodyTypeFromPercentage(float percentage , UniverseComposition universeComposition)
    {
        Debug.Log("[Body Generator ] Percentage is : " + percentage);

        float planetPercentage = universeComposition.planetPercentage;
        float starPercentage = universeComposition.starPercentage;
        float planetoidPercentage = universeComposition.planetoidPercentage;
        float blackHolePercentage = universeComposition.blackHolePercentage;
        float smallBodyPercentage =universeComposition.smallBodyPercentage;

        if (percentage < planetPercentage)
        {
            return AstralBodyType.Planet;
        }
        else if (percentage < planetPercentage + starPercentage)
        {
            return AstralBodyType.Star;
        }
        else if (percentage < planetPercentage + starPercentage + planetoidPercentage)
        {
            return AstralBodyType.Planetoid;
        }
        else if (percentage < planetPercentage + starPercentage + planetoidPercentage + blackHolePercentage)
        {
            return AstralBodyType.BlackHole;
        }
        
        else if(percentage < planetPercentage + starPercentage + planetoidPercentage + blackHolePercentage + smallBodyPercentage)
        {
            return AstralBodyType.SmallBody;
        }
            
        else
        {
            return AstralBodyType.other;
        }
    }
    #endregion

    #region Utils

    private T GetRandomEnumValue<T>()
    {
        System.Array values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    
    #endregion
    
    #region Get Prefab/Material etc

    public GameObject GetBodyPrefab(AstralBodyType bodyType)
    {
        if (_astralBodyDictionnary.Count == 0) return null;
        if (_showDebugLog) Debug.Log("[Astral Body Manager] Getting right prefab  : " + bodyType);

        List<GameObject> prefabList = new List<GameObject>();

        foreach (var body in _astralBodyDictionnary)
        {
            if (body == null) continue;
            if (body.bodyType == bodyType) prefabList.Add(body.bodyPrefab);
        }

        if (prefabList.Count == 0) return null;

        if (_showDebugLog && prefabList.Count >1 ) Debug.Log("[Astral Body Manager] More than one prefab of type : "+bodyType+ " found , choosing at random ");

        return prefabList.Count == 1 ? prefabList[0] : prefabList[random.Next(prefabList.Count - 1)];
    }

    private Material GetMaterial(AstralBody body)
    {

        Material material = null;
      
        
        if (body is Planet) 
        {
            Planet planet = body as Planet;
            material = GetMaterial(_planetDictionnary, planet.PltType);
            if (_showDebugLog) Debug.Log("[Body Generator] Looking For planet Material for specific planet type :" + planet.PltType);
        }

        if (body is Star) 
        {
            Star star = body as Star;
            material = GetMaterial(_starDictionnary, star.StrType);
        }

        if (material == null) material = GetMaterial(_astralBodyDictionnary, body.BodyType);

        if (material == null) if (_showDebugLog) Debug.Log("[Body Generator] No material found for :" + body.ID +" of type : " + body.BodyType);
        else if (_showDebugLog) Debug.Log("[Body Generator] Material :" + material+ " found for specific body ("+body.BodyType+"):" + body.ID);

        return material;
    }

    private Material GetMaterial(List<AstralBodyDictionnary> bodyDictionary,AstralBodyType bodyType)
    {
        Material material = null;
        List<Material> materialList = new List<Material>();
        var dictionary = bodyDictionary;

        if (dictionary.Count == 0) return material;

        foreach (var obj in dictionary)
        {
            if (obj == null) continue;
            if (obj.bodyType == bodyType) materialList = obj.bodyMaterials;
            material = ChooseMaterial(materialList);
        }

        return material;
    }

    private Material GetMaterial(List<PlanetDictionnary> planetDictionary, PlanetType planetType)
    {
        Material material = null;
        List<Material> materialList = new List<Material>();
        var dictionary = planetDictionary;

        if (dictionary.Count == 0) return material;
        if (dictionary.Count == 1) materialList = dictionary[0].planetMaterials;

        foreach (var obj in dictionary) 
        {
            if (obj == null) continue;
            if (obj.planetType == planetType)
            {
                materialList.AddRange(obj.planetMaterials);
            }
        }
        material = ChooseMaterial(materialList);
        
        
        return material;

    }

    private Material GetMaterial(List<StarDictionnary> starDictionary, StarType starType , StarSpectralType spectralType = StarSpectralType.none)
    {
        Material material = null;
        List<Material> materialList = new List<Material>();
        var dictionary = starDictionary;

        if (dictionary.Count == 0) return material;
        if (dictionary.Count == 1) materialList = dictionary[0].starMaterials;
       
        foreach (var obj in dictionary)
        {
            if (obj == null) continue;
            if (obj.starType == starType)
            {
                if (obj.starType != StarType.MainSequenceStar) materialList.AddRange(obj.starMaterials);
                else 
                {
                    if (obj.mainSequenceStar.Count > 0)
                    {
                        foreach (var mainSequence in obj.mainSequenceStar) 
                        {
                            if (mainSequence.spectralType == spectralType)  materialList.AddRange(mainSequence.maisSequenceStarMaterials);
                        }
                    }    
                }
            }
           
        }
        material = ChooseMaterial(materialList);


        return material;

    }

    private Material ChooseMaterial(List<Material> materialList, bool generateRandom = true)
    {
        if (materialList.Count == 0) return null;


        int randomIndex = generateRandom ? random.Next(materialList.Count - 1) : 0;
        if (_showDebugLog) Debug.Log("[Body Generator]  Random Material at index:" + randomIndex + " : " + materialList[randomIndex]);


        return materialList[randomIndex];
    }

    #endregion
    
    #region Generate Rings

    

    
    
    public AstralBody GenerateRings(AstralBody body)
    {
        //TODO : not implemented yet 
        if (!body.CanHaveRings) return body;
        
        return body;

    }

    #endregion
    
    #region Generate Satellites

    public int DetermineMaxNumberOfSatellites(AstralBody astralBody)
    {
        return 3; //TODO for now - eventually check with mass and other criteria 
    }

    public SatellitesData CreateSatelliteData(GenerationPrefs pref, AstralBody astralBody)
    {
        if (astralBody.satellitesData.initialized) return astralBody.satellitesData;

        
        Debug.Log("Creating satellite data");
        
        int min = pref.minNumberOfSatellites;
        int max = (astralBody.BodyType == AstralBodyType.Fragment || astralBody.BodyType == AstralBodyType.SmallBody)
            ? 0
            : pref.maxNumberOfSatellites;

        int number = pref.generateRandomNumberOfSatellites
            ? UnityEngine.Random.Range(min, max)
            : pref.numberOfSatellites;

        var satData = new SatellitesData()
        {
            maxNumberOfSatellites = number,
            canHaveSatellites = pref.generateSatellites && number != 0,
            initialized = true
        };

        return satData;
    }

    public SatellitesData CreateSatelliteData(AstralBody astralBody)
    {
        if (astralBody.satellitesData.initialized) return astralBody.satellitesData;
        Debug.Log("Creating Sat Data");
        int min = 0;
        int max = DetermineMaxNumberOfSatellites(astralBody);

        int number = UnityEngine.Random.Range(min, max);
        
        var satData = new SatellitesData()
        {
            maxNumberOfSatellites = number,
            canHaveSatellites = number != 0,
            initialized = true,
        };

        return satData;
    }

    public List<AstralBodyHandler> GenerateSatellites(AstralBodyHandler bodyHandler, int nestedLevel = 1) =>
        GenerateSatellites(bodyHandler,bodyHandler.body.satellitesData.maxNumberOfSatellites, nestedLevel);
    
    public List<AstralBodyHandler> GenerateSatellites(AstralBodyHandler bodyHandler, int number, int nestedLevel = 1)
    {
        //TODO : not fully implemented yet 

        Debug.Log("[BodyGenerator] Generating satellites");
        
        List<AstralBodyHandler> satellites = new List<AstralBodyHandler>();
        
        if (!bodyHandler.body.CanHaveSatellites) return satellites;
        
        /*check how many "nested satellites level" we have - to not have satellite that has a satellite that have a satellite that has a satellite... you know*/
        if(ReachedMaxNestedSatellites(bodyHandler, nestedLevel)) return satellites;
        
        /*to control what can be a satellite - will depend on body ie a star cant orbit a planet */
        UniverseComposition bodyProbability = GetSatelitteBodyTypeProbability(bodyHandler);
        if (bodyProbability.Empty) return satellites; 
        
        
        float minRadius = (float)bodyHandler.Radius + .5f;
        float maxRadius =10f; 
        
        int maxNumber = bodyHandler.body.satellitesData.maxNumberOfSatellites < number
            ? bodyHandler.body.satellitesData.maxNumberOfSatellites
            : number;
        
        /*Here to generate the satellites*/
        for (int i = 0; i < maxNumber; i++)
        {
            double rad = Random.Range(minRadius, maxRadius);
            float radius = (float)rad;
            float velocity = DetermineOrbitingVelocity(bodyHandler, rad);
            velocity *= 1.5f;
            velocity *= Random.Range(0, 2) == 0 ? 1 : -1; //clockwise or not 
     
            var genBody = GenerateRandomBody(bodyProbability);
            
            genBody.astralBody.orbitingData = new OrbitingData() 
            {
                isSatellite = true,
                centerOfRotation = bodyHandler,
                distanceFromCenter = radius,
                orbitAngularVelocity = velocity
            };
         
          Vector3 randomPointOnSphere = Random.onUnitSphere * radius;

          // Set the object's position to the random point on the sphere
          var startPosition = randomPointOnSphere + bodyHandler.transform.position;

          // Calculate a velocity vector tangent to the sphere at the chosen point
          Vector3 tangentVelocity = Vector3.Cross(randomPointOnSphere.normalized, Vector3.up).normalized;

          genBody.astralBody.StartVelocity = tangentVelocity*velocity;
            
           // Vector3 position = new Vector3(0, 0 , radius);
            
            var newHandler = GenerateBody(genBody , startPosition, false, true);
            //newHandler.transform.parent = bodyHandler.transform;
            
            if(newHandler) satellites.Add(newHandler);
            
        }
        return satellites;
    }

    private UniverseComposition GetSatelitteBodyTypeProbability(AstralBodyHandler bodyHandler)
    {
        var bodyType = bodyHandler.BodyType;

        switch (bodyType)
        {
            case AstralBodyType.Planet:
                
                return  new UniverseComposition()
                {
                    smallBodyPercentage = 90,
                    planetPercentage = 1,
                    planetoidPercentage = 9,
                };
                
              
            case AstralBodyType.Star:
                return  new UniverseComposition()
                {
                    smallBodyPercentage = 50,
                    planetPercentage = 34,
                    planetoidPercentage = 15,
                    starPercentage = 1
                };
           
            case AstralBodyType.BlackHole:
                return  new UniverseComposition()
                {
                    smallBodyPercentage =30,
                    planetPercentage = 34,
                    planetoidPercentage = 14,
                    starPercentage = 15.9f,
                    blackHolePercentage = .1f
                };
              
            default:
                return  new UniverseComposition();
            
            
        }
        
    }

    private float DetermineOrbitingVelocity(AstralBodyHandler bodyHandler, double radius) => FormulaLibrary.DetermineOrbitingVelocity(bodyHandler,  radius);
   

    private double DetermineOrbitingRadius(AstralBodyHandler bodyHandler, float velocity) =>
        FormulaLibrary.DetermineOrbitingRadius(bodyHandler, velocity);



    private bool ReachedMaxNestedSatellites(AstralBodyHandler bodyHandler, int nestedLevel)
    {
        var parent = bodyHandler;
        int i = 0;
        while (parent.IsSatellite)
        {
            i++;
            parent = parent.CenterOfRotation;
            if (!parent)
            {
             
                Debug.Log("End of satellites loop");   
                break;
                
            }
        }

        return i >= nestedLevel;

    }
    
    public OrbitingData CreateOrbitData(AstralBodyHandler centerBody, AstralBody astralBody, float distanceFromCenter, float velocity)
    {
        var orbitData = new OrbitingData()
        {
            centerOfRotation = centerBody,
            distanceFromCenter = distanceFromCenter,
            orbitAngularVelocity = velocity,
            isSatellite = true
            
        };

        return orbitData;
    }

    #endregion
    
    #region Characteristics

   
    
    internal AstralBodyType PredictBodyTypeFromCharacteristic(List<AstralBodyPhysicalCharacteristics> astralBodyCharacteristics, AstralBody body)
    {
        List<AstralBodyPhysicalCharacteristics> possibleCharacteristics = new List<AstralBodyPhysicalCharacteristics>(astralBodyCharacteristics);



        /*eliminate possibilities based on density*/

        var bodyDensity = body.Density;
        List<AstralBodyPhysicalCharacteristics> possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            if (bodyDensity >= characteristic._minDensity && bodyDensity <= characteristic._maxDensity) continue;

            possibleCharacteristics.Remove(characteristic);
        }


        /*eliminate possibilities based on mass*/
        var bodyMass = body.Mass;
        possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            if (bodyDensity >= characteristic._minMass && bodyDensity <= characteristic._maxMass) continue;

            possibleCharacteristics.Remove(characteristic);
        }

        if (possibleCharacteristics.Count == 0) return AstralBodyType.Uninitialized;

        Debug.Log("Possible Characteristics count " + possibleCharacteristics.Count);

        if (possibleCharacteristics.Count == 1) return possibleCharacteristics[0].bodyType;

        /*if more than one possibility get at random*/
        int randomIndex= UnityEngine.Random.Range((int)0, (int)(possibleCharacteristics.Count-1));

        foreach (var possibleBody in possibleCharacteristics) 
        {
            Debug.Log("possble body :" + possibleBody.bodyType +" " +possibleBody.name);
        }

        return possibleCharacteristics[randomIndex].bodyType;



    }

    
  private AstralBodyPhysicalCharacteristics GetCharacteristics(StarType starType, StarSpectralType starSpectralType)
    {
        var list = AstralBodyCharacteristics;
        foreach (var characteristics in list) 
        {
            if (characteristics.bodyType == AstralBodyType.Star && characteristics.starType == starType) 
            {
                if (starSpectralType == StarSpectralType.none) return characteristics;

                else 
                {
                    if (characteristics.starSpectralType == starSpectralType) return characteristics;
                }
            }
        }

        return null;
    }

    private AstralBodyPhysicalCharacteristics GetCharacteristics(PlanetType planetType)
    {
        var list = AstralBodyCharacteristics;
        foreach (var characteristics in list)
        {
            if (characteristics.bodyType == AstralBodyType.Planet&& characteristics.planetType == planetType)
            {
               return characteristics;
            }
        }

        return null;
    }
    private AstralBodyPhysicalCharacteristics GetCharacteristics(AstralBodyType bodyType)
    {
        var list = AstralBodyCharacteristics;
        foreach (var characteristics in list)
        {
            if (characteristics.bodyType == bodyType)
            {
                return characteristics;
            }
        }

        return null;
    }


  
    
    #endregion

    #region Generate Velocity

    public Vector3 GenerateRandomVelocity(float min, float max)
    {
        Vector3 velocity = new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));

        Debug.Log("[Body Generator] Generated Random Velocity : " + velocity);
        return velocity;
    }



    public Vector3 GenerateVelocity(float min, float max) => GenerateRandomVelocity(min, max);
    
    #endregion
    
    #region Generate AstralBody and GeneratedBody

         public GeneratedBody GeneratedBodyFromCharacteristic(AstralBody body) =>
        GeneratedBodyFromCharacteristic(AstralBodyCharacteristics, body);

         
           private AstralBody GenerateBodyPhysicalProperties(StarType starType, StarSpectralType starSpectralType)
    {

        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(starType, starSpectralType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(10000, 20000, 3000, 5000);

       // double mass = UnityEngine.Random.Range((float)bodyCharacteristics._minMass, (float)bodyCharacteristics._maxMass);
       // double density = UnityEngine.Random.Range((float)bodyCharacteristics._minDensity, (float)bodyCharacteristics._maxDensity);

        BodyPhysicalCharacteristics physicalCharacteristics = bodyCharacteristics.GenerateRandomPhysicalCharacteristics();
        
        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);


        return new Star(physicalCharacteristics, velocity, angularVelocity);
    }


    private AstralBody GenerateBodyPhysicalProperties(PlanetType planetType)
    {
        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(planetType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(1000, 2000, 1000, 2000);
        
       BodyPhysicalCharacteristics physicalCharacteristics = bodyCharacteristics.GenerateRandomPhysicalCharacteristics();

        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);

        var planet = new Planet(physicalCharacteristics, velocity, angularVelocity);

       // Debug.Log("[Body Generator] Velocity : " + velocity);

        return planet;
    }

    public AstralBody GenerateBodyPhysicalProperties(AstralBodyType bodyType)
    {
        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(bodyType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(100,2000,1000,2000);

        BodyPhysicalCharacteristics physicalCharacteristics = bodyCharacteristics.GenerateRandomPhysicalCharacteristics();


        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);
        return new AstralBody(physicalCharacteristics, velocity, angularVelocity);
    }


    private AstralBody GenerateBodyPhysicalProperties(AstralBody body)
    {

        Debug.Log("[Body Generator] Generating Random physical characteristic");
        //AstralBody astralBody = new AstralBody();

        if (body is Planet)
        {
            Debug.Log("[Body Generator] Generating Random physical for planet");
            var planet = body as Planet;

            Planet newPlanet = new Planet(GenerateBodyPhysicalProperties(planet.PltType));

            newPlanet.StartVelocity = body.StartVelocity;
            newPlanet.StartAngularVelocity = body.StartAngularVelocity;
            newPlanet.PltType = planet.PltType;
            newPlanet.BodyType = AstralBodyType.Planet;
            newPlanet.satellitesData = body.satellitesData;
            return newPlanet;
        }

        else if (body is Star)
        {
            Debug.Log("[Body Generator] Generating Random physical for Star");
            var star = body as Star;


            Star newStar = new Star(GenerateBodyPhysicalProperties(star.StrType, star.SpectralType));

            newStar.StartVelocity = body.StartVelocity;
            newStar.StartAngularVelocity = body.StartAngularVelocity;
            newStar.BodyType = AstralBodyType.Star;
            newStar.StrType = star.StrType;
            newStar.SpectralType = star.SpectralType;
            newStar.satellitesData = body.satellitesData;
            return newStar;
        }

        else
        {
            Debug.Log("[Body Generator] Generating Random physical for Other Body : " + body.BodyType);
            AstralBody newAstralBody = GenerateBodyPhysicalProperties(body.BodyType);
            newAstralBody.StartVelocity = body.StartVelocity;
            newAstralBody.StartAngularVelocity = body.StartAngularVelocity;
            newAstralBody.BodyType = body.BodyType;
            newAstralBody.satellitesData = body.satellitesData;
            return newAstralBody;
        }
    }
         
    public GeneratedBody GeneratedBodyFromCharacteristic(List<AstralBodyPhysicalCharacteristics> characteristics, AstralBody body) 
    {
        List< AstralBodyPhysicalCharacteristics > possibleCharacteristics = new List< AstralBodyPhysicalCharacteristics >(characteristics);

        List<GeneratedBody> generatedBodies = new List<GeneratedBody>();

        /*eliminate possibilities based on density*/
      
        var bodyDensity = body.Density;
        List<AstralBodyPhysicalCharacteristics> possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones) 
        {
            if (bodyDensity >= characteristic._minDensity && bodyDensity <= characteristic._maxDensity) continue;
            
            possibleCharacteristics.Remove(characteristic);
        }


        /*eliminate possibilities based on mass*/
        var bodyMass = body.Mass;
        possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            if (bodyDensity >= characteristic._minMass && bodyDensity <= characteristic._maxMass) continue;
            
            possibleCharacteristics.Remove(characteristic);
        }

        /*eliminate possibilities based on composition*/
        //TODO

        if (possibleCharacteristics.Count == 0) return null;

        /*for each possible characteristic get all the planets - and subtype */
        possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            if (characteristic.bodyType != AstralBodyType.Planet) continue;

            var subType = characteristic.planetType;
            Planet possiblePlanet = new Planet(body.Mass, body.Density, Vector3.zero, Vector3.zero, subType);
            
            generatedBodies.Add(new GeneratedBody(possiblePlanet));
            possibleCharacteristics.Remove(characteristic);
        }



        /*for each possible characteristic get all the stars - and subtype */
        possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            if (characteristic.bodyType != AstralBodyType.Star) continue;

            var subType = characteristic.starType;
            var spectraType = characteristic.starSpectralType;

            if (subType == StarType.MainSequenceStar) 
            {
                Star possibleStar = new Star(body.Mass, body.Density, Vector3.zero, Vector3.zero ,spectraType);
                generatedBodies.Add(new GeneratedBody(possibleStar));
                possibleCharacteristics.Remove(characteristic);
            }

            else 
            {
                Star possibleStar = new Star(body.Mass, body.Density, Vector3.zero, Vector3.zero, subType);
                generatedBodies.Add(new GeneratedBody(possibleStar));
                possibleCharacteristics.Remove(characteristic);
            }
        }

        /*for each possible characteristic get all the other type */
        possibleCharacteristicsClones = new List<AstralBodyPhysicalCharacteristics>(possibleCharacteristics);
        foreach (var characteristic in possibleCharacteristicsClones)
        {
            var type = characteristic.bodyType;

            AstralBody possibleBody = new AstralBody(body.Mass, body.Density, Vector3.zero, Vector3.zero, type);

            generatedBodies.Add(new GeneratedBody(possibleBody));
            possibleCharacteristics.Remove(characteristic);
        }

        if (generatedBodies.Count == 0) return null;

        if (generatedBodies.Count == 1) return generatedBodies[0];

        /*if more than one possibility get at random*/
        int randomIndex = 0;//= UnityEngine.Random.Range((int)0, (int)(generatedBodies.Count-1));

        return generatedBodies[randomIndex];
    }

    
    public GeneratedBody GenerateBody(AstralBodyType bodyType, bool generateRandomPhysical = false) 
    {
        AstralBody newBody = new AstralBody(bodyType);
        return GenerateBody(newBody, true);
    }

    public GeneratedBody GenerateBody(AstralBody body, bool generateRandomPhysical = false)
    {
        var defaultPrefab = _astralBodyDictionnary.Count > 0 ? _astralBodyDictionnary[0].bodyPrefab : null;

        var bodyType = body.BodyType;

        if (_showDebugLog) Debug.Log("[Body Generator] Generate Body  : " + bodyType);

        GameObject prefab = GetBodyPrefab(bodyType);
        if (!prefab)
        {
            if (_showDebugLog) Debug.Log("[Body Generator] No prefab found , using default one : " + defaultPrefab);
            prefab = defaultPrefab;
        }

        if (_showDebugLog) Debug.Log("[Body Generator] Prefab found : " + prefab);

        Material material = GetMaterial(body);

        if (material != null) 
        {
            Debug.Log("[Body Generator] Material found : " + material);
            prefab.GetComponent<Renderer>().material = material; 
        }

        else Debug.Log("[Body Generator] No Material found");
        GeneratedBody generateBody = new GeneratedBody();
        generateBody.prefab = prefab;

        Debug.Log("[Body Generator] Generate random body characteristics ? " + generateRandomPhysical);

    
        
        if (generateRandomPhysical) generateBody.astralBody = GenerateBodyPhysicalProperties(body);

        else  generateBody.astralBody = body;

        

        generateBody.astralBody.InternalResistance = body.InternalResistance; //TODO: generate resistance based on body characterisitics 

        Debug.Log("[Body Generator] Velocity : " + body.StartVelocity);
        Debug.Log("[Body Generator] Resistance : " + body.InternalResistance);
        return generateBody;
    }

    public GeneratedBody GenerateRandomBody(UniverseComposition universeComposition)
    {

        AstralBody astralBody = new AstralBody();

        AstralBodyType bodyType = AstralBodyType.other;

        do 
        {
            bodyType = GetBodyTypeFromPercentage(UnityEngine.Random.Range(0, 100), universeComposition);
            

            Debug.Log("[Body Generator] random body type : " + bodyType);
                
        } while (bodyType == AstralBodyType.other);


        if (bodyType == AstralBodyType.Planet)
        {
            PlanetType planetType = PlanetType.none;
            do
            {
                planetType = GetRandomEnumValue<PlanetType>();
            
            } while (planetType == PlanetType.none);

            astralBody = GenerateBodyPhysicalProperties(planetType);
            Planet planet = new Planet(astralBody.Mass, astralBody.Density, astralBody.StartVelocity, astralBody.StartAngularVelocity);
            planet.PltType = planetType;
            planet.BodyType = bodyType;
            Debug.Log("[Body Generator] random planet type : " + planetType);
            Debug.Log("[Body Generator] Velocity : " + planet.StartVelocity);
   
            return GenerateBody(planet);
        }

        else if (bodyType == AstralBodyType.Star)
        {
            StarType starType = StarType.none;
            do {
                starType = GetRandomEnumValue<StarType>();
                
            }while(starType == StarType.none);

            Debug.Log("[Body Generator] random star type : " + starType);
            StarSpectralType starSpectralType = StarSpectralType.none;

            if (starType == StarType.MainSequenceStar)
            {
                do
                {
                    starSpectralType = GetRandomEnumValue<StarSpectralType>();
                    
                } while (starSpectralType == StarSpectralType.none);

                Debug.Log("[Body Generator] random star spectral type : " + starSpectralType);
            }

           

            astralBody = GenerateBodyPhysicalProperties(starType, starSpectralType);
            Star star = new Star(astralBody);
            star.BodyType = bodyType;
            star.StrType = starType;
            star.SpectralType = starSpectralType;
            
            return GenerateBody(star);
        }

        else
        {
            astralBody = GenerateBodyPhysicalProperties(bodyType);
            astralBody.BodyType = bodyType;
            
            return GenerateBody(astralBody);
        }
    }
    
    public AstralBody CreateAstralBody(AstralBody body, PlanetType planetType, StarType starType, StarSpectralType starSpectralType)
    {
        if (body.BodyType == AstralBodyType.Planet)
        {
            if (planetType == PlanetType.none) 
            {
                do
                {
                    planetType = GetRandomEnumValue<PlanetType>();

                }while (planetType == PlanetType.none);
            }

            Planet planet = new Planet(body);
            planet.PltType = planetType;
            
            
            return planet;
        }

        if(body.BodyType == AstralBodyType.Star) {
            if (starType != StarType.none)
            {
                Star star = new Star(body);
                star.StrType = starType;

                if (starType == StarType.MainSequenceStar)
                {
                    if (starSpectralType == StarSpectralType.none)
                    {
                        do
                        {
                            starSpectralType = GetRandomEnumValue<StarSpectralType>();

                        } while (starSpectralType == StarSpectralType.none);
                    }

                    star.SpectralType = starSpectralType;
                }

                return star;
            }
        }
        
        return body;
    }

  
    #endregion
    
    #region Instantiate Body
    
    public AstralBodyHandler GenerateBody(Vector3 position, AstralBody body, GenerationPrefs prefs)
    {
        return GenerateBody(position, body, prefs.generateEverythingAtRandom, prefs.randomPhysicalCharacteristic,
            prefs.randomVelocity, prefs.randomAngularVelocity);
       
    }
    
    public AstralBodyHandler GenerateBody(Vector3 position, AstralBody body, bool generateAtRandom, bool generateRandomPhysicalCharacteristic , bool randomVelocity, bool randomAngularVelocity) 
    {
       
        if (generateAtRandom) 
        {
            return GenerateRandomBody(UniverseManager.Instance.universeComposition ,position, generateAtRandom, randomVelocity, randomAngularVelocity);
        }
        
        else 
        {
            Debug.Log("[AstralBody Manager] random characteristic" + generateRandomPhysicalCharacteristic);
            return GenerateBody(body, position,  generateRandomPhysicalCharacteristic, randomVelocity, randomAngularVelocity);
        }
    
    }

    public AstralBodyHandler GenerateBody(AstralBody body, Vector3 spawnPoint, bool generateRandomPhysicalCharacteristic = false , bool randomVelocity = false, bool randomAngularVelocity = false)
    {
        GeneratedBody generatedBody = GenerateBody(body, generateRandomPhysicalCharacteristic);
        
        return GenerateBody(generatedBody, spawnPoint, randomVelocity, randomAngularVelocity);
    }
    
    
        public AstralBodyHandler GenerateBody(GeneratedBody generatedBody,Vector3 spawnPoint,  bool randomVelocity = false, bool randomAngularVelocity = false)
    {
        var prefab = generatedBody.prefab;

        if (prefab == null) return null;

        GameObject universeContainer = UniverseManager.Instance.UniverseContainer;
        
        GameObject newBodyClone = Instantiate(prefab, spawnPoint, Quaternion.identity, universeContainer ? universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;
        
        if (randomVelocity)  astralBody.StartVelocity = GenerateVelocity(-.5f, 0.5f);
        if (randomAngularVelocity) astralBody.StartAngularVelocity = GenerateVelocity(-.5f, 0.5f);

        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

        if (bodyHandler == null) return null;

        if (astralBody is Planet)
        {
            Planet planet = (Planet)astralBody;
            bodyHandler.body = new Planet(planet);
            Planet planetToSet = bodyHandler.body as Planet;
            Debug.Log("[AstralBodyManager] planet resistance: " + planet.InternalResistance);

            if (planetToSet != null)
            {
                planetToSet.PltType = planet.PltType;
                Debug.Log("[AstralBodyManager] planet to set resistance: " + planetToSet.InternalResistance);
            }

        }

        else if (astralBody is Star)
        {
            Star star = (Star)astralBody;
            bodyHandler.body = new Star(star);
            Star starToSet = bodyHandler.body as Star;
            if (starToSet != null)
            {
                starToSet.StrType = star.StrType;
                starToSet.SpectralType = star.SpectralType;
            }
      
        }

        else
        {
            bodyHandler.body = new AstralBody(astralBody);
        }
        
         bodyHandler.body.CreateSatelliteData(); 
         bodyHandler.Satellites = GenerateSatellites(bodyHandler);

        return bodyHandler;

    }

        public AstralBodyHandler GenerateRandomBody(UniverseComposition composition, Vector3 spawnPoint, GenerationPrefs generationPrefs, List<Vector3> spawnPoints = null)
        {
             float delta = 10;
        int maxIteration = 15;

        GeneratedBody generatedBody = GenerateRandomBody(composition);
        var prefab = generatedBody.prefab;

        if (prefab == null) return null;
        bool reSpawn = false;

        bool randomSpawnPoint = generationPrefs.randomSpawnPoint;
        bool randomVelocity = generationPrefs.randomVelocity;
        bool randomAngularVelocity = generationPrefs.randomAngularVelocity;
       
        int j = 0; // j is to track max number of try to set sapwn point , if reach that maybe space is saturated, so stop generating body

        do
        {
            if (randomSpawnPoint) spawnPoint = GenerateRandomSpawnPoint(UniverseManager.Instance.spawnZoneMin, UniverseManager.Instance.spawnZoneMax);
            Debug.Log("[AstralBodiesManager] Generating spawn point for : " + generatedBody.astralBody.BodyType.ToString() + " :" + generatedBody.astralBody.ID + " Try # : " + j);

            reSpawn = !CanSpawnAtPosition(spawnPoint, spawnPoints, delta, generatedBody.astralBody);
            j++;

            if (j >= maxIteration)
            {
                Debug.Log("[AstralBodiesManager] Cant spawn body without collision. Generating body : " + generatedBody.astralBody.BodyType.ToString() + " :" + generatedBody.astralBody.ID + "cancelled");
                return null;
            }

        } while (reSpawn);
        

        spawnPoints.Add(spawnPoint);
        

        GameObject universeContainer = UniverseManager.Instance.UniverseContainer;
        
        GameObject newBodyClone = Instantiate(prefab, spawnPoint, Quaternion.identity, universeContainer ? universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;
       

        if (!randomVelocity) astralBody.StartVelocity = generationPrefs.startVelocity;
        if (!randomAngularVelocity) astralBody.StartAngularVelocity = generationPrefs.startAngularVelocity;

        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

        if (bodyHandler == null) return null;
        
        if (astralBody is Planet)
        {
            Planet planet = (Planet)astralBody;
            bodyHandler.body = new Planet(planet);
            Planet planetToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Planet;


            if (planetToSet != null) planetToSet.PltType = planet.PltType;
         
        }

        else if (astralBody is Star)
        {
            Star star = (Star)astralBody;
            bodyHandler.body = new Star(star);
            Star starToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Star;
            if (starToSet != null)
            {
                starToSet.StrType = star.StrType;
                starToSet.SpectralType = star.SpectralType;
            }
          
        }

        else
        {
            bodyHandler.body = new AstralBody(astralBody);
        }

        bodyHandler.body.CreateSatelliteData(generationPrefs);
        
        bodyHandler.Satellites = GenerateSatellites(bodyHandler);
        
        return bodyHandler;
        }

        public AstralBodyHandler GenerateRandomBody(UniverseComposition composition, Vector3 spawnPoint, bool randomSpawnPoint = true, bool randomVelocity = true, bool randomAngularVelocity = true, List < Vector3> spawnPoints = null) 
    {
      
        var generationPrefs = new GenerationPrefs()
        {
            randomVelocity = randomVelocity,
            randomAngularVelocity = randomAngularVelocity,
            randomSpawnPoint = randomSpawnPoint,
            generateSatellites = true,
            generateRandomNumberOfSatellites = true
        };
        
        return GenerateRandomBody(composition, spawnPoint, generationPrefs, spawnPoints);

    }

    public List<AstralBodyHandler> GenerateRandomBodies(int numberOfBody, GenerationPrefs generationPrefs)
    {
        if (numberOfBody <= 0) return null;
        List<Vector3> spawnPoints = new List<Vector3>();
        var spawnPoint = Vector3.zero;

        List<AstralBodyHandler> bodyHandlers = new List<AstralBodyHandler>();
       
        for (int i = 0; i < numberOfBody; i++) 
        {
            var newHandler =  GenerateRandomBody(UniverseManager.Instance.universeComposition,spawnPoint,generationPrefs, spawnPoints);
            if(newHandler)  bodyHandlers.Add(newHandler);
        }
        
        return bodyHandlers; 
    }

    public List<AstralBodyHandler> GenerateRandomBodies(int numberOfBody, bool randomSpawnPoint = true, bool randomVelocity = true, bool randomAngularVelocity = true) 
    {
        if (numberOfBody <= 0) return null;
        List<Vector3> spawnPoints = new List<Vector3>();
        var spawnPoint = Vector3.zero;

        List<AstralBodyHandler> bodyHandlers = new List<AstralBodyHandler>();
       
        for (int i = 0; i < numberOfBody; i++) 
        {
           var newHandler =  GenerateRandomBody(UniverseManager.Instance.universeComposition,spawnPoint,randomSpawnPoint,randomVelocity, randomAngularVelocity, spawnPoints);
           if(newHandler)  bodyHandlers.Add(newHandler);
        }
        
        return bodyHandlers; 
    }

    
   

    public AstralBodyHandler GenerateBody(AstralBodyType bodyType, Vector3 position, Quaternion rotation) 
    {
        if (bodyType == AstralBodyType.Uninitialized) return null;

        GeneratedBody generatedBody = GenerateBody(bodyType);
        var prefab = generatedBody.prefab;

        if (prefab == null) return null;

        GameObject universeContainer = UniverseManager.Instance.UniverseContainer;
        GameObject newBodyClone = Instantiate(prefab, position, rotation, universeContainer ? universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;

        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();
        if(!bodyHandler) bodyHandler = newBodyClone.AddComponent<AstralBodyHandler>();

        bodyHandler.body = new AstralBody(astralBody);

        

        return bodyHandler;
    }

    public void GenerateBody(CollisionData collision)
    {
        Debug.Log("[AstralBodiesManager] Number of Body to generate from collision: " + collision + " :  " + collision._resultingBodies.Count);
        List<AstralBody> bodies = collision._resultingBodies;

        if (bodies.Count == 0) return;

        foreach (AstralBody body in bodies)
        {

            Debug.Log("[AstralBodiesManager] Body to generate : " + body.ID);

            GeneratedBody generatedBody =GenerateBody(body);
            var prefab = generatedBody.prefab;

            if (prefab == null) return;

            GameObject universeContainer = UniverseManager.Instance.UniverseContainer;

            GameObject newBodyClone = Instantiate(prefab, collision._target.transform.position, Quaternion.identity, universeContainer ? universeContainer.transform : null);

            var astralBody = generatedBody.astralBody;

            var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

            if (bodyHandler == null) return;

            //newBodyClone.GetComponent<AstralBodyHandler>().body = new AstralBodyInternal(astralBody);


            if (astralBody is Planet)
            {
                Planet planet = (Planet)astralBody;
                newBodyClone.GetComponent<AstralBodyHandler>().body = new Planet(planet);
                Planet planetToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Planet;


                if (planetToSet != null) planetToSet.PltType = planet.PltType;
                //bodyHandler.subType = planet.PltType.ToString();
            }

            else if (astralBody is Star)
            {
                Star star = (Star)astralBody;
                newBodyClone.GetComponent<AstralBodyHandler>().body = new Star(star);
                Star starToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Star;
                if (starToSet != null)
                {
                    starToSet.StrType = star.StrType;
                    starToSet.SpectralType = star.SpectralType;
                }
                //bodyHandler.subType = star.StrType + " " + star.SpectralType;
            }

            else
            {
                newBodyClone.GetComponent<AstralBodyHandler>().body = new AstralBody(astralBody);
            }

            foreach(var collidingBody in collision._collidingBodies) 
            {
                AstralBodiesManager.Instance.DestroyBody(collidingBody._body);
            }
           
        } 
    }

    #endregion
    
    #region SpawnPoint

    public Vector3 GenerateRandomSpawnPoint(float min, float max)
    {
        float randomX = UnityEngine.Random.Range(min, max);
        float randomY = UnityEngine.Random.Range(min, max);
        float randomZ = UnityEngine.Random.Range(min, max);

        return new Vector3(randomX, randomY, randomZ);
    }

   
    private bool CanSpawnAtPosition(Vector3 spawnPoint, List<Vector3> spawnPoints, float delta, AstralBody astralBody)
    {
        bool canSpawn = false;
        if (spawnPoints.Count == 0) return true;

        canSpawn = !astralBody.PredictCollisionAtPosition(spawnPoint, delta);
       
        return canSpawn;
    }
    #endregion

    public void Awake()
    {
        _instance = this;
    }



}
