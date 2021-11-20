using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScrBall : MonoBehaviour
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
    private GameObject Paddle;
    private Vector3 SpawnLocation;
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
        Paddle = FindObjectOfType<ScrPaddle>().gameObject;
        SpawnLocation = Paddle.transform.TransformPoint(new Vector3(0.0f, 1.0f, 0.0f));
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            LaunchBall();
        }

        if (Launched)
        {
            // Ensure rigidbody is always at the same speed
            RB.velocity = Speed * RB.velocity.normalized;
        }
        else
        {
            SpawnLocation = Paddle.transform.TransformPoint(new Vector3(0.0f, 1.0f, 0.0f));
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
        SpawnLocation = Paddle.transform.TransformPoint(new Vector3(0.0f, 1.0f, 0.0f));
        transform.position = SpawnLocation;
        RB.velocity = Vector3.zero;
        Launched = false;
    }

    public void LaunchBall()
    {
        Launched = true;
        Direction = Random.insideUnitSphere.normalized;

        // Get the angle of that direction relative to world up
        float ShootAngle = GetAngleToWorldUp(Direction);

        // Ensure the angle is within the allowed MaxShootAngle
        while (ShootAngle > MaxShootAngle || ShootAngle == 0)
        {
            Direction = Random.insideUnitSphere.normalized;
            ShootAngle = GetAngleToWorldUp(Direction);
        }

        Direction.z = 0.0f;

        if (ShowDebugMessages)
        {
            Debug.Log("Spawn angle: " + GetAngleToWorldUp(Direction));
        }

        RB.velocity = Direction.normalized * Speed;
    }

    private void OnBecameInvisible()
    {
        // Restart if the ball can no longer be seen
        Restart();
    }

    /// <summary>
    ///     Returns the angle of the given vector compared
    ///     to world up
    /// </summary>
    private float GetAngleToWorldUp(Vector3 vector)
    {
        return Vector3.Angle(transform.position + Vector3.up, vector);
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
