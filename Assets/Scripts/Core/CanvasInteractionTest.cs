using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.CanvasInteraction;

namespace Core
{
    /// <summary>
    /// Test script to validate canvas interaction functionality
    /// </summary>
    public class CanvasInteractionTest : MonoBehaviour
    {
        [Header("Test Results")]
        public TextMeshProUGUI statusText;
        public Button runTestsButton;
        
        [Header("Test Objects")]
        public DraggableComponent testComponent1;
        public DraggableComponent testComponent2;
        public CanvasController canvasController;
        
        private void Start()
        {
            if (runTestsButton != null)
                runTestsButton.onClick.AddListener(RunAllTests);
            
            UpdateStatusText("Canvas Interaction System Ready");
        }
        
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            UpdateStatusText("Running Canvas Interaction Tests...");
            
            bool allTestsPassed = true;
            string results = "Test Results:\n";
            
            // Test 1: Cursor Manager
            allTestsPassed &= TestCursorManager(ref results);
            
            // Test 2: Draggable Components
            allTestsPassed &= TestDraggableComponents(ref results);
            
            // Test 3: Canvas Controller
            allTestsPassed &= TestCanvasController(ref results);
            
            // Test 4: Input System Integration
            allTestsPassed &= TestInputSystemIntegration(ref results);
            
            results += $"\nOverall Result: {(allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}";
            UpdateStatusText(results);
        }
        
        private bool TestCursorManager(ref string results)
        {
            try
            {
                var cursorManager = CursorManager.Instance;
                if (cursorManager == null)
                {
                    results += "❌ CursorManager: Instance not found\n";
                    return false;
                }
                
                // Test cursor type switching
                cursorManager.SetCursor(CursorManager.CursorType.CrossMove);
                cursorManager.SetCursor(CursorManager.CursorType.Hand);
                cursorManager.SetCursor(CursorManager.CursorType.Normal);
                
                results += "✅ CursorManager: All cursor types can be set\n";
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ CursorManager: Exception - {e.Message}\n";
                return false;
            }
        }
        
        private bool TestDraggableComponents(ref string results)
        {
            try
            {
                int draggableCount = FindObjectsOfType<DraggableComponent>().Length;
                
                if (draggableCount == 0)
                {
                    results += "⚠️ DraggableComponents: No draggable components found (this may be expected)\n";
                    return true;
                }
                
                results += $"✅ DraggableComponents: Found {draggableCount} draggable components\n";
                
                // Test that components have required dependencies
                foreach (var draggable in FindObjectsOfType<DraggableComponent>())
                {
                    if (draggable.GetComponent<RectTransform>() == null)
                    {
                        results += "❌ DraggableComponents: Missing RectTransform\n";
                        return false;
                    }
                }
                
                results += "✅ DraggableComponents: All have required RectTransform\n";
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ DraggableComponents: Exception - {e.Message}\n";
                return false;
            }
        }
        
        private bool TestCanvasController(ref string results)
        {
            try
            {
                var canvasController = FindObjectOfType<CanvasController>();
                if (canvasController == null)
                {
                    results += "❌ CanvasController: Not found in scene\n";
                    return false;
                }
                
                // Test settings
                if (canvasController.panSensitivity <= 0)
                {
                    results += "❌ CanvasController: Invalid pan sensitivity\n";
                    return false;
                }
                
                if (canvasController.zoomSensitivity <= 0)
                {
                    results += "❌ CanvasController: Invalid zoom sensitivity\n";
                    return false;
                }
                
                if (canvasController.minZoom >= canvasController.maxZoom)
                {
                    results += "❌ CanvasController: Invalid zoom range\n";
                    return false;
                }
                
                results += "✅ CanvasController: All settings are valid\n";
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ CanvasController: Exception - {e.Message}\n";
                return false;
            }
        }
        
        private bool TestInputSystemIntegration(ref string results)
        {
            try
            {
                var canvasController = FindObjectOfType<CanvasController>();
                if (canvasController == null)
                {
                    results += "❌ Input System: CanvasController not found\n";
                    return false;
                }
                
                var playerInput = canvasController.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput == null)
                {
                    results += "⚠️ Input System: PlayerInput component not found (will be created at runtime)\n";
                    return true;
                }
                
                results += "✅ Input System: PlayerInput component configured\n";
                return true;
            }
            catch (System.Exception e)
            {
                results += $"❌ Input System: Exception - {e.Message}\n";
                return false;
            }
        }
        
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
            
            Debug.Log($"[CanvasInteractionTest] {message}");
        }
        
        /// <summary>
        /// Create a simple test draggable component for testing
        /// </summary>
        [ContextMenu("Create Test Draggable")]
        public void CreateTestDraggable()
        {
            GameObject testObj = new GameObject("TestDraggable");
            testObj.transform.SetParent(transform);
            
            RectTransform rect = testObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            rect.anchoredPosition = Vector2.zero;
            
            Image image = testObj.AddComponent<Image>();
            image.color = Color.cyan;
            
            DraggableComponent draggable = testObj.AddComponent<DraggableComponent>();
            
            UpdateStatusText("Test draggable component created");
        }
    }
}