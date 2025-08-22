using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    private void Awake()
    {
        // Check if another instance already exists
        if (instance != null && instance != this)
        {
            // Destroy this duplicate instance
            Destroy(gameObject);
            return;
        }

        // Set this as the persistent instance
        instance = this;

        // Make this object persist between scenes
        DontDestroyOnLoad(gameObject);

        // Optional: Initialize any components that might need it
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Add any initialization logic for components here
        // For example:
        // GetComponent<AudioSource>().playOnAwake = false;
    }

    // Optional: Add method to manually destroy the persistent object
    public static void DestroyPersistentObject()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
}