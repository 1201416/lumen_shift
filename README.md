# LUMEN SHIFT

## Level Framework

By using Unity scene templates (Assets/Settings/Scenes), we developed separate scenes for each levels

Lighting/time-of-day settings are located in shared assets. Each scene reads from it so level-specific tweaking stays minimal.

We also use Unity’s Global Volume (Assets/DefaultVolumeProfile.asset) plus a blendable lighting controller. Animate color grading, exposure, and shadows to convey time changes without duplicating assets.

## Block System

Block types are ScriptableObjects, each defining visuals, collision, whether it reacts to time of day, etc.. Further, we can drop blocks into levels without modifying code.

Group blocks are using URP’s 2D Renderer/Tilemap, which concatenates the information. 

There will also be a block manager per level that listens to the global time-of-day event and enables/disables or animates blocks that opt into that behavior.

## Time-of-Day Gameplay Layer

TimeOfDayController handles state (Day/Night) and drives:

- Lighting/volumetric settings.
- Block behavior.
- Enemy AI modifiers.

Exposition of hooks so future systems can subscribe.
