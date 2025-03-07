using UnityEngine;
using UnityEngine.UI; // For handling UI elements

public class ArrowFollowParcel : MonoBehaviour
{
    public string parcelTag = "Parcel"; // Tag for the parcel
    public Camera mainCamera;
    public float screenBorderOffset = 50f; // Padding from screen edges
    public Image arrowImage; // Reference to the UI Image component

    private Transform parcel;

    void Start()
    {
        FindParcel();
    }

    void Update()
    {
        if (parcel == null)
        {
            FindParcel();
            return;
        }

        Vector3 screenPos = mainCamera.WorldToViewportPoint(parcel.position);
        bool isOffScreen = screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1;

        if (isOffScreen)
        {
            // Make arrow visible
            SetArrowAlpha(1f);

            // Convert parcel position to screen space
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(parcel.position);

            // Get screen center position
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            // Direction from center to parcel
            Vector3 direction = (targetScreenPos - screenCenter).normalized;

            // Clamp position within screen borders
            Vector3 clampedPosition = screenCenter + direction * (Mathf.Min(screenCenter.x, screenCenter.y) - screenBorderOffset);
            transform.position = clampedPosition;

            // 🔥 Fix Rotation: Adjust the arrow to point **correctly**
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // 🔥 Offset by -90° to align correctly
        }
        else
        {
            // Make arrow invisible instead of disabling it
            SetArrowAlpha(0f);
        }
    }

    void FindParcel()
    {
        GameObject parcelObject = GameObject.FindGameObjectWithTag(parcelTag);
        if (parcelObject != null)
        {
            parcel = parcelObject.transform;
        }
    }

    void SetArrowAlpha(float alpha)
    {
        if (arrowImage != null)
        {
            Color color = arrowImage.color;
            color.a = alpha;
            arrowImage.color = color;
        }
    }
}
