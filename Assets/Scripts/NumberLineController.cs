using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfiniteNumberLine : MonoBehaviour
{
    public GameObject tickPrefab; // Prefab with tick + number
    public Transform tickParent; // Parent for ticks
    public NumberPickerUI numberPicker; // UI input
    public LineRenderer lineRenderer; // Number line
    public float tickSpacing = 1f; // Distance between ticks
    public float moveDuration = 1f; // Movement animation
    public int visibleTickCount = 40; // Number of ticks visible
    public float maxSpeed = 10f; // Speed limit

    private List<GameObject> activeTicks = new List<GameObject>();
    private Dictionary<GameObject, int> tickNumbers = new Dictionary<GameObject, int>();
    private bool isMoving = false;
    private Vector3 startPos, targetPos;

    void Start()
    {
        GenerateTicks();
        UpdateLineRenderer();
    }

    void GenerateTicks()
    {
        float startX = -visibleTickCount / 2 * tickSpacing;
        
        for (int i = 0; i < visibleTickCount; i++)
        {
            GameObject tick = Instantiate(tickPrefab, tickParent);
            tick.SetActive(true);
            float xPos = startX + i * tickSpacing;
            tick.transform.position = new Vector3(xPos, 0, 0);
            activeTicks.Add(tick);

            // **Assign a permanent number once and store it**
            int tickValue = Mathf.RoundToInt(xPos / tickSpacing);
            tickNumbers[tick] = tickValue;

            // Set the label once
            UpdateTickLabel(tick);
        }
    }

    public void MoveRight()
    {
        MoveMarkers(numberPicker.GetFinalNumber());
    }

    public void MoveLeft()
    {
        MoveMarkers(-numberPicker.GetFinalNumber());
    }

    private void MoveMarkers(float moveDistance)
    {
        if (isMoving) return;

        moveDistance = Mathf.Clamp(moveDistance, -maxSpeed * moveDuration, maxSpeed * moveDuration);
        startPos = tickParent.position;
        targetPos = tickParent.position - new Vector3(moveDistance, 0, 0);
        StartCoroutine(MoveSmoothly());
    }

    IEnumerator MoveSmoothly()
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float progress = elapsedTime / moveDuration;
            tickParent.position = Vector3.Lerp(startPos, targetPos, progress);
            elapsedTime += Time.deltaTime;

            ExtendTicksIfNeeded();
            yield return null;
        }

        tickParent.position = targetPos;
        ExtendTicksIfNeeded();
        isMoving = false;
    }

    void ExtendTicksIfNeeded()
    {
        float leftBoundary = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float rightBoundary = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        while (activeTicks[0].transform.position.x < leftBoundary - tickSpacing)
        {
            GameObject tick = activeTicks[0];
            activeTicks.RemoveAt(0);
            
            float newX = activeTicks[activeTicks.Count - 1].transform.position.x + tickSpacing;
            tick.transform.position = new Vector3(newX, 0, 0);
            activeTicks.Add(tick);

            // **Do NOT change its assigned number**
        }

        while (activeTicks[activeTicks.Count - 1].transform.position.x > rightBoundary + tickSpacing)
        {
            GameObject tick = activeTicks[activeTicks.Count - 1];
            activeTicks.RemoveAt(activeTicks.Count - 1);
            
            float newX = activeTicks[0].transform.position.x - tickSpacing;
            tick.transform.position = new Vector3(newX, 0, 0);
            activeTicks.Insert(0, tick);

            // **Do NOT change its assigned number**
        }

        UpdateLineRenderer();
    }

    void UpdateTickLabel(GameObject tick)
    {
        TextMeshProUGUI text = tick.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = tickNumbers[tick].ToString();
        }
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = activeTicks.Count;
        for (int i = 0; i < activeTicks.Count; i++)
        {
            lineRenderer.SetPosition(i, activeTicks[i].transform.position);
        }
    }
}
