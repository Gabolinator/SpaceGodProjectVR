using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLine : LineDrawer
{

    public Vector3 startPoint;
    public Vector3 direction;
    public float length;
    public float offset; // astal body thickness 
   
   

    public void UpdateLine()
    {
        if (!lineRenderer) return;

        Vector3 endPoint = startPoint + direction.normalized * (length+offset);

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    public void UpdateLine(Vector3 vector, float offset, Vector3 start)
    {
        if (!lineRenderer) return;

        direction = vector;
        length = vector.magnitude;
        startPoint = start;
        

       UpdateLine();
    }

   
    public override void Awake()
    {
        base.Awake();

       
    }

    public override void Start()
    {
        base.Start();
        lineRenderer.positionCount = 2;
        startPoint = transform.position;
    }
}
