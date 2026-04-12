# Intro Flow

## Goal

The intro introduces the mission context before the player gains control.

## Narrative Purpose

The player is the last active operator in Outpost-17.
A contamination breach has occurred.
The entire zone will be destroyed in 6 hours to protect humanity.
The player must hold the line until extraction or total purge.

## Functional Behavior

When the main scene starts:
1. The game enters Intro state
2. Player input is disabled
3. Intro text appears on screen
4. Briefing audio is played
5. Optional alarm / ambience starts
6. After the intro ends, the game switches to Playing state
7. Player input is enabled

## Optional Features

- Skip intro with a key press
- Fade in / fade out
- Camera lock during intro
- Subtitle display synchronized with audio