# Requirements Analysis

## Gameplay

- Player moves with WASD
- Camera controlled with mouse
- Player can shoot using left click
- Player starts with first gun
- Player can buy additional guns
- Player can reload ammo
- Ammo must be consumed when shooting
- Player loses if HP reaches 0
- Player loses if enemies reach core
- Player wins after last wave

## Enemies

- 3 enemy types
- Inheritance required
- Unique attack behaviors
- Unique movement behavior
- Animated (idle, move, attack, death)
- Must stop acting when dead
- Must grant resources when killed

## Systems

- Wave system with scaling
- Pause between waves
- EnemyManager (max 3 chase player rule)
- ResourceManager (singleton)
- UI must use events
- Tooltips required
- Post-processing required
- VFX required
- Audio required