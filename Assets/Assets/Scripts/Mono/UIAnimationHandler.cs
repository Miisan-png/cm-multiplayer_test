using UnityEngine;
using UnityEngine.UI;

public class UIAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string closingAnim;


    public void PlayCloseUI()
    {
        animator.Play(closingAnim);
    }

    public void DisableUI()
    {
        Debug.Log("LOL");
        gameObject.SetActive(false);
    }
}
