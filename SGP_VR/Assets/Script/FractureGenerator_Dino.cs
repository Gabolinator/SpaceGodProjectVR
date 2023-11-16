using System.Collections;
using System.Collections.Generic;
using DinoFracture;
using UnityEngine;

public class FractureGenerator_Dino : RuntimeFracturedGeometry, IFractureGenerator
{
    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint, int numberOfFragment)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(Vector3 impactPoint)
    {
       base.FractureAndForget(impactPoint);
       return null;
    }

    public List<Rigidbody> FractureBody()
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(List<Fragment> fragments, Vector3 impactPoint)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(List<Fragment> fragments)
    {
        throw new System.NotImplementedException();
    }
}
