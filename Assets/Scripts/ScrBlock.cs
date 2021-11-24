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
    ///     Spawns the block breaking VFX on the network
    ///     and changes the VFX colour to be that of the 
    ///     block's colour. 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdSpawnVFX()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("CmdSpawnVFX");
        }

        // Spawn the breaking VFX before destroying (On client that called the command)
        GameObject vfx = Instantiate(BlockBreakVFX, transform.position, transform.rotation);
        Color blockColour = GetComponentInChildren<Renderer>().material.color;
        vfx.GetComponent<Renderer>().material.color = blockColour;

        // Spawn the same object on every other client
        NetworkServer.Spawn(vfx);
        RpcSpawnVFX(vfx, blockColour);

        // Needed to destory object on the server if
        // it is not being hosted by a client
        if (isServerOnly)
        {
            Destroy(gameObject);
        }

        ScrEventManager.Instance.BlockDestroyed(Points);
    }

    /// <summary>
    ///     Changes the colour of the VFX spawned
    ///     on the network to be the block's colour
    ///     on the client. Will also destroy the block
    ///     after.
    /// </summary>
    /// <param name="vfx">VFX to change the colour of</param>
    [ClientRpc]
    private void RpcSpawnVFX(GameObject vfx, Color blockColour)
    {
        // Set its colour to match the block
        vfx.GetComponent<Renderer>().material.color = blockColour;
        NetworkServer.Destroy(gameObject);
    }

    [Command(requiresAuthority = false)]
    private void Serverdelet()
    {
        Destroy(gameObject);
    }


    [ClientRpc]
    public void ScaleBlock()
    {
        BlockAnimator.Play("BounceScale", 0, 0.0f);
    }

    private void OnDestroy()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was destroyed");
        }
    }
}
