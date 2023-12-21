using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class FXHandler : MonoBehaviour
{
    public FX fx;
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private List<AudioClip> _audioClips = new List<AudioClip>();
    [SerializeField] private GameObject _audioContainer;

    public GameObject AudioContainer
    {
        get
        {

            if (!_audioContainer)
            {
                var newContainer = new GameObject("FX_Audio");
                newContainer.transform.parent = this.transform;
                newContainer.transform.localPosition = Vector3.zero;
                newContainer.transform.localRotation = Quaternion.identity;

                _audioContainer = newContainer;
            }

            return _audioContainer;

        }

    }

    [SerializeField] private GameObject _visualFXContainer;

    public GameObject VisualFXContainer
    {
        get
        {

            if (!_visualFXContainer)
            {
                var newContainer = new GameObject("FX_Visual");
                newContainer.transform.parent = this.transform;
                newContainer.transform.localPosition = Vector3.zero;
                newContainer.transform.localRotation = Quaternion.identity;

                _visualFXContainer = newContainer;
            }

            return _visualFXContainer;

        }

    }

   
   
    
    public List<AudioSource> audioSources;
    bool initialized;

    public void GetAllParticulesSystem(GameObject parent)
    {
        if (!parent) return;

        particleSystems.AddRange(parent.GetComponentsInChildren<ParticleSystem>());
        
    }

    public void Initialize(FX efx)
    {
        fx = efx;

        _audioClips = GetAudioClips(efx);
        
        audioSources = new List<AudioSource>(AudioContainer.GetComponents<AudioSource>());

        /*make sure we have right amount of audio source*/
        AddAudioSources();
        
        AddClipToAudioSources();

        SpawnFXPrefab(efx.fxPrefabs, efx.particleSelectionMode);
        
        GetAllParticulesSystem(VisualFXContainer.gameObject);

        initialized = true;
    }

    private void SpawnFXPrefab(List<GameObject> fxPrefabs, SelectionMode particleSelectionMode)
    {
        if(fxPrefabs.Count == 0 || particleSelectionMode == SelectionMode.None) return;
        
        List<GameObject> visualFXs = new List<GameObject>();
        
        if (particleSelectionMode == SelectionMode.PlayAll) visualFXs = fxPrefabs;
        else visualFXs.Add(SelectParticuleSystem(fxPrefabs, particleSelectionMode));

        if(visualFXs.Count ==0) return;
        
        foreach (var visualFX in visualFXs)
        {
            var newFX = Instantiate(visualFX, VisualFXContainer.transform);
        }
        
    }

    
    private GameObject SelectParticuleSystem(List<GameObject> fxPrefabs, SelectionMode particleSelectionMode)
    {
        if (fxPrefabs.Count == 0) return null;
        
        switch (particleSelectionMode)
        {
            case SelectionMode.Random:
                return fxPrefabs[ UnityEngine.Random.Range((int)0, (int)(fxPrefabs.Count - 1))];
 
            case SelectionMode.TakeLast:
                return fxPrefabs[(int)(fxPrefabs.Count - 1)];
               
            
            case SelectionMode.TakeFirst:
                return fxPrefabs[0];
            
            default:
                return null;

        }
    }
    
    private List<AudioClip> GetAudioClips(FX efx)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        if (fx.audios.Count == 0) return null;

        foreach (var audioClip in fx.audios)
        {
            clips.Add(audioClip);
        }

        return clips;

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
            if (state) partSystem.Play();

            else partSystem.Stop();
        }
    }

    public bool IsAnyParticuleSystemPlaying()
    {
        if (particleSystems.Count == 0) return false;

        foreach (var partSystem in particleSystems)
        {
            if (partSystem.isEmitting) return true;
        }

        return false;
    }

    public void PlayAudio()
    {
        PlayAudioAtIndex();
    }

    public void PlayAll()
    {
        /*nothing to play*/
        if(_audioClips.Count ==0 || audioSources.Count ==0) return;

        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
            
           // Debug.Log("[FX Handler] playing: " + source.clip );
            source.Play();
            
            }
        }
        
       
    }

    private void AddClipToAudioSources()
    {
        if(_audioClips.Count == 0 || audioSources.Count ==0) return;

        int i = 0;
        foreach (var source in audioSources)
        {
            source.clip = _audioClips[i];
            i++;
        }
    }

    public void AddAudioSources()
    {
        if(_audioClips.Count == 0) return;
        
        /*make sure we have right amount of audio source*/
        int dif = _audioClips.Count - audioSources.Count ;
        
        Debug.Log("[FX handler] dif:" + dif);
        if(dif == 0) return;
        
            
            /*we are missing audio sources*/
            if (dif > 0) 
            {
                for (int i = 0; i < dif; i++)
                {
                    audioSources.Add(AudioContainer.AddComponent<AudioSource>());
                }
            }
            
            /*we have too many*/
            else{    
                for (int i = audioSources.Count-1; i > audioSources.Count-math.abs(dif); i--)
                {
                    var audioSource = audioSources[i];
                    audioSources.Remove(audioSource);   
                    Destroy(audioSource);
                }
                
            }
    }

   
    
    public void PlayAudioAtIndex(int index = 0) 
    {
        /*make sure we have an audio source*/
        if (audioSources.Count == 0)
        {
            audioSources.Add(AudioContainer.AddComponent<AudioSource>());
        }
        
        var audioSource = index < audioSources.Count - 1 ?  audioSources[0] : audioSources[index]; 
        

        /*make sure we have a clip to play*/
        if (audioSource.clip == null) return;

        audioSource.Play();
    }

    public void StopAll()
    {
        if(_audioClips.Count == 0 || audioSources.Count ==0) return;
        
        foreach (var source in audioSources)
        {
            if(source.isPlaying) source.Stop();
        }
    }
    
    public void StopAudio(int index = 0) 
    {
        if (audioSources.Count == 0) return;
        if (audioSources.Count <= index) return;
        
        var audioSource = audioSources[index];
        
        if(audioSource == null) return ;
        if(!audioSource.isPlaying) return;

        audioSource.Stop();
    }

    private void Start()
    {
       // Debug.Log("[FX handler] Start to exist");
       // FX efx = new FX();
        if (!initialized) Initialize(fx);
        PlayAudio(fx.audioSelectionMode);
    }

    private void PlayAudio(SelectionMode audioSelectionMode)
    {
        switch (audioSelectionMode)
        {
            case SelectionMode.Random:
               PlayAudioAtIndex(UnityEngine.Random.Range((int)0, (int)(_audioClips.Count - 1)));
                break;
            case SelectionMode.PlayAll:
                PlayAll();
                break;
            case SelectionMode.TakeFirst:
                PlayAudioAtIndex();
                break;
            case SelectionMode.TakeLast:
                PlayAudioAtIndex(_audioClips.Count - 1);
                break;
        }
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
