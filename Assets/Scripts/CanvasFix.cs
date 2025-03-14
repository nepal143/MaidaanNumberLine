using UnityEngine;
using System.Collections;

public class CanvasFix : MonoBehaviour
{
    void Awake()
    {
        // Enable the GameObject for proper UI calculation
        gameObject.SetActive(true);
    }

    void Start()
    {
        // Disable the GameObject after one frame
        StartCoroutine(DisableCanvasAfterFrame());
    }

    IEnumerator DisableCanvasAfterFrame()
    {
        yield return null; // Wait for one frame
        gameObject.SetActive(false);
    }

    // Call this function when you want to show the UI again
    public void ShowCanvas()
    {
        gameObject.SetActive(true);
    }
}
