using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Hero : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        input = Vector2.zero;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;

        input = input.normalized;

        if (input.x != 0f && sr != null)
        {
            sr.flipX = input.x < 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}