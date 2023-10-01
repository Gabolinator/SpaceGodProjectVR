using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    
    public virtual void UpdateLineColor(Color startColor, Color endColor, bool endAlphaToZero)
    {
        var renderer = lineRenderer;

        if (!renderer) { Debug.Log("No renderer"); return; }

        renderer.startColor = startColor;
        renderer.endColor = endColor;
        //lineRenderer.startColor;

    }
    public virtual void UpdateLineColor(Color color, bool endAlphaToZero = true)
    {
        
        UpdateLineColor(color, color, endAlphaToZero);


    }

    public virtual void Awake()
    {
        if(!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
    }

    public virtual void Start() { }


}
