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
Project planning
The user stories and requirements proposed for the prototype are updated with the status.

First Prototype

User Stories
US01: As a player, I want to alternate between light and darkness with a single button, so I can quickly experiment with both world states and understand the core game mechanic without unnecessary friction. The transition must be instant and visually clear. Done
	This mechanic was smoothly implemented with all the needed details
US02: As a player, I want to see blocks and platforms appear only in darkness, so I understand that darkness is not just dangerous, but also necessary for progression. These blocks must be completely invisible in light, with no visual indicator suggesting their presence. Done
	This mechanic was implemented in a way where some are in both states, and some are in only one state
US03: As a player, I want to collect lightning bolts visible in both states, so I have clear progression objectives that don't depend exclusively on one state or the other. Lightning bolts may be positioned in a way that requires navigation through both states to reach them. Done
	Lightning bolts may be visible during the day but sometimes unreachable.
US04: As a player, I want to encounter enemies that appear exclusively in darkness, so I experience the tension and risk inherent to exploring darkness. These enemies should be a clear threat but not instantly fatal, allowing learning through trial and error. In progress
	The enemies were not made correctly as intended previously. There are some enemies which effectively deal “death” damage, but it wasn’t fully developed
US05: As a player, I want basic platform movement (jump, run, fall) that works identically in both states, so the light-darkness mechanic is the main variable, not control. In progress
	The mechanics are already implemented, but it doesn’t perform the images for each different movement correctly. But the character makes them all
Functional Requirements
FR01: Responsive input system with state alternation mapped to dedicated key/button; Done
FR02: Rendering of two asset sets of the same level: "light" version and "darkness" version; Done
FR03: Collision system that works correctly for objects visible only in one state; In progress
FR04: Spawn/despawn of enemies based on current state; Done
FR05: Lightning collection system with visual counter; Done
FR06: Map with 3-5 areas that demonstrate different uses of the light-darkness mechanic; Done
FR07: Basic death/respawn system when colliding with enemies; In progress
FR08: Minimal HUD showing current state (light/darkness) and number of collected lightning bolts; In progress
Non-functional Requirements
NFR01: Transitions between states must be complete in less than 0.1 seconds without perceptible stuttering. Done
NFR02: The alternation key must be immediately intuitive (e.g., Space or Shift) and clearly communicated to the player Done
NFR03: The prototype must run for 30-minute test sessions without crashes or significant memory leaks Not Done
NFR04: The game must start without additional configuration on varied test PCs, with adaptive resolution between 1280x720 and 1920x1080 Not Done

Final Prototype

User Stories
US06: As a player, I want a narrative progression that unfolds through lightning collection, so I feel my actions have purpose beyond the immediate mechanic. Each collected lightning bolt should reveal more about the world and its story. In progress
US07: As a player, I want to face complex puzzles that require multiple strategic alternations between light and darkness, where I must memorize layouts, plan routes, and make quick decisions under enemy pressure. In progress
US08: As a player, I want to experience variations in the light-darkness mechanic, such as partially illuminated areas, temporary light sources I can activate, or zones where alternation is limited for some narrative reason, so gameplay doesn't become repetitive. In progress
US09: As a player, I want rich audiovisual feedback when I alternate states: changes in music or sound atmosphere, visual effects that indicate the transition, and perhaps even small camera shakes, so each alternation feels impactful. In progress
US10: As a player, I want different enemy types with varied behaviors related to light: some that flee from illuminated areas, others that are more aggressive near lightning bolts, and perhaps enemies that transform between states. In progress
US11: As a player, I want an intelligent checkpoint/respawn system that doesn't frustrate but also doesn't completely eliminate challenge, allowing me to take risks without losing significant progress when I fail. In progress
US12: As a player, I want to discover secrets and alternative paths through careful exploration of both states, rewarding my curiosity and attention to detail. In progress

Functional Requirements
FR08: Complete level system with at least 10-15 progressively challenging levels, each introducing new elements or mechanic variations In progress
FR09: Multiple enemy types (minimum 3-5) with differentiated AI and unique behaviors related to light mechanics In progress
FR10: Progression system with unlock of new areas based on number of collected lightning bolts In progress
FR11: Mechanical variations: partially lit areas, interactive objects that modify illumination, timed sequences In progress
FR12: Dynamic audio system with music/atmosphere that changes based on current state and smooth transitions between sound states In progress
FR13: Particle system and visual effects for transitions, lightning collection, and enemy interactions
FR14: Main menu, options system (audio, graphics, customizable controls), and credits screen
FR15: Save/load system that preserves progress between sessions
FR16: Integrated tutorial system that teaches mechanics gradually without interrupting flow
FR17: Dynamic camera with smooth adjustments and possibly effects like zoom or shake at appropriate moments

Non-functional Requirements 
NFR05: Stable 60 FPS on recommended hardware, minimum 45 FPS on minimum hardware, with culling optimizations, LOD for effects, and efficient memory management for objects that appear/disappear In progress
NFR06: Consistent artistic style that works well in both lighting states, with distinct but harmonious color palettes for light and darkness In progress
NFR07: Each level must be completable in 3-10 minutes for an experienced player, with difficulty curve that increases gradually without abrupt spikes In progress
NFR08: Options to adjust difficulty (if applicable), support for multiple control schemes, and consideration for colorblind players in color/contrast choices In progress
NFR09: Crash rate below 0.1% of game sessions, with robust save system that prevents progress loss In progress
NFR10: Tested and functional on at least 5 different PC hardware configurations (varying GPU, CPU, resolution) to ensure broad compatibility In progress
NFR11: Preparation of codebase for future localization (externalized texts), even if initial version is in a single language In progress
Achieved during sprint 1
•	US01, US02, US03
•	FR01, FR02, FR05, FR06
•	NFR01, NFR02
Planned for sprint 2
All remaining, in progress and not yet implemented user stories and requirements.
