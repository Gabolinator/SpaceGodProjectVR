using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;
using UnityEngine.UIElements;

public class SGPScreen : MonoBehaviour
{
   public List<Button> _buttons;
   public List<Slider> _sliders;
   public List<Toggle> _toggles;
   public List<DropdownField> _dropdowns;

   public List<Button> GetAllButtons() => GetAllComponentsOfType<Button>();
   public List<Slider> GetAllSlider() => GetAllComponentsOfType<Slider>();
   public List<Toggle> GetAllToggles() => GetAllComponentsOfType<Toggle>();
   public List<DropdownField> GetAllDropdown() => GetAllComponentsOfType<DropdownField>();
   
   private List<T> GetAllComponentsOfType<T>()
   {
     
      var comps = GetComponentsInChildren<T>();
      if (comps.Length == 0) return null;

      List<T> newList = new List<T>();
      newList.AddRange(comps);
      
      return newList;
   }

   public virtual void Awake()
   {
      // _buttons = GetAllButtons();
      // _sliders = GetAllSlider();
      // _toggles = GetAllToggles();
      // _dropdowns = GetAllDropdown();
      
   }

 


}
