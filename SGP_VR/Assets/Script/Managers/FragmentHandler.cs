using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


[CustomEditor(typeof(FragmentHandler))]
public class AddComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FragmentHandler script = (FragmentHandler)target;

        if (GUILayout.Button("Add Components to Child Fragments"))
        {
          
            script.AddComponentsToFragments();
        }
    }
}
public class FragmentHandler: MonoBehaviour
{
    
    public List<Rigidbody> allRb;
    
    public float breakForce = 0.01f;
    public float breakTorque = 0.01f;
    // public double volumeFragment;
    // public double volumeBody;
    // public double massBody;
    // public double massFragment; 
    
    private List<Type> _components = new List<Type> { typeof(GuiContainer), typeof(XRGrabInteractable), typeof(GrabHelper), typeof(SGP_GrabTransformer) };
    
    
    public void AddComponentsToFragments()
    {
        allRb =  GetAllRigidBodies(this.gameObject);
        AddFixedJoints(allRb, breakForce, breakTorque);
        AddComponentsToFragments(allRb, _components);
    }

    public void AddComponentsToFragments(List<Rigidbody> allRb, List<Type> components ) 
    {
        if(allRb.Count == 0 || components.Count == 0) return;

        foreach (var rb in this.allRb)
        {
            var go = rb.gameObject;
            if(!go) continue;

            AssignComponent(components, go);
        }
    }

    private List<Rigidbody> GetAllRigidBodies(GameObject go)
    {
        return new List<Rigidbody>(go.GetComponentsInChildren<Rigidbody>().ToArray());
    }
    
    private void AssignComponent(List<Type> components , GameObject go)
    {
        if (!go || components.Count == 0) return; 
        foreach (var component in components)
        {
            AssignComponent(component, go);
        }
        
    }

    private void AssignComponent(Type comp, GameObject go)
    {
        if (!go) return;
        if (typeof(Component).IsAssignableFrom(comp))
        {
            if (go.GetComponent(comp) == null) go.AddComponent(comp);
            else
            {
                Debug.Log(go + " has already right component : " + comp.Name);
            }
        }
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

        var otherFixedJoints = go.GetComponents<FixedJoint>();
        if (otherFixedJoints.Length != 0)
        {
            foreach (var otherJoint in otherFixedJoints)
            {
                if (otherJoint.connectedBody == otherRb)
                {
                    otherJoint.breakForce = breakForce;
                    otherJoint.breakTorque = breakTorque;
                    return;
                }
            }
        }

        FixedJoint fixedJoint = go.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = otherRb;

        fixedJoint.breakForce = breakForce;
        fixedJoint.breakTorque = breakTorque;
    }


}
