using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ScrGameManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI PointsText;
    [SerializeField] private float PointsMultiplier;

    private float CurrentPoints;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Check for missing refernce
        if (!PointsText)
        {
            // Try to find it
            GameObject pointsTextTemp = GameObject.Find("PointsText");

            if (pointsTextTemp)
            {
                Debug.LogWarning("Missing Points Text Reference. Found: " + pointsTextTemp.gameObject);

                PointsText = pointsTextTemp?.GetComponent<TextMeshProUGUI>();
            }

            // The points text is missing the required component
            if (!PointsText)
            {
                Debug.LogError(PointsText.gameObject + " does not have a TextMeshProUGUI component!");
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        // Reset points when server closes
        CurrentPoints = 0;
        PointsText.text = "Points: " + Mathf.CeilToInt(CurrentPoints);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        // Reset points when client closes
        CurrentPoints = 0;
        PointsText.text = "Points: " + Mathf.CeilToInt(CurrentPoints);
    }

    private void OnEnable()
    {
        ScrEventManager.Instance.onBlockDestroyed += AddPoints;
    }

    private void OnDisable()
    {
        ScrEventManager.Instance.onBlockDestroyed -= AddPoints;
    }

    /// <summary>
    ///     Adds up the points and updates the points
    ///     text UI
    /// </summary>
    /// <param name="points">The amount of points to add</param>
    [Server]
    private void AddPoints(float points)
    {
        CurrentPoints += points * PointsMultiplier;
        PointsText.text = "Points: " + Mathf.CeilToInt(CurrentPoints);
        RpcAddPoints(CurrentPoints);
    }

    [ClientRpc]
    private void RpcAddPoints(float points)
    {
        PointsText.text = "Points: " + Mathf.CeilToInt(points);
    }
}
