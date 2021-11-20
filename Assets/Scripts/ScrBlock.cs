using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrBlock : MonoBehaviour
{
    [Header("Block Properties")]
    public Vector2Int GridPosition;
    public float Points;

    [Header("Debugging")]
    [SerializeField] bool ShowDebugMessages;

    private void OnCollisionEnter(Collision collision)
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was hit");
        }

        ScrEventManager.Instance.BlockDestroyed(Points);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ShowDebugMessages)
        {
            Debug.Log("Block at " + GridPosition + " was destroyed");
        }
    }
}
