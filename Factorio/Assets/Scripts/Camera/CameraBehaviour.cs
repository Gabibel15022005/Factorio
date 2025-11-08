using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Player player;
    private bool followPlayer = true;
    private Transform target;


    [Header("Camera Follow Settings")]
    [SerializeField] float followSpeed = 5f;

    void OnEnable()
    {
        Player.GivePlayerOnStart += GetPlayer;
    }

    void OnDisable()
    {
        Player.GivePlayerOnStart -= GetPlayer;
    }
    
    void GetPlayer(Player newPlayerRef)
    {
        player = newPlayerRef;
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("No player Referenced");
            return;
        }

        if (followPlayer)
            FollowTarget(player.transform);
        else if (target != null)
            FollowTarget(target);

    }

    void FollowTarget(Transform targetTransform)
    {
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3 (targetTransform.position.x, targetTransform.position.y, transform.position.z),
            followSpeed * Time.deltaTime
        );
    }

}
