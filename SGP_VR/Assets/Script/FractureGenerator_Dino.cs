using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using DinoFracture;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class FractureGenerator_Dino : RuntimeFracturedGeometry, IFractureGenerator
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

   

    public void FractureBody(Vector3 impactPoint)
    {
        base.FractureInternal(impactPoint);

        
       
    }

    public void FractureBody(CollisionData collisionData)
    {
        ColData = collisionData;
        var point =collisionData._impactPoint.point;
         point   -= (collisionData._target.transform.position - point).normalized * collisionData._collisionPrefs.impactPointOffset;
        FractureBody(point );

    }


    public void FractureBody()
    {
        throw new System.NotImplementedException();
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

    public void AddComponentsToFragments()
    {
        if(AllFragment.Count == 0) return;

        foreach (var fragment in AllFragment)
        {
            var handler = fragment.gameObject.AddComponent<AstralBodyHandler>();
            if(!handler) return;
            handler.body = new FragmentBody();
            handler.body.BodyType = AstralBodyType.Fragment;
            handler.enableCollisionDelay = 30;
        }
    }

    public void Explode(OnFractureEventArgs args)
    {
        //Debug.Log("args.FracturePiecesRootObject " + args.FracturePiecesRootObject);
        AllFragment = new List<Rigidbody>(args.FracturePiecesRootObject.GetComponentsInChildren<Rigidbody>());
        Explode(ColData);
    }


    public void Awake()
    {
       //  base.Asynchronous = false;
       // // base.FractureType = FractureType.Slice;
       //  base.FractureSize = new SizeSerializable();
       //  base.FractureSize.Space = SizeSpace.WorldSpace;
    }

  
}
