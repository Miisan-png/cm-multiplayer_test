using System;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
    public delegate void startminigame();
    //定义了一个名为 startminigame 的委托类型，它代表不带参数、没有返回值的方法。
    //delegate（委托）：类似一个“函数类型”，可以存储并调用符合特定签名的方法。
    public event startminigame onMiniGameStart;
    //定义了一个名为 onMiniGameStart 的公开事件，事件的类型是 startminigame

    public delegate void endminigame();
    public event endminigame onMiniGameEnd;

    [SerializeField] protected MinigameState State;
    // 管理小游戏状态（逻辑控制核心）
    public MinigameState state => State;
    [SerializeField] int ID;
    public int id => ID;

    protected void setState(MinigameState S)
    {
        State = S;
    }
    public virtual void StartMinigame()
    {
        onMiniGameStart?.Invoke();
        //为了避免报错。如果没有任何人订阅这个事件，直接调用 onMiniGameStart() 会导致空引用异常（NullReferenceException）。
        //所以用 ?.Invoke() 是一种安全的写法。
        setState(MinigameState.Active);
    }
    public virtual void StopMinigame()
    {
        onMiniGameEnd?.Invoke();
        MiniGameManager.Instance.EndCurrentMinigame();
        setState(MinigameState.None);
    }

}
public enum MinigameState
{
    None,Active
}
