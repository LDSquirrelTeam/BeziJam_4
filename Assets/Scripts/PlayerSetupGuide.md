# 2D Platformer Player Setup Guide

## Setup Instructions

### 1. Create Input Actions Asset
1. In the Project window, right-click in the Assets folder
2. Go to **Create > Input Actions**
3. Name it `PlayerInputActions`
4. Double-click to open the Input Actions Editor

### 2. Configure Input Actions
In the Input Actions Editor:

#### Create Action Map:
- Rename the default Action Map to "Gameplay"

#### Add Actions:
1. **Move Action:**
   - Name: "Move"
   - Action Type: Value
   - Control Type: Vector2

2. **Jump Action:**
   - Name: "Jump" 
   - Action Type: Button
   - Control Type: Button

#### Add Bindings:
For **Move** action:
- Add "2D Vector Composite"
- Set Up: W, Down: S, Left: A, Right: D
- Add another "2D Vector Composite" for Arrow Keys
- Add Gamepad Left Stick binding

For **Jump** action:
- Add Spacebar binding
- Add Gamepad South Button (A/X button)

### 3. Generate C# Class
- In the Input Actions Editor, click "Generate C# Class"
- This creates the script interface for your actions

### 4. Save the Asset
- Click "Save Asset" in the Input Actions Editor

## Player GameObject Setup

### Required Components:
1. **Rigidbody2D** - For physics movement
2. **Collider2D** (BoxCollider2D or CapsuleCollider2D) - For collision detection
3. **SpriteRenderer** - For visual representation
4. **PlayerInput** - For Input System integration
5. **PlayerController** - The provided movement script

### Component Configuration:

#### Rigidbody2D Settings:
- Body Type: Dynamic
- Gravity Scale: 3-5 (adjust for desired fall speed)
- Freeze Rotation Z: Check this to prevent spinning

#### PlayerInput Settings:
- Actions: Assign your PlayerInputActions asset
- Default Map: Gameplay
- Behavior: Send Messages or Invoke Unity Events

#### PlayerController Settings:
- Move Speed: 8 (adjust as needed)
- Jump Force: 16 (adjust for jump height)
- Ground Layer Mask: Set to your ground layer

### Ground Check Setup:
1. Create an empty GameObject as a child of the player
2. Name it "GroundCheck"
3. Position it at the bottom of the player's collider
4. Assign this GameObject to the Ground Check field in PlayerController

### Layer Setup:
1. Create a "Ground" layer in the Layers dropdown
2. Assign your ground/platform GameObjects to this layer
3. Set the Ground Layer Mask in PlayerController to include only the Ground layer

## Features Included:
- WASD and Arrow Key movement
- Gamepad support
- Coyote time (grace period for jumping after leaving ground)
- Jump buffering (early jump input registration)
- Variable jump height (release jump early for shorter jumps)
- Air control with reduced effectiveness
- Sprite flipping based on movement direction
- Ground detection with visual gizmos

## Testing:
1. Assign the PlayerController script to your player GameObject
2. Configure all the required components and references
3. Create some ground platforms with colliders on the Ground layer
4. Test movement with keyboard/gamepad inputs