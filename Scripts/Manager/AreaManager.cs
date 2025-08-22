using System;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    private static AreaManager instance;
    public static AreaManager Instance => instance;

    public Action<Areas> onSwitchArea;

    private void Awake()
    {
        instance = this;
     
    }

    private void Start()
    {
        setArea(startArea);
    }

    [SerializeField] private Areas startArea;
    [SerializeField] private Areas currentArea;
    public Areas currentarea => currentArea;

    public void setArea(Areas a)
    {
        if (currentArea == a) return;
        currentArea = a;
        onSwitchArea?.Invoke(currentArea);
    }

}
public enum Areas
{
    None, Home, StartingTown, School
}