using UnityEngine;

namespace Core.CanvasInteraction
{
    /// <summary>
    /// Utility class to create default cursor textures if none are provided
    /// This ensures the system works out of the box without requiring custom cursors
    /// </summary>
    public static class DefaultCursorTextures
    {
        public static Texture2D CreateNormalCursor()
        {
            // Create a simple arrow cursor (16x16 pixels)
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            
            // Fill with transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw simple arrow shape
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int index = y * 16 + x;
                    
                    // Arrow shape
                    if ((x == 0 && y < 12) || // Left edge
                        (y == 0 && x < 8) ||  // Top edge
                        (x == y && x < 8))    // Diagonal
                    {
                        pixels[index] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D CreateCrossMoveCursor()
        {
            // Create a cross/plus cursor (16x16 pixels)
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            
            // Fill with transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw cross shape
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int index = y * 16 + x;
                    
                    // Vertical line
                    if (x == 8 && y >= 2 && y <= 13)
                        pixels[index] = Color.white;
                    
                    // Horizontal line
                    if (y == 8 && x >= 2 && x <= 13)
                        pixels[index] = Color.white;
                    
                    // Arrow heads
                    if ((x == 7 || x == 9) && (y == 1 || y == 14))
                        pixels[index] = Color.white;
                    if ((y == 7 || y == 9) && (x == 1 || x == 14))
                        pixels[index] = Color.white;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D CreateHandCursor()
        {
            // Create a hand cursor (16x16 pixels)
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            
            // Fill with transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw hand shape (simplified)
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int index = y * 16 + x;
                    
                    // Simplified hand shape
                    if ((x >= 4 && x <= 11 && y >= 6 && y <= 13) ||  // Palm
                        (x >= 6 && x <= 9 && y >= 2 && y <= 6) ||    // Fingers
                        (x >= 3 && x <= 5 && y >= 8 && y <= 11))     // Thumb
                    {
                        pixels[index] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
    
    /// <summary>
    /// Enhanced CursorManager that creates default cursors if none are assigned
    /// </summary>
    public class CursorManagerWithDefaults : CursorManager
    {
        private new void Awake()
        {
            base.Awake();
            
            // Create default cursors if none are assigned
            if (normalCursor == null)
                normalCursor = DefaultCursorTextures.CreateNormalCursor();
                
            if (crossMoveCursor == null)
                crossMoveCursor = DefaultCursorTextures.CreateCrossMoveCursor();
                
            if (handCursor == null)
                handCursor = DefaultCursorTextures.CreateHandCursor();
        }
    }
}