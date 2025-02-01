# Game Engine for RTS

This is a game engine designed for creating and playing real-time strategy (RTS) games. Below are the controls and features provided by the engine:

---

## Controls

### Unit Management
- **Barracks Unit Production:**
  - Press `1`, `2`, or `3` (not on the numpad) to train different types of units in the barracks.

- **Camera Centering:**
  - Press `Space` to center the camera on the scene's center or selected units.

- **Command Queueing:**
  - Hold `Left Shift` to queue commands for units, allowing you to set a sequential order of tasks.

---

## Building System
- **Unit Builder:**
  - The builder unit can now be trained in the barracks by pressing `1`.
  - Press `B` to open the building menu (interface not implemented yet).
  - Available buildings:
    - `B` - Barracks
    - `T` - Wall
  - A construction grid has been implemented for easier building placement.

---

## Testing Features
- `Numpad 1`: All enemy units attack friendly units.
- `Numpad 2`: All friendly units attack enemy units.

---

## Combat Commands
- `A + Left Click` (A-click): Issue an attack-move command. Units will move to the selected point and attack all enemies encountered along the way.
- `H` (Hold): Units hold their position and will not auto-attack nearby enemies.

---

## Additional Controls
- `F8`: Locks the camera in place, preventing any further movement.

---

## Note
This is an early version of the engine. Many new features and improvements are planned for future updates!

---
