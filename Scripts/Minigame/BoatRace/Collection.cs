using UnityEngine;
using System.Collections;

public class Collection : MonoBehaviour
{
    [Tooltip("重生时间（秒）")]
    public float respawnTime = 5f;

    private Collider col;
    private Renderer[] renderers;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// 被拾取时调用，自动隐藏并计时重生
    /// </summary>
    public void Collect()
    {
        SetActiveState(false);
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        SetActiveState(true);
    }

    /// <summary>
    /// 控制显示/碰撞体
    /// </summary>
    private void SetActiveState(bool state)
    {
        if (col != null) col.enabled = state;
        foreach (var r in renderers)
        {
            r.enabled = state;
        }
    }
}
