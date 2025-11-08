using UnityEngine;

public class SyncAnimComponent : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ConveyorAnimationManager.Instance?.Register(animator);
    }

    private void OnDestroy()
    {
        ConveyorAnimationManager.Instance?.Unregister(animator);
    }
}
