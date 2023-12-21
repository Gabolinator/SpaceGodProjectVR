using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FractureGenerator_OpenFrac : Fracture, IFractureGenerator
{
    public CollisionData ColData { get; set; }
    public List<Rigidbody> AllFragment { get; set; }

    public void FractureBody(AstralBodyHandler body, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public void FractureBody(AstralBodyHandler body, Vector3 impactPoint, int numberOfFragment)
    {
        throw new System.NotImplementedException();
    }

    public void FractureBody(Vector3 impactPoint) => FractureBody();

    public void FractureBody(CollisionData collisionData)
    {

        ColData = collisionData;
        FractureBody();
    }

    public void FractureBody()
    {
        
        
        ComputeFracture();
        

        AllFragment = new List<Rigidbody>( GetFragmentRoot() ? GetFragmentRoot().GetComponentsInChildren<Rigidbody>() : null);
        Explode(ColData);
        
    }


    public void FractureBody(List<Fragment> fragments, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public void FractureBody(List<Fragment> fragments)
    {
        throw new System.NotImplementedException();
    }

    public void Explode(CollisionData collisionData)
    {
        
        CollisionManager.Instance.ExplosionImpact(AllFragment, GetComponent<AstralBodyHandler>(), collisionData);
    }


    private void Awake()
    {
        fractureOptions.insideMaterial = CollisionManager.Instance._fractureManager.InsideMaterial;
        refractureOptions.enableRefracturing = false;
        refractureOptions.maxRefractureCount = 1;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
       
    }

    protected override void  OnTriggerEnter(Collider collider)
    {
       
    }
    
    protected override void Update()
    {
      
        return;
    }

}
