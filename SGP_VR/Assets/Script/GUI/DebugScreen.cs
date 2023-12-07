using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static CollisionType;

public class DebugScreen : GUIBehaviour
{
   [SerializeField]
   private TMP_Dropdown _collisionDropDown;

   private int numberOfBodyToSpawn;
   
   public void OnPlayerScaleChange(float value)
   {
   }
   
   public void OnUniverseScaleChange(float value)
   {
   }
   
   public void OnNumberOfBodyToSpawnChange(float value)
   {
      
      numberOfBodyToSpawn = (int)value;
   }

   public void SpawnBody() => AstralBodiesManager.Instance.GenerateRandomBodies(UniverseManager.Instance.generationPrefs,numberOfBodyToSpawn);
   
   
   public void DestroyAllBody() =>  AstralBodiesManager.Instance.DestroyAllBodies();
 

   public void OnToggleCollision(bool value)
   {
      
   }

   public void PopulateCollisionTypeDropDown()
   {
      if(_collisionDropDown == null) return;

      //var collisionManager = CollisionManager.Instance;
      CollisionType[] collisionTypes = (CollisionType[])Enum.GetValues(typeof(CollisionType));
      if(collisionTypes.Length ==0) return;
      
    // List<string> collisionNames = new List<string>();
     var options = new List<TMP_Dropdown.OptionData>();
     var newOption = new TMP_Dropdown.OptionData();
     newOption.text = "Dont Force";
     options.Add(newOption);
     
     foreach (var collision in collisionTypes)
      {
         newOption = new TMP_Dropdown.OptionData();
         newOption.text = collision.ToString();
         options.Add(newOption);
      }


      _collisionDropDown.options = options;

   }

   
   public void OnCollisionDropDownChange(int value)
   {
   
      CollisionType[] collisionTypes = (CollisionType[])Enum.GetValues(typeof(CollisionType));
      CollisionType collisionType = value == 0 ?CollisionType.Unknown : collisionTypes[value-1];
      
      bool enableTestCollision = (collisionType != CollisionType.Unknown);
      CollisionManager.Instance.testMode = enableTestCollision;
      
      if(!enableTestCollision) return;
      
      CollisionManager.Instance.testCollisionType = collisionType;
   }

   public void DestroyAllFx() => FXManager.Instance.DestroyAllFX();
   

   public override void Start()
   {
      base.Start();
      PopulateCollisionTypeDropDown();
   }
}
