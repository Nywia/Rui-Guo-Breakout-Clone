using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrBlockSpawner : MonoBehaviour
{
    private enum GizmosType
    {
        Always,
        OnSelected,
        Never,
    }

    [Header("Spawn Properties")]
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private int Columns;
    [SerializeField] private int Rows;
    [SerializeField] private float OffsetX;
    [SerializeField] private float OffsetY;

    [SerializeField] private Gradient ColourGradient;

    private Renderer BlockRenderer;

    [Header("Debugging")]
    [SerializeField] private GizmosType ShowSpawnArea;
    [SerializeField] private Color SpawnAreaColour;
    [SerializeField] private Vector3 SpawnSize;

    // Start is called before the first frame update
    void Start()
    {
        BlockRenderer = BlockPrefab.GetComponent<Renderer>();

        SpawnBlocks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    ///     Spawns x columns and y rows of the given 
    ///     BlockPrefab from the current position of 
    ///     the spawner
    /// </summary>
    private void SpawnBlocks()
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
                Renderer blockRenderer = go.GetComponent<Renderer>();

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
            }
        }
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (ShowSpawnArea == GizmosType.Always)
        {
            DrawGizmoSpawnArea(SpawnAreaColour);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (ShowSpawnArea == GizmosType.OnSelected)
        {
            DrawGizmoSpawnArea(SpawnAreaColour);
        }
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
