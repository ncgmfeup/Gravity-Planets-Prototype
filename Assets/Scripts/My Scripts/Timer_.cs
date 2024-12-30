using UnityEngine;
using UnityEngine.UI;

public class Timer_ : MonoBehaviour
{
    public Text timerText;
    public Text levelTimeText; // Text for displaying the exact time taken to beat the level
    public Text bestTimeText;  // Text for displaying the best time
    public GameObject newRecord;

    private float elapsedTime = 0f;
    private float bestTime = float.MaxValue; // Initialize with a very high value

    public int levelIndex;

    void Start()
    {
        // Load the best time for the current level
        bestTime = PlayerPrefs.GetFloat("BestTime_Level_" + levelIndex, bestTime);
        
        newRecord.SetActive(false);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);
        int milliseconds = (int)((elapsedTime * 100) % 100);

        string timerString = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        timerText.text = timerString;
    }

    public void OnLevelComplete()
    {
        float levelCompletionTime = elapsedTime;

        int minutes = (int)(levelCompletionTime / 60);
        int seconds = (int)(levelCompletionTime % 60);
        int milliseconds = (int)((levelCompletionTime * 100) % 100);

        string levelTimeString = string.Format("Your Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        levelTimeText.text = levelTimeString;

        string bestTimeKey = "BestTime_Level_" + levelIndex;

        bestTime = PlayerPrefs.GetFloat(bestTimeKey, bestTime); // Get the best time for the current level
        minutes = (int)(bestTime / 60);
        seconds = (int)(bestTime % 60);
        milliseconds = (int)((bestTime * 100) % 100);

        string bestTimeString = string.Format("Best Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        bestTimeText.text = bestTimeString;

        if (levelCompletionTime < bestTime)
        {
            newRecord.SetActive(true);

            bestTime = levelCompletionTime;
            PlayerPrefs.SetFloat(bestTimeKey, bestTime); // Save the best time for the current level

            minutes = (int)(bestTime / 60);
            seconds = (int)(bestTime % 60);
            milliseconds = (int)((bestTime * 100) % 100);

            string newBestTimeString = string.Format("Best Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
            bestTimeText.text = newBestTimeString;
        }
    }
}
