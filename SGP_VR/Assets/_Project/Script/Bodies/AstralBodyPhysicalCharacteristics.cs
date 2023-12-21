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
        public float _cMin;
        public float _cMax;
        public float _uMin;
        public float _uMax;
        public float _sMin;
        public float _sMax;
        
    public AstralBodyPhysicalCharacteristics(double minDensity , double maxDensity,double minMass,double maxMass) 
    {
        _minDensity = minDensity;
        _maxDensity = maxDensity;
        _minMass = minMass;
        _maxMass = maxMass;
    }
    
    public double GenerateRandomMass() => UnityEngine.Random.Range((float)_minMass, (float)_maxMass);
    public double GenerateRandomDensity() => UnityEngine.Random.Range((float)_minDensity, (float)_maxDensity);
    
   

    public BodyPhysicalCharacteristics GenerateRandomPhysicalCharacteristics()
    {
        BodyPhysicalCharacteristics characteristics = new BodyPhysicalCharacteristics()
        {
            _mass = UnityEngine.Random.Range((float)_minMass, (float)_maxMass),
            _density = UnityEngine.Random.Range((float)_minDensity, (float)_maxDensity),
            _c = UnityEngine.Random.Range((float)_cMin, (float)_cMax),
            _u = UnityEngine.Random.Range((float)_uMin, (float)_uMax),
            _s = UnityEngine.Random.Range((float)_sMin, (float)_sMax)
        };

        Debug.Log("Random mass  for " + bodyType  +" : "+ characteristics._mass );
        Debug.Log("Random _density for " + bodyType  +" : "+ characteristics._density );
        Debug.Log("Random _c for " + bodyType  +" : "+ characteristics._c );
        Debug.Log("Random _u for " + bodyType  +" : "+ characteristics._u );
        Debug.Log("Random _s for " + bodyType  +" : "+ characteristics._s );
        
        return characteristics;

    }

 

}
