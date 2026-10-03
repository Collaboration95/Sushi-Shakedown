using UnityEngine;
using UnityEngine.UI;             // for Button
using UnityEngine.SceneManagement; // for loading scenes
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{

    public event System.Action ButtonClicked;
    public GuruAudioManager gm;
    public event System.Action OnSettingsOpened;

    public void OnPlayButtonClicked()
    {
        if (gm != null) gm.PlayButtonClickSound();
        RuntimeLog.Write("Play button clicked! Loading Game scene...");
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        var settings = FindFirstObjectByType<SettingsController>();
        if (settings != null && settings.customerData != null) settings.customerData.ResetEverything();
        if (GameSceneManager.instance != null) GameSceneManager.instance.StartGame();
    }

    public void OnSettingsButtonClicked()
    {
        if (gm != null) gm.PlayButtonClickSound();
        RuntimeLog.Write("Settings button clicked! Opening Settings...");
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        OnSettingsOpened?.Invoke();
    }

    public void OnExitButtonClicked()
    {
        if (gm != null) gm.PlayButtonClickSound();
        RuntimeLog.Write("Exit button clicked! Quitting...");
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
