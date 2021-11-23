using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class ScrBall : NetworkBehaviour
{
    public Vector3 SpawnLocation;

    [Header("Ball Properties")]
    [SerializeField] private float Speed;
    [SerializeField] private float MaxShootAngle;

    private Rigidbody RB;

    private float ShootAngle;
    private bool Launched;

    [Header("Debugging")]
    [SerializeField] bool ShowDebugMessages;
    [SerializeField] bool ShowGizmos;
    [Space(10)]
    [SerializeField] private Color MaxShootAngleColour;
    [Space(5)]
    [SerializeField] private Color SpawnAngleColour;


    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.isKinematic = true;
        Restart();
    }

    // Update is called once per frame
    void Update()
    {

        if (!hasAuthority)
        {
            return;
        }

        // Check if the ball has been launched by the player or not
        if (Launched)
        {
            // Ensure rigidbody is always at the same speed
            RB.velocity = Speed * RB.velocity.normalized;
        }
        else
        {
            transform.position = SpawnLocation;
        }
    }

    /// <summary>
    ///     Resets the ball at the starting position
    ///     and then shoots it out in a random direction
    ///     within given angle
    /// </summary>
    public void Restart()
    {
        // Reset back to spawn location and choose a random direction
        transform.position = SpawnLocation;
        RB.velocity = Vector3.zero;
        Launched = false;
    }

    /// <summary>
    ///     Launches the ball upwards within the given 
    ///     max shooting angle
    /// </summary>
    public void LaunchBall()
    {
        // Restart if player tries launching ball when it's already launched
        if (Launched)
        {
            Restart();
            return;
        }

        Launched = true;
        RB.isKinematic = false;

        ShootAngle = Random.Range(-MaxShootAngle, MaxShootAngle);

        // Reset rotation before setting new rotation
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, ShootAngle);

        // Give ball initial velocity
        RB.velocity = transform.up * Speed;
    }

    private void OnBecameInvisible()
    {
        // Restart if the ball can no longer be seen
        Restart();
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!hasAuthority || !ShowGizmos)
        {
            return;
        }

        DrawGizmoNextAngle(SpawnAngleColour);
        DrawGizmoMaxShootAngle(MaxShootAngleColour);
    }

    /// <summary>
    ///     Draws the left and right lines to show
    ///     the maximum angle the ball can be launched 
    /// </summary>
    /// <param name="colour">Colour of the lines</param>
    private void DrawGizmoMaxShootAngle(Color colour)
    {
        Gizmos.color = colour;

        Vector3 rightAngle = Quaternion.AngleAxis(MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f;
        Vector3 leftAngle = Quaternion.AngleAxis(-MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f;

        if (Application.isPlaying)
        {
            Gizmos.DrawLine(SpawnLocation, SpawnLocation + leftAngle);
            Gizmos.DrawLine(SpawnLocation, SpawnLocation + rightAngle);
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + leftAngle);
            Gizmos.DrawLine(transform.position, transform.position + rightAngle);
        }
    }

    /// <summary>
    ///     Draws the next angle that the ball
    ///     is going to launch towards
    /// </summary>
    /// <param name="colour">Colour of the line</param>
    private void DrawGizmoNextAngle(Color colour)
    {
        Gizmos.color = colour;

        Vector3 shootAngle = Quaternion.AngleAxis(ShootAngle, Vector3.forward) * Vector3.up * 3;

        if (Application.isPlaying)
        {
            Gizmos.DrawRay(SpawnLocation, shootAngle);
        }
        else
        {
            Gizmos.DrawRay(transform.position, shootAngle);
        }

    }
    #endregion
}
