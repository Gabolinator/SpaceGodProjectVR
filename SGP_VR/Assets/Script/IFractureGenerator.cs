using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFractureGenerator
{
   public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint);
   
   public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint, int numberOfFragment);

   public List<Rigidbody> FractureBody(Vector3 impactPoint);
   
   public List<Rigidbody> FractureBody();
   
   public List<Rigidbody> FractureBody(List<Fragment> fragments, Vector3 impactPoint);
   
   public List<Rigidbody> FractureBody(List<Fragment> fragments);
   
   
}
