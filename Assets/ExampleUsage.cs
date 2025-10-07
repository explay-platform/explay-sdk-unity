using UnityEngine;
using Explay;

/// <summary>
/// Example usage of the Explay SDK in Unity
/// </summary>
public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        // Get current user
        GetUserExample();

        // Save and load data
        SaveDataExample();
        LoadDataExample();

        // Save and load progress
        SaveProgressExample();
        LoadProgressExample();

        // High score
        SaveHighScoreExample();
        GetHighScoreExample();
    }

    #region User Examples

    void CheckIfSignedInExample()
    {
        ExplaySDK.Instance.IsUserSignedIn((signedIn) =>
        {
            if (signedIn)
            {
                Debug.Log("User is signed in");
            }
            else
            {
                Debug.Log("User is not signed in");
            }
        });
    }

    void GetUserExample()
    {
        ExplaySDK.Instance.GetUserDetails((user) =>
        {
            if (user != null)
            {
                Debug.Log($"Current user: {user.username} (ID: {user.id})");
                if (!string.IsNullOrEmpty(user.avatar))
                {
                    Debug.Log($"Avatar: {user.avatar}");
                }
            }
            else
            {
                Debug.Log("No user logged in");
            }
        });
    }

    #endregion

    #region Data Storage Examples

    void SaveDataExample()
    {
        // Save a simple key-value pair
        ExplaySDK.Instance.SetData("playerName", "CoolPlayer123", false, (data) =>
        {
            if (data != null)
            {
                Debug.Log($"Saved: {data.key} = {data.value}");
            }
        });

        // Save public data (visible to others)
        ExplaySDK.Instance.SetData("level", "42", true, (data) =>
        {
            Debug.Log($"Saved public data: {data.key} = {data.value}");
        });
    }

    void LoadDataExample()
    {
        // Load a specific key
        ExplaySDK.Instance.GetData("playerName", (data) =>
        {
            if (data != null)
            {
                Debug.Log($"Loaded: {data.key} = {data.value}");
            }
            else
            {
                Debug.Log("Data not found");
            }
        });
    }

    void ListAllDataExample()
    {
        // List all stored data
        ExplaySDK.Instance.ListData((dataList) =>
        {
            if (dataList != null && dataList.data != null)
            {
                Debug.Log($"Found {dataList.data.Length} items:");
                foreach (var item in dataList.data)
                {
                    Debug.Log($"  {item.key}: {item.value} (public: {item.isPublic})");
                }
            }
            else
            {
                Debug.Log("No data stored");
            }
        });
    }

    void DeleteDataExample()
    {
        ExplaySDK.Instance.DeleteData("playerName", (success) =>
        {
            if (success)
            {
                Debug.Log("Data deleted successfully");
            }
        });
    }

    #endregion

    #region Progress Examples

    void SaveProgressExample()
    {
        // Define your progress data structure
        var progress = new PlayerProgress
        {
            level = 5,
            experience = 1250,
            health = 85,
            inventory = new string[] { "sword", "shield", "potion" }
        };

        // Save progress
        ExplaySDK.Instance.SaveProgress(progress, false, (data) =>
        {
            Debug.Log("Progress saved!");
        });
    }

    void LoadProgressExample()
    {
        // Load progress
        ExplaySDK.Instance.LoadProgress<PlayerProgress>((progress) =>
        {
            if (progress != null)
            {
                Debug.Log($"Loaded progress: Level {progress.level}, XP {progress.experience}");
                Debug.Log($"Health: {progress.health}");
                Debug.Log($"Inventory: {string.Join(", ", progress.inventory)}");

                // Apply the loaded progress to your game
                ApplyProgress(progress);
            }
            else
            {
                Debug.Log("No saved progress found");
            }
        });
    }

    void ApplyProgress(PlayerProgress progress)
    {
        // Apply the loaded progress to your game
        // Example:
        // player.level = progress.level;
        // player.experience = progress.experience;
        // player.health = progress.health;
        // etc.
    }

    #endregion

    #region High Score Examples

    void SaveHighScoreExample()
    {
        int score = 9999;

        ExplaySDK.Instance.SaveHighScore(score, true, (data) =>
        {
            Debug.Log($"High score saved: {score}");
        });
    }

    void GetHighScoreExample()
    {
        ExplaySDK.Instance.GetHighScore((score) =>
        {
            Debug.Log($"High score: {score}");

            // Display on UI
            // highScoreText.text = score.ToString();
        });
    }

    #endregion

    #region Game Logic Examples

    // Example: Save when player completes a level
    public void OnLevelComplete(int level)
    {
        ExplaySDK.Instance.GetUser((user) =>
        {
            if (user != null)
            {
                // Save the completed level
                ExplaySDK.Instance.SetData($"level_{level}_complete", "true", false);
                Debug.Log($"Level {level} completion saved for {user.username}");
            }
        });
    }

    // Example: Check if user has unlocked a level
    public void CheckLevelUnlock(int level, System.Action<bool> callback)
    {
        ExplaySDK.Instance.GetData($"level_{level}_complete", (data) =>
        {
            bool unlocked = data != null && data.value == "true";
            callback?.Invoke(unlocked);
        });
    }

    // Example: Auto-save progress periodically
    void SaveGameState()
    {
        var gameState = new GameState
        {
            currentLevel = 3,
            playerPosition = new Vector3Serializable
            {
                x = transform.position.x,
                y = transform.position.y,
                z = transform.position.z
            },
            enemiesDefeated = 25,
            timePlayedSeconds = 1800
        };

        ExplaySDK.Instance.SaveProgress(gameState, false);
    }

    // Example: Load game state on start
    void LoadGameState()
    {
        ExplaySDK.Instance.LoadProgress<GameState>((state) =>
        {
            if (state != null)
            {
                // Restore game state
                // SceneManager.LoadScene(state.currentLevel);
                // transform.position = new Vector3(state.playerPosition.x, state.playerPosition.y, state.playerPosition.z);
                Debug.Log($"Game state loaded: Level {state.currentLevel}");
            }
        });
    }

    #endregion
}

#region Data Structures

[System.Serializable]
public class PlayerProgress
{
    public int level;
    public int experience;
    public int health;
    public string[] inventory;
}

[System.Serializable]
public class GameState
{
    public int currentLevel;
    public Vector3Serializable playerPosition;
    public int enemiesDefeated;
    public float timePlayedSeconds;
}

[System.Serializable]
public class Vector3Serializable
{
    public float x;
    public float y;
    public float z;
}

#endregion
