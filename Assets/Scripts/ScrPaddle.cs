using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScrPaddle : MonoBehaviour
{
    [SerializeField] private GameObject Ball;
    [SerializeField] private LayerMask WallLayer;
    [SerializeField] private float Speed;

    private Rigidbody RB;
    private Vector3 PaddleSize;
    private float MovementInput;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize
        RB = GetComponent<Rigidbody>();
        PaddleSize = GetComponent<Renderer>().bounds.size;

        if (!Ball)
        {
            // Get ball if it isn't referenced
            Ball = FindObjectOfType<ScrBall>().gameObject;
        }
    }

    private void Update()
    {
        // Get raw input (For keyboards)
        MovementInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyUp(KeyCode.Space))
        {
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Let paddle move right if it's not hitting right wall and vice versa
        if (MovementInput > 0 && !HitWallRight() ||
            MovementInput < 0 && !HitWallLeft())
        {
            RB.MovePosition(transform.position + (new Vector3(MovementInput, 0.0f) * Speed * Time.deltaTime));
        }
    }

    /// <summary>
    ///     Returns true if paddle has hit left wall
    /// </summary>
    /// <returns></returns>
    private bool HitWallLeft()
    {
        return Physics.Raycast(transform.position, -transform.right, (PaddleSize.x / 2) + 0.1f, WallLayer);
    }

    /// <summary>
    ///     Returns true if paddle has hit right wall
    /// </summary>
    /// <returns></returns>
    private bool HitWallRight()
    {
        return Physics.Raycast(transform.position, transform.right, (PaddleSize.x / 2) + 0.1f, WallLayer);
    }


    /// <summary>
    ///     Provide some good default 
    ///     values on reset
    /// </summary>
    private void Reset()
    {
        Speed = 1.0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = HitWallLeft() ? Color.black : Color.cyan;
        Gizmos.DrawRay(transform.position, -transform.right * (PaddleSize.x / 2 + 0.1f));

        Gizmos.color = HitWallRight() ? Color.black : Color.cyan;
        Gizmos.DrawRay(transform.position, transform.right * (PaddleSize.x / 2 + 0.1f));
    }
}
