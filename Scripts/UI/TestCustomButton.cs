using UnityEngine;
using UnityEngine.Events;

public class TestCustomButton : MonoBehaviour
{
    [SerializeField] KeyCode Inputtoread;
    [SerializeField] UnityEvent eventtohold;


    private void Update()
    {
        if(Input.GetKeyDown(Inputtoread))
        {
            eventtohold?.Invoke();
        }
    }

}
