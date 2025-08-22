using UnityEngine;

public class DividerTile : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            BoatController_Player playerBoat = collision.gameObject.GetComponent<BoatController_Player>();

            //如果 playerBoat 不为 null（即已存在），而且前方被阻挡（IsFrontBlocked == true），就调用 playerBoat 的 OnHitDivider() 方法。
            if (playerBoat != null && playerBoat.IsFrontBlocked)
            {
                
            }


        }
    }

}
