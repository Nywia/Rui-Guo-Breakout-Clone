using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Mirror;

[RequireComponent(typeof(Rigidbody), 
                  typeof(AudioSource))]
public class ScrBall : NetworkBehaviour
{
    public Vector3 SpawnLocation;

    [Header("Ball Properties")]
    [SerializeField] private float Speed;
    [SerializeField] private float MaxShootAngle;
    [SyncVar] private bool Launched;

    private Rigidbody RB;
    private float ShootAngle;


    [Header("Ball Juice")]
    [SerializeField] private AnimationCurve ScaleCurve;
    [SerializeField] private GameObject VFXImpactPrefab;

    private TrailRenderer BallTrail;
    private ScrCamera CameraScript;
    private Transform BallMesh;


    [Header("Ball Audio")]
    private AudioSource SFXSource;
    private List<AudioClip> SFXBallSpawn = new List<AudioClip>();
    private List<AudioClip> SFXBallLaunch = new List<AudioClip>();
    private List<AudioClip> SFXBlockBreak = new List<AudioClip>();
    private List<AudioClip> SFXBallBounce = new List<AudioClip>();

    private enum ClipType
    {
        Spawn,
        Break,
        Launch,
        Bounce,
    }


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
        BallTrail = GetComponentInChildren<TrailRenderer>();

        CameraScript = FindObjectOfType<ScrCamera>();
        BallMesh = transform.Find("BallMesh");

        // Get and cache all sfx for later use
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>("SFX"))
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

        // Make sure there is a VFX asset
        if (!VFXImpactPrefab)
        {
            VFXImpactPrefab = Resources.Load<GameObject>("VFX/VFXHit");
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
        RB.isKinematic = true;
        Launched = false;

        // Reset trail
        BallTrail.Clear();
        BallTrail.time = 0.5f;

        if (hasAuthority)
        {
            CmdSFXPlayRandom(ClipType.Spawn, 1.0f);
        }
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
        BallTrail.time = 1.5f;


        // Set shooting angle
        ShootAngle = Random.Range(-MaxShootAngle, MaxShootAngle);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, ShootAngle);

        // Give ball initial velocity
        RB.velocity = transform.up * Speed;

        // Reset rotation as it's no longer needed
        transform.rotation = Quaternion.identity;

        if (hasAuthority)
        {
            CmdSFXPlayRandom(ClipType.Launch, 1.0f);
        }
    }


    /// <summary>
    ///     Chooses a random SFX and then plays a 
    ///     one shot on all clients 
    /// </summary>
    /// <param name="clipType"></param>
    /// <param name="volume"></param>
    [Command]
    private void CmdSFXPlayRandom(ClipType clipType, float volume)
    {
        RpcSFXPlayRandom(clipType, volume);
    }

    /// <summary>
    ///     Get server to call scale ball on 
    ///     all clients. Authority is false to allow scaling on 
    ///     Ball to Ball collisions.
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdScaleBall() => RpcScaleBall();

    /// <summary>
    ///     Tell the server to scale the blocks
    /// </summary>
    [Command]
    private void CmdScaleBlocks() => FindObjectOfType<ScrBlockSpawner>().ScaleBlocks();

    /// <summary>
    ///     Tell the server to scale the blocks
    /// </summary>
    [Command]
    private void CmdQuiverBlocks() => FindObjectOfType<ScrBlockSpawner>().QuiverBlocks();

    /// <summary>
    ///     Plays the hit VFX at position and scale 
    /// </summary>
    /// <remarks>
    ///     Tells server to call <seealso cref="RpcPlayVFX(Vector3, Vector3)"/> 
    ///     on all the clients
    /// </remarks>
    /// <param name="position">Where to play the VFX</param>
    /// <param name="scale">Scale of the VFX</param>
    [Command]
    private void CmdPlayVFX(Vector3 position, Vector3 scale)
    {
        RpcPlayVFX(position, scale);
    }

    /// <summary>
    ///     Scale the ball on all clients
    /// </summary>
    [ClientRpc]
    private void RpcScaleBall()
    {
        // Scale the ball for a bit when they hit anything
        StartCoroutine(BounceScale(0.2f));
    }

    /// <summary>
    ///     Plays the hit VFX at position and scale 
    /// </summary>
    /// <remarks>
    ///     NOTE: VFX will get -1.0f added to its Z 
    ///     position in order to bring it in front of 
    ///     the ball
    /// </remarks>
    /// <param name="position">Where to play the VFX</param>
    /// <param name="scale">Scale of the VFX</param>
    [ClientRpc]
    private void RpcPlayVFX(Vector3 position, Vector3 scale)
    {
        // -1.0 here on the position to bring it in front of the ball
        GameObject go = Instantiate(VFXImpactPrefab, position, Quaternion.identity);
        go.transform.localScale = scale;
        Destroy(go, 0.5f);
    }

    /// <summary>
    ///     Plays a random SFX on the clients given
    ///     the type of the clip
    /// </summary>
    /// <param name="clipType">The type of clip to play</param>
    /// <param name="volume">The volume to play it at</param>
    [ClientRpc]
    private void RpcSFXPlayRandom(ClipType clipType, float volume)
    {
        int randomIndex;

        // A check here in case the source is disabled
        if (!SFXSource.enabled)
        {
            return;
        }

        // Determine the type of clip and play from that list
        switch (clipType)
        {
            case ClipType.Spawn:
                randomIndex = Random.Range(0, SFXBallSpawn.Count - 1);
                SFXSource.PlayOneShot(SFXBallSpawn[randomIndex], volume);
                break;

            case ClipType.Break:
                randomIndex = Random.Range(0, SFXBlockBreak.Count - 1);
                SFXSource.PlayOneShot(SFXBlockBreak[randomIndex], volume);
                break;

            case ClipType.Launch:
                randomIndex = Random.Range(0, SFXBallLaunch.Count - 1);
                SFXSource.PlayOneShot(SFXBallLaunch[randomIndex], volume);
                break;

            case ClipType.Bounce:
                randomIndex = Random.Range(0, SFXBallBounce.Count - 1);
                SFXSource.PlayOneShot(SFXBallBounce[randomIndex], volume);
                break;

            default:

                if (ShowDebugMessages)
                {
                    Debug.Log("Unknown clip type was given!", gameObject);
                }
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Authority check
        if (!hasAuthority)
        {
            return;
        }

        // Scale the ball on all collisions
        CmdScaleBall();

        // Play the other juice effects depending on what the ball hit
        if (collision.gameObject.name.Contains("Block"))
        {
            StartCoroutine(TimeSlow(0.1f, 0.05f));
            CameraScript.CmdPushCamera(RB.velocity.normalized, 5.0f, 0.1f);
            CmdPlayVFX(collision.GetContact(0).point, Vector3.one * 5.0f);
            CmdScaleBlocks();
            CmdSFXPlayRandom(ClipType.Break, 1.0f);
        }
        else
        {
            CameraScript.CmdPushCamera(RB.velocity.normalized, 2.5f, 0.1f);
            CmdQuiverBlocks();
            CmdPlayVFX(collision.GetContact(0).point, Vector3.one * 2.5f);
            CmdSFXPlayRandom(ClipType.Bounce, 0.7f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("KillFloor"))
        {
            // Restart if the ball collides the kill floor
            Restart();
        }
    }

    /// <summary>
    ///     Scales the ball using the ScaleCurve when the ball
    ///     collides with anything (Bounces)
    /// </summary>
    /// <param name="duration">How long the animation should be played for</param>
    /// <returns></returns>
    IEnumerator BounceScale(float duration)
    {
        for (float elapsedTime = 0; elapsedTime < duration; elapsedTime += Time.deltaTime)
        {
            float newScale = ScaleCurve.Evaluate(elapsedTime / duration);
            BallMesh.localScale = Vector3.one * newScale;

            yield return null;
        }
    }

    /// <summary>
    ///     Slow down the game a given amount 
    ///     for a set period of time 
    /// </summary>
    /// <param name="scale">The new timeScale</param>
    /// <param name="duration">How long to slow down for</param>
    /// <returns></returns>
    IEnumerator TimeSlow(float scale, float duration)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
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
