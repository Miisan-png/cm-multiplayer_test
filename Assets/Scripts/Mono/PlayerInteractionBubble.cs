using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionBubble : MonoBehaviour
{
    [SerializeField] private Dictionary<string, GameObject> BubbleDictionary;
    [SerializeField] private GameObject BubbleParent;
    [SerializeField] private GameObject MultiBubble;
    [SerializeField] private GameObject FishingBubble;
    [SerializeField] private GameObject DoorBubble;
    [SerializeField] private GameObject LadderBubble;

    private void Start()
    {
        BubbleDictionary = new Dictionary<string, GameObject>();
        BubbleDictionary["Multi"] = MultiBubble;
        BubbleDictionary["Fishing"] = FishingBubble;
        BubbleDictionary["Door"] = DoorBubble;
        BubbleDictionary["Ladder"] = LadderBubble;
    }

    public void HandleBubble(string bubblename,bool activate)
    {
      if(BubbleDictionary.TryGetValue(bubblename,out GameObject obj))
        {
            obj.SetActive(activate);
        }
      else
        {
            Debug.Log("Bubble Not Found!");
        }
    }
}
