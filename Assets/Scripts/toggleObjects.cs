using UnityEngine;

public class ToggleObjects : MonoBehaviour
{
    public GameObject objectToDisable;
    public GameObject objectToEnable;

    public void Toggle()
    {
        if (objectToEnable != null) objectToEnable.SetActive(true);
        if (objectToDisable != null) objectToDisable.SetActive(false);
    }
}
