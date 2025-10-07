# Building Your Game for explay

Follow these instructions to build your Unity game for the explay platform.

## Build Settings

1. Open **File → Build Settings**
2. Select **WebGL** platform
3. Click **Switch Platform** (if not already on WebGL)

### Recommended Settings

**Resolution and Presentation:**
- Default Canvas Width: `1920`
- Default Canvas Height: `1080`
- Run In Background: `✓ Enabled`

**Other Settings:**
- Color Space: `Linear` (for better graphics)
- Auto Graphics API: `✓ Enabled`

## Upload to explay

1. Go to [explay](https://explay.com) and sign in
2. Click **Upload Game**
3. Select your build folder
4. The folder should contain:
   - `index.html`
   - `Build/` folder with `.data`, `.framework.js`, `.loader.js`, `.wasm` files
   - `TemplateData/` folder (optional)

## Integrating explay SDK

1. Copy the following files to your Unity project:
   - `Assets/explay/GameServices/explayGameServices.cs`
   - `Assets/explay/GameServices/explayGameServices.jslib`

2. Use the SDK in your game:

```csharp
using explay.GameServices;

// Get user info
explayGameServices.Instance.GetUserDetails((user) => {
    if (user != null) {
        Debug.Log($"Welcome {user.username}!");
    }
});

// Save data
explayGameServices.Instance.SetData("highScore", "9999", true);

// Load data
explayGameServices.Instance.GetData("highScore", (data) => {
    if (data != null) {
        Debug.Log($"High score: {data.value}");
    }
});
```

3. See `Examples/explaySDKDemo.cs` for a complete interactive demo

## Testing Locally in the editor

1. Open **explay → Mock Server Manager** from the menu
2. Configure mock user settings
3. Add test data
4. Enter Play Mode
5. The SDK will use mock data instead of real API calls

## Troubleshooting

### Build is too large
- Reduce texture sizes
- Enable texture compression (DXT, ASTC)
- Strip engine code in Player Settings → Other Settings → Stripping Level: `Medium` or `High`
- Disable unnecessary Unity modules in Player Settings

### Game doesn't load on explay
- Check browser console for errors
- Ensure all files are uploaded (especially the Build folder)
- If the files aren't loading correctly it might be a CDN issue, in that case please contact us