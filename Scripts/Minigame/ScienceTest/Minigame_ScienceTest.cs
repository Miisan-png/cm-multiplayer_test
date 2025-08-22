using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Minigame_ScienceTest : Minigame
{
    private void SubsribetoInputs(bool subscribe)
    {
        if(subscribe)
        {
            UIInputManager.Instance.OnNavigate += HandleInputs;
            UIInputManager.Instance.OnInteract += HandleConfirm;
        }
        else
        {
            UIInputManager.Instance.OnNavigate -= HandleInputs;
            UIInputManager.Instance.OnInteract -= HandleConfirm;
        }
    }

    [SerializeField] private List<SpriteRenderer> Lines;
    [SerializeField] private LanguageLocalizedSpriteRenderer TestPaperSR;
    [SerializeField] private float LinesScale;
    [SerializeField] private List<LineTransforms> VectorsWrapper;
    [SerializeField] private DG.Tweening.Sequence LineDrawingTween;

    private Tween HighlightArrowTween;
    [SerializeField] private RectTransform TopArrow;
    [SerializeField] private RectTransform BtmArrow;
    [SerializeField] private RectTransform TopArrowImage;
    [SerializeField] private RectTransform BtmArrowImage;
    [SerializeField] private float ArrowHorizontalIncrement;
    [SerializeField] private Vector3 CompleteArrowVector;
    [SerializeField] private int CurrentRow;
    [SerializeField] private int SelectedTopRow;
    [SerializeField] private int SelectedBtmRow;

    [SerializeField] private bool TestResult;
    public bool testresult => TestResult;

    [SerializeField] private GameObject MainUI;

    [SerializeField] private List<ScienceTestObjects> TestObjects;
    [SerializeField] private Interactable _Interactable;

    private void Start()
    {
        _Interactable.onInteraction += () => {
            MiniGameManager.Instance.StartMiniGame(MiniGameManager.Instance.sciencetest);
        };
    }

    public override void StartMinigame()
    {
        PlayerManager.Instance.player.setstate(PlayerState.None);
        TimeManager.Instance.pauseTimer();
        TestPaperSR.UpdateSprite();
        TestResult = false;
        ResetMinigame();
        SubsribetoInputs(true);
        MainUI.SetActive(true);
        base.StartMinigame();
    }
    public void ResetMinigame()
    {
        // Deactivate all line GameObjects
        foreach (var line in Lines)
        {
            line.gameObject.SetActive(false);
        }

        // Reset arrow positions
        TopArrow.anchoredPosition = Vector2.zero;
        BtmArrow.anchoredPosition = Vector2.zero;

        // Reset row selections
        CurrentRow = 0;
        SelectedTopRow = 0;
        SelectedBtmRow = 0;

        // Clean up any active tweens
        if (LineDrawingTween != null)
        {
            LineDrawingTween.Complete();
            LineDrawingTween.Kill();
            LineDrawingTween = null;
        }

        if (HighlightArrowTween != null)
        {
            HighlightArrowTween.Complete();
            HighlightArrowTween.Kill();
            HighlightArrowTween = null;
        }

        // Reset the highlight tween to the top arrow
        TopArrowImage.anchoredPosition = Vector2.zero;

        HighlightArrowTween = TopArrowImage
            .DOAnchorPosY(15f, 0.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Hide bottom arrow
        BtmArrow.gameObject.SetActive(false);
    }

    private void DrawLine(int from, int to)
    {
        if (LineDrawingTween != null)
        {
            LineDrawingTween.Complete();
            LineDrawingTween = null;
        }
        Lines[from].gameObject.SetActive(true);
        LineDrawingTween = DOTween.Sequence();
        LineDrawingTween.Append(Lines[from].transform.DOLocalRotate(VectorsWrapper[from].Vectors[to], 0f));
        LinesScale = 0f;
        Lines[from].size = new Vector2(Lines[from].size.x, 0.1f);
        LineDrawingTween.Append(DOTween.To(() => LinesScale, x => LinesScale = x, VectorsWrapper[from].Scale[to], 0.6f)).OnUpdate(() => { UpdateLine(from); }).OnComplete(() => { UpdateLine(from); });
        LineDrawingTween.AppendCallback(LineDrawingCallback);


        TestObjects[from].CurrentNumber = to;
    }


    private void UpdateLine(int index)
    {
        Lines[index].size = new Vector2(Lines[index].size.x, LinesScale);
    }

    private void LineDrawingCallback()
    {
        LineDrawingTween = null;
    }

    private void SkiptoNextRow()
    {
        CurrentRow = CurrentRow == 1 ? 0 : 1;
        CurrentRow = Mathf.Clamp(CurrentRow, 0, 1);

        if (HighlightArrowTween != null)
        {
            HighlightArrowTween.Complete(); // Ensure the tween finishes
            HighlightArrowTween.Kill();    // Kill it to prevent leaks
            HighlightArrowTween = null;    // Clear the reference
        }

        if (CurrentRow == 1)
        {
            // Reset position before animating
            BtmArrowImage.transform.localPosition = new Vector3(
                BtmArrowImage.transform.localPosition.x,
                0f, // Original Y position (adjust if needed)
                BtmArrowImage.transform.localPosition.z
            );

            HighlightArrowTween = BtmArrowImage
                .DOAnchorPosY(15f, 0.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            BtmArrow.gameObject.SetActive(true);
        }
        else
        {
            // Reset position before animating
            TopArrowImage.transform.localPosition = new Vector3(
                TopArrowImage.transform.localPosition.x,
                0f, // Original Y position (adjust if needed)
                TopArrowImage.transform.localPosition.z
            );

            HighlightArrowTween = TopArrowImage
                .DOAnchorPosY(15f, 0.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            BtmArrow.gameObject.SetActive(false);
        }
    }

    private void HandleInputs(Vector2 input)
    {
        // Horizontal movement (option selection)
        if (input.x < -0.1f) // Left
        {
            if (CurrentRow == 0)
            {
                SelectedTopRow--;
                SelectedTopRow = Mathf.Clamp(SelectedTopRow, 0, 5);

                if(SelectedTopRow < 5)
                {
                    TopArrow.DOAnchorPos(new Vector3((ArrowHorizontalIncrement * (SelectedTopRow)),0,0) , 0.1f);
                }
                else
                {
                    TopArrow.DOAnchorPos(CompleteArrowVector, 0.1f);
                }
            }
            else
            {
                SelectedBtmRow--;
                SelectedBtmRow = Mathf.Clamp(SelectedBtmRow, 0, 4);
                BtmArrow.DOAnchorPosX((ArrowHorizontalIncrement * (SelectedBtmRow)), 0.1f);
            }
        }
        else if (input.x > 0.1f) // Right
        {
            if (CurrentRow == 0)
            {
                SelectedTopRow++;
                SelectedTopRow = Mathf.Clamp(SelectedTopRow, 0, 5);

                if (SelectedTopRow < 5)
                {
                    TopArrow.DOAnchorPos(new Vector3((ArrowHorizontalIncrement * (SelectedTopRow)), 0, 0), 0.1f);
                }
                else
                {
                    TopArrow.DOAnchorPos(CompleteArrowVector, 0.1f);
                }
            }
            else
            {
                SelectedBtmRow++;
                SelectedBtmRow = Mathf.Clamp(SelectedBtmRow, 0, 4);
                BtmArrow.DOAnchorPosX((ArrowHorizontalIncrement * (SelectedBtmRow)), 0.1f);
            }
        }
    }

    private void HandleConfirm()
    {
        if(CurrentRow == 1 && SelectedTopRow != 5)
        {
            DrawLine(SelectedTopRow, SelectedBtmRow);
        }
        else if(SelectedTopRow == 5)
        {
            StopMinigame();
            return;
        }

        SkiptoNextRow();
    }

    private void CalculateResult()
    {
        bool success = true;
        for(int i=0;i<TestObjects.Count;i++)
        {
            if (TestObjects[i].CurrentNumber != TestObjects[i].RequiredNumber && success)
            {
                success = false;
            }
            TestObjects[i].CurrentNumber = -1;
        }

        TestResult = success;
    }

    public override void StopMinigame()
    {
        CalculateResult();

        PlayerManager.Instance.player.setstate(PlayerState.Idle);
        TimeManager.Instance.resumeTimer();

        SubsribetoInputs(false);
        MainUI.SetActive(false);
        _Interactable.resetInteraction();

        if (TestResult)
        {
            PlayerManager.Instance.player.idetector.HandleOnLeaveInteractable(_Interactable);
        }

        base.StopMinigame();
    }

}

[System.Serializable]
public class LineTransforms
{
    [SerializeField] public List<UnityEngine.Vector3> Vectors;
    [SerializeField] public List<float> Scale;
}

[System.Serializable]
public class ScienceTestObjects
{
    [SerializeField] public int CurrentNumber;
    [SerializeField] public int RequiredNumber;
}