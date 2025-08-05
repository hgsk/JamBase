# Canvas Interaction System

This system implements the requested canvas drag and zoom operations with the following specifications:

## Features

1. **Component Dragging**: Individual UI components can only be dragged when the cursor shows a cross-move icon
2. **Canvas Panning**: The entire canvas can only be moved when using the hand cursor (middle mouse button)
3. **Zoom Control**: Zoom functionality is only available through middle mouse button scroll
4. **Layering**: When multiple components overlap, only the topmost element can be dragged

## Implementation

### Core Components

#### 1. CursorManager
- Manages cursor states for different interaction modes
- Provides cursor types: Normal, CrossMove, Hand
- Singleton pattern for global access

#### 2. DraggableComponent
- Makes UI elements draggable with proper layering support
- Implements IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
- Automatically detects if it's the topmost element when overlapping occurs
- Changes cursor to cross-move icon when hovering over draggable elements

#### 3. CanvasController
- Handles canvas panning and zooming
- Uses Unity's new Input System for precise control
- Middle mouse button for panning (shows hand cursor)
- Middle mouse scroll for zooming
- Prevents conflicts with component dragging

### Input System Configuration

The system extends the Unity Input Actions with a new "Canvas" action map:

```json
{
    "name": "Canvas",
    "actions": [
        {
            "name": "ComponentDrag",
            "type": "Value",
            "expectedControlType": "Vector2"
        },
        {
            "name": "CanvasPan", 
            "type": "Value",
            "expectedControlType": "Vector2"
        },
        {
            "name": "CanvasZoom",
            "type": "Value", 
            "expectedControlType": "Vector2"
        },
        {
            "name": "Point",
            "type": "Value",
            "expectedControlType": "Vector2"
        }
    ]
}
```

### Setup Instructions

1. **Add CursorManager to Scene**:
   - Create an empty GameObject named "CursorManager"
   - Add the CursorManager component
   - Assign cursor textures (normal, cross-move, hand) in the inspector

2. **Setup Canvas**:
   - Add CanvasController component to your Canvas GameObject
   - Configure pan sensitivity, zoom settings, and enable/disable flags
   - Ensure the Canvas has a GraphicRaycaster component

3. **Make UI Elements Draggable**:
   - Add DraggableComponent to any UI element you want to be draggable
   - Set sortingOrder if you need specific layering behavior
   - Ensure the element has a RectTransform and appropriate UI components

4. **Demo Scene**:
   - Use CanvasInteractionDemo component to automatically create sample draggable components
   - Attach to Canvas and configure number of components and spawn area

### Usage

#### For Developers:
```csharp
// Make an existing UI element draggable
gameObject.AddComponent<DraggableComponent>();

// Access cursor manager
CursorManager.Instance.SetCursor(CursorManager.CursorType.Hand);

// Configure canvas controller
var canvasController = canvas.GetComponent<CanvasController>();
canvasController.panSensitivity = 2.0f;
canvasController.enableZooming = false;
```

#### For Users:
- **Drag Components**: Hover over UI components until cursor changes to cross-move icon, then click and drag
- **Pan Canvas**: Hold middle mouse button and drag to move the entire canvas view (cursor shows hand icon)
- **Zoom**: Use middle mouse scroll wheel to zoom in/out
- **Layering**: Only the topmost component responds to drag operations when multiple elements overlap

### Technical Details

#### Interaction Priority:
1. Component dragging has highest priority (only works with cross cursor)
2. Canvas panning works when not over draggable components (middle mouse + hand cursor)  
3. Zooming works independently via scroll wheel

#### Cursor Management:
- Normal cursor: Default state
- Cross-move cursor: When hovering over draggable components
- Hand cursor: When panning the canvas

#### Layering System:
- Uses Unity's UI layering (sibling index) and sorting order
- Raycasting determines topmost element for overlapping scenarios
- Only topmost element responds to drag events

### Files Added:
- `Assets/Scripts/Core/CanvasInteraction.cs` - Core interaction system
- `Assets/Scripts/Core/CanvasInteractionDemo.cs` - Demo and testing utilities
- `Assets/Scenes/CanvasInteractionDemo.unity` - Sample scene
- Updated `Assets/InputSystem_Actions.inputactions` - Added Canvas action map

### Dependencies:
- Unity 2023.2.20f1 or later
- Unity Input System package
- Unity UI package
- TextMeshPro package (for demo components)

This implementation provides a clean, modular system that exactly matches the requested specifications for canvas interaction behavior.