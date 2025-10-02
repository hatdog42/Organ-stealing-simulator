using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGameTools
{
    public class MazeWallFinder : EditorWindow
    {
        [SerializeField] private SpriteRenderer mazeRenderer;
        //[SerializeField] private GameObject wallTest;
        private Texture2D _mazeTexture;
        
        private Vector2Int _mazeSize;
        private Color _wallColor;
        
        [MenuItem("Window/Tools/Maze Wall Generator")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MazeWallFinder));
        }

        private void OnEnable()
        {
            _wallColor = Color.black;
            _mazeTexture = mazeRenderer.sprite.texture;
        }

        private void OnGUI()
        {
            mazeRenderer = (SpriteRenderer)EditorGUILayout.ObjectField(mazeRenderer, typeof(SpriteRenderer), true);
            //wallTest = (GameObject)EditorGUILayout.ObjectField(wallTest, typeof(GameObject), true);
            
            if (GUILayout.Button("Generate Walls"))
            {
                for (int y = 0; y < _mazeTexture.height; y++)
                {
                    for (int x = 0; x < _mazeTexture.width; x++)
                    {
                        //_mazeSize = new Vector2Int(x, y);
                        Color newColor = _mazeTexture.GetPixel(x, y); //
                        if (_wallColor == newColor)
                        {
                            Debug.Log("Wall found at: " + x + ", " + y);
                            //Instantiate((Object)wallTest, _mazeSize, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
}
