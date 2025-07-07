using UnityEngine;
using System.Runtime.CompilerServices;

public class DebuggableMonoBehaviour : MonoBehaviour
{
    protected LogSettings logSettings;
    protected bool isDebugEnabled;

    protected virtual string LogCategory => "General"; // Optional override

    protected virtual void Awake()
    {
        if (!logSettings)
        {
            logSettings = Resources.Load<LogSettings>("Guru/ScriptableObjects/LogSettings");
            if (!logSettings)
                Debug.LogWarning("LogSettings not found. Please check the path.");
        }
    }

    protected virtual void OnEnable()
    {
        if (logSettings != null)
        {
            logSettings.OnSettingsChanged += UpdateLogStatus;
            UpdateLogStatus();
        }
    }

    protected virtual void OnDisable()
    {
        if (logSettings != null)
            logSettings.OnSettingsChanged -= UpdateLogStatus;
    }

    protected virtual void UpdateLogStatus()
    {
        isDebugEnabled = true;// Or override this in child
    }

    protected void Log(string message, [CallerMemberName] string caller = "")
    {
        if (isDebugEnabled)
            Debug.Log($"[{GetType().Name}::{caller}] {message}");
    }
}
