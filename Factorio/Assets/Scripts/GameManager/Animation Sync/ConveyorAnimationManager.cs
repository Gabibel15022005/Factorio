using UnityEngine;
using System.Collections.Generic;

public class ConveyorAnimationManager : MonoBehaviour
{
    public static ConveyorAnimationManager Instance;

    private List<Animator> conveyors = new();
    private float globalTime = 0f;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(Animator anim)
    {
        if (!conveyors.Contains(anim))
            conveyors.Add(anim);
    }

    public void Unregister(Animator anim)
    {
        conveyors.Remove(anim);
    }

    private void Update()
    {
        globalTime += Time.deltaTime;
        float normalizedTime = globalTime % 1f;

        foreach (var anim in conveyors)
        {
            if (anim != null)
                anim.Play("Idle", 0, normalizedTime);
        }
    }
}