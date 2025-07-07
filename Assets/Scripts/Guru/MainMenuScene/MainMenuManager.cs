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
        gm.PlayButtonClickSound();
        Debug.Log("Play button clicked! Loading Game scene...");
        EventSystem.current.SetSelectedGameObject(null);
        GameSceneManager.instance.StartGame();
    }

    public void OnSettingsButtonClicked()
    {
        gm.PlayButtonClickSound();
        Debug.Log("Settings button clicked! Opening Settings...");
        EventSystem.current.SetSelectedGameObject(null);
        OnSettingsOpened?.Invoke();
    }

    public void OnExitButtonClicked()
    {
        gm.PlayButtonClickSound();
        Debug.Log("Exit button clicked! Quitting...");
        EventSystem.current.SetSelectedGameObject(null);

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
