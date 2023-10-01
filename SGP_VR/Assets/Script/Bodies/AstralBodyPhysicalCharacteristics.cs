using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




[CreateAssetMenu(fileName = "newAstralBody", menuName = "Custom/Create new AstralBody dictionnairy Entry")]
public class AstralBodyPhysicalCharacteristics : ScriptableObject
{
   

        public AstralBodyType bodyType = AstralBodyType.Uninitialized;
        [ShowIf("@this.bodyType == AstralBodyType.Planet")] public PlanetType planetType = PlanetType.none;
        [ShowIf("@this.bodyType == AstralBodyType.Star")] public StarType starType = StarType.none;
        [ShowIf("@this.bodyType == AstralBodyType.Star && this.starType == StarType.MainSequenceStar")] public StarSpectralType starSpectralType = StarSpectralType.none;
        //public float percentageInUniverse;
        public double _minDensity = 1000;  // g/cm3 -> 1000 kg/m3
        public double _maxDensity = 2000;
        public double _minMass = 1000;  //kg 
        public double _maxMass = 2000; // 5.97 × 10^24 kg  => 5970 * 10^21 kg
        public List<ChemicalBodyCompositionElement>  _bodyComposition = new List<ChemicalBodyCompositionElement>();
    public AstralBodyPhysicalCharacteristics(double minDensity , double maxDensity,double minMass,double maxMass) 
    {
        _minDensity = minDensity;
        _maxDensity = maxDensity;
        _minMass = minMass;
        _maxMass = maxMass;
    }

}
