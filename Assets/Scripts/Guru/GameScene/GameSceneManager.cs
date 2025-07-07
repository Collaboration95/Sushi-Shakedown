using UnityEngine;
// alias Unity’s SceneManager so we can keep our class name
using UnityEngine.SceneManagement;

public class GameSceneManager : Singleton<GameSceneManager>
{
    // Optional: if you need to do anything special on awake
    public override void Awake()
    {
        base.Awake();

        Debug.Log("Displays is " + Display.displays.Length);
        // e.g. ensure Display2 is activated, if you’re using multi-display:
        // if (Display.displays.Length > 1) Display.displays[1].Activate();
    }

    public void PrintActiveScreens()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"Active scene: {scene.name}");
        }
    }

    /// <summary>
    /// Call from your MainMenu “Start Game” button
    /// </summary>
    public void StartGame()
    {
        // 1) Load Customers as the base scene (unloads MainMenu)
        SceneManager.LoadScene("Customers", LoadSceneMode.Single);

        // 2) Immediately add KitchenScene on top
        SceneManager.LoadScene("KitchenScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Call when you want to go to your Upgrades screen
    /// </summary>
    public void LoadUpgrades()
    {
        SceneManager.LoadScene("Upgrades", LoadSceneMode.Single);
    }

    /// <summary>
    /// Return to the Main Menu (you might use this on a “Back” button)
    /// </summary>
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    /// <summary>
    /// Optional: clean exit
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
