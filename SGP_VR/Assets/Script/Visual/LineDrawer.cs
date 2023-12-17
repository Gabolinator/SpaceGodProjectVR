using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;


    public virtual void ToggleRenderer(LineRenderer renderer, bool state)
    {
        if (!renderer) { Debug.Log("No renderer"); return; }
        
        renderer.enabled = state;

    }

    public virtual void UpdateLineColor(Color startColor, Color endColor, bool endAlphaToZero)
    {
      
        UpdateLineColor(lineRenderer, startColor, endColor);
        

    }

    public virtual void UpdateLineColor(LineRenderer renderer, Color startColor, Color endColor)
    {
        if (!renderer) { Debug.Log("No renderer"); return; }

        renderer.startColor = startColor;
        renderer.endColor = endColor;
       
        

    }

    public virtual void UpdateLineColor(LineRenderer renderer, Color startColor, Color midColor, Color endColor)
    {
        if (!renderer) { Debug.Log("No renderer"); return; }

        renderer.startColor = startColor;
        //renderer.
        renderer.endColor = endColor;
       
        

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
