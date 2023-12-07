using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallBackScreen : GUIBehaviour
{
    private GUIManager GuiManager => GUIManager.Instance;
    //public List<GameObject> instantiatedGuis = new List<GameObject>();

    public void OpenScreen(string screenName) => GuiManager.OpenScreen(screenName);

    
}
