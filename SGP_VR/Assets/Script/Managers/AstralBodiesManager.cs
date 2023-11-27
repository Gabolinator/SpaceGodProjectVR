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
    
    public List<AstralBodyHandler> _allBodies = new();

    private GameObject _universeContainer => UniverseManager.Instance.UniverseContainer;

    public bool _showDebugLog = true;
    
    private float t;

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
    
    private Vector3 GenerateRandomSpawnPoint(float min, float max) =>BodyGenerator.Instance.GenerateRandomSpawnPoint(min, max);
    
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
