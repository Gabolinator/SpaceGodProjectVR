using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DinoFracture;
using Script.Physics;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
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
    
    public List<AstralBodyHandler> _allBodies = new();

    private GameObject _universeContainer => UniverseManager.Instance.UniverseContainer;

    
    
    public bool _showDebugLog = true;
    
    private float t;
    [SerializeField] private bool useJobs;

    public bool generateRandomBodies => UniverseManager.Instance.generateRandomBodies;
    public int numberOfBodyToGenerate => UniverseManager.Instance.numberOfBodyToGenerate;
    public float spawnZoneMin => UniverseManager.Instance.spawnZoneMin;
    public float spawnZoneMax=> UniverseManager.Instance.spawnZoneMax;
    
    public float MaxAstralBodyScale => UniverseManager.Instance.MaxAstralBodyScale;

    public Action<AstralBodyHandler> OnBodyDestroyed => EventBus.OnAstralBodyDestroyed;

   

    public string GenerateName() => BodyGenerator.Instance.GenerateRandomName();

    public GeneratedBody GeneratedBodyFromCharacteristic(AstralBody body) => BodyGenerator.Instance.GeneratedBodyFromCharacteristic(body);

    public AstralBody GeneratedBodyFromCharacteristic(AstralBodyType bodyType) =>BodyGenerator.Instance.GenerateBodyPhysicalProperties( bodyType);

    public void GenerateRandomBodies(int numberOfBody, bool randomSpawnPoint = true, bool randomVelocity = true,
        bool randomAngularVelocity = true) => BodyGenerator.Instance.GenerateRandomBodies(numberOfBody, randomSpawnPoint,
        randomVelocity, randomAngularVelocity);
    public void GenerateRandomBodies(GenerationPrefs generationPrefs, int numberOfBody) => BodyGenerator.Instance.GenerateRandomBodies(numberOfBody, generationPrefs);
    
    #region Utils
    private Vector3 GenerateRandomSpawnPoint(float min, float max) =>BodyGenerator.Instance.GenerateRandomSpawnPoint(min, max);
    
    private bool IsWithinDistance(Vector3 point, Vector3 center, float radius)
    {
        float distance = Vector3.Distance(point, center);
        return distance <= radius;
    }

    private float GetVelocity(AstralBodyHandler newSatellite)
    {
        return newSatellite.Velocity.magnitude;
    }

    private float GetDistance(AstralBodyHandler body, AstralBodyHandler otherBody)
    {
        return Vector3.Distance(body.transform.position, otherBody.transform.position);
    }
    #endregion
    
    #region Satellites

    private void ReleaseSatellites(AstralBodyHandler body)
    {
        if(!body.HasSatellite) return;
        foreach (var satellite in body.Satellites)
        {
            satellite.StopOrbiting();
            satellite.ResetOrbitingData();
        }
       
    }

    private void ReleaseSatellite(AstralBodyHandler body, AstralBodyHandler satellite)
    {
        if(!body.HasSatellite) return;
        if (!body.Satellites.Contains(satellite)) return;
        
       
        satellite.ResetOrbitingData();
    }


    private void CaptureSatellite(AstralBodyHandler body, AstralBodyHandler newSatellite) =>
        body.CaptureSatellite(newSatellite, GetDistance(body, newSatellite), GetVelocity(newSatellite));
    


    private void EvaluateIfBecomesSatellite(AstralBodyHandler body, List<AstralBodyHandler> possibleCenterOfRotation, float maxDistance )
    {
        
        //todo doesnt work 
       if(possibleCenterOfRotation.Count == 0) return;
       
       foreach (var possibleCenter in possibleCenterOfRotation)
       {
          float distance = GetDistance(body, possibleCenter);
          if(distance > maxDistance) continue;

          if (IsAlreadyOrbitingBody(body, possibleCenter) && !CouldBecomeSatellite(possibleCenter, body, distance,  .5f*distance))
          {
           Debug.Log(body + " Starts to orbit :" + possibleCenter);
             // ReleaseSatellite(possibleCenter, body); //we were orbiting but not anymore
          
          }

              
          else if (!IsAlreadyOrbitingBody(body, possibleCenter) && CouldBecomeSatellite(possibleCenter, body, distance,   .5f*distance))
          {
              Debug.Log(body + " Stops to orbit :" + possibleCenter);
             // CaptureSatellite(possibleCenter, body);
          }
         
       }

    }

    private bool IsAlreadyOrbitingBody(AstralBodyHandler body, AstralBodyHandler possibleCenter)
    {
        return (body.IsSatellite && body.CenterOfRotation == possibleCenter);
    }

    private bool CouldBecomeSatellite(AstralBodyHandler body, AstralBodyHandler possibleSatellite, float distance, float buffer)
    {
        
        
        float velocity = FormulaLibrary.DetermineOrbitingVelocity(body, distance);
        
        float currentVelocity = GetVelocity(possibleSatellite);
        
        return (currentVelocity >= velocity - buffer && currentVelocity <= velocity + buffer);
        
    }
    
    #endregion   
    
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
        //ReleaseSatellites(body);
       // body.StopOrbiting();
        
        Destroy(body.gameObject);
    }

    public void DestroyAllBodies()
    {
        if(_allBodies.Count ==0) return;
        var cachedList = new List<AstralBodyHandler>();
        cachedList.AddRange(_allBodies);
        
        foreach (var body in cachedList)
        {
            DestroyBody(body);
        }
    }
    private IEnumerator DestroyBodyCoroutine(AstralBodyHandler body, float delay)
    {
        yield return new WaitForSeconds(delay);
        var collider = body.gameObject.GetComponent<Collider>();
        if(collider) collider.enabled = false;
        DestroyBody(body);
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
        
        AddGravityPullToBody(bodyHandler);

        bodyHandler.IsInView = IsInView(bodyHandler);

       bodyHandler.IsWithinDistance = IsWithinDistance(bodyHandler.transform.position, Camera.main.transform.position, 100);

        //EvaluateIfBecomesSatellite(bodyHandler, _allBodies, 100);

    }

    private bool IsInView(AstralBodyHandler bodyHandler) =>
        IsObjectInCameraFrustum(bodyHandler.gameObject, Camera.main);
  
    
    bool IsObjectInCameraFrustum(GameObject obj, Camera camera)
    {
        if (obj == null || camera == null)
        {
            Debug.LogWarning("Object or main camera is null.");
            return false;
        }

        Renderer objectRenderer = obj.GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogWarning("Object doesn't have a renderer.");
            return false;
        }

        Bounds objectBounds = objectRenderer.bounds;

        // Check if the object's bounds intersect with the camera's frustum planes
        bool isIntersecting = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), objectBounds);

        return isIntersecting;
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
 
        var totalForceOnObject = bodyHandler.totalForceOnObject;
        
        if (bodyHandler.thisRb && bodyHandler.EnableGravity && totalForceOnObject != Vector3.zero)
        {
            var ratioMass =
                bodyHandler.Mass /
                bodyHandler.thisRb.mass; // if we lost mass cause of the cast to float , compensate force applied
    
           // Debug.Log("Total force : " + bodyHandler+ " : " +totalForceOnObject);
              bodyHandler.thisRb.AddForce(bodyHandler.totalForceOnObject =
                  ratioMass == 1 ? totalForceOnObject : totalForceOnObject / (float)ratioMass);
        }
        
    }


    private void CalculateGravityPulls(List<AstralBodyHandler> bodiesInRange, bool usingJobs)
    {
        if( bodiesInRange.Count == 0) return;
        
        if(usingJobs) CalculateGravityPullToBodyJob(bodiesInRange);



        else
        {
            foreach (var body in bodiesInRange)
            {
                CalculateGravityPullToBody(bodiesInRange, body);
            }
        }

       
        

    }

    private void CalculateGravityPullToBody(List<AstralBodyHandler> bodies, AstralBodyHandler bodyHandler)
    {
        if(bodies.Count == 0) return;
      //  float range = bodyHandler.InfluenceRange < 0 ? bodyHandler.MaxDetectionRange : bodyHandler.InfluenceRange;
        
        var allBodiesInRange =bodies;
        
        //allBodiesInRange = GetAllBodyInRange(range, allBodiesInRange, bodyHandler);
        
        var totalForceOnObject =
            CalculateTotalGravityPull(allBodiesInRange, bodyHandler, bodyHandler.transform.position)* UniverseManager.Instance.PhysicsProperties.DirectGravityPullMultiplier;
        
        bodyHandler.totalForceOnObject = totalForceOnObject;
//        Debug.Log("Total force : " + bodyHandler+ " : " +totalForceOnObject);
       // bodyHandler.totalForceOnObject = totalForceOnObject;

    }

    
    private void CalculateGravityPullToBodyJob(List<AstralBodyHandler> bodies)
    {
     
        /*Using job */
      
        if(bodies.Count == 0) return;
        
        var allBodiesInRange = bodies;

        int numberOfBodies = allBodiesInRange.Count;

        NativeArray<Vector3> positions = new NativeArray<Vector3>(numberOfBodies, Allocator.TempJob);
        NativeArray<float> masses = new NativeArray<float>(numberOfBodies, Allocator.TempJob);
        NativeArray<Vector3> totalForces = new NativeArray<Vector3>(numberOfBodies, Allocator.TempJob);
        NativeArray<float> influencesStrengths = new NativeArray<float>(numberOfBodies, Allocator.TempJob);
        
        for (int i = 0; i < numberOfBodies; i++)
        {
          //  Debug.Log("Job: " + i);
            positions[i] = allBodiesInRange[i].transform.position;
            masses[i] = (float)allBodiesInRange[i].Mass;
            influencesStrengths[i] = allBodiesInRange[i].InfluenceStrength;
        }

        GravityCalculationJob gravityJob = new GravityCalculationJob
        {
            positions = positions,
            masses = masses,
            totalForces = totalForces,
            influenceStrength = influencesStrengths,
            directGravityMultiplier = UniverseManager.Instance.PhysicsProperties.DirectGravityPullMultiplier,
            gConstant = UniverseManager.Instance.PhysicsProperties.GravitationnalConstant* UniverseManager.Instance.PhysicsProperties.GravitationnalConstantFactor
        };

        JobHandle jobHandle = gravityJob.Schedule(numberOfBodies, 64); // Adjust the batch size (64 is an example)
        jobHandle.Complete();
      
        
        for (int i = 0; i < numberOfBodies; i++)
        { 
            allBodiesInRange[i].totalForceOnObject = totalForces[i];
          //  Debug.Log("Total force job: " + allBodiesInRange[i] +" : " +totalForces[i]);
        }
        
        positions.Dispose();
        masses.Dispose();
        totalForces.Dispose();
       
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
        _instance = this;
    }

    private void Start()
    {
        
        var generationPrefs = UniverseManager.Instance.generationPrefs;
        if (generateRandomBodies) GenerateRandomBodies(generationPrefs,numberOfBodyToGenerate);


    }

    private void Update()
    {
        CalculateGravityPulls(_allBodies, useJobs);
    }

    private void FixedUpdate()
    {
        ManageBodies(_allBodies);
        
    }


   
}
