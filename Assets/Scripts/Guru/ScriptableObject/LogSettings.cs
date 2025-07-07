using UnityEngine;
using System;

[CreateAssetMenu(fileName = "LogSettings", menuName = "ScriptableObjects/LogSettings")]
public class LogSettings : ScriptableObject
{
    public bool CustomerControllerLogs;
    public bool DayManagerLogs;
    public bool OrderBubbleLogs;
    public bool PatienceBarLogs;
    public bool WaveManagerLogs;
    public bool NPCControllerLogs;
    public bool NPCSpawnerLogs;
    public bool FoodManagerLogs;
    public bool FoodSpawnerLogs;

    public bool OverLayManagerLogs;

    public Action OnSettingsChanged;

    public void TriggerUpdate()
    {
        OnSettingsChanged?.Invoke();
    }

    private void OnValidate()
    {
        TriggerUpdate();
    }
}
