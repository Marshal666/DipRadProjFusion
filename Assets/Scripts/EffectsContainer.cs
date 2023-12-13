using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsContainer : MonoBehaviour
{

    private static EffectsContainer _Instance;

    private static EffectsContainer Instance => _Instance;


    public ObjectHolder _ExplosionsHolder;

    public static ObjectHolder ExplosionsHolder => Instance._ExplosionsHolder;

    public static bool Initialized { get; private set; } = false;

    private void Awake()
    {
        _Instance = this;
        Initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
