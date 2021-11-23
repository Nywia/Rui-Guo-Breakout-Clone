using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class ScrPaddle : NetworkBehaviour
{
    [Header("Paddle Properties")]
    public GameObject BallPrefab;

    [SerializeField] private LayerMask WallLayer;
    [SerializeField] private float Speed;

    private ScrBall Ball;
    private Rigidbody RB;
    private Renderer PaddleRenderer;

    private Vector3 PaddleSize;
    private Vector3 BallOffset;

    private float MovementInput;

    [Header("Debugging")]
    [SerializeField] private bool ShowGizmos;

    // Start is called before the first frame update
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // Initialize
        RB = GetComponent<Rigidbody>();
        PaddleRenderer = GetComponent<Renderer>();
        PaddleSize = PaddleRenderer.bounds.size;
        BallOffset = new Vector3(0.0f, 1.15f, 0.0f);
        CmdFindBall();
    }

    /// <summary>
    ///     Server tells all the clients to find
    ///     the nearest ball to them
    /// </summary>
    [ClientRpc]
    private void RpcFindBall()
    {
        float closestDistance = 1000.0f;

        // Find the closest ball to the paddle
        foreach(ScrBall ball in FindObjectsOfType<ScrBall>())
        {
            float ballDistance = Vector3.Distance(ball.transform.position, transform.position); 

            if (ballDistance < closestDistance)
            {
                // Set it as the paddle's ball
                closestDistance = ballDistance;
                Ball = ball;
            }
        }

        // Set it's spawn to be slightly offset to the paddle
        Ball.SpawnLocation = transform.TransformPoint(BallOffset);
    }

    /// <summary>
    ///     Asks the server to find the ball
    /// </summary>
    [Command]
    private void CmdFindBall() => RpcFindBall();

    // Update is called once per frame
    private void Update()
    {
        // Ensure only those with authority can control
        if (!hasAuthority || !Ball)
        {
            return;
        }
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Ball.LaunchBall();
        }
        
        Ball.SpawnLocation = transform.TransformPoint(BallOffset);

        // Get raw input (For keyboards)
        MovementInput = Input.GetAxisRaw("Horizontal");
    }

    // Fixed update for rigidbodies
    void FixedUpdate()
    {
        // Ensure only those with authority can control
        if (!hasAuthority || !Ball)
        {
            return;
        }

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
