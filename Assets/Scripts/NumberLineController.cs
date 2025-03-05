using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfiniteNumberLine : MonoBehaviour
{
    public GameObject tickPrefab;  // Prefab for tick marks
    public Transform tickParent;   // Parent for tick marks
    public TMP_InputField inputField; // Input field for movement
    public LineRenderer lineRenderer; // LineRenderer component
    public float tickSpacing = 1f; // Distance between tick marks
    public float moveDuration = 1f; // Movement duration (always 1 sec)
    public int visibleTickCount = 40; // Number of visible ticks on screen
    public float maxSpeed = 10f; // Prevents things from moving too fast

    private Queue<GameObject> tickPool = new Queue<GameObject>(); // Object pool
    private List<GameObject> activeTicks = new List<GameObject>(); // Active ticks
    private bool isMoving = false; // Prevent multiple moves
    private float movementSpeed; // Speed in units per second
    private Vector3 startPos, targetPos; // Movement positions

    void Start()
    {
        float startX = -visibleTickCount / 2 * tickSpacing;
        
        // Create tick pool
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

    public void MoveMarkers()
    {
        if (isMoving) return;

        if (float.TryParse(inputField.text, out float moveDistance))
        {
            moveDistance = Mathf.Clamp(moveDistance, -maxSpeed * moveDuration, maxSpeed * moveDuration); // Prevent too fast movement
            startPos = tickParent.position;
            targetPos = tickParent.position - new Vector3(moveDistance, 0, 0);
            StartCoroutine(MoveSmoothly());
        }
        else
        {
            Debug.LogWarning("Invalid input. Enter a number.");
        }
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

        tickParent.position = targetPos; // Ensure it reaches exact position
        ExtendTicksIfNeeded();
        UpdateLineRenderer();
        isMoving = false;
    }

    void ExtendTicksIfNeeded()
    {
        float leftBoundary = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float rightBoundary = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        // Move leftmost tick to right if it goes out of screen
        while (activeTicks[0].transform.position.x < leftBoundary - tickSpacing)
        {
            GameObject tick = activeTicks[0];
            activeTicks.RemoveAt(0);
            float newX = activeTicks[activeTicks.Count - 1].transform.position.x + tickSpacing;
            tick.transform.position = new Vector3(newX, 0, 0);
            activeTicks.Add(tick);
        }

        // Move rightmost tick to left if it goes out of screen
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
