using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
   // public float rotationSpeed = 30;
   public Vector3 speed = new Vector3(0,0,60);
   
    void Update()
    {
        transform.Rotate(speed * Time.deltaTime);
    }
}
