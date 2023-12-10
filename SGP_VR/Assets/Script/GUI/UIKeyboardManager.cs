using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Weelco.VRKeyboard;

namespace UI
{
    public enum KeyboardType
    {
        Full,
        Compact,
        Numpad
    }

    public class UIKeyboard : SingletonMonoBehaviour<UIKeyboard>
    {
        [Serializable]
        public class Keyboard
        {
            [SerializeField] private KeyboardType _type;
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private VRKeyboardBase _VRKeyboard;

            public KeyboardType Type => _type;
            public CanvasGroup CanvasGroup => _canvasGroup;

            public VRKeyboardBase VRKeyboard => _VRKeyboard;
        }
        
        [SerializeField] private Keyboard[] _keyboards;
        [SerializeField] private float _fadeDuration = 3f;
        private Keyboard _currentKeyboard;
        public Keyboard CurrentKeyboard => _currentKeyboard;
        private TMP_InputField _currentTextField;
        private Coroutine _fadingOutCoroutine;
        
        [SerializeField] private bool _startHiden;

        public static Action<string> OnSubmit { get; set; }
        public static Action<bool> OnKeyboardToggle { get; set; }



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RunOnStart()
        {
            _instance = null;
            OnSubmit = null;
        }

        protected  void Awake()
        {
            RunOnStart();
        }

        void Start()
        {
            // Hide the keyboards
            if(!_startHiden) return;
            
            foreach (var keyboard in _keyboards)
            {
                if(!keyboard.CanvasGroup) continue;
                
                keyboard.CanvasGroup.alpha = 0f;
                keyboard.CanvasGroup.interactable = false;
                keyboard.CanvasGroup.blocksRaycasts = false;
            }
            
            
            
        }

        public static void Show(TMP_InputField textField, string text = "", KeyboardType keyboardType = KeyboardType.Full, bool fade = true)
        {
            if(Instance._currentKeyboard != null)
                Hide(false);

            if(!Instance.enabled || Instance._currentKeyboard != null)
                return;
            
            Instance.ShowInternal(textField, text, keyboardType, fade);
        }

        private void ShowInternal(TMP_InputField textField, string text, KeyboardType keyboardType, bool fade)
        {
            _currentTextField = textField;

            OnKeyboardToggle?.Invoke(true);

            // Prepopulate the text field with any provided text 
            if(!string.IsNullOrEmpty(text))
                _currentTextField.text = text;
            
            // If the keyboard is already visible, do nothing
            if (_currentKeyboard != null && _currentKeyboard.Type == keyboardType)
                return;
            
            // Get the keyboard canvas group
            var requestedKeyboard = _keyboards.FirstOrDefault(k => k.Type == keyboardType);
            if (requestedKeyboard == null)
            {
                Debug.LogError($"Keyboard type {keyboardType} is not supported");
                return;
            }
            
          
            // Set the current keyboard
            _currentKeyboard = requestedKeyboard;
            
            _currentKeyboard.VRKeyboard.gameObject.SetActive(true);
            _currentKeyboard.VRKeyboard.transform.localPosition = Vector3.zero;
            
            // Initialize the keyboard
            _currentKeyboard.VRKeyboard.Init();
            
            var canvasGroup = requestedKeyboard.CanvasGroup;

            // If we're currently fading out, stop it
            if (_fadingOutCoroutine != null)
            {
                Debug.Log($"Stopping fading out");
                StopCoroutine(_fadingOutCoroutine);
                _fadingOutCoroutine = null;
            }
            
            // If we're fading
            if (fade)
            {
                // Fade the keyboard in
                IEnumerator FadeIn()
                {
                    // Fade in the keyboard
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.gameObject.SetActive(true);
                    yield return canvasGroup.DOFade(1f, _fadeDuration).WaitForCompletion();
                }
                StartCoroutine(FadeIn());
            }
            // Otherwise
            else
            {
                // Show it immediately
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.gameObject.SetActive(true);
            }
            
            // Listen for keyboard events
            requestedKeyboard.VRKeyboard.OnVRKeyboardBtnClick += OnKeyPressed;
            
            // Give the input field focus
            // EventSystem.current.SetSelectedGameObject(_currentTextField.gameObject);
        }

        public static void Hide(bool fade = true)
        {
            if(!Instance.enabled)
                return;
            
            Instance.HideInternal(fade);
        }

        private void HideInternal(bool fade)
        {
            // If we have a current keyboard
            if (_currentKeyboard != null)
            {
                OnKeyboardToggle?.Invoke(false);

                var canvasGroup = _currentKeyboard.CanvasGroup;

                // Stop listening for keyboard events on that keyboard
                _currentKeyboard.VRKeyboard.OnVRKeyboardBtnClick -= OnKeyPressed;
                
                // If we should fade
                if (fade)
                {
                    IEnumerator FadeOut()
                    {
                        // Fade out the keyboard
                        yield return canvasGroup.DOFade(0f, _fadeDuration).WaitForCompletion();
                        canvasGroup.gameObject.SetActive(false);

                        // Disable the keyboard
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;
                        
                        _fadingOutCoroutine = null;
                    }

                    _fadingOutCoroutine = StartCoroutine(FadeOut());
                }
                else
                {
            
                    // Disable the keyboard
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
            else
                // Error
                Debug.LogError("No keyboard is currently active");

            //  Make sure the keyboard is Hiden 
            _currentKeyboard.VRKeyboard.gameObject.SetActive(false);
           
            // Clear the current keyboard
            _currentKeyboard = null;
        }

        private IEnumerator FadeInKeyboard(KeyboardType keyboardType)
        {
            // If the keyboard is already visible, do nothing
            if (_currentKeyboard != null && _currentKeyboard.Type == keyboardType)
                yield break;
            
            // Get the keyboard canvas group
            var requestedKeyboard = _keyboards.FirstOrDefault(k => k.Type == keyboardType);
            if (requestedKeyboard == null)
            {
                Debug.LogError($"Keyboard type {keyboardType} is not supported");
                yield break;
            }
            
            // Set the current keyboard
            _currentKeyboard = requestedKeyboard;
            
            // Initialize the keyboard
            _currentKeyboard.VRKeyboard.Init();
            
            // Fade in the keyboard
            var canvasGroup = requestedKeyboard.CanvasGroup;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.gameObject.SetActive(true);
            yield return canvasGroup.DOFade(1f, _fadeDuration).WaitForCompletion();
            
            // Listen for keyboard events
            requestedKeyboard.VRKeyboard.OnVRKeyboardBtnClick += OnKeyPressed;
            
            // Give the input field focus
            EventSystem.current.SetSelectedGameObject(_currentTextField.gameObject);
        }

        private void OnKeyPressed(string key)
        {
            switch (key)
            {
                case VRKeyboardData.BACK:
                    _currentTextField.ProcessEvent(Event.KeyboardEvent("backspace"));
                    _currentTextField.ForceLabelUpdate();
                    // _currentTextField.text = _currentTextField.text.Substring(0, _currentTextField.text.Length - 1);
                    break;
                case VRKeyboardData.ENTER:
                    OnEnterPressed();
                    break;
                default:
                    // If the key is uppercase
                    if (char.IsUpper(key[0]))
                    {
                        // Make a keyboard event for the key and give it the shift modifier
                        var keycode = $"#{key.ToLower()}";
                        Debug.Log($"Uppercase key {key} detected, sending keyboard event '{keycode}'");
                        
                        // HACK: This seems to be the only way to send a capital letter
                        var uppercaseKeyEvent = Event.KeyboardEvent("#t");
                        uppercaseKeyEvent.character = key[0];
                        
                        // uppercaseKeyEvent.modifiers |= EventModifiers.Shift;
                        _currentTextField.ProcessEvent(uppercaseKeyEvent);
                    }
                    else
                    {
                        // Make a keyboard event for the key
                        Debug.Log($"Lowercase key {key} detected");
                        var lowercaseKeyEvent = Event.KeyboardEvent(key);
                        _currentTextField.ProcessEvent(lowercaseKeyEvent);
                    }
                    _currentTextField.ForceLabelUpdate();
                    break;
            }
        }

        private IEnumerator FadeOutKeyboard()
        {
            // If we have a current keyboard
            if (_currentKeyboard != null)
            {
                // Fade out the keyboard
                var canvasGroup = _currentKeyboard.CanvasGroup;
                yield return canvasGroup.DOFade(0f, _fadeDuration).WaitForCompletion();
                canvasGroup.gameObject.SetActive(false);
                
                // Stop listening for keyboard events on that keyboard
                _currentKeyboard.VRKeyboard.OnVRKeyboardBtnClick -= OnKeyPressed;

                // Disable the keyboard
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                // Error
                Debug.LogError("No keyboard is currently active");
                yield break;
            }

            // Clear the current keyboard
            _currentKeyboard = null;
            
            yield return null;
        }

        public bool IsKeyboardShown() 
        {
            return _currentKeyboard != null;
        }

        private void HideImmediate()
        {
        }

        public void OnEnterPressed()
        {
            // Let any waiting callback know about the entered text
           
            OnSubmit?.Invoke(_currentTextField.text);
        }

        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.Return))
            //     OnEnterPressed();
        }

        public static void SetPasswordInput(bool active)
        {
            if(!Instance.enabled)
                return;
            
            Instance.SetPasswordInputInternal(active);
        }

        private void SetPasswordInputInternal(bool active)
        {
            if(!Instance.enabled)
                return;
            
            _currentTextField.inputType = active ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;
        }
    }
}
