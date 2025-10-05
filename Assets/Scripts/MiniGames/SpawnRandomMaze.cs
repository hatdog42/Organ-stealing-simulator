using UnityEngine;

namespace MiniGames
{
    public class SpawnRandomMaze : MonoBehaviour
    {
        [SerializeField] private Vector3 spawnPosition;
        
        [SerializeField] private GameObject[] maze;

        public void SpawnMaze()
        {
            Instantiate(maze[Random.Range(0, maze.Length)], new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), Quaternion.identity);
        }
    }
}
