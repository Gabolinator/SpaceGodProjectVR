using System;
using System.Collections;
using System.Collections.Generic;
using DinoFracture;
using Script.Physics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public enum CollisionType 
{   
    PerfectMerge,
    Disruption,
    SuperCatastrophic,
    HitAndRun,
    Errosion,
    PartialAccretion,
    Unknown
   
}

public enum CollidingBodyRole 
{
    Target,
    Projectile,
    Other
}


[System.Serializable]
public struct CollisionPreferences
{
    public CollisionType collisionType;

    public float energyMultiplier;
    
    public float delay;

    public float impactPointOffset;

    public float lossOfEnergy;

    public double energyThreshold; 
    
    
}

[System.Serializable]
public class CollidingBody 
{
    public AstralBodyHandler _body;
    public CollidingBodyRole _role = CollidingBodyRole.Other;
    public Vector3 _bodyImpactVelocity;
    public Vector3 _bodyImpactAngularVelocity;
    public double _impactEnergy; 
    public Vector3 _impactEnergyDirection;
    public double _impactEnergyAlongNormal; 
   
    public CollidingBody(AstralBodyHandler body, Vector3  collisionNormal,CollidingBodyRole role = CollidingBodyRole.Other) 
    {
        _body = body;
        _role = role;

        _impactEnergyDirection = new Vector3((((float)body.KyneticEnergy * body.Velocity).normalized).x,(((float)body.KyneticEnergy * body.Velocity).normalized).y, (((float)body.KyneticEnergy * body.Velocity).normalized).z);
        
        _bodyImpactVelocity = new Vector3(body.Velocity.x, body.Velocity.y, body.Velocity.z);
        var relativeBodyImpactVelocity = Vector3.Dot(_bodyImpactVelocity, collisionNormal);
        _impactEnergyAlongNormal = .5 * body.Mass * relativeBodyImpactVelocity * relativeBodyImpactVelocity;
        
        _impactEnergy = .5 * body.Mass * _bodyImpactVelocity.magnitude * _bodyImpactVelocity.magnitude;
        
        _impactEnergyDirection = _bodyImpactVelocity.normalized * (float)_impactEnergy;
        
        
    }

}

    /// <summary>
    /// Holds the Data relative to one specific collision between two astral bodies 
    /// </summary>
    [System.Serializable]
public class CollisionData
{
    [System.Serializable]
    public class ImpactParameters
    {
        
        //Todo struct might be better 
        [Header("Energy Parameters")]
        public double impactEnergy;
        public double criticalImpactEnergy; //Critical Impact energy (Qr*)
        public double superCatEnergy; //energy necessary to disperce 90% of mass of target (Qr'*)
        public double erosionEnergy;
        
        [Header("Velocity Parameters")]
        public double impactVelocity;
        public double critImpactVelocity;//energy necessary to disperce half of mass of target
        public double superCatVelocity; //Velocity of SuperCatastrophic energy - eq.
        public double erosionVelocity;
        public double escapeVelocity;
        
        [Header("Mass Parameters")]
        public double interactingMass;
        public double reducedMass;
        public double reducedAlphaMass;

        [Header("Other Parameters")] 
        public double criticalImpactParameter;
        public double impactParameter;
        public ImpactParameters(CollidingBody target, CollidingBody projectile, float impactAngle)
        {
            
            double df = UniverseManager.Instance.PhysicsProperties.DistanceFactor;
            double ef = UniverseManager.Instance.PhysicsProperties.EnergyFactor ;
            ef /= 20000; 
            
            interactingMass = FormulaLibrary.CalculateInteractingMass(projectile, target ,  impactAngle);
            reducedMass = (target._body.Mass * projectile._body.Mass) / (target._body.Mass + projectile._body.Mass);
            reducedAlphaMass =  (target._body.Mass * interactingMass) / (target._body.Mass + interactingMass);
            
            escapeVelocity = FormulaLibrary.CalculateEscapeVelocity(target,projectile , interactingMass)*1000  ;
            
            impactEnergy = FormulaLibrary.CalculateImpactEnergy(target, projectile)/ef;
            impactVelocity = FormulaLibrary.CalculateVelocity(impactEnergy, target, projectile);
            
            criticalImpactEnergy = FormulaLibrary.CalculateCriticalImpactEnergy(target, projectile)/ef;
            critImpactVelocity = FormulaLibrary.CalculateVelocity(criticalImpactEnergy, target, projectile);
         
            superCatEnergy = criticalImpactEnergy* 1.8f;
            superCatVelocity = FormulaLibrary.CalculateVelocity( superCatEnergy, target, projectile);
            
            erosionEnergy = FormulaLibrary.CalculateErosionEnergy(superCatEnergy,target, projectile);
            erosionVelocity =  FormulaLibrary.CalculateVelocity( erosionEnergy, target, projectile);

            criticalImpactParameter = (target._body.Radius / (target._body.Radius + projectile._body.Radius));
            impactParameter = Mathf.Sin(impactAngle);
        }

       

        public ImpactParameters()
        {
            
        }

    }

    public string _id;
    public List<CollidingBody> _collidingBodies = new();
   
    public CollidingBody _targetBody; 
    public CollidingBody _projectileBody;
    public AstralBodyHandler _target => _targetBody._body;
    public AstralBodyHandler _projectile => _projectileBody._body;
    public Vector3 _collisionEnergyDirection; 
    public double _collisionEnergy;
    public Vector3 impactImpulse;
    public ContactPoint _impactPoint;

    public List<AstralBody> _resultingBodies = new();

    public CollisionType _collisionType = CollisionType.Unknown;

    public AstralBodyType _resultingAstralBodyType = AstralBodyType.other;

    

    public CollisionPreferences _collisionPrefs;

    public ImpactParameters _impactParameters;
    
    public float _energyLoss;
    public bool _inProcess = false; 
    bool _showDebugLog = true;
    public bool processed = false;
    
    
    public CollisionData(AstralBodyHandler body1, AstralBodyHandler body2, ContactPoint contactPoint, Vector3 impulse ,float lossOfEnergy, bool showDebug)
    {

        if(!body1 || !body2) return;

        body1.gravityDisabled = true;
        body2.gravityDisabled = true;
        
        var collidingBody1 = new CollidingBody(body1, contactPoint.normal, DetermineCollidingRole(body1, body2));
        var collidingBody2 = new CollidingBody(body2,contactPoint.normal, DetermineCollidingRole(body2, body1));

        _collidingBodies.Add(collidingBody1);
        _collidingBodies.Add(collidingBody2);

        if (_collidingBodies[0]._role == CollidingBodyRole.Target) 
        {
            _targetBody = _collidingBodies[0];
            _projectileBody = _collidingBodies[1];
        }

        else if (_collidingBodies[0]._role == CollidingBodyRole.Projectile) 
        {
            _targetBody = _collidingBodies[1];
            _projectileBody = _collidingBodies[0];
        }

        _impactParameters = new ImpactParameters(_targetBody, _projectileBody, CalculateImpactAngle(_projectileBody, contactPoint.normal ));
        
        _collisionEnergy = _impactParameters.impactEnergy ; //energy used to apply force to rb  
        
        _collisionEnergyDirection = (_targetBody._impactEnergyDirection + _projectileBody._impactEnergyDirection).normalized;

        _energyLoss = lossOfEnergy;
        _collisionEnergy *= 1-_energyLoss;

        impactImpulse = impulse;
         _id = "Collision " + _projectile + " on " + _target;
        _impactPoint = contactPoint;


        _showDebugLog = showDebug;

        if(_showDebugLog) Debug.Log("[CollisionData] New collision data for : " + _id);
    }

    private float CalculateImpactAngle(CollidingBody projectileBody, Vector3 contactPointNormal) =>
        FormulaLibrary.CalculateImpactAngle(projectileBody._bodyImpactVelocity, contactPointNormal);
  
    private CollidingBodyRole DetermineCollidingRole(AstralBodyHandler body, AstralBodyHandler otherBody)
    {
    
        var  _bodyImpactVelocity = new Vector3(body.Velocity.x, body.Velocity.y, body.Velocity.z);
        var  _otherBodyImpactVelocity = new Vector3(otherBody.Velocity.x, otherBody.Velocity.y, otherBody.Velocity.z);
        if (_bodyImpactVelocity.magnitude > _otherBodyImpactVelocity.magnitude) return CollidingBodyRole.Projectile;
        else if (_bodyImpactVelocity.magnitude < _otherBodyImpactVelocity.magnitude) return CollidingBodyRole.Target;

        else return CollidingBodyRole.Other;

    }
}



/// <summary>
/// Takes care of the handling what happens after the collision. 
/// Choosing the collision regime and what happens with each of those regime.
/// </summary>
public class CollisionManager : MonoBehaviour
{
    static CollisionManager _instance;
    public static CollisionManager Instance => _instance;

    public FractureManager _fractureManager = new FractureManager();
    
    [SerializeField]
    private List<CollisionPreferences> _collisionPreferences = new List<CollisionPreferences>();
    
    public AstralBodiesManager _astralBodyManager => AstralBodiesManager.Instance;

    public List<CollisionData> _unProcessedCollisions;
    public List<CollisionData> _processedCollisions;

    public float _processRate = 0.1f;
    public float _lossOfEnergyOnImpact = .15f;

    //public float _explosionEnergyMultiplier = 0.1f;


    public bool testMode = true;
    public CollisionType testCollisionType = CollisionType.PerfectMerge;

    public bool showDebugLog = true;

    public  Action<CollisionData> OnImpact => EventBus.OnCollision;
    public  Action<CollisionData> OnAfterImpact => EventBus.OnCollisionProcessed;

    
    [SerializeField]
    private bool _forceDisableCollisions;

    public bool ForceDisableCollisions
    {
        get => _forceDisableCollisions;
        set => _forceDisableCollisions = value;
    }


    public void CreatingCollision(AstralBodyHandler body1, AstralBodyHandler body2, Collision collision)
    {
        
        if (!body1 || !body2) return;

        CollisionData collisionData = new CollisionData(body1, body2, collision.GetContact(0), collision.impulse,
            _lossOfEnergyOnImpact, showDebugLog);

        if (CollisionAlreadyPresent(collisionData))
        {
            if (showDebugLog) Debug.Log("[Collision Manager] collision already present");
            return;
        }

        collisionData._collisionType = DeterminingCollisionType(collisionData);

        collisionData._collisionPrefs =  GetCollisionPrefs(collisionData._collisionType);
        
        Debug.Log("[Collision Manager] collision energy: " + collisionData._collisionEnergy );
       //ToggleFX(collisionData);
        
        RegisterCollision(_unProcessedCollisions, collisionData);
        
        OnImpact?.Invoke(collisionData);
        
    }

    private CollisionPreferences GetCollisionPrefs(CollisionType collisionType)
    {
        var defaultPref = new CollisionPreferences()
        {
            collisionType = collisionType,
            energyMultiplier = 1f,
            delay = 0.5f,
            impactPointOffset = 0f,
            lossOfEnergy = .6f
        };

        
        if (_collisionPreferences.Count == 0)
            return defaultPref;

        foreach (var pref in _collisionPreferences)
        {
            if (pref.collisionType == collisionType) return pref;
        }

        return defaultPref;
    }

    private CollisionType DeterminingCollisionType(CollisionData collision)
    {
        if (showDebugLog) Debug.Log("[Collision Manager] Determining Collision Type for :" + collision._id);

        var type = CollisionType.Unknown;
        //TODO:  logic to determine collision type here - now just testing 

        if (testMode) return testCollisionType;

        /*SuperCatastrophic*/
        if (collision._impactParameters.impactVelocity > collision._impactParameters.superCatVelocity)
            return CollisionType.SuperCatastrophic;
        
        /*Perfect Merge */
        if(collision._impactParameters.impactVelocity < collision._impactParameters.erosionVelocity) 
           return CollisionType.PerfectMerge;
        
        bool isGrazingImpact = collision._impactParameters.impactParameter > collision._impactParameters.criticalImpactParameter;

        if (isGrazingImpact)
        {
            /*hit and run*/
            if (collision._impactParameters.impactVelocity < collision._impactParameters.erosionVelocity &&
                collision._impactParameters.impactVelocity > collision._impactParameters.escapeVelocity)
                return CollisionType.HitAndRun;


            if (collision._impactParameters.impactVelocity < collision._impactParameters.superCatVelocity &&
                collision._impactParameters.impactVelocity > collision._impactParameters.erosionVelocity)
            {
                if(CalculateLargestRemnantMass(collision._target.Mass+collision._projectile.Mass,collision._impactParameters) < collision._target.Mass) return CollisionType.Disruption;
                
            }

            
        }

        else
        {
            if (collision._impactParameters.impactVelocity < collision._impactParameters.superCatVelocity &&
                collision._impactParameters.impactVelocity > collision._impactParameters.escapeVelocity)
                return CollisionType.Disruption;
            
            if (collision._impactParameters.impactVelocity < collision._impactParameters.erosionVelocity &&
                collision._impactParameters.impactVelocity > collision._impactParameters.escapeVelocity)
                return CollisionType.PartialAccretion;
        }


        /*errosion*/
        if (collision._impactParameters.impactVelocity > collision._impactParameters.erosionVelocity)
            return CollisionType.Errosion;


        if (showDebugLog) Debug.Log("[Collision Manager] Collision Type for :" + collision._id + " is : " + type);
        return type;
    }

    private double CalculateLargestRemnantMass(double totalMass, CollisionData.ImpactParameters impactParameters) =>
        FormulaLibrary.CalculateLargestRemnantMass(totalMass, impactParameters.impactEnergy,
            impactParameters.superCatEnergy);
 

    private List<CollisionType> BasedOnImpactEnergy(CollisionData collision, List<CollisionType> possibleType)
    {
        if (_collisionPreferences.Count == 0) return possibleType;
        var collisionEnergy = collision._collisionEnergy;
        
        
        foreach (var colPref in _collisionPreferences)
        {
            if (collisionEnergy < colPref.energyThreshold)
            {
                if(possibleType.Contains(colPref.collisionType)) possibleType.Remove(colPref.collisionType);
            }
        }

        return possibleType;

    }

    private float GetIncidenceAngle(CollisionData collision)
    {
        var normal = collision._impactPoint.normal;
        var direction = collision._projectileBody._bodyImpactVelocity;
        float angle = Vector3.Angle(normal, direction);

        Debug.Log("[Collision Manager] Impact direction  :" + direction + "/ angle : " + angle);
        return angle;
    }

    private bool CollisionAlreadyPresent(CollisionData collisionData)
    {
        if (_unProcessedCollisions.Count == 0) return false;

        if (_unProcessedCollisions.Contains(collisionData)) return true;

        foreach (CollisionData collision in _unProcessedCollisions)
        {
            if (collision._target == collisionData._target &&
                collision._projectile == collisionData._projectile) return true;
        }

        return false;
    }

    #region ProcessColision

    private IEnumerator ProcessCollisionCoroutine(float delay)
    {
        do
        {
            ProcessCollisions(_unProcessedCollisions);
            yield return new WaitForSeconds(delay);
        } while (true);
    }

    public bool ProcessCollision(CollisionData collision)
    {
        bool processed = false;


        if (collision == null) return processed;
        if (showDebugLog) Debug.Log("[Collision Manager] Processing : " + collision._id);

        if (collision._inProcess) return processed;

        var collisionType = collision._collisionType;

        processed = true;

        switch (collisionType)
        {
            case CollisionType.PerfectMerge:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: PerfectMerge");
                processed = ProcessPerfectMerge(collision);
                break;

            case CollisionType.Disruption:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: Catastrophic");
                processed = ProcessDisruption(collision);
                break;
            case CollisionType.SuperCatastrophic:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: SuperCatastrophic");
                processed = ProcessSuperCatastrophic(collision);
                break;
            case CollisionType.PartialAccretion:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: Grazing");
                processed = ProcessPartialAccretion(collision);
                break;
            case CollisionType.HitAndRun:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: HitAndRun");
                processed = ProcessHitAndRun(collision);
                break;
            
            case CollisionType.Errosion:
                if (showDebugLog) Debug.Log("[Collision Manager] Processing Collision type: Errosion");
                processed = ProcessErrosion(collision);
                break;
            case CollisionType.Unknown:
                if (showDebugLog) Debug.Log("[Collision Manager] Collision type: Unknown, cant process");
                processed = false;
                break;

            default:
                if (showDebugLog) Debug.Log("[Collision Manager] Cant process collision type");
                processed = false;
                break;

        }

        collision.processed = processed;

        //ToggleFX(collision);
        
        OnAfterImpact?.Invoke(collision);
        
        return processed;
    }

    private bool ProcessErrosion(CollisionData collision)
    {
        throw new NotImplementedException();
    }

    public void ProcessCollisions(List<CollisionData> collisionList)
    {
        if (collisionList.Count == 0) return;

        List<CollisionData> collisionListClone = new List<CollisionData>(collisionList);
        int index = 0;

        foreach (var collision in collisionListClone)
        {
            if (ProcessCollision(collision))
            {
                CollisionProcessed(collision);
            }

            index++;
        }
    }

    #endregion

    private void UnregisterCollision(List<CollisionData> collisionList, int index)
    {
        if (collisionList.Count == 0) return;
        if (index < 0 || index > collisionList.Count) return;



        if (collisionList[index] == null) return;

        if (showDebugLog) Debug.Log("[Collision Manager] Removing : " + collisionList[index]._id);
        collisionList.RemoveAt(index);

    }

    private void UnregisterCollision(CollisionData collision, List<CollisionData> collisionList)
    {

        if (collision == null || collisionList.Count == 0) return;

        if (!collisionList.Contains(collision)) return;
        collisionList.Remove(collision);

        //if (collisionList.Count == 0) return;
        //if (index < 0 || index > collisionList.Count) return;



        //if (collisionList[index] == null) return;

        //if (showDebugLog) Debug.Log("[Collision Manager] Removing : " + collisionList[index]._id);
        //collisionList.RemoveAt(index);

    }

    private void RegisterCollision(List<CollisionData> collisions, CollisionData collisionData)
    {
        if (collisions == null || collisionData == null) return;

        if (showDebugLog) Debug.Log("[Collision Manager] Registering Collision :" + collisionData._id);
        collisions.Add(collisionData);
    }

    #region Regimes

    private bool ProcessHitAndRun(CollisionData collision)
    {
        if (collision == null) return false;
        if (showDebugLog) Debug.Log("[Collision Manager] Hit and run regime not yet implemented");
        return true;
    }

    private bool ProcessPartialAccretion(CollisionData collision)
    {
        if (collision == null) return false;
        if (showDebugLog) Debug.Log("[Collision Manager] Grazing regime not yet implemented");
        return true;
    }

    private bool ProcessSuperCatastrophic(CollisionData collision)
    {
        if (collision == null) return false;
        if (showDebugLog) Debug.Log("[Collision Manager] Processing Super Catastrophic Regime");

        var point = collision._impactPoint.point;
        point -= (collision._target.transform.position - point).normalized * collision._collisionPrefs.impactPointOffset;
        
        
        var collisionEnergy = collision._collisionEnergy;
        float energy = (float)collisionEnergy;
        energy *= collision._collisionPrefs.energyMultiplier;


        var fractureLogic = _fractureManager.AssignFractureComponent(collision._target.gameObject);
        if (fractureLogic != null) fractureLogic.FractureBody(collision);

        fractureLogic = _fractureManager.AssignFractureComponent(collision._projectile.gameObject);
        if (fractureLogic != null) fractureLogic.FractureBody(collision);
        
        return true;
    }




    private bool ProcessDisruption(CollisionData collision)
    {
        if (collision == null) return false;
        if (showDebugLog)
            Debug.Log("[Collision Manager] Catastrophic Regime not yet implemented- doing super catastrophic");


        //AstralBody newBody = new AstralBody(collision._target.Mass + collision._projectile.Mass, (collision._target.Density + collision._target.Density)) / 2, collision._target.Velocity + collision._projectile.Velocity);

        return ProcessSuperCatastrophic(collision);
    }

    private bool ProcessPerfectMerge(CollisionData collision)
    {
        if (collision == null) return false;
        if (showDebugLog) Debug.Log("[Collision Manager] Perfect Merge");


        double mass = collision._target.Mass + collision._projectile.Mass;
        double density =
            mass / (collision._target.Volume + collision._projectile.Volume); // TODO not right, fix with ratio

        Vector3 velocity = Mathf.Sqrt(2 * (float)collision._collisionEnergy / (float)mass) *
                           collision._collisionEnergyDirection;
        Vector3 angularVelocity = Vector3.zero;

        AstralBody astralBody =
            CreateNewBody(mass, density, velocity, angularVelocity, DetermineResultingBodyType(collision));

        if (showDebugLog)
            Debug.Log("[Collision Manager] Perfect Merge : resulting Body (" + astralBody.BodyType + ") : " +
                      collision._target.ID);

        collision._resultingBodies.Add(astralBody);

        if (showDebugLog) Debug.Log("[Collision Manager] Resulting Bodies:" + collision._resultingBodies.Count);

        collision._inProcess = true;

        return MergingBodies(collision, !CanPlayerSee());
    }



    public bool MergingBodies(CollisionData collision, bool instant = false)
    {
        /*are we mergin instantly or are we doing it over time */
        if (!instant)
        {
            StartCoroutine(MergingCoroutine(collision, collision._collisionPrefs.delay));
        }
        else MergingBodies(collision);

        return instant;
    }

    public bool MergingBodies(CollisionData collision)
    {
        if (collision._collidingBodies.Count == 0) return false;

        collision._target.thisRb.velocity = collision._targetBody._bodyImpactVelocity;
        collision._projectile.gravityDisabled = true;

        var velocityLoss = MathF.Sqrt(collision._energyLoss / (float)collision._projectile.Mass);

        StartCoroutine(collision._target.LerpToBodyOverTimeCoroutine(collision._resultingBodies[0], collision._collisionPrefs.delay, false));
        
        var meshDeformer = collision._target.GetComponent<MeshDeformer>();
        var point = collision._impactPoint.point;
        point -= (collision._target.transform.position - point).normalized * collision._collisionPrefs.impactPointOffset;
        
        if (meshDeformer) StartCoroutine(meshDeformer.AddDeformingForceCoroutine(collision._collisionPrefs.delay,.01f,point, (float)collision._collisionEnergy*.1f));
        
        AstralBodiesManager.Instance.DestroyBody(collision._projectile);

        return true;
    }

    private IEnumerator MergingCoroutine(CollisionData collision, float delay)
    {

        if (collision._collidingBodies.Count == 0) yield return null;

        foreach (var collidingBody in collision._collidingBodies)
        {
            var collider = collidingBody._body.gameObject.GetComponent<Collider>();
            if (collider)
            {
                collider.isTrigger = true;
                collider.enabled = false;
            }
        }

        collision._target.thisRb.velocity = collision._targetBody._bodyImpactVelocity;
        collision._projectile.thisRb.velocity = collision._projectileBody._bodyImpactVelocity;
        collision._projectile.gravityDisabled = true;
        var visualIndicator = collision._projectile.GetComponent<VisualIndicatorHandler>();
        if(visualIndicator) visualIndicator.forceDisableTrail = visualIndicator.forceDisableTrajectory = true;

        var velocityLoss = MathF.Sqrt(collision._energyLoss / (float)collision._projectile.Mass);
        float velocityMagnitude = collision._projectileBody._bodyImpactVelocity.magnitude / 200 * (1 - velocityLoss);

        collision._projectile.thisRb.isKinematic = true;

        collision._projectile.transform.parent = collision._target.transform;
        collision._projectile.transform.localRotation = Quaternion.identity;


        var meshDeformer = collision._target.GetComponent<MeshDeformer>();
        var point = collision._impactPoint.point;
        point -= (collision._target.transform.position - point).normalized * collision._collisionPrefs.impactPointOffset;

        var drag = 0;
        float t = 0;
        do
        {
            Vector3 targetCenter = collision._target.transform.position;
            Vector3 direction = (targetCenter - collision._projectile.transform.position).normalized;

            Vector3 newPosition = collision._projectile.transform.position +
                                  direction * velocityMagnitude * (1 - drag * t);
            if (Vector3.Distance(collision._projectile.transform.position, targetCenter) >=
                (collision._target.transform.localScale.x - collision._projectile.transform.localScale.x) * .5)
                collision._projectile.transform.position = newPosition;

            collision._target.LerpToBodyOverTime(collision._resultingBodies[0], t, delay, false);

            if (meshDeformer) meshDeformer.AddDeformingForceInternal(Time.deltaTime, point, (float)collision._collisionEnergy*collision._collisionPrefs.energyMultiplier);
            
            
            t += Time.deltaTime;
            yield return null;

        } while (t < delay);

        collision._projectile.thisRb.drag = 0;
        // collision._projectile.thisRb.velocity = collision._target.thisRb.velocity;

        AstralBodiesManager.Instance.DestroyBody(collision._projectile, .1f);


        CollisionProcessed(collision);
    }




    private void CollisionProcessed(CollisionData collision)
    {
        if (showDebugLog) Debug.Log("[Collision Manager] " + collision._id + " Succesfully processed");

        RegisterCollision(_processedCollisions, collision);
        UnregisterCollision(collision, _unProcessedCollisions);
    }

    private AstralBody CreateNewBody(double mass, double density, Vector3 velocity, Vector3 angularVelocity,
        AstralBodyType bodyType)
    {
        //var bodyType = DetermineResultingBodyType(collision._target, collision._projectile);
        AstralBody astralBody;
        Planet planet;
        Star star;

        if (bodyType == AstralBodyType.Planet)
        {
            planet = new Planet(mass, density, velocity, angularVelocity);
            planet.BodyType = bodyType;

            return planet;
        }
        else if (bodyType == AstralBodyType.Star)
        {
            star = new Star(mass, density, velocity, angularVelocity);
            star.BodyType = bodyType;
            return star;
        }


        astralBody = new AstralBody(mass, density, velocity, angularVelocity);


        return astralBody;
    }

    private AstralBodyType DetermineResultingBodyType(CollisionData collision)
    {
        return DetermineResultingBodyType(collision._target, collision._projectile);
    }

    private AstralBodyType DetermineResultingBodyType(AstralBodyHandler target, AstralBodyHandler projectile)
    {
        var targetBody = target.BodyType;
        var projectileBody = projectile.BodyType;
        AstralBodyType resultingType = AstralBodyType.other;

        //TODO:  all cases for becoming a planet or not, a black hole or not, etc - now just for testing

        /*star conditions*/
        if (targetBody == AstralBodyType.Star || projectileBody == AstralBodyType.Star)
            resultingType = AstralBodyType.Star;
        // if(targetBody == projectileBody) 

        /*planet condition*/
        if (targetBody == AstralBodyType.Planet && projectileBody == AstralBodyType.Planet)
            resultingType = AstralBodyType.Planet;

        /*planetoid condition*/

        if (showDebugLog) Debug.Log("[Collision Manager] Determining resulting body type : " + resultingType);


        return resultingType;
    }

    #endregion

    public void ExplosionImpact(List<Rigidbody> allRb, AstralBodyHandler targetBody, Vector3 impactPoint, float force,
        Vector3 forceDirection)
    {

        targetBody.DestroySelf();

        StartCoroutine(DelayedForce(.005f, allRb, impactPoint, force, forceDirection));

    }

    public void ExplosionImpact(List<Rigidbody> allRb, AstralBodyHandler targetBody, CollisionData collision)
    {
        
        var point = collision._impactPoint.point;
        var collisionEnergy = collision._collisionEnergy;
        float energy = (float)collisionEnergy;
        energy *= collision._collisionPrefs.energyMultiplier;
       

        energy *= (targetBody == collision._target) ? 1 : -.5f;
        
        ExplosionImpact(allRb, targetBody,point,energy, collision._collisionEnergyDirection); 
        
    }
    


    private IEnumerator DelayedForce(float delay, List<Rigidbody> allRb,Vector3 impactPoint, float forceMagnitude, Vector3 forceDirection)
    {
        yield return new WaitForSeconds(delay);
        //Debug.Log("Adding force of "+ forceMagnitude +" to rb ");
        AddForceToAllRb(allRb, impactPoint, forceMagnitude, forceDirection);
       
    }
    
    void AddForceToAllRb(List<Rigidbody> allRb, Vector3 point, float forceMagnitude, Vector3 forceDirection)
    {
        if(allRb.Count == 0) return;
        foreach (var rb in allRb)
        {
           // Debug.Log("Adding force of "+ forceMagnitude +" to rb: " + rb + "of mass: " + rb.mass);
          
            AddExplosionForceToRb(rb, forceMagnitude,forceDirection ,point);
        }
    }
    
    void AddExplosionForceToRb(Rigidbody rb, float force, Vector3 forceDir, Vector3 position)
    {
        if (!rb) return;
        
        Vector3 vectorForce = forceDir*force;
        Vector3 centerOfMassToPoint = position - rb.position;
        Vector3 torque = Vector3.Cross(centerOfMassToPoint, vectorForce);
    
        rb.AddTorque(torque, ForceMode.Impulse);
       
        rb.AddForceAtPosition(vectorForce, position, ForceMode.Impulse);
        //Debug.Log("Adding force of "+ vectorForce +" to : "+rb );
    }
    private void ToggleFX(CollisionData collisionData)
    {   
        
        //TODO coould use delegate OnCollisionProcessed ? have that there ? 
        /*dont do any fx if player not there*/
        if( !CanPlayerSee()) return;

        var angle = collisionData._projectile.Velocity.normalized *-1;

        Quaternion rotation = Quaternion.Euler(angle);
        var position = collisionData._impactPoint.point;

        string keyword = collisionData.processed ?  collisionData._collisionType.ToString() : "Impact";
       
        
        FXManager.Instance.ToggleFX(FXCategory.Collision, FXElement.All, keyword ,position, rotation, collisionData._target.transform, false);

    }

    
    
    
    private bool CanPlayerSee()
    {
        //TODO set to instant when player is not looking 
        return true;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }


    private void Start()
    {
        StartCoroutine(ProcessCollisionCoroutine(_processRate));
    }


}
