using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PointsText;
    [SerializeField] private float PointsMultiplier;

    private float CurrentPoints;

    // Start is called before the first frame update
    void Start()
    {
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

        ScrEventManager.Instance.onBlockDestroyed += AddPoints;
    }

    /// <summary>
    ///     Adds up the points and updates the points
    ///     text UI
    /// </summary>
    /// <param name="points">The amount of points to add</param>
    private void AddPoints(float points)
    {
        CurrentPoints += points * PointsMultiplier;
        PointsText.text = "Points: " + Mathf.CeilToInt(CurrentPoints);
    }
}
