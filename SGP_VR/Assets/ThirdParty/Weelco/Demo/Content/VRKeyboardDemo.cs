using UnityEngine;
using UnityEngine.UI;
using Weelco.VRKeyboard;

namespace Weelco {

    public class VRKeyboardDemo : MonoBehaviour {

        public int maxOutputChars = 30;

        public Text inputFieldLabel;
        public VRKeyboardFull keyboardFull;
        public VRKeyboardExtra keyboardExtra;
        public VRKeyboardLite keyboardLite;        

        void Start() {
            if (keyboardFull) {
                keyboardFull.OnVRKeyboardBtnClick += HandleClick;
                keyboardFull.Init();
            }
            
            if (keyboardExtra) {
                keyboardExtra.OnVRKeyboardBtnClick += HandleClick;
                keyboardExtra.Init();
            }

            if (keyboardLite) {
                keyboardLite.OnVRKeyboardBtnClick += HandleClick;
                keyboardLite.Init();
            }            
        }

        void OnDestroy() {
            if (keyboardFull) {
                keyboardFull.OnVRKeyboardBtnClick -= HandleClick;
            }

            if (keyboardExtra) {
                keyboardExtra.OnVRKeyboardBtnClick -= HandleClick;
            }

            if (keyboardLite) {
                keyboardLite.OnVRKeyboardBtnClick -= HandleClick;
            }
        }        

        void OnGUI() {
            if (GUI.Button(new Rect(10, 10, 120, 30), "VR Keyboard Full")) {
                keyboardFull.gameObject.SetActive(true);
                keyboardExtra.gameObject.SetActive(false);
                keyboardLite.gameObject.SetActive(false);
            }

            if (GUI.Button(new Rect(140, 10, 130, 30), "VR Keyboard Extra")) {
                keyboardExtra.gameObject.SetActive(true);
                keyboardFull.gameObject.SetActive(false);
                keyboardLite.gameObject.SetActive(false);
            }

            if (GUI.Button(new Rect(280, 10, 120, 30), "VR Keyboard Lite")) {
                keyboardLite.gameObject.SetActive(true);
                keyboardFull.gameObject.SetActive(false);
                keyboardExtra.gameObject.SetActive(false);
            }
        }

        private void HandleClick(string value) {
            if (value.Equals(VRKeyboardData.BACK)) {
                BackspaceKey();
            }
            else if (value.Equals(VRKeyboardData.ENTER)) {
                EnterKey();
            }
            else {
                TypeKey(value);
            }
        }

        private void BackspaceKey() {
            if (inputFieldLabel.text.Length >= 1) {
                inputFieldLabel.text = inputFieldLabel.text.Remove(inputFieldLabel.text.Length - 1, 1);
            }
        }    

        private void EnterKey() {
            // Add enter key handler
        }

        private void TypeKey(string value) {
            char[] letters = value.ToCharArray();
            for (int i = 0; i < letters.Length; i++) {
                TypeKey(letters[i]);
            }
        }

        private void TypeKey(char key) {
            if (inputFieldLabel.text.Length < maxOutputChars) {
                inputFieldLabel.text += key.ToString();
            }
        }    
    }
}