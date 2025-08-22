using UnityEngine;

public class ScreenTransitionUI : MonoBehaviour
{
    public void sendSignal()
    {
        UIManager.Instance.sendSignal();
    }

    public void OnTransitionEnd()
    {
        UIManager.Instance.onScreenTransitionEnded();
    }

    public void turnOff()
    {
        UIManager.Instance.CloseMenubyName("ScreenTransitionUI");
    }
}
