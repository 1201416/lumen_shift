# UI Setup Guide - Day/Night Toggle Button

## How to Add the Day/Night Toggle Button

1. **Create a Canvas** (if you don't have one):
   - Right-click in Hierarchy → UI → Canvas
   - This creates a Canvas with an EventSystem

2. **Create a Button**:
   - Right-click on Canvas → UI → Button
   - Position it where you want (e.g., top-right corner)

3. **Add the DayNightToggle Script**:
   - Select the Button in Hierarchy
   - In Inspector, click "Add Component"
   - Search for "DayNightToggle" and add it

4. **Configure the Script**:
   - **Game Manager**: Drag the GameManager object from your scene (or leave empty - it will find it automatically)
   - **Button Text** (Optional): If you want text on the button showing "Day" or "Night":
     - Right-click on Button → UI → Text (Legacy) or TextMeshPro - Text (UI)
     - Drag this Text component to the "Button Text" field in DayNightToggle

5. **Customize Button Appearance** (Optional):
   - Select the Button
   - In Inspector, expand "Button" component
   - Customize colors, text, etc.

## Alternative: Use Keyboard (T key)

You can also toggle day/night by pressing **T** on the keyboard (already implemented in GameManager).

## How It Works

- **Day Mode**: Boxes with `visibleDuringDay = true` are visible
- **Night Mode**: Boxes with `visibleDuringDay = false` are visible
- Floor blocks are always visible (they have grass+dirt texture)
- Toggling switches which boxes appear/disappear

