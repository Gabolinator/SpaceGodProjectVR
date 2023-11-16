using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DinoFracture;
using UnityEngine;

public class FractureGenerator_Base : MonoBehaviour, IFractureGenerator
{
    private GameObject defaultSphereFragmentPrefab => CollisionManager.Instance._fractureManager.DefaultFragmentPrefab; 
    
    
    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 impactPoint, int numberOfFragment)
    {
        throw new System.NotImplementedException();
    }

    public List<Rigidbody> FractureBody(Vector3 impactPoint)
    {
        var body = GetComponent<AstralBodyHandler>();
        if (!body) return null;

       return FractureBody(body, impactPoint);
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
    
    
    
    public List<Rigidbody> FractureBody(AstralBodyHandler body, Vector3 pos)
    {
        if (!body) return null;
        
        List<Rigidbody> allRb = SpawnFragmentSphere(defaultSphereFragmentPrefab, body, UniverseManager.Instance.UniverseContainer ?  UniverseManager.Instance.UniverseContainer.transform : null);
        Debug.Log("[AstralBodyManager] allRB count: " + allRb.Count);

        float breakTorque = body.InternalResistance;
        float breakForce = body.InternalResistance;

   
        UpdateFixedJoint(allRb, breakForce, breakTorque);

        AddAstralBody(allRb, body);

       

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

    List<Rigidbody> SpawnFragmentSphere(GameObject prefab, AstralBodyHandler targetBody, Transform parent)
    {
        if(!prefab) return null;
        var target = targetBody.transform;
        
        
        
        var prefabClone = Instantiate(prefab,target.position, target.rotation,parent);
        
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

    
    
}
