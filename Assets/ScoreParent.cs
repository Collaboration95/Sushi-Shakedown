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
        if (cm == null) cm = FindFirstObjectByType<CustomerAudioManager>();
        // Initialize the score text UI with the initial score
        UpdateScoreUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DeleteCoin(GameObject coin)
    {
        Destroy(coin);
        if (cm != null) cm.PlayCoinsSound();
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
        if (scoreText != null) scoreText.text = score.ToString(); // Update the UI text with the current score
    }
    public void OnEnable()
    {
        if (CustomerData != null)
        {
            CustomerData.OnScoreChanged += HandleScoreChanged;
            SetScore(CustomerData.score);
        }
    }
    public void OnDisable()
    {
        if (CustomerData != null) CustomerData.OnScoreChanged -= HandleScoreChanged;
    }





    public void HandleScoreChanged(int newScore)
    {
        SetScore(newScore);
    }
}
