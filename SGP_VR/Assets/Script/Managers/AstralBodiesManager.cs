using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DinoFracture;
using UnityEngine;


#region CelestialBodyType
public enum AstralBodyType 
{
    Planet,
    Star,
    Planetoid,
    BlackHole,
    ProtoBody,
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
    private GameObject defaultSphereFragmentPrefab;

    public GameObject fragmentPrefab => defaultSphereFragmentPrefab;

    [SerializeField] private List<AstralBodyPhysicalCharacteristics> _astralBodyCharacteristics = new List<AstralBodyPhysicalCharacteristics>();
    public List<AstralBodyPhysicalCharacteristics> AstralBodyCharacteristics => _astralBodyCharacteristics;


    public List<AstralBodyHandler> _allBodies = new();

    private BodyGenerator _bodyGenerator;

    private GameObject _universeContainer => UniverseManager.Instance.UniverseContainer;

    public bool _showDebugLog = true;
    private float t;

    public bool generateRandomBodies => UniverseManager.Instance.generateRandomBodies;
    public int numberOfBodyToGenerate => UniverseManager.Instance.numberOfBodyToGenerate;
    public float spawnZoneMin => UniverseManager.Instance.spawnZoneMin;
    public float spawnZoneMax=> UniverseManager.Instance.spawnZoneMax;


    public float planetPercentage => UniverseManager.Instance.PlanetPercentage;
    public float starPercentage =>UniverseManager.Instance.StarPercentage;
    public float planetoidPercentage => UniverseManager.Instance.PlanetoidPercentage;
    public float blackHolePercentage => UniverseManager.Instance.BlackHolePercentage;
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

    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 pos)
    {
       


        // var fractureScript = body.GetComponent<RuntimeFracturedGeometry>();
        // if (fractureScript)
        // {
        //     fractureScript.FractureAndForget(pos);
        //     return new List<Rigidbody>();
        // }
        
        List<Rigidbody> allRb = SpawnFragmentSphere(defaultSphereFragmentPrefab, body);
        Debug.Log("[AstralBodyManager] allRB count: " + allRb.Count);

        float breakTorque = body.InternalResistance;
        float breakForce = body.InternalResistance;

   
        UpdateFixedJoint(allRb, breakForce, breakTorque);

        AddAstralBody(allRb, body);

        
        // /*only for debug reason - make sure it make sense - no mass is lost or extra */
        // if (fragHandler)
        // {
        //     double totalVolume = 0;
        //     double totalMass = 0;
        //     if (bodies.Count != 0)
        //     {
        //         foreach (var  bodyHandler in bodies)
        //         {
        //             totalVolume += bodyHandler.Volume;
        //             totalMass += bodyHandler.Mass;
        //         }
        //         fragHandler.volumeBody = body.Volume;
        //         fragHandler.volumeFragment = totalVolume;
        //         Debug.Log("%volume : " + totalVolume / totalVolume);
        //         fragHandler.massBody = body.Mass;
        //         fragHandler.massFragment = totalMass;
        //         Debug.Log("%mass : " + totalMass /body.Mass);
        //     }
        // }
    

    return allRb;
    }

    private void UpdateFixedJoint(List<Rigidbody> allRb, float breakForce, float breakTorque)
    {
        if(allRb.Count == 0) return;

        List<FixedJoint> allFixedJoints = new List<FixedJoint>();
        foreach (var rb in allRb)
        {
            var fixedJoints = rb.GetComponents<FixedJoint>();
            if(fixedJoints.Length == 0) continue;
            
            allFixedJoints.AddRange(fixedJoints.ToArray());
        }

        UpdateFixedJoint(allFixedJoints, breakForce, breakTorque);
    }

    private void UpdateFixedJoint(List<FixedJoint> allFixedJoints, float breakForce, float breakTorque)
    {
        if(allFixedJoints.Count == 0) return;

        foreach (var joint in allFixedJoints)
        {
            joint.breakForce = breakForce;
            joint.breakTorque = breakTorque;
        }
    }

    List<Rigidbody> SpawnFragmentSphere(GameObject prefab, AstralBodyHandler targetBody)
    {
        if(!prefab) return null;
        var target = targetBody.transform;
        
        var prefabClone = Instantiate(prefab,target.position, target.rotation, _universeContainer.transform);
        
        if(!prefabClone) return null;
        
        prefabClone.transform.localScale = target.localScale;
        prefabClone.transform.localScale /= 2;

        prefabClone.gameObject.name = "Fragments_"+targetBody.ID;
        
        var fragmentHandler = prefabClone.GetComponent<FragmentHandler>();
        
        if(!fragmentHandler)   return new List<Rigidbody>(prefabClone.GetComponentsInChildren<Rigidbody>());
        
        
        return fragmentHandler.allRb.Count != 0  ? fragmentHandler.allRb : new List<Rigidbody>(prefabClone.GetComponentsInChildren<Rigidbody>());
    }
    
     private List<AstralBodyHandler> AddAstralBody(List<Rigidbody> allRb, AstralBodyHandler targetBody)
    {
        
        if(allRb.Count == 0) return null;
        List<AstralBodyHandler> allBodyHandlers = new List<AstralBodyHandler>();
        
        foreach (var rb in allRb)
        {
            var bodyHandler = rb.GetComponent<AstralBodyHandler>();
            if(bodyHandler) continue;
            
            
            bodyHandler = rb.gameObject.AddComponent<AstralBodyHandler>();
            bodyHandler.ShouldInitialize = false;
            bodyHandler.EnableCollision = false;
            
            bodyHandler.body = new AstralBody( AstralBodyType.other);
            bodyHandler.EstimateVolume(targetBody.Volume);
            bodyHandler.Density = targetBody.Density;
            bodyHandler.CalculateMass();
            
            bodyHandler.thisRb = rb;
           
            bodyHandler.ID += "_Fragment_Of_"+targetBody.ID;
            bodyHandler.SetBodyName();
            
            allBodyHandlers.Add(bodyHandler);
        }

        return allBodyHandlers;
    }
     
    
    void AddFixedJoints(List<Rigidbody> allRb, float breakForce, float breakTorque)
    {
        if(allRb.Count == 0) return;

        foreach (var rb in allRb)
        {
            var go = rb.gameObject;
            AddFixedJointsToGo(go, allRb, breakForce, breakTorque);
        }
    }

    void AddFixedJointsToGo(GameObject go, List<Rigidbody> allRb, float breakForce,  float breakTorque)
    {
        if(!go || allRb.Count == 0) return;
        foreach (var rb in allRb)
        {
            AddFixedJointToGo(go, rb, breakForce, breakTorque);
        }
    }
    
    void AddFixedJointToGo(GameObject go, Rigidbody otherRb, float breakForce,  float breakTorque)
    {
        if(!go || !otherRb) return;
        if(go.GetComponent<Rigidbody>() == otherRb)return;
        
        FixedJoint fixedJoint = go.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = otherRb;

        fixedJoint.breakForce = breakForce;
        fixedJoint.breakTorque = breakTorque;
    }

    
    
    
    
    
    
    

    private IEnumerator DestroyBodyCoroutine(AstralBodyHandler body, float delay)
    {
        yield return new WaitForSeconds(delay);
        var collider = body.gameObject.GetComponent<Collider>();
        if(collider) collider.enabled = false;
        DestroyBody(body);
    }

    public void GenerateBody(Vector3 spawnPoint, AstralBody body, bool generateRandomPhysicalCharacteristic = false , bool randomVelocity = false, bool randomAngularVelocity = false)
    {

        GeneratedBody generatedBody = _bodyGenerator.GenerateBody(body, generateRandomPhysicalCharacteristic);
        var prefab = generatedBody.prefab;

        if (prefab == null) return;

        GameObject newBodyClone = Instantiate(prefab, spawnPoint, Quaternion.identity, _universeContainer ? _universeContainer.transform : null);

        var astralBody = generatedBody.astralBody;
        
        if (randomVelocity)  astralBody.StartVelocity = GenerateVelocity(-.5f, 0.5f);
        if (randomAngularVelocity) astralBody.StartAngularVelocity = GenerateVelocity(-.5f, 0.5f);
        

        
        var bodyHandler = newBodyClone.GetComponent<AstralBodyHandler>();

        if (bodyHandler == null) return;



        if (astralBody is Planet)
        {
            Planet planet = (Planet)astralBody;
            newBodyClone.GetComponent<AstralBodyHandler>().body = new Planet(planet);
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

    public void GenerateBody(Vector3 position, AstralBody body, bool generateAtRandom, bool generateRandomPhysicalCharacteristic , bool randomVelocity, bool randomAngularVelocity) 
    {
       
        if (generateAtRandom) 
        {
            GenerateRandomBody(position, generateAtRandom, randomVelocity, randomAngularVelocity);
        }
        
        else 
        {
            Debug.Log("[AstralBody Manager] random characteristic" + generateRandomPhysicalCharacteristic);
            GenerateBody(position, body, generateRandomPhysicalCharacteristic, randomVelocity, randomAngularVelocity);
        }
    
    }

    public void GenerateRandomBody(Vector3 spawnPoint, bool randomSpawnPoint = true, bool randomVelocity = true, bool randomAngularVelocity = true, List < Vector3> spawnPoints = null) 
    {
        float delta = 10;
        int maxIteration = 15;

        GeneratedBody generatedBody = _bodyGenerator.GenerateRandomBody();
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
            GenerateRandomBody(spawnPoint,randomSpawnPoint,randomVelocity, randomAngularVelocity, spawnPoints);
        }
    }

    private bool CanSpawnAtPosition(Vector3 spawnPoint, List<Vector3> spawnPoints, float delta, AstralBody astralBody)
    {
        bool canSpawn = false;
        if (spawnPoints.Count == 0) return true;

        canSpawn = !astralBody.PredictCollisionAtPosition(spawnPoint, delta);
       
        return canSpawn;
    }

    private Vector3 GenerateRandomSpawnPoint(float min, float max)
    {
        float randomX = UnityEngine.Random.Range(min, max);
        float randomY = UnityEngine.Random.Range(min, max);
        float randomZ = UnityEngine.Random.Range(min, max);

        return new Vector3(randomX, randomY, randomZ);
    }

    private bool IsWithinDistance(Vector3 point, Vector3 center, float radius)
    {
        float distance = Vector3.Distance(point, center);
        return distance <= radius;
    }

    public void RegisterBody(AstralBodyHandler body) 
    {
        if (!_allBodies.Contains(body))
        {
 
        _allBodies.Add(body);
        
        var count = _allBodies.Count;
        body._delayStart = count % 5;
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

    private void Awake()
    {
      Initialize();
       
        _instance = this;
    }

    private void Start()
    {
        if (generateRandomBodies) GenerateRandomBodies(numberOfBodyToGenerate);


    }

}
