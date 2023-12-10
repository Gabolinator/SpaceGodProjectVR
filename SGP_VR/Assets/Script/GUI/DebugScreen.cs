using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DebugScreen : GUIBehaviour
{
   [SerializeField]
   private TMP_Dropdown _collisionDropDown;

   private int _numberOfBodyToSpawn;
   [SerializeField]
   private Slider _numberOfBodySlider;
   [SerializeField]
   private TMP_Text _numberOfBodyValue;
   
   [SerializeField]
   private Slider _playerScaleSlider;
   [SerializeField]
   private TMP_Text _playerScaleValue;
   
   [SerializeField]
   private Slider _universeScaleSlider;
   [SerializeField]
   private TMP_Text _universeScaleValue;

   [SerializeField] 
   private Toggle _collisionToggle;
   
   public void OnPlayerScaleChange(float value)
   {

      var player = GameManager.Instance.localPlayer;
      player.ScalePlayer(value);
      UpdateText(_playerScaleValue, player.playerScale);
      
   }

   

   public void OnUniverseScaleChange(float value)
   {
      UniverseManager.Instance.ScaleUniverse(value);
      UpdateText(_universeScaleValue, UniverseManager.Instance.UniverseScale);
   }
   
   public void OnNumberOfBodyToSpawnChange(float value)
   {
      _numberOfBodyToSpawn = (int)value;
      UpdateText(_numberOfBodyValue,  _numberOfBodyToSpawn);
   }

   public void SpawnBody() => AstralBodiesManager.Instance.GenerateRandomBodies(UniverseManager.Instance.generationPrefs,_numberOfBodyToSpawn);
   
   
   public void DestroyAllBody() =>  AstralBodiesManager.Instance.DestroyAllBodies();


   public void OnToggleCollision(bool value) => CollisionManager.Instance.ForceDisableCollisions = value;
   


   
   
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
      
      SetToggleValue( _collisionToggle, CollisionManager.Instance.ForceDisableCollisions);
      
      SetSliderValue(_numberOfBodySlider, UniverseManager.Instance.numberOfBodyToGenerate);
      UpdateText(_numberOfBodyValue, _numberOfBodySlider.value);
     
      SetSliderValue(_playerScaleSlider, GameManager.Instance.localPlayer.playerScale);
      UpdateText(_playerScaleValue, _playerScaleSlider.value);
     
      SetSliderValue(_universeScaleSlider, UniverseManager.Instance.UniverseScale);
      UpdateText(_universeScaleValue, _universeScaleSlider.value);
   }
   
   
}
