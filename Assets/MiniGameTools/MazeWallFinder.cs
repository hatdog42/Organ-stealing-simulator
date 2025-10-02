using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGameTools
{
    public class MazeWallFinder : EditorWindow
    {
        private SpriteRenderer _mazeRenderer;
        private GameObject _wallTest;
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
            _mazeTexture = _mazeRenderer.sprite.texture;
        }

        private void OnGUI()
        {
            _mazeRenderer = (SpriteRenderer)EditorGUILayout.ObjectField(_mazeRenderer, typeof(SpriteRenderer), true);
            _wallTest = (GameObject)EditorGUILayout.ObjectField(_wallTest, typeof(GameObject), true);
            
            if (GUILayout.Button("Generate Walls"))
            {
                for (int y = 0; y < _mazeTexture.height; y++)
                {
                    for (int x = 0; x < _mazeTexture.width; x++)
                    {
                        _mazeSize = new Vector2Int(x, y);
                        Color newColor = _mazeTexture.GetPixel(x, y);
                        if (_wallColor == newColor)
                        {
                            //Debug.Log("Wall found at: " + x + ", " + y);
                            GameObject spawnedObj = Instantiate(_wallTest, _mazeRenderer.gameObject.transform.GetChild(0), false);
                            spawnedObj.transform.position = new Vector3(_mazeSize.x - _mazeTexture.width/2 + 0.5f, _mazeSize.y - _mazeTexture.height/2 + 0.5f, 0);
                        }
                    }
                }
            }

            if (GUILayout.Button("Delete All Walls"))
            {
                for (int i = _mazeRenderer.gameObject.transform.GetChild(0).childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(_mazeRenderer.gameObject.transform.GetChild(0).GetChild(i).gameObject);
                }
            }
        }
    }
}
