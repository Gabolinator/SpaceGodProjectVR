using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosionTest : MonoBehaviour
{
    public List<Rigidbody> allRb;
    public GameObject spherePrefab;
    public float forceMagnitude; 
    public float breakForce = 10;
    public float breakTorque= 10;
    bool processing =false;
    void SpawnFragmentSphere(GameObject prefab)
    {
        if(!prefab) return;
        
        var prefabClone = Instantiate(prefab, transform.position, transform.rotation);
        
        if(!prefabClone) return;
       
        
        allRb = new List<Rigidbody>(prefabClone.GetComponentsInChildren<Rigidbody>());
        AddFixedJoints(allRb, breakForce, breakTorque);
    }

    void Update () {
        if (Mouse.current.leftButton.isPressed)
        {
            HandleInput();
        }
     
    }

    private void HandleInput()
    {
        if(!spherePrefab) return;
       
        if(processing) return;
        
        processing = true;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		
        RaycastHit hit;
		
        if (Physics.Raycast(inputRay, out hit)) {
            if (hit.collider == this.GetComponent<Collider>())
            {
                SpawnFragmentSphere(spherePrefab);
                
                Vector3 point = hit.point;
                //point -= hit.normal * 1f;
                StartCoroutine(DelayedForce(.1f, point, forceMagnitude));
                
            
                var meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer) meshRenderer.enabled = false;
                
            }
        }
        
        
        
        //StartCoroutine(DestroySelf(1f));
    }

    private IEnumerator DelayedForce(float delay, Vector3 point, float forceMagnitude)
    {
        yield return new WaitForSeconds(delay);
        
        AddForceToAllRb(point, forceMagnitude);
        processing = false;
    }

    private IEnumerator DestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Destroy(this.gameObject);
        processing = false;
    }

    // void Explode()
    // {
    //     Vector2 mousePosition = Mouse.current.position.ReadValue();
    //     Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		  //
    //     RaycastHit hit;
		  //
    //     if (Physics.Raycast(inputRay, out hit)) {
    //         if (hit.collider == this.GetComponent<Collider>())
    //         {
    //             Vector3 point = hit.point;
    //             point -= hit.normal * 0.5f;
    //             AddForceToAllRb(point, forceMagnitude);
    //         
    //         }
    //     }
    //     
    //     
    // }
    //
    void AddForceToAllRb(Vector3 point, float forceMagnitude)
    {
        if(allRb.Count == 0) return;
        foreach (var rb in allRb)
        {
            AddExplosionForceToRb(rb, forceMagnitude, point, 1);
        }
    }

    void AddExplosionForceToRb(Rigidbody rb, float force, Vector3 position, float radius)
    {
        if (!rb) return;

        //rb.AddExplosionForce(force,position, radius);
        Vector3 vectorForce = (rb.position - position).normalized * force;
        Vector3 centerOfMassToPoint = position - this.transform.position;
        Vector3 torque = Vector3.Cross(centerOfMassToPoint, vectorForce);
        rb.AddTorque(torque, ForceMode.Force);
        // Vector3 vectorForce = (rb.position - position).normalized * force;
       rb.AddForceAtPosition(vectorForce, position, ForceMode.Force);

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
