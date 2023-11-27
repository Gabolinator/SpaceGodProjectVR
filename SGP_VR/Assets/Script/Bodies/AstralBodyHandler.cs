using System;
using System.Collections;
using System.Collections.Generic;
using DinoFracture;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[System.Serializable]
public struct StartPreferences
{
    [SerializeField] private bool _setRandomVelocity;
    public bool SetRandomVelocity => _setRandomVelocity;

    [SerializeField] private bool _setRandomAngularVelocity;
    public bool SetRandomAngularVelocity => _setRandomAngularVelocity;

    [SerializeField] private int _delayStart ;
    public int DelayStart
    {
        get { return _delayStart; }
        set { _delayStart = value; }
    }
    
    [SerializeField] private float _enableCollisionDelay;
    public float EnableCollisionDelay
    {
        get { return _enableCollisionDelay; }
        set { _enableCollisionDelay = value; }
    }
    
}


public class AstralBodyHandler : MonoBehaviour
{
    public AstralBody body;

    [HideInInspector] public AstralBodyDescriptor bodyDescriptor;

    public PhysicsProperties physicsProperties => UniverseManager.Instance.PhysicsProperties;
    
    public string ID
    {
        get => body.ID;
        set => body.ID = value;
    }
    
    
    
    public double KyneticEnergy => CalculateBodyEnergy();
    
    public AstralBodyType BodyType
    {
        get => body.BodyType;
        set => body.BodyType = value;
    }

    public double Mass
    {
        get => body.Mass;
        set => body.Mass = value;
    }

   


    public double Density
    {
        get => body.Density;
        set => body.Density = value;
    }

    public double Radius
    {
        get => body.Radius;
        set => body.Radius = value;
    }

    public double Volume
    {
        get => body.Volume;
        set => body.Volume = value;
    }

    
    public float InternalResistance 
    {
        get => body.InternalResistance ;
        set => body.InternalResistance  = value;
    }

    public float c => body.c;
    public float u => body.u;
    public float s => body.s;
    
    public Rigidbody thisRb;
    
    
    [Header("Velocities")] 
    [SerializeField] protected Vector3 _velocity;

    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    
    [SerializeField] protected Vector3 _angularVelocity;

    public Vector3 AngularVelocity
    {
        get => _angularVelocity;
        set => _angularVelocity = value;
    }

    [Header("Gravity Pull Prefs")] 
    [SerializeField] protected float _influenceRange = -1; //0  = no influence ,  -1 =  total influence
    public float InfluenceRange => _influenceRange;
    public float MaxDetectionRange => UniverseManager.Instance.MaxDetectionRange;

    [SerializeField] protected float _influenceStrength = 1; //1  = normal influence, 0  = no influence
    public float InfluenceStrength
    {
        get { return _influenceStrength; }
        set { _influenceStrength = value; }

    }
    
    public float _processRate = 0.1f;
    
    public Vector3 totalForceOnObject = Vector3.zero;
    public bool EnableGravity => UniverseManager.Instance.enableGravity || !isGrabbed || !gravityDisabled;
    
    
    [Header("Start Preferences")] 
    public StartPreferences startPreferences = new StartPreferences();
    public bool SetRandomVelocity => startPreferences.SetRandomVelocity;
    
    public bool SetRandomAngularVelocity => startPreferences.SetRandomAngularVelocity;

    public float EnableCollisionDelay
    {
        get => startPreferences.EnableCollisionDelay; 
        set => startPreferences.EnableCollisionDelay = value;
    }

    public int DelayStart
    {
        get  => startPreferences.DelayStart;
        set => startPreferences.DelayStart = value;
    }

    private bool _shouldInitialize = true;
   
    public bool ShouldInitialize
    {
        get => _shouldInitialize;
        set => _shouldInitialize = value;
    }

    public bool _enableCollision = false;

    public bool EnableCollision
    {
        get => _enableCollision;
        set => _enableCollision = value;
    }

  
    public double CurrentRadiusOfTrajectory
    {
        get => body.CurrentRadiusOfTrajectory;
        set => body.CurrentRadiusOfTrajectory = value;
    }

    public AstralBodyHandler CenterOfRotation => body.CenterOfRotation;
    
    public bool IsOrbiting => body.IsOrbiting;
    
    public bool IsSatellite => body.IsSatellite;
    //
    // [Header("Satellites")] 
    // [Header("Satellites")] 
    // public int maxNumberOfSatellites;
    public List<AstralBodyHandler> Satellites
    {
        get => body.Satellites;
        set => body.Satellites = value;
    }
    // public bool canHaveSatellites;
    // public bool HasSatellite => satellites.Count != 0;
    //
    // [Header("Ring")] 
    // public List<CelestialRing> rings;
    // public bool canHaveRings;
    // public bool HasRings => rings.Count != 0;
    
    [Header("Grab")] 
    public bool isGrabbed = false;

    #region Events

    Action<AstralBodyHandler> OnAstralBodyStartToExist => EventBus.OnAstralBodyStartToExist;
    Action<AstralBody> OnBodyUpdate => EventBus.OnBodyUpdated;

    Action<AstralBodyHandler, Vector3, Vector3> OnAstralBodyAnyVelocityChange =>
        EventBus.OnAstralBodyAnyVelocitiesChanged;

    Action<AstralBodyHandler, Vector3> OnAstralBodyAngularVelocityChange => EventBus.OnAstralBodyAngularVelocityChange;

    Action<AstralBodyDescriptor> OnBodyDescriptorUpdate => EventBus.OnBodyDescriptorUpdated;
    #endregion
 
    
    [Header("Debug")]
    public List<AstralBodyHandler> allBodiesInRange = new List<AstralBodyHandler>();
    public List<Vector3> positions = new List<Vector3>();
    public List<Vector3> velocities = new List<Vector3>();
    public List<Vector3> accelerations = new List<Vector3>();
   
    private float timeElapsed;
    public bool firstUpdate = true;
    public bool gravityDisabled;
    public bool ShowDebugLog => AstralBodiesManager.Instance._showDebugLog;
    
    public void UpdateMass(double delta)
    {
        // Debug.Log("[Body Handler] Updating mass : " + ((Mass * delta) * UniverseManager.Instance.PhysicsProperties.MassFactor));

        if ((Mass * delta * UniverseManager.Instance.PhysicsProperties.MassFactor) < 0.01f) return;
        Mass *= delta;

        thisRb.mass = (float)Mass;
        
        UpdateBody();
    }

    private void Initialize()
    {
        if(!_shouldInitialize) return;

        FracturedObject fracturedObject = GetComponent<FracturedObject>();
        if (fracturedObject)
        {
            Mass = fracturedObject.ThisMass;
            Volume = fracturedObject.ThisVolume;
            Density = body.CalculateDensity(Mass, Volume);
        }


        else
        {
            if (Radius == 0) Radius = body.CalculateRadius(transform.localScale);
            else SetScaleFromRadius(Radius);

            if (Volume == 0) Volume = body.CalculateVolume(Radius);

            if (Density == 0 && Mass == 0)
            {
                Density = 1000;
                Mass = body.CalculateMass(Density, Volume);
            }

            else if (Density == 0 && Mass != 0) Density = body.CalculateDensity(Mass, Volume);
        }

        if (thisRb == null) thisRb = GetComponent<Rigidbody>();
        
        if (Mass != 0) thisRb.mass = (float)Mass;
        
        if (ID == "") ID = GenerateId();




        SetBodyName();

        if (SetRandomVelocity) Velocity = GetRandomVelocity(-0.5f, 0.5f);
        else Velocity = body.StartVelocity;

        if (SetRandomAngularVelocity) AngularVelocity = GetRandomVelocity(-0.5f, 0.5f);
        else AngularVelocity = body.StartAngularVelocity;
        AddVelocity(Velocity);

        AddAngularVelocity(AngularVelocity);

        Debug.Log("[Astral body] Initialized " + this);
    }


    private IEnumerator EnableCollisionCoroutine(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableCollision = state;
    }

    public void InjectMassOverTime(double mass, float duration)
    {
        var totalMass = mass + Mass;
        //  StartCoroutine(LerpOverTime(totalMass, duration));

    }

    public IEnumerator LerpToBodyOverTimeCoroutine(AstralBody astralBody, float delay, bool setScale = true)
    {
        float t = 0;

        do
        {

            LerpToBodyOverTime(astralBody, t, delay, false);


            t += Time.deltaTime;

            yield return null;

        } while (t < delay);


    }

    public void LerpToBodyOverTime(AstralBody astralBody, float t, float delay, bool setScale = true)
    {
        Debug.Log("[Body Handler] " + this + " Lerping");

        if (t < delay)
        {
            Mass = LerpOverTime(Mass, astralBody.Mass, t, delay);
            Density = LerpOverTime(Density, astralBody.Density, t, delay);
            Volume = body.CalculateVolume(Mass, Density);
            Radius = body.CalculateRadius(Volume);
            thisRb.velocity = Velocity = LerpVectorOverTime(Velocity, astralBody.StartVelocity, t / 10, delay);

        }

        else
        {
            Mass = astralBody.Mass;
            Density = astralBody.Density;
            Volume = body.CalculateVolume(Mass, Density);
            Radius = body.CalculateRadius(Volume);
            thisRb.velocity = Velocity = astralBody.StartVelocity;

        }

        CalculateBodyEnergy();
        if (setScale) SetScaleFromRadius(Radius);

        //UpdateBody();
    }

    public double LerpOverTime(double currentValue, double targetValue, float t, float duration)
    {

        float lerpProgress = t / duration;
        if (t < duration) return Mathf.Lerp((float)currentValue, (float)targetValue, lerpProgress);
        return targetValue;
    }

    public Vector3 LerpVectorOverTime(Vector3 currentVector, Vector3 targetVector, float t, float duration)
    {

        float lerpProgress = t / duration;
        if (t < duration) return Vector3.Lerp(currentVector, targetVector, lerpProgress);
        else return targetVector;
    }

    private IEnumerator LerpOverTime(double targetValue, float lerpDuration)
    {
        float currentTime = 0f;

        while (currentTime < lerpDuration)
        {
            // Calculate the lerp progress as a value between 0 and 1.
            float lerpProgress = currentTime / lerpDuration;

            // Perform the linear interpolation.
            Mass = Mathf.Lerp((float)Mass, (float)targetValue, lerpProgress);

            ScaleBody();
            // Increment the current time.
            currentTime += Time.deltaTime;

            yield return null; // Wait for the next frame.
        }

        Mass = targetValue;
    }

    public void ScaleBody()
    {
        Volume = body.CalculateVolume(Mass, Density);
        SetScaleFromRadius(Radius = body.CalculateRadius(Volume));
        UpdateBody();
    }

    public void ScaleBody(bool keepMassConstant = true)
    {
        //if we keep mass constant , density will vary
        //if we change mass density will stay constant

        //Debug.Log("[Astral body] Update" + this);

        Radius = body.CalculateRadius(transform.localScale);
        Volume = body.CalculateVolume(Radius);


        if (keepMassConstant)
        {
            Density = CalculateDensity(Mass, Volume);
        }
        else
        {
            Mass = CalculateMass(Density, Volume);
            thisRb.mass = (float)Mass;
        }

        UpdateBody();
    }

    private double CalculateDensity(double mass, double volume) => body.CalculateDensity(mass, volume);
    
    private double CalculateDensity() => Density = body.CalculateDensity(Mass, Volume);

    public double CalculateMass(double density, double volume) => body.CalculateMass(density, volume);

    public double CalculateMass() => Mass = body.CalculateMass(Density, Volume);
    
    public void UpdateBody()
    {
        UpdateBody(body);
    }

    public void UpdateBody(AstralBody body)
    {
        /*update descriptor*/
        bodyDescriptor = new AstralBodyDescriptor(body);

        OnBodyUpdate?.Invoke(body);
        OnBodyDescriptorUpdate?.Invoke(bodyDescriptor);
    }


    public void SetBodyName()
    {
        this.gameObject.name = body.GetBodyName();
    }

    private string GenerateId() => ID != "" ? ID : AstralBodiesManager.Instance.GenerateName();

    public void AddVelocity(Vector3 velocity)

    {
        if (thisRb == null) return;
        if (thisRb.velocity != Vector3.zero || velocity == Vector3.zero) return;

        thisRb.velocity = velocity;
    }

    private void AddAngularVelocity(Vector3 angularVelocity)
    {
        if (thisRb == null) return;
        if (thisRb.angularVelocity != Vector3.zero || angularVelocity == Vector3.zero) return;


        thisRb.angularVelocity = angularVelocity;
    }

    private Vector3 GetRandomVelocity(float min, float max) => BodyGenerator.Instance.GenerateVelocity(min, max);

    public void SetScaleFromRadius(double radius)
    {
        if(radius == 0) return;
        
        float diameter = (float)radius * 2;

        if (diameter > AstralBodiesManager.Instance.MaxAstralBodyScale)
        {
            diameter = AstralBodiesManager.Instance.MaxAstralBodyScale;
            Radius = diameter / 2;
        }

        transform.localScale = new Vector3(diameter, diameter, diameter);

    }

    public void SetInfluence(float directGravityPullMultiplier)
    {
        _influenceStrength = directGravityPullMultiplier;
    }

    public Vector3
        CalculateTotalGravityPull(List<AstralBodyHandler> listOfBody, Vector3 position, float timeStep = 0) =>
        FormulaLibrary.CalculateTotalGravityPull(listOfBody, this, position, timeStep);

    public List<AstralBodyHandler> GetAllBodyInRange(float range, List<AstralBodyHandler> listOfBody, Vector3 position)
    {

        listOfBody.Clear();

        Collider[] hitColliders = Physics.OverlapSphere(position, range);

        if (hitColliders.Length == 0) return listOfBody;

        foreach (Collider hitCollider in hitColliders)
        {
            AstralBodyHandler body = hitCollider.GetComponent<AstralBodyHandler>();

            if (!body || hitCollider.gameObject == gameObject) continue;

            listOfBody.Add(body);
        }

        return listOfBody;
    }

    private void RegisterSelf() => AstralBodiesManager.Instance.RegisterBody(this);

    private IEnumerator CalculateGravityPullCoroutine(float delay, int numFrames)
    {

        for (int i = 0; i < numFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        
        do
        {
            float range = InfluenceRange < 0 ? MaxDetectionRange : InfluenceRange;

            allBodiesInRange = GetAllBodyInRange(range, allBodiesInRange, transform.position);

            totalForceOnObject = CalculateTotalGravityPull(allBodiesInRange, this.transform.position);
            

            yield return new WaitForSeconds(delay);

        } while (true);
    }

    public double CalculateBodyEnergy() => FormulaLibrary.CalculateKineticEnergy(Mass, Velocity);

    private Vector3 GetVelocity()
    {
        if (!thisRb) return Vector3.zero;

        return thisRb.velocity;
    }


    private Vector3 GetAngularVelocity()
    {
        if (!thisRb) return Vector3.zero;

        return thisRb.angularVelocity;
    }


    private Vector3 GetAcceleration() => FormulaLibrary.CalculateAcceleration(Mass, totalForceOnObject);


    private IEnumerator CollectData(float processRate, int numFrames)
    {
        
        for (int i = 0; i < numFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        
        do
        {
            yield return new WaitForEndOfFrame();

            float deltaTime = Time.fixedDeltaTime;
            timeElapsed += deltaTime;

            Vector3 currentAcceleration = GetAcceleration();

            accelerations.Add(currentAcceleration);


            Vector3 currentVelocity = velocities.Count > 0
                ? velocities[velocities.Count - 1]
                : Vector3.zero + currentAcceleration * deltaTime;

            velocities.Add(currentVelocity);



            Vector3 currentPosition = positions.Count > 0
                ? positions[positions.Count - 1]
                : Vector3.zero + currentVelocity * deltaTime;

            positions.Add(currentPosition);


            CurrentRadiusOfTrajectory = currentPosition.magnitude;


            yield return new WaitForSeconds(processRate);

        } while (true);

    }



    public bool PredictCollisionAtPosition(AstralBodyHandler bodyHandler, Vector3 position, float buffer = 0.5f)
    {
        var radius = bodyHandler.Radius;
        if (body == null)
            Debug.LogWarning("[Astral Body] Trying to test collision at : " + position + "but body is null");
        if (radius == 0)
        {

            Debug.LogWarning("[Astral Body] Trying to test collision at : " + position + "but no body.Radius is set");
            //radius = 10;
        }



        Collider[] colliders = Physics.OverlapSphere(position, (float)radius + buffer);

        if (colliders.Length == 0) return false;

        foreach (Collider collider in colliders)
        {
            if (collider != bodyHandler.GetComponent<Collider>())
            {
                //Debug.Log("[Trajectory Predictor] Body: " + body + " Collision predicted at  " + position + "  on " + collider);
                return true;
            }
        }

        return false;
    }


    public bool PredictCollisionAtPosition(Vector3 position, float buffer = 0.5f)
    {
        return PredictCollisionAtPosition(this, position, buffer);
    }

    private IEnumerator RegisterSelfCoroutince()
    {
        while (AstralBodiesManager.Instance == null)
        {
            yield return null;
        }

        RegisterSelf();
    }

    public void UpdateVelocities(bool firstTime = false)
    {

        var velocityChanged = firstTime;

        if (_velocity != GetVelocity())
        {
            bodyDescriptor._velocity = _velocity = GetVelocity();
            velocityChanged = true;
        }

        if (_angularVelocity != GetAngularVelocity())
        {
            bodyDescriptor._angularVelocity = _angularVelocity = GetAngularVelocity();
            //OnAstralBodyAngularVelocityChange?.Invoke(this, _angularVelocity);
            velocityChanged = true;
        }

        if (velocityChanged) OnAstralBodyAnyVelocityChange?.Invoke(this, _velocity, _angularVelocity);

    }

    public void ToggleSelf(bool state)
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer) meshRenderer.enabled = state;
    }

    public void DestroySelf()
    {
        AstralBodiesManager.Instance.DestroyBody(this);
    }

    public virtual void Awake()
    {
        

        StartCoroutine(RegisterSelfCoroutince());
    }

    public virtual void Start()
    {
        //body = new AstralBodyInternal(2000, 2000, new Vector3(0, 0, 0));

        _enableCollision = false;
        
        
        StartCoroutine(EnableCollisionCoroutine(true, EnableCollisionDelay));
        Initialize();

        bodyDescriptor = new AstralBodyDescriptor(body);
        bodyDescriptor._velocity = GetVelocity();
        bodyDescriptor._angularVelocity = GetAngularVelocity();

        OnAstralBodyStartToExist?.Invoke(this);

        //StartCoroutine(CalculateGravityPullCoroutine(_processRate, DelayStart));

        StartCoroutine(CollectData(_processRate,DelayStart));
        
        
    }

//moved it to astralbody manager for optimization 
    protected virtual void FixedUpdate()
    {
        // UpdateVelocities(firstUpdate);
        // firstUpdate = false;
        //
        //
        //                 
        // if (thisRb && EnableGravity && totalForceOnObject != Vector3.zero)
        // {
        //     var ratioMass = Mass / thisRb.mass; // if we lost mass cause of the cast to float , compensate force applied
        //
        //     thisRb.AddForce(totalForceOnObject =
        //         ratioMass == 1 ? totalForceOnObject : totalForceOnObject / (float)ratioMass);
        // }
    }



    protected virtual void OnCollisionEnter(Collision collision)
    {
        if(!_enableCollision) return;
        AstralBodyHandler otherBodyHandler = collision.gameObject.GetComponent<AstralBodyHandler>();

        if (otherBodyHandler == null) return;

        var otherBody = otherBodyHandler.body;

        if (otherBody.BodyType != AstralBodyType.Uninitialized)
        {

            CollisionManager.Instance.CreatingCollision(otherBodyHandler, this, collision);
        }


    }


    public void EstimateVolume(double originalBodyVolume = 1)
    {
        Debug.LogWarning("[Astral Body] Estimating body volume based on triangle count not yet implemented");
        var meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) return;
        var mesh = meshFilter.sharedMesh;
        if(!mesh) return;

        var volume = FormulaLibrary.EstimateMeshVolume(mesh);
        Debug.LogWarning("[Astral Body] Estimating body volume based on triangle : " + volume);
        if (volume <= 0) return;
        
        
        
        Volume = volume*250000*originalBodyVolume / 1.0027; //to make sure volume ratio is respected - will have a tiny bit of loss 
    }
}
