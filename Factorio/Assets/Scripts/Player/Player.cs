using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float sensitivityMagnitude = 0.01f;
    private bool isMoving = false;
    public bool IsMoving => isMoving;
    private Vector2 moveInput;
    Rigidbody2D rb;

    public static Action<Player> GivePlayerOnStart;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GivePlayerOnStart?.Invoke(this);
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, transform.position.z) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + (Vector2)movement);
        
        isMoving = moveInput.sqrMagnitude > sensitivityMagnitude;
    }

    private void Rotate()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;

            float newAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(newAngle);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        //Debug.Log("Move");
    }
}
