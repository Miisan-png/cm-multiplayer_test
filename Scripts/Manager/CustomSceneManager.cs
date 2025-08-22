using Mirror.BouncyCastle.Asn1.X509;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    private static CustomSceneManager instance;
    public static CustomSceneManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void startMonsterBattle()
    {
        StartCoroutine(LoadBattleSceneAsync());
    }

    private IEnumerator LoadBattleSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Battle_Base", LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        GameManager.Instance.setState(gameState.Battle);
        yield return new WaitForSeconds(2f);

    }
}
