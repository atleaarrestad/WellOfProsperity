using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Mover : MonoBehaviour
{
    //public ScriptableObject data;
    public int Speed;
    public Rigidbody2D rb;
    public InputActionReference inputMove;
    public InputActionReference inputAttack;
    private Vector2 moveVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveVector = inputMove.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveVector.normalized * Speed;
    }
    private void OnEnable()
    {
        inputAttack?.action?.Enable();
        inputMove?.action?.Enable();

        inputAttack.action.started += Attack;
    }
    private void OnDisable()
    {
        inputAttack.action.started -= Attack;
    }

    private void Attack(InputAction.CallbackContext context)
    {

    }
}
