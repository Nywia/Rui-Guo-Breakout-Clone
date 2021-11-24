using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScrBlockSpawner : NetworkBehaviour
{
    [Header("Spawn Properties")]
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private int Columns;
    [SerializeField] private int Rows;
    [SerializeField] private float OffsetX;
    [SerializeField] private float OffsetY;

    [SerializeField] private Gradient ColourGradient;

    private Renderer BlockRenderer;

    [Header("Debugging")]
    [SerializeField] private bool ShowGizmos;
    [SerializeField] private Color SpawnAreaColour;
    [SerializeField] private Vector3 SpawnSize;


    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();

        BlockRenderer = BlockPrefab.GetComponentInChildren<Renderer>();
        SpawnBlocks();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        BlockRenderer = BlockPrefab.GetComponentInChildren<Renderer>();
        SetBlocks();
    }

    /// <summary>
    ///     Spawns x columns and y rows of the given 
    ///     BlockPrefab from the current position of 
    ///     the spawner on the server
    /// </summary>
    [Server]
    public void SpawnBlocks()
    {
        Vector3 position = transform.position;

        float blockSizeX = BlockRenderer.bounds.size.x;
        float blockSizeY = BlockRenderer.bounds.size.y;

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                GameObject go = Instantiate(BlockPrefab);
                ScrBlock blockScript = go.GetComponent<ScrBlock>();
                Renderer blockRenderer = go.GetComponentInChildren<Renderer>();

                if (blockScript)
                {
                    blockScript.GridPosition = new Vector2Int(x, y);
                    blockScript.Points = 100;
                }
                else
                {
                    Debug.Log(BlockPrefab.name + " does not have a block script!", go);
                }

                // Sets the block to a position offset by the size and current column/row it's on
                go.transform.position = position - new Vector3((blockSizeX + OffsetX) * x, (blockSizeY + OffsetY) * y);
                go.transform.parent = transform;

                // Set the blocks colour to be based on the colour gradient
                blockRenderer.material.color = ColourGradient.Evaluate(y * (1.0f / Rows));

                NetworkServer.Spawn(go);
            }
        }

        RpcSetBlocks();
    }

    /// <summary>
    ///     Tells all the clients to set their blocks
    /// </summary>
    [ClientRpc]
    private void RpcSetBlocks() => SetBlocks();

    /// <summary>
    ///     Finds all the currently spawned blocks
    ///     on the client and then sets its position 
    ///     and colour to the right values
    /// </summary>
    [Client]
    public void SetBlocks()
    {
        Vector3 position = transform.position;

        foreach (ScrBlock block in FindObjectsOfType<ScrBlock>())
        {
            int gridPosX = block.GridPosition.x;
            int gridPosY = block.GridPosition.y;

            Renderer blockRenderer = block.gameObject.GetComponentInChildren<Renderer>();
            float blockSizeX = blockRenderer.bounds.size.x;
            float blockSizeY = blockRenderer.bounds.size.y;

            // Sets the block to a position offset by the size and current column/row it's on
            block.transform.position = position - new Vector3((blockSizeX + OffsetX) * gridPosX, (blockSizeY + OffsetY) * gridPosY);
            block.transform.parent = transform;

            // Set the blocks colour to be based on the colour gradient
            blockRenderer.material.color = ColourGradient.Evaluate(gridPosY * (1.0f / Rows));
        }
    }

    /// <summary>
    ///     Tells all the clients to scale all their blocks
    /// </summary>
    [Server]
    public void ScaleBlocks()
    {
        foreach (ScrBlock block in FindObjectsOfType<ScrBlock>())
        {
            block.ScaleBlock();
        }
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!ShowGizmos)
        {
            return;
        }

        DrawGizmoSpawnArea(SpawnAreaColour);
    }


    /// <summary>
    ///     Draws the spawn area requied of the blocks
    ///     based on current parameters
    /// </summary>
    /// <param name="colour">The colour of the spawn area</param>
    private void DrawGizmoSpawnArea(Color colour)
    {
        Vector3 prefabSize;

        // Need to get renderer from prefab if BlockRender is empty
        if (BlockRenderer)
        {
            prefabSize = BlockRenderer.bounds.size;
        }
        else
        {
            Renderer blockRenderer = BlockPrefab.GetComponent<Renderer>();
            prefabSize = blockRenderer.bounds.size;
        }

        // Calculate the spawn size needed given columns and rows
        // (The -Offset removes the extra space from the final Column/Row's offset)
        SpawnSize = new Vector3(((prefabSize.x + OffsetX) * Columns) - OffsetX, ((prefabSize.y + OffsetY) * Rows) - OffsetY, prefabSize.z);

        // Offset the display location so it's at top right and center it at the prefab
        Vector3 spawnLocation = transform.position - (SpawnSize / 2) + (prefabSize / 2);

        Gizmos.color = colour;
        Gizmos.DrawCube(spawnLocation, SpawnSize);
    }
#endregion
}
