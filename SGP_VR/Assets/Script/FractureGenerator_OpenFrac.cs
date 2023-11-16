using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureGenerator_OpenFrac : Fracture, IFractureGenerator
{
    
    
    
    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint, int numberOfFragment)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(Vector3 impactPoint) => FractureBody();


    public List<Rigidbody> FractureBody()
    {
        
        
        ComputeFracture();
        

        List<Rigidbody> allRb = new List<Rigidbody>( GetFragmentRoot() ? GetFragmentRoot().GetComponentsInChildren<Rigidbody>() : null);
        
        
        return allRb;
    }


    public List<Rigidbody> FractureBody(List<Fragment> fragments, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(List<Fragment> fragments)
    {
        throw new System.NotImplementedException();
    }


    private void Awake()
    {
        fractureOptions.insideMaterial = CollisionManager.Instance._fractureManager.InsideMaterial;
        refractureOptions.enableRefracturing = true;
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
