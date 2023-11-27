using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DinoFracture;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;


#region CelestialBodyType
public enum AstralBodyType 
{
    Planet,
    Star,
    Planetoid,
    BlackHole,
    ProtoBody,
    Fragment,
    SmallBody,
    other,
    Uninitialized
}

public enum PlanetType
{
    Terrestrial,
    Jovian,
    IceGiants,
    Earthlike,
    none
}

public enum StarType
{
    MainSequenceStar,
    RedGiant,
    Supergiant,
    NeutronStar,
    BrownDwarfs,
    WolfRayetStar,
    none
}

public enum StarSpectralType 
{
    O,
    B,
    A,
    F,
    G,
    H,
    K,
    M,
    none
}

#endregion


[System.Serializable]
public class AstralBodyDictionnary
{
    public GameObject bodyPrefab;
   
    public AstralBodyType bodyType = AstralBodyType.Uninitialized;
    [ShowIf("@this.bodyType != AstralBodyType.Planet && this.bodyType != AstralBodyType.Star ")] public List<Material> bodyMaterials;
}

[System.Serializable]
public class PlanetDictionnary
{
    public List<Material> planetMaterials = new List<Material>(); 
    public PlanetType planetType = PlanetType.none;
}

[System.Serializable]
public class StarDictionnary  
{
    public List<Material> starMaterials = new List<Material>();
    public StarType starType = StarType.none;
    [ShowIf("@this.starType == StarType.MainSequenceStar")]
    public List<MainSequenceStarDictionnary> mainSequenceStar = new List<MainSequenceStarDictionnary>();

}


[System.Serializable]
public class MainSequenceStarDictionnary
{
    public List<Material> maisSequenceStarMaterials = new List<Material>();
    public StarSpectralType spectralType = StarSpectralType.none;


}


/// <summary>
/// Manage Astral body spawning 
/// </summary>
public class AstralBodiesManager : MonoBehaviour
{
    private static AstralBodiesManager _instance;
    public static AstralBodiesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AstralBodiesManager>();

                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("AstralBodiesManager");
                    _instance = singletonGO.AddComponent<AstralBodiesManager>();
                }
            }
            return _instance;
        }
    }

    


    [SerializeField] private List<AstralBodyDictionnary> _astralBodyDictionnary = new List<AstralBodyDictionnary>();
    [SerializeField] private List<PlanetDictionnary> _planetDictionnary = new List<PlanetDictionnary>();
    [SerializeField] private List<StarDictionnary> _starDictionnary = new List<StarDictionnary>();

    [SerializeField]
    private GameObject defaultSphereFragmentPrefab; //Todo: remove 
    
    public GameObject fragmentPrefab => defaultSphereFragmentPrefab;

    [SerializeField] private List<AstralBodyPhysicalCharacteristics> _astralBodyCharacteristics = new List<AstralBodyPhysicalCharacteristics>();
    public List<AstralBodyPhysicalCharacteristics> AstralBodyCharacteristics => _astralBodyCharacteristics;


    public List<AstralBodyHandler> _allBodies = new();

    private BodyGenerator _bodyGenerator;
    public BodyGenerator BodyGen => _bodyGenerator;

    private GameObject _universeContainer => UniverseManager.Instance.UniverseContainer;

    public bool _showDebugLog = true;
    public bool runtimeFracturing;
    
    private float t;

    public bool generateRandomBodies => UniverseManager.Instance.generateRandomBodies;
    public int numberOfBodyToGenerate => UniverseManager.Instance.numberOfBodyToGenerate;
    public float spawnZoneMin => UniverseManager.Instance.spawnZoneMin;
    public float spawnZoneMax=> UniverseManager.Instance.spawnZoneMax;


    // public float PlanetPercentage => UniverseManager.Instance.PlanetPercentage;
    // public float StarPercentage =>UniverseManager.Instance.StarPercentage;
    // public float PlanetoidPercentage => UniverseManager.Instance.PlanetoidPercentage;
    // public float BlackHolePercentage => UniverseManager.Instance.BlackHolePercentage;
    // public float SmallBodyPercentage => UniverseManager.Instance.SmallBodyPercentage;
    public float MaxAstralBodyScale => UniverseManager.Instance.MaxAstralBodyScale;

    public Action<AstralBodyHandler> OnBodyDestroyed => EventBus.OnAstralBodyDestroyed;

   

    public string GenerateName() => _bodyGenerator.GenerateRandomName();

    public GeneratedBody GeneratedBodyFromCharacteristic(AstralBody body) => _bodyGenerator.GeneratedBodyFromCharacteristic(AstralBodyCharacteristics, body);

    public AstralBody GeneratedBodyFromCharacteristic(AstralBodyType bodyType) => _bodyGenerator.GenerateBodyPhysicalProperties( bodyType);

    
    public void Initialize()
    {
        _bodyGenerator = new BodyGenerator(_astralBodyDictionnary, _planetDictionnary, _starDictionnary, _showDebugLog);
    }

    public string PredictSubtypeFromCharacteristic(AstralBody body, AstralBodyType bodyType) => _bodyGenerator.PredictSubtypeFromCharacteristic(AstralBodyCharacteristics, body, bodyType);

    public AstralBodyType PredictBodyTypeFromCharacteristic(AstralBody body) => _bodyGenerator.PredictBodyTypeFromCharacteristic(AstralBodyCharacteristics, body);


    public AstralBody CreateAstralBody(AstralBody body, PlanetType planetType, StarType starType, StarSpectralType starSpectralType) => _bodyGenerator.CreateAstralBody(body, planetType, starType, starSpectralType);

    public Vector3 GenerateVelocity(float min, float max) => _bodyGenerator.GenerateRandomVelocity(min, max);

    public GameObject GenerateBody(AstralBodyType bodyType, Vector3 position, Quaternion rotation) 
    {
        if (bodyType == AstralBodyType.Uninitialized) return null;

        GeneratedBody generatedBody = _bodyGenerator.GenerateBody(bodyType);
        var prefab = generatedBody.prefab;

        if (prefab == null) return null;

        GameObject newBodyClone = Instantiate(prefab, position, rotation, _universeContainer ? _universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;

        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();
        if(!bodyHandler) bodyHandler = newBodyClone.AddComponent<AstralBodyHandler>();

        bodyHandler.body = new AstralBody(astralBody);


        return newBodyClone;
    }

    public void GenerateBody(CollisionData collision)
    {
        Debug.Log("[AstralBodiesManager] Number of Body to generate from collision: " + collision + " :  " + collision._resultingBodies.Count);
        List<AstralBody> bodies = collision._resultingBodies;

        if (bodies.Count == 0) return;

        foreach (AstralBody body in bodies)
        {

            Debug.Log("[AstralBodiesManager] Body to generate : " + body.ID);

            GeneratedBody generatedBody = _bodyGenerator.GenerateBody(body);
            var prefab = generatedBody.prefab;

            if (prefab == null) return;

            

            GameObject newBodyClone = Instantiate(prefab, collision._target.transform.position, Quaternion.identity, _universeContainer ? _universeContainer.transform : null);

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
                DestroyBody(collidingBody._body);
            }
           
        } 
    }

    

    private IEnumerator DestroyBodyCoroutine(AstralBodyHandler body, float delay)
    {
        yield return new WaitForSeconds(delay);
        var collider = body.gameObject.GetComponent<Collider>();
        if(collider) collider.enabled = false;
        DestroyBody(body);
    }

    public AstralBody GenerateRings(AstralBody body)
    {
        //TODO : not implemented yet 
        if (!body.CanHaveRings) return body;
        
        return body;

    }

    public List<AstralBodyHandler> GenerateSatellites(AstralBodyHandler bodyHandler, int number = 1, int nestedLevel = 1)
    {
        //TODO : not fully implemented yet 
        List<AstralBodyHandler> satellites = new List<AstralBodyHandler>();
        
        if (!bodyHandler.body.CanHaveSatellites) return satellites;
        
        /*check how many "nested satellites level" we have - to not have satellite that has a satellite that have a satellite that ... you know*/
        if(ReachedMaxNestedSatellites(bodyHandler, nestedLevel)) return satellites;
        
        /*to control what can be a satellite*/
        UniverseComposition bodyProbability = new UniverseComposition()
        {
            smallBodyPercentage = 100
        };
        
        float radius = 3; //Todo generate radius and velocity so it matches logic
        var velocity = 1;

        int maxNumber = bodyHandler.body.satellitesData.maxNumberOfSatellites < number
            ? bodyHandler.body.satellitesData.maxNumberOfSatellites
            : number;
        
        /*Here to generate the satellites*/
        for (int i = 0; i < maxNumber; i++)
        {
            
            float angle =  UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            var genBody = _bodyGenerator.GenerateRandomBody(bodyProbability);

          
    
            genBody.astralBody.orbitingData = new OrbitingData() 
            {
                isSatellite = true,
                centerOfRotation = bodyHandler,
                distanceFromCenter = radius,
                orbitAngularVelocity = velocity
            };
            
            
            Vector3 position = new Vector3(radius * MathF.Cos(angle), 0 , radius * MathF.Sin(angle));
            
            var newHandler = GenerateBody(genBody , bodyHandler.transform.position + position, false, true);
            newHandler.transform.parent = bodyHandler.transform;
            newHandler.gravityDisabled = true; //Todo to test , dont keep
            
            if(newHandler) satellites.Add(newHandler);

            radius += 2; //Todo to test , dont keep
            velocity += 15;
        }
        return satellites;
    }

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

    public AstralBodyHandler GenerateBody(GeneratedBody generatedBody,Vector3 spawnPoint,  bool randomVelocity = false, bool randomAngularVelocity = false)
    {
        var prefab = generatedBody.prefab;

        if (prefab == null) return null;

        GameObject newBodyClone = Instantiate(prefab, spawnPoint, Quaternion.identity, _universeContainer ? _universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;
        
        if (randomVelocity)  astralBody.StartVelocity = GenerateVelocity(-.5f, 0.5f);
        if (randomAngularVelocity) astralBody.StartAngularVelocity = GenerateVelocity(-.5f, 0.5f);
        

        
        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

        if (bodyHandler == null) return null;



        if (astralBody is Planet)
        {
            Planet planet = (Planet)astralBody;
            bodyHandler.body = new Planet(planet);
            Planet planetToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Planet;
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
        
        bodyHandler.body.SetCanHaveSatellites();
        bodyHandler.Satellites = GenerateSatellites(bodyHandler, 3);

        return newBodyClone.GetComponent<AstralBodyHandler>();

    }

    public AstralBodyHandler GenerateBody(AstralBody body, Vector3 spawnPoint, bool generateRandomPhysicalCharacteristic = false , bool randomVelocity = false, bool randomAngularVelocity = false)
    {
        GeneratedBody generatedBody = _bodyGenerator.GenerateBody(body, generateRandomPhysicalCharacteristic);
        
        return GenerateBody(generatedBody, spawnPoint, randomVelocity, randomAngularVelocity);
    }

    public void GenerateBody(Vector3 position, AstralBody body, GenerationPrefs prefs)
    {
        GenerateBody(position, body, prefs.generateEverythingAtRandom, prefs.randomPhysicalCharacteristic,
            prefs.randomVelocity, prefs.randomAngularVelocity);
    }


    public void GenerateBody(Vector3 position, AstralBody body, bool generateAtRandom, bool generateRandomPhysicalCharacteristic , bool randomVelocity, bool randomAngularVelocity) 
    {
       
        if (generateAtRandom) 
        {
            GenerateRandomBody(UniverseManager.Instance.universeComposition ,position, generateAtRandom, randomVelocity, randomAngularVelocity);
        }
        
        else 
        {
            Debug.Log("[AstralBody Manager] random characteristic" + generateRandomPhysicalCharacteristic);
            GenerateBody(body, position,  generateRandomPhysicalCharacteristic, randomVelocity, randomAngularVelocity);
        }
    
    }

    public void GenerateRandomBody(UniverseComposition composition, Vector3 spawnPoint, bool randomSpawnPoint = true, bool randomVelocity = true, bool randomAngularVelocity = true, List < Vector3> spawnPoints = null) 
    {
        float delta = 10;
        int maxIteration = 15;

        GeneratedBody generatedBody = _bodyGenerator.GenerateRandomBody(composition);
        var prefab = generatedBody.prefab;

        if (prefab == null) return;
        bool reSpawn = false;
       
       
        int j = 0; // j is to track max number of try to set sapwn point , if reach that maybe space is saturated, so stop generating body

        do
        {

            if (randomSpawnPoint) spawnPoint = GenerateRandomSpawnPoint(spawnZoneMin, spawnZoneMax);
            Debug.Log("[AstralBodiesManager] Generating spawn point for : " + generatedBody.astralBody.BodyType.ToString() + " :" + generatedBody.astralBody.ID + " Try # : " + j);

            reSpawn = !CanSpawnAtPosition(spawnPoint, spawnPoints, delta, generatedBody.astralBody);
            j++;

            if (j >= maxIteration)
            {
                Debug.Log("[AstralBodiesManager] Cant spawn body without collision. Generating body : " + generatedBody.astralBody.BodyType.ToString() + " :" + generatedBody.astralBody.ID + "cancelled");
                return;
            }

        } while (reSpawn);
        

        spawnPoints.Add(spawnPoint);
        


        GameObject newBodyClone = Instantiate(prefab, spawnPoint, Quaternion.identity, _universeContainer ? _universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;
       

        if (!randomVelocity) astralBody.StartVelocity = Vector3.zero;
        if (!randomAngularVelocity) astralBody.StartAngularVelocity = Vector3.zero;

        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

        if (bodyHandler == null) return;
        
        if (astralBody is Planet)
        {
            Planet planet = (Planet)astralBody;
            newBodyClone.GetComponent<AstralBodyHandler>().body = new Planet(planet);
            Planet planetToSet = newBodyClone.GetComponent<AstralBodyHandler>().body as Planet;


            if (planetToSet != null) planetToSet.PltType = planet.PltType;
         
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
          
        }

        else
        {
            newBodyClone.GetComponent<AstralBodyHandler>().body = new AstralBody(astralBody);
        }

    }
    
    public void GenerateRandomBodies(int numberOfBody, bool randomSpawnPoint = true, bool randomVelocity = true, bool randomAngularVelocity = true) 
    {
        if (numberOfBody <= 0) return;
        List<Vector3> spawnPoints = new List<Vector3>();
        var spawnPoint = Vector3.zero;

       
        for (int i = 0; i < numberOfBody; i++) 
        {
            GenerateRandomBody(UniverseManager.Instance.universeComposition,spawnPoint,randomSpawnPoint,randomVelocity, randomAngularVelocity, spawnPoints);
        }
    }

    private bool CanSpawnAtPosition(Vector3 spawnPoint, List<Vector3> spawnPoints, float delta, AstralBody astralBody)
    {
        bool canSpawn = false;
        if (spawnPoints.Count == 0) return true;

        canSpawn = !astralBody.PredictCollisionAtPosition(spawnPoint, delta);
       
        return canSpawn;
    }

    private Vector3 GenerateRandomSpawnPoint(float min, float max) => _bodyGenerator.GenerateRandomSpawnPoint(min, max);
    
    private bool IsWithinDistance(Vector3 point, Vector3 center, float radius)
    {
        float distance = Vector3.Distance(point, center);
        return distance <= radius;
    }

    #region Register/Unregister/Destroy

    


    public void RegisterBody(AstralBodyHandler body) 
    {
        if (!_allBodies.Contains(body))
        {
 
        _allBodies.Add(body);
        
        var count = _allBodies.Count;
        body.DelayStart = count % 5;
        }
    }

    public void UnRegisterBody(AstralBodyHandler body)
    {
       if(_allBodies.Contains(body)) _allBodies.Remove(body);
    }
    public void DestroyBody(AstralBodyHandler body, float delay = 0)
    {
        if (body == null) return;

        if (delay == 0)
        {
            DestroyBody(body);
        }

        else StartCoroutine(DestroyBodyCoroutine(body, delay));

    }
    public void DestroyBody(AstralBodyHandler body) 
    {
        if (body == null) return;

        OnBodyDestroyed?.Invoke(body);
        UnRegisterBody(body);
       
        Destroy(body.gameObject);

      

    }
    #endregion
    
    #region Managing bodies( gravity and update)



    public void Orbit(AstralBodyHandler bodyHandler)
    {
        var centerTransform = bodyHandler.CenterOfRotation.transform;
        var radius = bodyHandler.body.orbitingData.distanceFromCenter;
        var rotationSpeed = bodyHandler.body.orbitingData.orbitAngularVelocity;
        
        float angleInRadians = Time.time * Mathf.Deg2Rad * rotationSpeed;
        Vector3 newPosition = new Vector3(
            centerTransform.position.x + radius * Mathf.Cos(angleInRadians),
            centerTransform.transform.position.y,
            centerTransform.position.z + radius * Mathf.Sin(angleInRadians)
        ); //Todo implement rotation axis and oval orbits 

        // Update the object's position
        bodyHandler.transform.position = newPosition;

        // Optionally, you can also rotate the object to face the center
        //transform.LookAt(centerTransform.position);
    }
    

    private IEnumerator CalculateGravityPullCoroutine(float delay)
    {
        
        do
        {
            ManageBodies(_allBodies);
            
            yield return new WaitForSeconds(delay);

        } while (true);
    }


    private void ManageBody(AstralBodyHandler bodyHandler)
    {
        if(!bodyHandler) return;
        
        
        
        bodyHandler.UpdateVelocities(bodyHandler.firstUpdate);
        bodyHandler.firstUpdate = false;
            
        if(bodyHandler.IsSatellite) Orbit(bodyHandler);
        else AddGravityPullToBody(bodyHandler);

        
    }

    private void ManageBodies(List<AstralBodyHandler> bodies)
    {
        
        if(bodies.Count == 0) return;
        
        foreach (var bodyHandler in bodies)
        {
            ManageBody(bodyHandler);
        }
    }

    private void AddGravityPullToBody(AstralBodyHandler bodyHandler)
    {
      
        if(bodyHandler.IsSatellite) return;  //TODO just to test , dont leave that here
        
        float range = bodyHandler.InfluenceRange < 0 ? bodyHandler.MaxDetectionRange : bodyHandler.InfluenceRange;

        var allBodiesInRange = new List<AstralBodyHandler>();

        allBodiesInRange = GetAllBodyInRange(range, allBodiesInRange, bodyHandler);

        var totalForceOnObject =
            CalculateTotalGravityPull(allBodiesInRange, bodyHandler, bodyHandler.transform.position);


        if (bodyHandler.thisRb && bodyHandler.EnableGravity && totalForceOnObject != Vector3.zero)
        {
            var ratioMass =
                bodyHandler.Mass /
                bodyHandler.thisRb.mass; // if we lost mass cause of the cast to float , compensate force applied

            bodyHandler.thisRb.AddForce(bodyHandler.totalForceOnObject =
                ratioMass == 1 ? totalForceOnObject : totalForceOnObject / (float)ratioMass);
        }
        
    }


    public List<AstralBodyHandler> GetAllBodyInRange(float range, List<AstralBodyHandler> listOfBody, AstralBodyHandler bodyHandler)
    {

        listOfBody.Clear();

        Collider[] hitColliders = Physics.OverlapSphere(bodyHandler.transform.position, range);

        if (hitColliders.Length == 0) return listOfBody;

        foreach (Collider hitCollider in hitColliders)
        {
            AstralBodyHandler body = hitCollider.GetComponent<AstralBodyHandler>();

            if (!body || hitCollider.gameObject == bodyHandler.gameObject) continue;

            listOfBody.Add(body);
        }

        return listOfBody;
    }
    
    public Vector3 CalculateTotalGravityPull(List<AstralBodyHandler> listOfBody, AstralBodyHandler body, Vector3 position, float timeStep = 0) =>
        FormulaLibrary.CalculateTotalGravityPull(listOfBody, body, position, timeStep);
    
    #endregion
    
    
    private void Awake()
    {
      Initialize();
       
        _instance = this;
    }

    private void Start()
    {
        if (generateRandomBodies) GenerateRandomBodies(numberOfBodyToGenerate);


    }

    private void FixedUpdate()
    {
        ManageBodies(_allBodies);
        
    }

  
}
