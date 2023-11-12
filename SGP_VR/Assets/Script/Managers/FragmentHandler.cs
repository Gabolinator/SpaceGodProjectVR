using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(FragmentHandler))]
public class YourScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FragmentHandler script = (FragmentHandler)target;

        if (GUILayout.Button("Execute Method"))
        {
            // Call the method you want to execute
            script.AddComponentsToFragments();
        }
    }
}
public class FragmentHandler: MonoBehaviour
{
    
    public List<Rigidbody> allRb;
    
    public float breakForce = 0.01f;
    public float breakTorque = 0.01f;

    
    public void AddComponentsToFragments()
    {
        allRb =  GetAllRigidBodies(this.gameObject);
        AddFixedJoints(allRb, breakForce, breakTorque);
        //AddAstralBody(allRb);
    }

    private List<Rigidbody> GetAllRigidBodies(GameObject go)
    {
        return new List<Rigidbody>(go.GetComponentsInChildren<Rigidbody>().ToArray());
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
            
            bodyHandler.body = new AstralBody(targetBody.Density,targetBody.Density,Vector3.zero, Vector3.zero, AstralBodyType.other);
            
            bodyHandler.thisRb = rb;
           
            bodyHandler.ID += "_Fragment_Of_"+targetBody.ID;
            
            allBodyHandlers.Add(bodyHandler);
        }

        return allBodyHandlers;
    }

    private List<AstralBodyHandler> AddAstralBody(List<Rigidbody> allRb)
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
            
            bodyHandler.body = new AstralBody(AstralBodyType.other);
            bodyHandler.EstimateVolume();
            
            bodyHandler.thisRb = rb;
           
            bodyHandler.ID += "(Fragment)";
            
            
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
