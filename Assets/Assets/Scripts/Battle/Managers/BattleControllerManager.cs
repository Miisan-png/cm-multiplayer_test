using UnityEngine;

public class BattleControllerManager : MonoBehaviour
{
    private static BattleControllerManager instance;
    public static BattleControllerManager Instance => instance;

    public void Awake()
    {
        instance = this;

    }

    [SerializeField] private BattleController_Player PlayerController;
    public BattleController_Player playercontroller => PlayerController;

    [SerializeField] private BattleController_NPC NPCController1;
    public BattleController_NPC npccontroller1 => NPCController1;
}
