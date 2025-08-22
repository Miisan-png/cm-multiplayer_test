using UnityEngine;

public class Interactablebillboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (gameObject.activeSelf && gameObject.activeInHierarchy)
        {
            mainCamera = Camera.main;
            transform.forward = mainCamera.transform.forward; // Make it face the camera
        }
    }

    public void switchofbillboard()
    {
        gameObject.SetActive(false);
    }
}


