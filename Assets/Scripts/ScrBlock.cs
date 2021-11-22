using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScrBlock : NetworkBehaviour
{
    [Header("Block Properties")]
    [SyncVar]
    public Vector2Int GridPosition;
    [SyncVar]
    public float Points;
    public ScrBlockSpawner Spawner;

    [Header("Debugging")]
    [SerializeField] bool ShowDebugMessages;

    [Client]
    private void OnCollisionEnter(Collision collision)
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was hit");
            Debug.Log("OnCollisionEnter");
        }

        CmdDestroyBlock();
    }

    [Command(requiresAuthority =false)]
    private void CmdDestroyBlock()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("CmdDestroyBlock");
        }

        RpcDestroyBlock();
        ScrEventManager.Instance.BlockDestroyed(Points);
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcDestroyBlock()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("RpcDestroyBlock");
        }
    }

    private void OnDestroy()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was destroyed");
        }
    }
}
