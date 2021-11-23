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

    [Header("Debugging")]
    [SerializeField] private bool ShowDebugMessages;

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

        ScrEventManager.Instance.BlockDestroyed(Points);
        NetworkServer.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was destroyed");
        }
    }
}
