using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class ScrBall : NetworkBehaviour
{
    private enum GizmosType
    {
        Always,
        OnSelected,
        Never,
    }

    [Header("Ball Properties")]
    [SerializeField] private float Speed;
    [SerializeField] private float MaxShootAngle;

    private Rigidbody RB;
    public Vector3 SpawnLocation;
    private Vector3 Direction;
    private bool Launched;

    [Header("Debugging")]
    [SerializeField] bool ShowDebugMessages;
    [Space(10)]
    [SerializeField] private GizmosType ShowMaxShootAngle;
    [SerializeField] private Color MaxShootAngleColour;
    [Space(5)]
    [SerializeField] private GizmosType ShowSpawnAngle;
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

        // Reset rotation before setting new rotation
        RB.rotation = Quaternion.identity;
        RB.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-MaxShootAngle, MaxShootAngle));

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
        if (ShowSpawnAngle == GizmosType.Always)
        {
            DrawGizmoSpawnAngle(SpawnAngleColour);
        }

        if (ShowMaxShootAngle == GizmosType.Always)
        {
            DrawGizmoMaxShootAngle(MaxShootAngleColour);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (ShowSpawnAngle == GizmosType.OnSelected)
        {
            DrawGizmoSpawnAngle(SpawnAngleColour);
        }

        if (ShowMaxShootAngle == GizmosType.OnSelected)
        {
            DrawGizmoMaxShootAngle(MaxShootAngleColour);
        }
    }

    private void DrawGizmoMaxShootAngle(Color colour)
    {
        Gizmos.color = colour;

        if (Application.isPlaying)
        {
            Gizmos.DrawLine(SpawnLocation, SpawnLocation + (Quaternion.AngleAxis(MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f));
            Gizmos.DrawLine(SpawnLocation, SpawnLocation + (Quaternion.AngleAxis(-MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f));
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + (Quaternion.AngleAxis(MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f));
            Gizmos.DrawLine(transform.position, transform.position + (Quaternion.AngleAxis(-MaxShootAngle, Vector3.forward) * Vector3.up * 3.0f));
        }
    }

    private void DrawGizmoSpawnAngle(Color colour)
    {
        Gizmos.color = colour;

        if (Application.isPlaying)
        {
            Gizmos.DrawRay(SpawnLocation, Direction.normalized * 3);
        }
        else
        {
            Gizmos.DrawRay(transform.position, Direction.normalized * 3);
        }

    }
    #endregion
}
