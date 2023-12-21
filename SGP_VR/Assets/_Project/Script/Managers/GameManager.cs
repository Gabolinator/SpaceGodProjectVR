using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    protected static GameManager _instance;
    public static GameManager Instance => _instance;

    public PlayerController localPlayer;

    private void Awake()
    {
        _instance = this;
    }

}
