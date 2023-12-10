
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SGPScreen : MonoBehaviour
{
   // public List<Button> _buttons;
   // public List<Slider> _sliders;
   // public List<Toggle> _toggles;
   // public List<DropdownField> _dropdowns;
   //
   // public List<Button> GetAllButtons() => GetAllComponentsOfType<Button>();
   // public List<Slider> GetAllSlider() => GetAllComponentsOfType<Slider>();
   // public List<Toggle> GetAllToggles() => GetAllComponentsOfType<Toggle>();
   // public List<DropdownField> GetAllDropdown() => GetAllComponentsOfType<DropdownField>();
  
   private TMP_InputField _currentlySelectedTextInput;
   [SerializeField] private Transform keyboardMount;
   
   
   private List<T> GetAllComponentsOfType<T>()
   {
     
      var comps = GetComponentsInChildren<T>();
      if (comps.Length == 0) return null;

      List<T> newList = new List<T>();
      newList.AddRange(comps);
      
      return newList;
   }

   public virtual void SetButtonColor(Button button, Color color)
   {
      if(!button) return;
      var buttonColors =  button.colors;
      buttonColors.normalColor = color;

      button.colors = buttonColors;
   }

   public virtual void UpdateText(TMP_Text text, string value)
   {
      if(!text) return;
      if (string.IsNullOrEmpty(value)) return;
      
      if(text.text == value) return;
      
      text.text = value;
   }

   public virtual void UpdateText(TMP_Text text, float value)
   {
      UpdateText(text, value.ToString(CultureInfo.CurrentCulture));
   }
   
   public virtual void UpdateText(TMP_Text text, int value)
   {
      UpdateText(text, value.ToString(CultureInfo.CurrentCulture));
   }


   public virtual void SetToggleValue(Toggle toggle, bool value)
   {
      if(toggle == null) return;

      toggle.isOn = value;
   }

   public virtual void SetSliderValue(Slider slider, float value)
   {
      if(slider == null) return;

      value = value < slider.minValue ? slider.minValue : value;
      value = value > slider.maxValue ? slider.maxValue : value;

      slider.value = value;

   }

   private void OnTextInputDeselected(TMP_InputField textInput)
   {
	   // if (_currentlySelectedTextInput == textInput)
	   // 	return;

	   _currentlySelectedTextInput = textInput;
	   Debug.Log($"Deselected text input {textInput.name}");
		
	   
	   UIKeyboard.Hide();
	   
	   UIKeyboard.OnSubmit -= OnKeyboardSubmit;
   }
   
   private void OnTextInputSelected(TMP_InputField textInput)
	{
		// if (_currentlySelectedTextInput == textInput)
		// 	return;

		_currentlySelectedTextInput = textInput;
		Debug.Log($"Selected text input {textInput.name}");
		
		UIKeyboard.Instance.gameObject.transform.parent = keyboardMount ? keyboardMount : transform;
		
		UIKeyboard.Instance.gameObject.transform.localPosition = Vector3.zero;
		//UIKeyboard.Instance.gameObject.transform.localScale = Vector3.one;
		
		UIKeyboard.Show(textInput, textInput.text,KeyboardType.Full);
		
		
		UIKeyboard.OnSubmit += OnKeyboardSubmit;
	}

	private void OnKeyboardSubmit(string input)
	{
		if (_currentlySelectedTextInput)
		{
			Debug.Log($"Submitted input {input} to {_currentlySelectedTextInput.name}");
			_currentlySelectedTextInput.text = input;
		
		}
		
		else Debug.Log($"Submitted input {input}");

		
		_currentlySelectedTextInput = null;
		UIKeyboard.Instance.gameObject.transform.parent = null;
		UIKeyboard.OnSubmit -= OnKeyboardSubmit;
		UIKeyboard.Hide(false);
	}

	private KeyboardType GetKeyboardType(TMP_InputField textInput)
	{
		switch (textInput.contentType)
		{
			case TMP_InputField.ContentType.Standard:
			case TMP_InputField.ContentType.Autocorrected:
			case TMP_InputField.ContentType.Alphanumeric:
			case TMP_InputField.ContentType.EmailAddress:
			case TMP_InputField.ContentType.Name:
			case TMP_InputField.ContentType.Custom:
			case TMP_InputField.ContentType.Password:
				return KeyboardType.Full;

			case TMP_InputField.ContentType.DecimalNumber:
				return KeyboardType.Compact;

			case TMP_InputField.ContentType.Pin:
			case TMP_InputField.ContentType.IntegerNumber:
				return KeyboardType.Numpad;

			default:
				throw new ArgumentOutOfRangeException(
					$"Unknown content type {textInput.contentType} when getting keyboard type");
		}
	}

	public virtual IEnumerator Show(Action onShowComplete = null)
	{
		// Show the screen
		// TODO: Add animations in child classes if needed
		gameObject.SetActive(true);

	
		
			// Hook up the OnSelect listeners on any text inputs on the screen to show the VR keyboard
			AddListenersToInputField();
		

		// TESTING: Simulating an animation delay
		yield return new WaitForEndOfFrame();
		onShowComplete?.Invoke();
	}

	public virtual IEnumerator Hide(Action onHideComplete = null)
	{
		
		
			// Hide the VR keyboard
			UIKeyboard.Hide();

			// Clear any OnSelect listeners on any text inputs on the screen
			RemoveListenersToInputField();
		

		// Hide the screen
		// TODO: Add animations
		gameObject.SetActive(false);

		// TESTING: Simulating an animation delay
		yield return new WaitForEndOfFrame();
		onHideComplete?.Invoke();
	}

	public void AddListenersToInputField()
	{
		foreach (var textInput in GetComponentsInChildren<TMP_InputField>(true))
		{
			textInput.onSelect.AddListener(_ => OnTextInputSelected(textInput));
			//textInput.onDeselect.AddListener(_=>OnTextInputDeselected(textInput));
		}
	}

	public void RemoveListenersToInputField()
	{
		foreach (var textInput in GetComponentsInChildren<TMP_InputField>(true))
		{
			textInput.onSelect.RemoveAllListeners();
		}
	}


	public virtual void Awake()
   {
      // _buttons = GetAllButtons();
      // _sliders = GetAllSlider();
      // _toggles = GetAllToggles();
      // _dropdowns = GetAllDropdown();
    
   }


	public virtual void OnEnable()
	{
		AddListenersToInputField();
	}

	public virtual void OnDisable()
	{
		RemoveListenersToInputField();
	}

  public void OnDestroy()
  {
	  RemoveListenersToInputField();
  }
}
