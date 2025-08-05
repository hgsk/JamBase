using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.CanvasInteraction;

namespace Core
{
    /// <summary>
    /// Creates sample UI components for testing the canvas interaction system
    /// </summary>
    public class CanvasInteractionDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        public GameObject draggableComponentPrefab;
        public int numberOfComponents = 5;
        public Vector2 spawnAreaSize = new Vector2(800, 600);
        
        private void Start()
        {
            CreateDemoComponents();
        }
        
        private void CreateDemoComponents()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("CanvasInteractionDemo must be attached to a Canvas GameObject");
                return;
            }
            
            // Ensure canvas has CanvasController
            if (canvas.GetComponent<CanvasController>() == null)
            {
                canvas.gameObject.AddComponent<CanvasController>();
            }
            
            // Create sample draggable components if prefab not provided
            if (draggableComponentPrefab == null)
            {
                CreateDefaultDraggableComponents();
            }
            else
            {
                CreatePrefabComponents();
            }
        }
        
        private void CreateDefaultDraggableComponents()
        {
            for (int i = 0; i < numberOfComponents; i++)
            {
                // Create UI element
                GameObject componentObj = new GameObject($"DraggableComponent_{i}");
                componentObj.transform.SetParent(transform);
                
                // Add RectTransform
                RectTransform rectTransform = componentObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, 100);
                
                // Random position within spawn area
                Vector2 randomPos = new Vector2(
                    Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                    Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
                );
                rectTransform.anchoredPosition = randomPos;
                
                // Add Image component
                Image image = componentObj.AddComponent<Image>();
                image.color = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), 0.8f);
                
                // Add text label
                GameObject textObj = new GameObject("Label");
                textObj.transform.SetParent(componentObj.transform);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = $"Item {i + 1}";
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 14;
                text.color = Color.white;
                
                // Add DraggableComponent
                DraggableComponent draggable = componentObj.AddComponent<DraggableComponent>();
                draggable.sortingOrder = i;
                
                // Add GraphicRaycaster target
                componentObj.AddComponent<GraphicRaycaster>();
            }
        }
        
        private void CreatePrefabComponents()
        {
            for (int i = 0; i < numberOfComponents; i++)
            {
                GameObject instance = Instantiate(draggableComponentPrefab, transform);
                
                // Random position within spawn area
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 randomPos = new Vector2(
                        Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                        Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
                    );
                    rectTransform.anchoredPosition = randomPos;
                }
                
                // Ensure DraggableComponent exists
                if (instance.GetComponent<DraggableComponent>() == null)
                {
                    DraggableComponent draggable = instance.AddComponent<DraggableComponent>();
                    draggable.sortingOrder = i;
                }
            }
        }
        
        /// <summary>
        /// Create a cursor manager if one doesn't exist
        /// </summary>
        [ContextMenu("Setup Cursor Manager")]
        public void SetupCursorManager()
        {
            if (CursorManager.Instance == null)
            {
                GameObject cursorManagerObj = new GameObject("CursorManager");
                cursorManagerObj.AddComponent<CursorManager>();
                
                Debug.Log("CursorManager created. Please assign cursor textures in the inspector.");
            }
        }
    }
}