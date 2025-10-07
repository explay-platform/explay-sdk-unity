# explay SDK for Unity

The explay SDK let's you access the users info and store game data, allowing you to personalize the experience for the user!

## Quick Start

### 1. Install the SDK

Copy the `Assets/explay` folder to your Unity project.

### 2. Use the SDK in Your Game

```csharp
using explay.GameServices;

public class MyGame : MonoBehaviour
{
    void Start()
    {
        // Check if user is signed in
        explayGameServices.Instance.IsUserSignedIn((signedIn) => {
            Debug.Log($"User signed in: {signedIn}");
        });

        // Get user details
        explayGameServices.Instance.GetUserDetails((user) => {
            if (user != null) {
                Debug.Log($"Welcome {user.username}!");
            }
        });

        // Save high score
        explayGameServices.Instance.SaveHighScore(9999, true);

        // Load high score
        explayGameServices.Instance.GetHighScore((score) => {
            Debug.Log($"High score: {score}");
        });
    }
}
```

### 3. Build for Explay

See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) for detailed build instructions.

## API Reference

### User Methods

#### `IsUserSignedIn(callback)`
Check if the user is signed in.

```csharp
explayGameServices.Instance.IsUserSignedIn((signedIn) => {
    if (signedIn) {
        Debug.Log("User is signed in");
    }
});
```

#### `GetUserDetails(callback)`
Get the current user's details.

```csharp
explayGameServices.Instance.GetUserDetails((user) => {
    if (user != null) {
        Debug.Log($"ID: {user.id}");
        Debug.Log($"Username: {user.username}");
        Debug.Log($"Avatar: {user.avatar}");
    }
});
```

### Data Methods

#### `GetData(key, callback)`
Get a stored value by key.

```csharp
explayGameServices.Instance.GetData("playerName", (data) => {
    if (data != null) {
        Debug.Log($"Player name: {data.value}");
    }
});
```

#### `SetData(key, value, isPublic, callback)`
Store a value for a key.

```csharp
explayGameServices.Instance.SetData("playerName", "John", false, (data) => {
    Debug.Log("Data saved!");
});
```

**Parameters:**
- `key` - The key to store the data under
- `value` - The value to store (string)
- `isPublic` - Whether the data is public (visible to other users)
- `callback` - Optional callback when save completes

#### `ListData(callback)`
Get all stored data for the current user.

```csharp
explayGameServices.Instance.ListData((dataList) => {
    if (dataList != null && dataList.data != null) {
        foreach (var item in dataList.data) {
            Debug.Log($"{item.key}: {item.value}");
        }
    }
});
```

#### `DeleteData(key, callback)`
Delete a stored value by key.

```csharp
explayGameServices.Instance.DeleteData("playerName", (success) => {
    if (success) {
        Debug.Log("Data deleted");
    }
});
```

### Helper Methods

#### `SaveProgress<T>(data, isPublic, callback)`
Save complex game progress as JSON.

```csharp
[System.Serializable]
public class SaveData {
    public int level;
    public int coins;
}

var save = new SaveData { level = 5, coins = 100 };
explayGameServices.Instance.SaveProgress(save, false);
```

#### `LoadProgress<T>(callback)`
Load game progress.

```csharp
explayGameServices.Instance.LoadProgress<SaveData>((save) => {
    if (save != null) {
        Debug.Log($"Level: {save.level}");
    }
});
```

#### `SaveHighScore(score, isPublic, callback)`
Save a high score.

```csharp
explayGameServices.Instance.SaveHighScore(9999, true);
```

#### `GetHighScore(callback)`
Get the saved high score.

```csharp
explayGameServices.Instance.GetHighScore((score) => {
    Debug.Log($"High score: {score}");
});
```

## Testing Locally

### Mock Server Manager

1. Open **explay → Mock Server Manager** from Unity menu
2. Configure mock user settings (ID, username, avatar)
3. Add test data
4. Enter Play Mode

The SDK will automatically use mock data when running in the Unity Editor.

### Example Demo Scene

See `Examples/ExplaySDKDemo.cs` for a complete interactive demo with UI buttons.

## Data Models

### User
```csharp
public class User {
    public int id;
    public string username;
    public string avatar;
}
```

### GameData
```csharp
public class GameData {
    public string key;
    public string value;
    public bool isPublic;
}
```

### GameDataList
```csharp
public class GameDataList {
    public GameData[] data;
}
```

## Platform Compatibility

- **Unity Version:** 2020.3 or later
- **Platform:** WebGL only
- **Browser Support:** Modern browsers with WebGL 2.0