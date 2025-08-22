using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button HostButton;

    private void Start()
    {
        // Assign click handler
        HostButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        SteamLobby.Instance.HostLobby();
        // Add your button logic here
    }
}
