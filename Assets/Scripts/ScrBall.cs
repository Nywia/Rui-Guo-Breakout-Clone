using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class ScrBall : NetworkBehaviour
{
    public Vector3 SpawnLocation;

    [Header("Ball Properties")]
    [SerializeField] private float Speed;
    [SerializeField] private float MaxShootAngle;

    private Rigidbody RB;
    private float ShootAngle;
    private bool Launched;

    [Header("Ball Juice")]
    [SerializeField] private AnimationCurve ScaleCurve;

    [Header("Ball Audio")]
    private AudioSource SFXSource;
    private List<AudioClip> SFXBallSpawn = new List<AudioClip>();
    private List<AudioClip> SFXBallLaunch = new List<AudioClip>();
    private List<AudioClip> SFXBlockBreak = new List<AudioClip>();
    private List<AudioClip> SFXBallBounce = new List<AudioClip>();

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

        SFXSource = GetComponent<AudioSource>();

        // Get and cache all sfx for later use
        foreach(AudioClip clip in Resources.LoadAll<AudioClip>("SFX"))
        {
            if (clip.name.Contains("BallSpawn"))
            {
                SFXBallSpawn.Add(clip);
            }
            else if (clip.name.Contains("BallLaunch"))
            {
                SFXBallLaunch.Add(clip);
            }
            else if (clip.name.Contains("BallBounce"))
            {
                SFXBallBounce.Add(clip);
            }
            else if (clip.name.Contains("BlockBreak"))
            {
                SFXBlockBreak.Add(clip);
            }
        }

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

        SFXPlayRandom(SFXBallSpawn, 1.0f);
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
        SFXPlayRandom(SFXBallLaunch, 1.0f);
    }


    /// <summary>
    ///     Randomly plays a one shot audio clip from
    ///     a given list
    /// </summary>
    /// <param name="list">List of audio clips</param>
    private void SFXPlayRandom(List<AudioClip> list, float volume)
    {
        // A check here because sometimes the source can be disabled
        if (SFXSource.enabled)
        {
            SFXSource.PlayOneShot(list[Random.Range(0, list.Count - 1)], volume);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdScaleBall() => RpcScaleBall();

    [ClientRpc]
    private void RpcScaleBall()
    {
        // Scale the ball for a bit when they hit anything
        StartCoroutine(BounceScale(0.2f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        CmdScaleBall();

        if (collision.gameObject.name.Contains("Block"))
        {
            SFXPlayRandom(SFXBlockBreak, 1.0f);
        }
        else
        {
            SFXPlayRandom(SFXBallBounce, 0.7f);
        }
    }

    private void OnBecameInvisible()
    {
        // Restart if the ball can no longer be seen
        Restart();
    }

    /// <summary>
    ///     Scales the ball using the ScaleCurve when the ball
    ///     collides with anything (Bounces)
    /// </summary>
    /// <param name="totalDuration">How long the animation should be played for</param>
    /// <returns></returns>
    IEnumerator BounceScale(float totalDuration)
    {
        for (float passedTime = 0; passedTime < totalDuration;)
        {
            passedTime += Time.deltaTime;

            float newScale = ScaleCurve.Evaluate(passedTime / totalDuration);
            transform.localScale = new Vector3(newScale, newScale, newScale);

            yield return null;
        }
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
