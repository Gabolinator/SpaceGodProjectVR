using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXHandler : MonoBehaviour
{
    public FX fx; 
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    
    public AudioSource audioSource;
    bool initialized; 

    public void GetAllParticulesSystem()
    {
        particleSystems = new List<ParticleSystem>(GetComponents<ParticleSystem>());
    }

    public void Initialize(FX efx)
    {
        fx = efx;

        audioSource = GetComponent<AudioSource>();
       
        GetAllParticulesSystem();

        initialized = true;
    }

    public void DestroySelf(float delay = 0) 
    {
        UnRegisterSelf();
        StartCoroutine(DestroySelfCoroutine(delay));
    }

    private IEnumerator DestroySelfCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(this.gameObject);
    }

    public void UnRegisterSelf() 
    {
        FXManager.Instance.UnRegisterFX(this);
    }

    public void RegisterSelf()
    {
        FXManager.Instance.RegisterFX(this);
    }
    public void ToggleParticuleSystems(bool state) 
    {
        if (particleSystems.Count == 0) return;

        foreach (var partSystem in particleSystems) 
        {
            if(state) partSystem.Play();

            else partSystem.Stop();
        }
     }

    public bool IsAnyParticuleSystemPlaying()
    {
        if (particleSystems.Count == 0) return false;

        foreach (var partSystem in particleSystems)
        {
            if(partSystem.isEmitting) return true;
        }

        return false;
    }

    public void PlayAudio() 
    {
        PlayAudio(false);
    }

    public void PlayAudio(bool fromList,int index = 0) 
    {
        /*make sure we have an audio source*/
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        /*get audio clip from the fx list */
        if (fromList)
        {
            if (fx.audios.Count == 0) return;
            if (index < fx.audios.Count - 1) audioSource.clip = fx.audios[index];
        }

        /*get the clip from the audio source - if there isnt take one from the list - is possible*/
        else
        {
            if (audioSource.clip == null)
            {
                if (fx.audios.Count == 0)return;
                if (index < fx.audios.Count - 1) audioSource.clip = fx.audios[index];
            }
        }

        /*make sure we have a clip to play*/
        if (audioSource.clip == null) return;

        audioSource.Play();
    }

    public void StopAudio() 
    {
        if(audioSource == null) return ;
        if(!audioSource.isPlaying) return;

        audioSource?.Stop();
    }

    private void Start()
    {
       // Debug.Log("[FX handler] Start to exist");
        FX efx = new FX();
        if (!initialized) Initialize(efx);
    }

    private void OnDestroy()
    {
        FXManager.Instance.UnRegisterFX(this);
    }

    public void InitiateDestroy(float delay = 0)
    {
     if(delay != 0) DestroySelf(delay);

     if(particleSystems.Count == 0) return;
     foreach (var part in particleSystems)
     {
     
     }
     
    }
    
   
}
