# Level Selection Menu Setup Guide

## How to Add the Level Selection Menu

### Option 1: Add to Existing Scene (Quick Setup)

1. **In your current scene (SampleScene or main menu scene)**:
   - Create an empty GameObject: Right-click in Hierarchy → Create Empty
   - Name it "LevelSelectMenu"
   - Add the `LevelSelectMenu` component: Select the GameObject → Add Component → Search for "LevelSelectMenu"

2. **Configure the Menu**:
   - **Level 1 Scene Name**: Set to "SampleScene" (or whatever your level 1 scene is named)
   - The script will automatically create the UI if buttons don't exist

3. **Test**:
   - Press Play
   - You should see a "Level 1" button in the center of the screen
   - Click it to load Level 1

### Option 2: Create a Dedicated Menu Scene (Recommended)

1. **Create a New Scene**:
   - File → New Scene → Basic (2D)
   - Save it as "MainMenu" in Assets/Scenes/

2. **Add the Menu**:
   - Create an empty GameObject named "LevelSelectMenu"
   - Add the `LevelSelectMenu` component
   - Configure the "Level 1 Scene Name" to "SampleScene"

3. **Add Scene to Build Settings**:
   - File → Build Settings
   - Click "Add Open Scenes" to add MainMenu
   - Make sure MainMenu is at index 0 (first scene)
   - Make sure SampleScene is also in the build

4. **Test**:
   - Press Play - you should start at the menu
   - Click "Level 1" to load the game

## How It Works

- The `LevelSelectMenu` script automatically creates a Canvas and UI elements if they don't exist
- It creates a "Level 1" button in the center of the screen
- Clicking the button loads the specified scene using `SceneManager.LoadScene()`
- The button has hover and press effects

## Customization

You can customize:
- **Button Colors**: Normal, Hover, Pressed colors in the inspector
- **Button Text**: Currently "Level 1" - can be changed in code or by modifying the text component
- **Button Size**: Currently 200x50 pixels - modify `rectTransform.sizeDelta` in code
- **Button Position**: Currently centered - modify `rectTransform.anchoredPosition` in code

## Adding More Levels

To add more levels in the future:
1. Create additional level scenes
2. Add more buttons in the `LevelSelectMenu` script
3. Create methods like `LoadLevel2()`, `LoadLevel3()`, etc.
4. Position buttons vertically or in a grid layout

