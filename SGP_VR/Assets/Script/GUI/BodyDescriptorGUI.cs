using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BodyDescriptorGUI : GUIConfigurator
{
    public AstralBodyDescriptor _descriptor;
    private bool _editMode;
    private string _predictedBodyString;


    public void SetEditMode(bool state) 
    {
        _editMode = state;
    }

    public void UpdateGUIText(AstralBodyDescriptor descriptor)
    {
        _descriptor = descriptor;
        string newText = BuildStringFromBodyDescriptor(descriptor, descriptor._velocity, descriptor._angularVelocity, _editMode);
        UpdateGUIText(bodyName, newText);

    }

    public void UpdateGUIText(AstralBodyHandler bodyHandler, AstralBody body)
    {
        _descriptor = bodyHandler.bodyDescriptor;
        UpdateGUIText(_descriptor);
    }

   

    public string PrintPredictedBody(GeneratedBody generatedBody) 
    {
        if (generatedBody == null) return "";
        if (generatedBody.astralBody == null) return "";

        
        return generatedBody.astralBody.BodyType.ToString() + " / " + generatedBody.astralBody.SubType;
    }

    public void UpdatePredictBody(AstralBodyHandler bodyHandler, GeneratedBody predictedBody) 
    {
        if (_descriptor._id != bodyHandler.bodyDescriptor._id )  return;

        if (predictedBody == null) _predictedBodyString = "";

        else _predictedBodyString = PrintPredictedBody(predictedBody);
    }

    private void SwitchToEditMode(bool editMode , AstralBodyHandler bodyHandler)
    {
        if (_descriptor._id != bodyHandler.bodyDescriptor._id) return;

      
        string newText = BuildStringFromBodyDescriptor(bodyHandler.bodyDescriptor, bodyHandler.Velocity, bodyHandler.AngularVelocity, _editMode = editMode);
        UpdateGUIText(bodyName, newText);
    }

    public void UpdateGUIText(AstralBodyHandler bodyHandler, Vector3 velocity, Vector3 angularVelocity) 
    {
        if (_descriptor._id != bodyHandler.bodyDescriptor._id) return;

        string newText = BuildStringFromBodyDescriptor(bodyHandler.bodyDescriptor, velocity, angularVelocity, _editMode, _predictedBodyString);
        UpdateGUIText(bodyName, newText);
    }

    private string BuildStringFromBodyDescriptor(AstralBodyDescriptor descriptor, Vector3 Velocity, Vector3 AngularVelocity, bool editMode = false, string predictedBody = "")
    {
        var sb = new StringBuilder(500);
        if(editMode) sb.AppendLine($"EDIT MODE\n");
        sb.AppendLine($"Name: {descriptor._bodyType} : {descriptor._id}");
        sb.AppendLine($"SubType:  {descriptor._subType} ");
        sb.AppendLine($"Mass: { FormatValue(descriptor._mass, UniverseManager.Instance.PhysicsProperties.MassFactor)} Kg");
        sb.AppendLine($"Radius: { FormatValue(descriptor._radius, UniverseManager.Instance.PhysicsProperties.DistanceFactor)} m");

        if (descriptor._density > 1E6) sb.AppendLine($"Density: {FormatValue(descriptor._density, 1)} Kg/m^3");
        else if (descriptor._density < 0.01) sb.AppendLine($"Density: {descriptor._density.ToString("E2")} Kg/m^3");
        else sb.AppendLine($"Density: {descriptor._density.ToString("F2")} Kg/m^3");

        sb.AppendLine($"Velocity: {FormatValue(Velocity.magnitude, UniverseManager.Instance.PhysicsProperties.DistanceFactor)} m/s");
        sb.AppendLine($"Angular Velocity: {AngularVelocity.magnitude} degres/s");

        if(editMode && !string.IsNullOrEmpty(predictedBody)) sb.AppendLine($"Predicted Body: {predictedBody}" ); 

        return sb.ToString();
    }


    private string FormatValue(double value, double scaleFactor)
    {
        //Debug.Log("value :" + value + " scaleFactor " + scaleFactor);
        double scaledValue = value * scaleFactor;
        string formattedValue = scaledValue.ToString("E2");
        return formattedValue;
    }

    private void OnEnable()
    {
        EventBus.OnAstralBodyAnyVelocitiesChanged += UpdateGUIText;
        EventBus.OnBodyDescriptorUpdated += UpdateGUIText;
        EventBus.OnBodyEdit += SwitchToEditMode;
        EventBus.OnPredictBody += UpdatePredictBody;
    }

    private void OnDisable()
    {
        EventBus.OnAstralBodyAnyVelocitiesChanged -= UpdateGUIText;
        EventBus.OnBodyEdit -= SwitchToEditMode;
        EventBus.OnPredictBody -= UpdatePredictBody;
    }

  
}
