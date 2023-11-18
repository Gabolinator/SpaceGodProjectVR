using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentBody : AstralBody
{

    private AstralBody _originalBody;
    public AstralBody OriginalBody => _originalBody;
    
    public FragmentBody(double mass, double density, Vector3 velocity, Vector3 angularVelocity) : base(mass, density, velocity, angularVelocity, AstralBodyType.Fragment) 
    {
        if (ShowDebugLog) Debug.Log("[Planet] Creating new fragment :" + _id + " of mass :" + mass + " of density : " + density + "  of velocity " + velocity);


    }

    public FragmentBody(AstralBody astralBody) : base(astralBody) 
    {
        Debug.Log("[Fragment] Constructor (AstralBody astralBody) based on astral body");
        _originalBody = astralBody;
        InternalResistance = astralBody.InternalResistance;
    }
    
    public FragmentBody(AstralBody astralBody, double mass, double volume)
    {
        Debug.Log("[Fragment] Constructor (AstralBody astralBody) based on astral body");
        _originalBody = astralBody;
        Density = astralBody.Density;
        Mass = mass;
        Volume = volume;
        InternalResistance = astralBody.InternalResistance;
    }
    
    public FragmentBody() : base()
    {
        Debug.Log("[Fragment] Constructor () based on astral body");
        
    }

    public override double CalculateVolume(double radius)
    {
        Debug.Log("[Fragment] Irregular shape , cant calculate volume with radius");
        return 0;
    }

    public override double CalculateRadius(Vector3 scale)  
    {
        Debug.Log("[Fragment] Irregular shape , cant calculate radius with scale");
        return 0;
    }

    public  override double CalculateRadius(double volume) 
    {
        Debug.Log("[Fragment] Irregular shape , cant calculate radius with volume");
        return 0;
    }

}
