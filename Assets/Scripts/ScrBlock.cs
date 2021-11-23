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

        // Spawn the breaking VFX before destroying (On client that called the command)
        GameObject go = Instantiate(BlockBreakVFX, transform.position, transform.rotation);
        go.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        // Spawn the same object on every other client
        NetworkServer.Spawn(go);
        SpawnVFX(go);

        ScrEventManager.Instance.BlockDestroyed(Points);
    }

    [ClientRpc]
    private void SpawnVFX(GameObject go)
    {
        // Set its colour to match the block
        go.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
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
