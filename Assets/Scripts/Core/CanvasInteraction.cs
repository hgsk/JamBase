using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace Core.CanvasInteraction
{
    /// <summary>
    /// Manages cursor states for different interaction modes
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        [Header("Cursor Textures")]
        public Texture2D normalCursor;
        public Texture2D crossMoveCursor;
        public Texture2D handCursor;
        
        private static CursorManager instance;
        public static CursorManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<CursorManager>();
                return instance;
            }
        }
        
        public enum CursorType
        {
            Normal,
            CrossMove,
            Hand
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public void SetCursor(CursorType cursorType)
        {
            Texture2D targetCursor = cursorType switch
            {
                CursorType.CrossMove => crossMoveCursor,
                CursorType.Hand => handCursor,
                _ => normalCursor
            };
            
            if (targetCursor != null)
            {
                Vector2 hotSpot = new Vector2(targetCursor.width / 2f, targetCursor.height / 2f);
                Cursor.SetCursor(targetCursor, hotSpot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }
    
    /// <summary>
    /// Component that can be dragged within the canvas
    /// </summary>
    public class DraggableComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Drag Settings")]
        public int sortingOrder = 0;
        public bool isDraggable = true;
        
        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private bool isDragging = false;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isDraggable && IsTopMostElement(eventData.position))
            {
                CursorManager.Instance?.SetCursor(CursorManager.CursorType.CrossMove);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDragging)
            {
                CursorManager.Instance?.SetCursor(CursorManager.CursorType.Normal);
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable || !IsTopMostElement(eventData.position))
                return;
                
            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
            
            // Bring to front by increasing sorting order
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.8f;
            }
            
            // Move to front in hierarchy
            transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || !isDraggable)
                return;
                
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform, 
                eventData.position, 
                parentCanvas.worldCamera, 
                out localPointerPosition))
            {
                rectTransform.anchoredPosition = localPointerPosition;
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
                
            isDragging = false;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1.0f;
            }
            
            CursorManager.Instance?.SetCursor(CursorManager.CursorType.Normal);
        }
        
        /// <summary>
        /// Check if this component is the topmost element at the given screen position
        /// </summary>
        private bool IsTopMostElement(Vector2 screenPosition)
        {
            var results = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            
            EventSystem.current.RaycastAll(eventData, results);
            
            // Find all DraggableComponents hit by the raycast
            var draggableResults = results
                .Where(r => r.gameObject.GetComponent<DraggableComponent>() != null)
                .OrderBy(r => r.sortingOrder)
                .ThenBy(r => r.gameObject.transform.GetSiblingIndex())
                .ToList();
                
            if (draggableResults.Count == 0)
                return false;
                
            // Return true if this is the last (topmost) element
            return draggableResults.Last().gameObject == gameObject;
        }
    }
    
    /// <summary>
    /// Manages canvas panning and zooming functionality
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        [Header("Pan Settings")]
        public float panSensitivity = 1.0f;
        public bool enablePanning = true;
        
        [Header("Zoom Settings")]
        public float zoomSensitivity = 0.1f;
        public float minZoom = 0.5f;
        public float maxZoom = 3.0f;
        public bool enableZooming = true;
        
        private RectTransform canvasRectTransform;
        private Vector2 lastPanPosition;
        private bool isPanning = false;
        private bool isOverDraggableComponent = false;
        
        // Input System references
        private PlayerInput playerInput;
        private InputAction canvasPanAction;
        private InputAction canvasZoomAction;
        private InputAction pointAction;
        private InputAction middleClickAction;
        
        private void Awake()
        {
            canvasRectTransform = GetComponent<RectTransform>();
            
            // Setup Input System
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }
            
            // Set to Canvas action map
            playerInput.SwitchCurrentActionMap("Canvas");
            
            canvasPanAction = playerInput.actions["CanvasPan"];
            canvasZoomAction = playerInput.actions["CanvasZoom"];
            pointAction = playerInput.actions["Point"];
            
            // Use UI action map for middle click
            var uiActionMap = playerInput.actions.FindActionMap("UI");
            middleClickAction = uiActionMap["MiddleClick"];
        }
        
        private void OnEnable()
        {
            if (canvasPanAction != null)
            {
                canvasPanAction.performed += OnPanPerformed;
                canvasPanAction.canceled += OnPanCanceled;
            }
            
            if (canvasZoomAction != null)
            {
                canvasZoomAction.performed += OnZoomPerformed;
            }
        }
        
        private void OnDisable()
        {
            if (canvasPanAction != null)
            {
                canvasPanAction.performed -= OnPanPerformed;
                canvasPanAction.canceled -= OnPanCanceled;
            }
            
            if (canvasZoomAction != null)
            {
                canvasZoomAction.performed -= OnZoomPerformed;
            }
        }
        
        private void Update()
        {
            CheckForDraggableComponents();
            HandlePanning();
        }
        
        private void CheckForDraggableComponents()
        {
            if (pointAction == null) return;
            
            Vector2 mousePosition = pointAction.ReadValue<Vector2>();
            
            var results = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };
            
            EventSystem.current.RaycastAll(eventData, results);
            
            bool wasOverDraggable = isOverDraggableComponent;
            isOverDraggableComponent = results.Any(r => r.gameObject.GetComponent<DraggableComponent>() != null);
            
            // Update cursor based on what we're hovering over
            if (isOverDraggableComponent && !wasOverDraggable)
            {
                // Don't override if already showing cross cursor for draggable
                return;
            }
            else if (!isOverDraggableComponent && wasOverDraggable)
            {
                CursorManager.Instance?.SetCursor(CursorManager.CursorType.Normal);
            }
        }
        
        private void HandlePanning()
        {
            if (!enablePanning) return;
            
            // Only pan when not over draggable components and middle mouse button is held
            bool middleMouseHeld = middleClickAction != null && middleClickAction.IsPressed();
            
            if (middleMouseHeld && !isOverDraggableComponent)
            {
                if (!isPanning)
                {
                    isPanning = true;
                    lastPanPosition = pointAction.ReadValue<Vector2>();
                    CursorManager.Instance?.SetCursor(CursorManager.CursorType.Hand);
                }
                
                Vector2 currentPosition = pointAction.ReadValue<Vector2>();
                Vector2 delta = (currentPosition - lastPanPosition) * panSensitivity;
                
                canvasRectTransform.anchoredPosition += delta;
                lastPanPosition = currentPosition;
            }
            else if (isPanning)
            {
                isPanning = false;
                CursorManager.Instance?.SetCursor(CursorManager.CursorType.Normal);
            }
        }
        
        private void OnPanPerformed(InputAction.CallbackContext context)
        {
            // This method can be used for additional pan logic if needed
        }
        
        private void OnPanCanceled(InputAction.CallbackContext context)
        {
            isPanning = false;
            CursorManager.Instance?.SetCursor(CursorManager.CursorType.Normal);
        }
        
        private void OnZoomPerformed(InputAction.CallbackContext context)
        {
            if (!enableZooming) return;
            
            Vector2 scrollDelta = context.ReadValue<Vector2>();
            float zoomDelta = scrollDelta.y * zoomSensitivity;
            
            Vector3 currentScale = canvasRectTransform.localScale;
            float newScale = Mathf.Clamp(currentScale.x + zoomDelta, minZoom, maxZoom);
            
            canvasRectTransform.localScale = Vector3.one * newScale;
        }
    }
}