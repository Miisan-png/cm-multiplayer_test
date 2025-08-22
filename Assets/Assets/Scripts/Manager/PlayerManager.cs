using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance => instance;

    private void Awake()
    {
        instance = this;

    }

    public void Start()
    {
        TimeManager.Instance.on8PM += () => { spawnMom(); };
    }

    [SerializeField] private Player Player;
    public Player player => Player;

    [SerializeField] private AreaTransitioner_PlayerHouse goHomeTransitioner;
    [SerializeField] private GameObject momPrefab;
    
    public void goHome()
    {
        goHomeTransitioner.GoHome(Player, deactivateMom);
    }

    public void spawnMom()
    {
        if (AreaManager.Instance.currentarea == Areas.Home) return;

        momPrefab.SetActive(true);
        NPC_Overworld npc = momPrefab.GetComponent<NPC_Overworld>();
        npc.feedDialogue("Mom_0");
    }

    public void teleportPlayer(GameObject obj)
    {
        Player.Teleport(obj.transform);
    }

    public void deactivateMom()
    {
        momPrefab.SetActive(false);
    }
}
