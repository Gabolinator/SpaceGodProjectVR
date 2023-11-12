using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


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

public class BodyGenerator
{
 

    #region NameGeneration
    private static System.Random random = new System.Random();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string digits = "0123456789";

    private string[] commonNames = {
        "Earth", "Mars", "Jupiter", "Venus", "Mercury", "Saturn", "Neptune", "Uranus",
        "Pluto", "Sun", "Moon", "Alpha Centauri", "Sirius", "Betelgeuse", "Polaris",
        "Andromeda", "Orion", "Cassiopeia", "Pleiades", "Hubble", "Chandra", "Kepler"
        // Add more common names as needed
    };
    #endregion

    public bool _showDebugLog = true;

    public List<AstralBodyDictionnary> _astralBodyDictionnary = new();
    public List<PlanetDictionnary> _planetDictionnary = new ();
    public List<StarDictionnary> _starDictionnary = new();


    public BodyGenerator(List<AstralBodyDictionnary> astralBodyDictionnary, List<PlanetDictionnary> planetDictionnary, List<StarDictionnary> starDictionnary, bool showDebugLog)
    {
        _astralBodyDictionnary = astralBodyDictionnary;
        _planetDictionnary = planetDictionnary;
        _starDictionnary = starDictionnary;
        _showDebugLog = showDebugLog;
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


    public string PredictSubtypeFromCharacteristic(List<AstralBodyPhysicalCharacteristics> astralBodyCharacteristics, AstralBody body, AstralBodyType bodyType)
    {
        //TODO : implement
        throw new NotImplementedException();
    }

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

    public GeneratedBody GenerateRandomBody()
    {

        AstralBody astralBody = new AstralBody();

        AstralBodyType bodyType = AstralBodyType.other;

        do 
        {

            bodyType = GetBodyTypeFromPercentage(UnityEngine.Random.Range(0, 100));
            //bodyType = GetRandomEnumValue<AstralBodyType>();

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

    private AstralBodyType GetBodyTypeFromPercentage(float percentage)
    {
        Debug.Log("[Body Generator ] Percentage is : " + percentage);

        float planetPercentage = AstralBodiesManager.Instance.planetPercentage;
        float starPercentage = AstralBodiesManager.Instance.starPercentage;
        float planetoidPercentage = AstralBodiesManager.Instance.planetoidPercentage;
        float blackHolePercentage = AstralBodiesManager.Instance.blackHolePercentage;

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
        else
        {
            return AstralBodyType.other;
        }
    }

  

  

    private AstralBodyPhysicalCharacteristics GetCharacteristics(StarType starType, StarSpectralType starSpectralType)
    {
        var list = AstralBodiesManager.Instance.AstralBodyCharacteristics;
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
        var list = AstralBodiesManager.Instance.AstralBodyCharacteristics;
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
        var list = AstralBodiesManager.Instance.AstralBodyCharacteristics;
        foreach (var characteristics in list)
        {
            if (characteristics.bodyType == bodyType)
            {
                return characteristics;
            }
        }

        return null;
    }


    private AstralBody GenerateBodyPhysicalProperties(StarType starType, StarSpectralType starSpectralType)
    {

        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(starType, starSpectralType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(10000, 20000, 3000, 5000);

        double mass = UnityEngine.Random.Range((float)bodyCharacteristics._minMass, (float)bodyCharacteristics._maxMass);
        double density = UnityEngine.Random.Range((float)bodyCharacteristics._minDensity, (float)bodyCharacteristics._maxDensity);

        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);


        return new Star(mass, density, velocity, angularVelocity);
    }


    private AstralBody GenerateBodyPhysicalProperties(PlanetType planetType)
    {
        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(planetType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(1000, 2000, 1000, 2000);

        double mass = UnityEngine.Random.Range((float)bodyCharacteristics._minMass, (float)bodyCharacteristics._maxMass);
        double density = UnityEngine.Random.Range((float)bodyCharacteristics._minDensity, (float)bodyCharacteristics._maxDensity);


        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);

        var planet = new Planet(mass, density, velocity, angularVelocity);

       // Debug.Log("[Body Generator] Velocity : " + velocity);

        return planet;
    }

    public AstralBody GenerateBodyPhysicalProperties(AstralBodyType bodyType)
    {
        AstralBodyPhysicalCharacteristics bodyCharacteristics = GetCharacteristics(bodyType);

        if (bodyCharacteristics == null) bodyCharacteristics = new AstralBodyPhysicalCharacteristics(100,2000,1000,2000);

        double mass = UnityEngine.Random.Range((float)bodyCharacteristics._minMass, (float)bodyCharacteristics._maxMass);
        double density = UnityEngine.Random.Range((float)bodyCharacteristics._minDensity, (float)bodyCharacteristics._maxDensity);


        Vector3 velocity = GenerateRandomVelocity(-0.5f, 0.5f);
        Vector3 angularVelocity = GenerateRandomVelocity(-0.5f, 0.5f);
        return new AstralBody(mass, density, velocity, angularVelocity);
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
            //Debug.Log("[Body Generator] random planet type : " + planetType);
           // Debug.Log("[Body Generator] Velocity : " + planet.StartVelocity);


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
            return newStar;
        }

        else
        {
            Debug.Log("[Body Generator] Generating Random physical for Other Body : " + body.BodyType);
            AstralBody newAstralBody = GenerateBodyPhysicalProperties(body.BodyType);
            newAstralBody.StartVelocity = body.StartVelocity;
            newAstralBody.StartAngularVelocity = body.StartAngularVelocity;
            newAstralBody.BodyType = body.BodyType;

            return newAstralBody;
        }
    }

    private T GetRandomEnumValue<T>()
    {
        System.Array values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

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

    public Vector3 GenerateRandomVelocity(float min, float max)
    {
        Vector3 velocity = new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));

        Debug.Log("[Body Generator] Generated Random Velocity : " + velocity);
        return velocity;
    }
}
