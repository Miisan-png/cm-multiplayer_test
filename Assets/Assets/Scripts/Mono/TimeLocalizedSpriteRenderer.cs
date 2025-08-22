using System.Collections.Generic;
using UnityEngine;

public class TimeLocalizedSpriteRenderer : MonoBehaviour
{
    [SerializeField] private List<LocalizedRenderers> Renderers;

    private void Start()
    {
        TimeManager.Instance.OnDayStateChanged += OnDayStateChanged;
    }

    private void OnEnable()
    {
        if (TimeManager.Instance == null) return;
        OnDayStateChanged(TimeManager.Instance.state);
    }

    private void OnDayStateChanged(DayState state)
    {
        if (Renderers == null || Renderers.Count < 1) return;

        for(int i=0;i<Renderers.Count;i++)
        {
            Renderers[i].ChangeSprite(state);
        }
    }
}

[System.Serializable]
public class LocalizedRenderers
{
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite MorningSprite;
    [SerializeField] private Sprite NoonSprite;
    [SerializeField] private Sprite NightSprite;

    public void ChangeSprite(DayState state)
    {
        Sprite spritetouse = null;

        switch (state)
        {
            case DayState.Morning:
                spritetouse = MorningSprite;
                break;
            case DayState.Afternoon:
                spritetouse = NoonSprite;
                break;
            case DayState.Night:
                spritetouse = NightSprite;
                break;
        }

        if(spritetouse != null)
        {
            SR.sprite = spritetouse;
        }
    }
}