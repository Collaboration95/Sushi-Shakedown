using TMPro;
using UnityEngine;

public class ScoreParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int score = 0;
    public TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component
    public AudioClip coinClip;

    public AudioSource CoinAudioSource;

    public CustomerAudioManager cm;
    public CustomerData CustomerData; // Reference to the CustomerData scriptable object
    void Start()
    {
        cm = GameObject.Find("CustomerAudioManager").GetComponent<CustomerAudioManager>();
        // Initialize the score text UI with the initial score
        UpdateScoreUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DeleteCoin(GameObject coin)
    {
        HandleScore(1);
        Destroy(coin);
        cm.PlayCoinsSound();
    }

    public void ResetScore()
    {
        score = 0; // Reset the score to zero
        UpdateScoreUI();
    }

    public void SetScore(int score)
    {
        this.score = score; // Set the score to the specified value
        UpdateScoreUI(); // Update the UI to reflect the new score
    }

    public void HandleScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        scoreText.text = score.ToString(); // Update the UI text with the current score
    }
    public void OnEnable()
    {
        CustomerData.OnScoreChanged += HandleScoreChanged;
    }
    public void OnDisable()
    {
        CustomerData.OnScoreChanged -= HandleScoreChanged;
    }





    public void HandleScoreChanged(int newScore)
    {
        // Only update if the new score is greater than the current score
        if (score > newScore)
        {
            score = newScore; // Update the local score variable
            UpdateScoreUI(); // Refresh the UI to reflect the new score
        }
    }
}