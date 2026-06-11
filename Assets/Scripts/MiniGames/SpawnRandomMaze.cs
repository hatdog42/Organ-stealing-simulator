using UnityEngine;

namespace MiniGames
{
    public class SpawnRandomMaze : MonoBehaviour
    {
        [SerializeField] private Vector3 spawnPosition;

        [SerializeField] private string[] mazeResourcePaths;
        [SerializeField] private GameObject[] maze;

        public void SpawnMaze()
        {
            Vector3 position = new(spawnPosition.x, spawnPosition.y, spawnPosition.z);
            GameObject prefab = PickMazePrefab();

            if (!prefab)
            {
                Debug.LogWarning("No maze prefab was found to spawn.");
                return;
            }

            Instantiate(prefab, position, Quaternion.identity);
        }

        private GameObject PickMazePrefab()
        {
            if (mazeResourcePaths != null && mazeResourcePaths.Length > 0)
            {
                string path = mazeResourcePaths[Random.Range(0, mazeResourcePaths.Length)];
                GameObject resourcePrefab = Resources.Load<GameObject>(path);
                if (resourcePrefab) return resourcePrefab;

                Debug.LogWarning($"Maze resource was not found at Resources/{path}.");
            }

            if (maze == null || maze.Length == 0) return null;

            return maze[Random.Range(0, maze.Length)];
        }
    }
}
