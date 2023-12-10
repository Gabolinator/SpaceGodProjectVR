using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VisualisationScreen : GUIBehaviour
{

    [SerializeField] private Toggle _trajectoryToggle;
    [SerializeField] private Toggle _trailToggle;
    [SerializeField] private Button _trajectoryColorButton;
    [SerializeField] private Button _trailColorButton;
    [SerializeField] private FlexibleColorPicker _trailColorPicker;
    [SerializeField] private FlexibleColorPicker _trajectoryColorPicker;
    
    
    
   public void ToggleTrajectoryLine(bool value) => VisualIndicatorManager.Instance.ShowTrajectory = value; 
   
   public void ToggleTrailLine(bool value) => VisualIndicatorManager.Instance.ShowTrail = value;

   public void SetColorPickerColor(FlexibleColorPicker colorPicker, Color color)
   {
       if(!colorPicker) return;
       Debug.Log("Setting picker " +  colorPicker.name+ " color : " + color); 
       
       colorPicker.SetColor(color);

   }

   public void OnTrajectoryColorButtonClick()
   {
       bool state =  _trajectoryColorPicker.gameObject.activeSelf;
       //_trajectoryColorButton.gameObject.SetActive(state);
      
       
       ToggleTrajectoryColorPicker(!state);
      
       if (state) SetButtonColor(_trajectoryColorButton, _trajectoryColorPicker.color);
       
       else SetColorPickerColor(_trajectoryColorPicker, _trajectoryColorButton.colors.normalColor);   
        
   }
   
   public void OnTrailColorButtonClick()
   {
       bool state = _trailColorPicker.gameObject.activeSelf;
       //_trailColorButton.gameObject.SetActive(state);
      
       
       ToggleTrailColorPicker(!state);
       if(state) SetButtonColor(_trailColorButton, _trailColorPicker.color);
       else SetColorPickerColor(_trailColorPicker, _trailColorButton.colors.normalColor);   
   }

   public void ToggleTrailColorPicker(bool state)
   {
       if(!_trailColorPicker) return;
       if (state)
       {
           var canvas = canvasGroup.GetComponent<Canvas>();
           if(canvas) canvas.worldCamera = Camera.main;
       }

       else
       {
           var canvas = canvasGroup.GetComponent<Canvas>();
           if(canvas) canvas.worldCamera = null;
       }
       _trailColorPicker.gameObject.SetActive(state);
      // SetColorPickerColor(_trailColorPicker, _trailColorButton.image.color); 
       
      
       SubsribeToColorPickerEvent(_trailColorPicker, state, UpdateTrailButtonColor);
       SubsribeToColorPickerEvent(_trailColorPicker, state, UpdateTrailColor);
      
   }

   private void UpdateTrailButtonColor(Color color)
   {
       SetButtonColor(_trailColorButton, color);
   }

   private void UpdateTrajectoryButtonColor(Color color)
   {
       SetButtonColor(_trajectoryColorButton, color);
   }
   public void ToggleTrajectoryColorPicker(bool state)
   {
       if(!_trajectoryColorPicker) return;
       if (state)
       {
           var canvas = canvasGroup.GetComponent<Canvas>();
           if(canvas) canvas.worldCamera = Camera.main;
       }

       else
       {
           var canvas = canvasGroup.GetComponent<Canvas>();
           if(canvas) canvas.worldCamera = null;
       }

       _trajectoryColorPicker.gameObject.SetActive(state);
      
       
       SubsribeToColorPickerEvent(_trajectoryColorPicker, state, UpdateTrajectoryColor);
       SubsribeToColorPickerEvent(_trajectoryColorPicker, state, UpdateTrajectoryButtonColor);
       
    
   }
   private void SubsribeToColorPickerEvent(FlexibleColorPicker colorPicker, bool state, UnityAction<Color> colorEvent)
   {
       if (state) colorPicker.onColorChange.AddListener(colorEvent);
       else colorPicker.onColorChange.RemoveListener(colorEvent);
   }


   public void UpdateTrailColor(Color color) => VisualIndicatorManager.Instance.TrailColor = color;

   
   public void UpdateTrajectoryColor(Color color) => VisualIndicatorManager.Instance.TrajectoryColor = color;

   

   public override void Start()
   {
     base.Start();

    
     SetToggleValue(_trajectoryToggle, VisualIndicatorManager.Instance.ShowTrajectory);
     SetToggleValue(_trailToggle, VisualIndicatorManager.Instance.ShowTrail);
     
    
     SetButtonColor(_trajectoryColorButton, VisualIndicatorManager.Instance.TrajectoryColor);
     SetButtonColor(_trailColorButton, VisualIndicatorManager.Instance.TrailColor);
   }
}
