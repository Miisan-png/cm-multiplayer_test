using UnityEngine;

public class MinimapCameraYFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 20, 0); // 高度偏移

    void LateUpdate()
    {
        if (player == null) return;
        // 跟随玩家位置
        transform.position = player.position + offset;
        // 只跟随玩家Y轴旋转
        transform.rotation = Quaternion.Euler(90, player.eulerAngles.y, 0);
    }
}
