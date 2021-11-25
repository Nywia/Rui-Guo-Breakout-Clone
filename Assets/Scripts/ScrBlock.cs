using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScrBlock : NetworkBehaviour
{
    [Header("Block Properties")]
    public ScrBlockSpawner Spawner;

    [SyncVar] public Vector2Int GridPosition;
    [SyncVar] public float Points;

    [SerializeField] private GameObject BlockBreakVFX;

    private bool VFXSpawned;

    private Animator BlockAnimator;

    [Header("Debugging")]
    [SerializeField] private bool ShowDebugMessages;

    private void Start()
    {
        BlockAnimator = GetComponentInChildren<Animator>();
    }

    [Client]
    private void OnCollisionEnter(Collision collision)
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was hit");
            Debug.Log("OnCollisionEnter");
        }

        // Tell the server to destroy the block
        CmdSpawnVFX();
    }

    /// <summary>
    ///     Spawns the block breaking VFX and changes the
    ///     colour to be that of the block's colour. 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdSpawnVFX()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("CmdSpawnVFX");
        }

        // Spawn the breaking VFX
        GameObject vfx = Instantiate(BlockBreakVFX, transform.position, transform.rotation);
        Color blockColour = GetComponentInChildren<Renderer>().material.color;
        vfx.GetComponent<Renderer>().material.color = blockColour;

        // Spawn the same object on every other client
        RpcSpawnVFX();

        // Needed to destory object on the server if
        // it is not being hosted by a client
        if (isServerOnly)
        {
            Destroy(gameObject);
        }

        ScrEventManager.Instance.BlockDestroyed(Points);
    }

    /// <summary>
    ///     Spawns the block breaking VFX and changes the
    ///     colour to be that of the block's colour. 
    ///     Will also destroy the block on network after.
    /// </summary>
    /// <param name="vfx">VFX to change the colour of</param>
    [ClientRpc]
    private void RpcSpawnVFX()
    {
        // Spawn the breaking VFX before destroying (On client that called the command)
        GameObject vfxx = Instantiate(BlockBreakVFX, transform.position, transform.rotation);
        Color blockColourx = GetComponentInChildren<Renderer>().material.color;
        vfxx.GetComponent<Renderer>().material.color = blockColourx;

        NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    ///     Plays the block scale animation on layer 0
    ///     from the start on all clients
    /// </summary>
    [ClientRpc]
    public void BlockScale() => BlockAnimator.Play("BlockScale", 0, 0.0f);

    /// <summary>
    ///     Plays the block quiver animation on layer 1
    ///     from the start on all clients
    /// </summary>
    [ClientRpc]
    public void BlockQuiver() => BlockAnimator.Play("BlockQuiver", 1, 0.0f);

    private void OnDestroy()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was destroyed");
        }
    }
}
