using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class SGP_GrabTransformer : XRGeneralGrabTransformer
{
    public Action<XRGrabInteractable> OnGrabbableProcess => EventBus.OnGrabbableProcess;
    public Action<bool, XRGrabInteractable> OnDoubleGrab => EventBus.OnDoubleGrab;

    public int previousInteractorCount;
    public bool firstFrameSinceTwoHandeGab;


    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);
        OnGrabbableProcess?.Invoke(grabInteractable);
    }

    public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
        int count = grabInteractable.interactorsSelecting.Count;

        if (previousInteractorCount != count)
        {
            //# of grabbing hands has changed
            // we released or grabbed with other hand 

            if (count == 1)
            {

                // we release
                //do only once
                if (!firstFrameSinceTwoHandeGab) return;

                OnDoubleGrab?.Invoke(false, grabInteractable);
                firstFrameSinceTwoHandeGab = false;
            }

            else 
            {
                //we grabbed with other hand

                //do only once
                if (firstFrameSinceTwoHandeGab) return;
                OnDoubleGrab?.Invoke(true, grabInteractable);
                firstFrameSinceTwoHandeGab = true;
            }

        }
        else 
        {
            //# of grabbing hands has not changed
            //do nothing
        }

    }

    
    
}
