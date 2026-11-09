using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 5;
    [SerializeField, Range(0f, 1f)] private float obstacleProbability = 0.2f;

    [SerializeField] private Vector2Int start = new Vector2Int(0, 1);
    [SerializeField] private Vector2Int goal = new Vector2Int(4, 4);

    private int[,] grid;
    private List<Vector2Int> path = new List<Vector2Int>();

    // The four directions BFS can move.
    private Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private void Start()
    {
        // Generate the grid and calculate the first path.
        GenerateRandomGrid(gridWidth, gridHeight, obstacleProbability);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // R: Generate a new random grid.
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            GenerateRandomGrid(gridWidth, gridHeight, obstacleProbability);
        }

        // T: Change the start and goal positions.
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            SetStartAndGoal(new Vector2Int(2, 2), new Vector2Int(4, 0));
        }

        // O: Add an obstacle during gameplay.
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            AddObstacle(new Vector2Int(2, 3));
        }
    }

    // Checks whether a position is inside the grid.
    private bool IsInBounds(Vector2Int point)
    {
        return grid != null &&
               point.x >= 0 && point.x < grid.GetLength(1) &&
               point.y >= 0 && point.y < grid.GetLength(0);
    }

    // Generates a new grid with random obstacles.
    public void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("Grid dimensions must be positive.");
            return;
        }

        // Create a new grid with the requested dimensions.
        grid = new int[height, width];

        // Keep the stored settings synchronized.
        gridWidth = width;
        gridHeight = height;
        this.obstacleProbability = Mathf.Clamp01(obstacleProbability);

        // Populate each cell randomly.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = Random.value < this.obstacleProbability ? 1 : 0;
            }
        }

        // Ensure start and goal are inside the new grid.
        start = new Vector2Int(
            Mathf.Clamp(start.x, 0, width - 1),
            Mathf.Clamp(start.y, 0, height - 1)
        );

        goal = new Vector2Int(
            Mathf.Clamp(goal.x, 0, width - 1),
            Mathf.Clamp(goal.y, 0, height - 1)
        );

        // Start and goal must always be walkable.
        grid[start.y, start.x] = 0;
        grid[goal.y, goal.x] = 0;

        FindPath(start, goal);
    }

    // Changes the start and goal at runtime.
    public void SetStartAndGoal(Vector2Int newStart, Vector2Int newGoal)
    {
        if (!IsInBounds(newStart) || !IsInBounds(newGoal))
        {
            Debug.LogWarning("Start or goal is outside the grid.");
            return;
        }

        if (grid[newStart.y, newStart.x] == 1 ||
            grid[newGoal.y, newGoal.x] == 1)
        {
            Debug.LogWarning("Start or goal is blocked.");
            return;
        }

        start = newStart;
        goal = newGoal;

        FindPath(start, goal);
    }

    // Adds an obstacle to a cell during gameplay.
    public void AddObstacle(Vector2Int position)
    {
        if (!IsInBounds(position))
        {
            Debug.LogWarning("Obstacle position is outside the grid.");
            return;
        }

        // Do not block the start or goal.
        if (position == start || position == goal)
        {
            Debug.LogWarning("Cannot place an obstacle on start or goal.");
            return;
        }

        // Mark the cell as blocked.
        grid[position.y, position.x] = 1;

        // Recalculate the path after the obstacle changes.
        FindPath(start, goal);
    }

    // Breadth-First Search pathfinding.
    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        // Clear the previous path before searching again.
        path.Clear();

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom =
            new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            // Stop searching when the goal is reached.
            if (current == goal)
                break;

            // Check the four neighboring cells.
            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                // Only visit walkable, unvisited cells.
                if (IsInBounds(next) &&
                    grid[next.y, next.x] == 0 &&
                    !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log("Path not found.");
            return;
        }

        // Reconstruct the path from goal to start.
        Vector2Int step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Add(start);
        path.Reverse();

        Debug.Log("Path found! Steps: " + (path.Count - 1));
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
            return;

        float cellSize = 1f;

        // Draw the grid cells.
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition =
                    new Vector3(x * cellSize, 0, y * cellSize);

                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;

                Gizmos.DrawCube(
                    cellPosition,
                    new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f)
                );
            }
        }

        // Draw the calculated path in blue.
        foreach (Vector2Int step in path)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawCube(
                new Vector3(step.x * cellSize, 0.05f, step.y * cellSize),
                new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f)
            );
        }

        // Draw the starting position in green.
        Gizmos.color = Color.green;
        Gizmos.DrawCube(
            new Vector3(start.x * cellSize, 0.1f, start.y * cellSize),
            new Vector3(cellSize * 0.7f, 0.1f, cellSize * 0.7f)
        );

        // Draw the goal position in red.
        Gizmos.color = Color.red;
        Gizmos.DrawCube(
            new Vector3(goal.x * cellSize, 0.1f, goal.y * cellSize),
            new Vector3(cellSize * 0.7f, 0.1f, cellSize * 0.7f)
        );
    }
}

