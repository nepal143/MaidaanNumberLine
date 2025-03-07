using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfiniteNumberLine : MonoBehaviour
{
    public GameObject tickPrefab;
    public Transform tickParent;
    public NumberPickerUI numberPicker;
    public LineRenderer lineRenderer;
    public float tickSpacing = 1f;
    public float moveDuration = 1f;
    public int visibleTickCount = 40;
    public float maxSpeed = 10f;

    private Queue<GameObject> tickPool = new Queue<GameObject>();
    private List<GameObject> activeTicks = new List<GameObject>();
    private bool isMoving = false;
    private Vector3 startPos, targetPos;

    void Start()
    {
        float startX = -visibleTickCount / 2 * tickSpacing;

        for (int i = 0; i < visibleTickCount + 2; i++)
        {
            GameObject tick = Instantiate(tickPrefab, tickParent);
            tick.SetActive(true);
            float xPos = startX + i * tickSpacing;
            tick.transform.position = new Vector3(xPos, 0, 0);
            tickPool.Enqueue(tick);
            activeTicks.Add(tick);
        }

        UpdateLineRenderer();
    }

    public void MoveRight()
    {
        MoveMarkers(numberPicker.GetFinalNumber()); // Takes the value as positive
    }

    public void MoveLeft()
    {
        MoveMarkers(-numberPicker.GetFinalNumber()); // Takes the value as negative
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
            UpdateLineRenderer();
            yield return null;
        }

        tickParent.position = targetPos;
        ExtendTicksIfNeeded();
        UpdateLineRenderer();
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
        }

        while (activeTicks[activeTicks.Count - 1].transform.position.x > rightBoundary + tickSpacing)
        {
            GameObject tick = activeTicks[activeTicks.Count - 1];
            activeTicks.RemoveAt(activeTicks.Count - 1);
            float newX = activeTicks[0].transform.position.x - tickSpacing;
            tick.transform.position = new Vector3(newX, 0, 0);
            activeTicks.Insert(0, tick);
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
