using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Fragment
{
   public double mass;
   public double volume;
   public AstralBodyHandler originalBody;
   public Mesh mesh; 
}

[System.Serializable]
public class FractureManager
{
  
      public enum FractureFramework
    {
        Prefab, 
        OpenFracture, 
        DinoFracture,
        none
    }

   
      

    [SerializeField, Tooltip("Choose the desired grab interaction framework. Base : Is a non implemented framework (for testing purposes), None : will disable grab interaction as no components will be added ")] private FractureFramework _fractureFramework = FractureFramework.none;

    
    public FractureFramework FracFramework
    {
        get => _fractureFramework;
    }

    public bool atRuntime = false;  
    
    [SerializeField]
    private List<GameObject> _sphereFragmentPrefabs;

    public GameObject DefaultFragmentPrefab
    {
        get { return _sphereFragmentPrefabs.Count == 0 ? null : _sphereFragmentPrefabs[0]; }
    }

    [SerializeField] private Material _insideMaterial;
    public Material InsideMaterial => _insideMaterial;

    private List<Type> _fractureLogic = new List<Type> { typeof(FractureGenerator_Base), typeof(FractureGenerator_OpenFrac), typeof(FractureGenerator_Dino)}; //List of fracture script 


    
    public Type FractureLogic
    {
        get
        {
            
            var i = atRuntime ? (int)FracFramework : 0;  //if not at runtime - get base fracture system (with prefabs) - put at index 0 of list

            if (i >= 0)
            {
                return (i > _fractureLogic.Count) ? _fractureLogic[_fractureLogic.Count - 1] : _fractureLogic[i];
            }
            
            Debug.LogWarning("[FractureManager] Couldnt assign Framework ");
            return null;
        }
    }
    
    public IFractureGenerator GetCurrentFrameworkFractureLogic()
    {   
        Type fractureType = FractureLogic;

        if (fractureType != null)
        {
            object instance = Activator.CreateInstance(fractureType);

            if (instance is IFractureGenerator)
            {
                return (IFractureGenerator)instance;
            }
            else
            {
                Debug.LogError("[FractureManager] The instantiated object does not implement IFractureGenerator.");
                return null;
            }
        }
        else
        {
            Debug.LogError("[FractureManager] FractureLogic is null.");
            return null;
        }
    }
    
    
    
    public IFractureGenerator AssignFractureComponent(GameObject go)
    {
        if (!go) return null;
        
        return AssignComponent(FractureLogic, go) as IFractureGenerator;
        
    }

    private Component AssignComponent(Type comp, GameObject go)
    {
        if (!go) return null;

        if (!typeof(Component).IsAssignableFrom(comp)) return null;
        
            
        if (go.GetComponent(comp) == null)
        {
            Debug.Log("[FractureManager] Adding : " + comp.Name + " to " + go );
            return go.AddComponent(comp);
        }
            
       
        Debug.Log("[FractureManager] " + go +  " has already right component : " + comp.Name);
        return go.GetComponent(comp);
    }



    public bool AsCorrectFractureComponent(GameObject go)
    {
        if (!go) return true; //returning false would imply we need to add component 

        var components = go.GetComponents<IFractureGenerator>();
        
        if (components.Length == 0) return false;  //no component 

        foreach (var component in components)
        {
            if (component == GetCurrentFrameworkFractureLogic()) return true;
        }

        return false;
    }

    public IFractureGenerator GetFractureComponent(GameObject go)
    {
        if (!go) return null; 

        Debug.Log("[FactureManager] Looking For :" +GetCurrentFrameworkFractureLogic().ToString() + " on " + go );
        
        var components = go.GetComponents<IFractureGenerator>();

        if (components.Length == 0) return null;

        foreach (var component in components)
        {
            if (component == GetCurrentFrameworkFractureLogic()) return component;
        }

        return AssignFractureComponent(go);
    }
    

}
