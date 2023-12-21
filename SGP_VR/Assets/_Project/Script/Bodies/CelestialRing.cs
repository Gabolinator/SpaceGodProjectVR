using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CelestialRing
{
    public int numberOfRings;
    public BodyPhysicalCharacteristics physicalCharacteristics = new BodyPhysicalCharacteristics();
    public double distance;
    public Vector3 orientation;

}
