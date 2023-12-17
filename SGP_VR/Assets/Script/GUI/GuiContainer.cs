using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

public class GuiContainer : MonoBehaviour
{
    [SerializeField] private GameObject _guiMount;

    public GameObject GuiMount
    {
        get => _guiMount ? _guiMount : gameObject;
    }

    [SerializeField] protected GameObject _currentGui;

    public virtual GameObject CurrentGui
    {
        get => _currentGui;
        set => _currentGui = value;
    }

    [SerializeField]
    private Transform _keyboardMount;

    public bool forceKeepOpen = true;
    public Transform KeyboardMount => _keyboardMount ? _keyboardMount : this.transform;

    public void ToggleGui(GameObject gui, bool state, bool fade = false)
    {
        if (!gui) return;


        if (gui.activeInHierarchy == state) return;


        var guiLogic = gui.GetComponent<GUIBehaviour>();

        if (!guiLogic || !fade) gui.SetActive(state);
        else
        {
            if (state) gui.SetActive(state);
            guiLogic.Fade(state, 1f);
        }

    }

    public GUIBehaviour GetGuiScript(GameObject go)
    {
        if (!go) return null;
        return go.GetComponent<GUIBehaviour>();
    }

    public void SetGuiAlpha(GameObject gui, float alpha)
    {
        if (!gui) return;

        var guiLogic = GetGuiScript(gui);

        if (!guiLogic) return;
        guiLogic.SetAlpha(alpha);

    }

    public void SetMaxGuiAlpha(GameObject gui, float alpha, bool fade = true)
    {
        if (!gui) return;

        //Debug.Log("Setting alpha");
        var guiLogic = GetGuiScript(gui);

        if (!guiLogic) return;
        guiLogic.SetMaxAlpha(alpha);
        guiLogic.SetAlpha(alpha, fade);

    }


    public virtual void SnapGuiOnTriggerEnter(GUIBehaviour gui)
    {
        
    }


    public virtual void ReleaseGui(GUIBehaviour gui)
    {
    }

    public virtual bool IsAllowedToSnap(GUIBehaviour gui)
    {
       
        return true;
    }

    public void OnTriggerEnter(Collider other)
    {
      
    }

    public void OnTriggerExit(Collider other)
    {
       
    }
}
