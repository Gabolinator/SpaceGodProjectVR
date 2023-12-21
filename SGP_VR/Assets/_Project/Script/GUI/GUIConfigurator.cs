using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public class GUIConfigurator : GUIBehaviour
{
    public TextMeshProUGUI bodyName;
    public GameObject mount;

    public virtual void UpdateGUIText(TextMeshProUGUI textElement, string newText)
    {
        if (textElement == null) return;

        textElement.text = newText;

    }

    public virtual void UpdateGUIText(string newText)
    {
        UpdateGUIText(bodyName, newText);

    }


 
}
