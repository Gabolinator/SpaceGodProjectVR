using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public enum FXCategory 
{
    Collision,
    BodyEditing, 
    UI,
    Other
   
}

public enum FXElement
{
    Audio,
    Visual,
    All
}

[System.Serializable]
public struct FX
{

    public List<GameObject> fxPrefabs;
    public List<AudioClip> audios;
    //public List<ParticleSystem> particleSystems;
    public string keyword;
    [HideInInspector]
    public FXCategory category;

 
}

[System.Serializable]
public struct FXDictionnairy
{
    public List<FX> fxList;
    public FXCategory category;
}

public class FXManager : MonoBehaviour
{
    private static FXManager _instance;
    public static FXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FXManager>();

                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("FX_Manager");
                    _instance = singletonGO.AddComponent<FXManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private List<FXDictionnairy> fxDictionnairy = new List<FXDictionnairy>();

    public List<FXHandler> activeFX = new List<FXHandler>();

    public void ToggleFX(FXCategory category, FXElement whatToToggle, string keyword, Vector3 position, Quaternion rotation, Transform transform ,bool parent ,bool state = true, bool destroyAfterPlay = true)
    {
        Debug.Log("[FX Manager] Keyword : " + keyword);
        
        var fxList = new List<FX>();
        //FXCategory[] categories = (FXCategory[])System.Enum.GetValues(typeof(FXCategory));  
        foreach (var entry in fxDictionnairy)
        {
            if (entry.category != category) continue;

            fxList = entry.fxList;
            break;
        }

        if (fxList.Count == 0) return;

        var fx = new List<FX>();
        if (string.IsNullOrEmpty(keyword)) fx = fxList;

        foreach (var element in fxList)
        {
            if (element.keyword.Contains(keyword))
            {
                var cloneFX = new FX();
                cloneFX = element;
                cloneFX.category = category;
                fx.Add(cloneFX);
            }
        }

        ToggleFX(fx, whatToToggle, position, rotation, transform, parent, state, destroyAfterPlay);

    }

    public void ToggleFX(FXCategory category, FXElement whatToToggle, string keyword, Transform transform , bool parent , bool state = true, bool destroyAfterPlay = true ) 
    {
        if (fxDictionnairy.Count == 0) return;

        if (!state) ToggleOffFX(transform);
        if(parent) ToggleFX(category, whatToToggle, keyword, transform, state);
        else ToggleFX(category, whatToToggle, keyword, transform.position, transform.rotation, transform , parent, state);
     }

    public void ToggleOffFX(Transform transform, string keyword = null)
    {
        if (activeFX.Count == 0) return;
        List<FXHandler> fxHandlers = new List<FXHandler>(transform.GetComponentsInChildren<FXHandler>());
        List <FXHandler> activeFXClone = new List<FXHandler>(activeFX);


        if (fxHandlers.Count > 0) 
        {
            foreach (var handler in fxHandlers) 
            {


                if(!string.IsNullOrEmpty(keyword)) if(handler.fx.keyword != keyword) continue;
             
                if(activeFXClone.Contains(handler)) 
                {
                  handler.DestroySelf();
                }  
                
                
            }
        }

    }

    public void ToggleFX(List<FX> fxList, FXElement whatToToggle, Transform transform, bool parent , bool state, bool destroyAfterPlay)
    {
        
    
        ToggleFX(fxList, whatToToggle, transform.position, transform.rotation, transform, parent ,state, destroyAfterPlay);
       
    }
   

    public void ToggleFX(List<FX> fxList, FXElement whatToToggle, Vector3 position, Quaternion rotation, Transform transform, bool parent, bool state, bool destroyAfterPlay)
    {
        if (fxList.Count == 0) return;

        int index = 0;
        if (fxList.Count != 1) index = UnityEngine.Random.Range((int)0, (int)(fxList.Count - 1));

        ToggleFX(fxList[index], whatToToggle, position, rotation, transform ,parent, state, destroyAfterPlay);
    }

    public void ToggleFX(FX fx, FXElement whatToToggle, Vector3 position, Quaternion rotation, Transform transform, bool parent ,bool state,bool destroyAfterPlay) 
    {
        FXHandler fxHandler;
        switch (whatToToggle)
        {

            case FXElement.All:
                if(fx.fxPrefabs.Count == 0) return;
                foreach (var fxPrefab in fx.fxPrefabs)
                {
                    if(parent) fxHandler = SpawnFXPrefab(fxPrefab,transform);
                    else fxHandler = SpawnFXPrefab(fxPrefab, position, rotation);
                    fxHandler.Initialize(fx);
                    //PlayAudio(fx.audios, true);
                    fxHandler.PlayAudio();
                    activeFX.Add(fxHandler);
                }
                

                break;

            case FXElement.Audio:
                PlayAudio(transform.GetComponent<AudioSource>(), fx.audios, true);

                break;
            case FXElement.Visual:
                if(fx.fxPrefabs.Count == 0) return;
                foreach (var fxPrefab in fx.fxPrefabs)
                {
                    if (parent) fxHandler = SpawnFXPrefab(fxPrefab, transform);
                    else fxHandler = SpawnFXPrefab(fxPrefab, position, rotation);
                    fxHandler.Initialize(fx);
                    activeFX.Add(fxHandler);
                }

                break;

            default:
                break;

                fxHandler.InitiateDestroy();
            
        }
    }

    public void ToggleFX(FX fx, FXElement whatToToggle, Transform transform, bool parent ,bool state,bool destroyAfterPlay) 
    {
        if (transform == null) return;
        ToggleFX(fx, whatToToggle, transform.position, transform.rotation, transform, parent, state, destroyAfterPlay);
       
    }

    public void ToggleParticulesSystems(List<ParticleSystem> partSystems, bool state) 
    {
        
    }

    public void PlayAudio(AudioSource source, List<AudioClip> audioClips, bool state)
    {

    }
    public FXHandler SpawnFXPrefab(GameObject fxPrefab, Transform transform) 
    {
        var prefabClone = Instantiate(fxPrefab, transform);
        var fxHandler = prefabClone.GetComponent<FXHandler>();
        if (!fxHandler) fxHandler = prefabClone.AddComponent<FXHandler>();


        return fxHandler;
    }

    public FXHandler SpawnFXPrefab(GameObject fxPrefab, Vector3 position, Quaternion rotation)
    {
        var prefabClone = Instantiate(fxPrefab, position, rotation);
        var fxHandler = prefabClone.GetComponent<FXHandler>();
        if (!fxHandler) fxHandler = prefabClone.AddComponent<FXHandler>();

        
        return fxHandler;
    }

    public void RegisterFX(FXHandler fx)
    {
        if (!activeFX.Contains(fx)) activeFX.Add(fx);
    }

    public void UnRegisterFX(FXHandler fx)
    {
        if (activeFX.Contains(fx)) activeFX.Remove(fx);
    }

    public void DestroyFX(FXHandler fx) 
    {
        fx.DestroySelf();
    }

    public void DestroyAllFX()
    {
        if (activeFX.Count == 0) return;
        var cachedList = new List<FXHandler>();
        cachedList.AddRange(activeFX);
            
        foreach (var fx in cachedList)
        {
           DestroyFX(fx);
        }
    }

    private void OnEnable()
    {
        EventBus.OnCollision += ToggleCollisionFX;
        EventBus.OnCollisionProcessed += ToggleCollisionFX;
    }

    private void OnDisable()
    {
        EventBus.OnCollision-= ToggleCollisionFX;
        EventBus.OnCollisionProcessed -= ToggleCollisionFX;
    }
    
    private void ToggleCollisionFX(CollisionData collisionData)
    {
        var position = collisionData._impactPoint.point;
        bool canPlayerSee = false;
        
        if (!collisionData._target)
        {
            Debug.LogWarning("[FX manager] Trying to access collisionData._target but its been destroyed " );
        }
        else
        {
            canPlayerSee = collisionData._target.CanPlayerSee();
        }

        
        /*dont do any fx if player not there*/
        if( !canPlayerSee) return;

        if (!collisionData._projectile)
         {
             Debug.LogWarning("[FX manager] Trying to access collisionData._projectile but its been destroyed " );
         }
           
        var angle = collisionData._projectile ? collisionData._projectile.Velocity.normalized *-1 : collisionData._impactPoint.normal*-1;

        Quaternion rotation = Quaternion.Euler(angle);
        

        string keyword = collisionData.processed ?  collisionData._collisionType.ToString() : "Impact";
       
        
       ToggleFX(FXCategory.Collision, FXElement.All, keyword ,position, rotation, collisionData._target ? collisionData._target.transform : null , false);
    }

   
   
}
