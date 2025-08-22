using UnityEngine;

public class CapturePrompt : MonoBehaviour
{
    private bool CaptureCutsceneStarted = false;

    private void Start()
    {
        BattleInputManager.Instance.OnPause += SkipCaptureCutscene;
    }

    public void CloseWinUI()
    {
        BattleUIManager.Instance.DeactivateWinUI();
    }
    public void SpawnCaptureUI()
    {
        BattleUIManager.Instance.OnCaptureUI();
    }
    public void StartCaptureCutscene()
    {
        CaptureCutsceneStarted = true;
    }
    public void EnterRewardsScreen()
    {
        BattleManager.Instance.EnterRewardScreen(1f);
    }
    public void SkipCaptureCutscene()
    {
        if(CaptureCutsceneStarted)
        {
            BattleManager.Instance.EnterRewardScreen(0);
            gameObject.SetActive(false);
            CaptureCutsceneStarted = false; 
        }
    }
}
