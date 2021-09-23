using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PosDir
{
    public Vector2Int pos, dir;

    public PosDir(Vector2Int pos)
    {
        this.pos = pos;
        this.dir = Vector2Int.zero;
    }

    public PosDir(Vector2Int pos, Vector2Int dir)
    {
        this.pos = pos;
        this.dir = dir;
    }

    public Vector2Int NextPos()
    {
        return pos + dir;
    }

    public PosDir Advance()
    {
        pos += dir;
        return this;
    }

    public Vector2Int Rotate90()
    {
        return new Vector2Int(dir.y, dir.x);
    }
}

public class MazeGen : MonoBehaviour
{
    public float CorridorMin, CorridorMax, CorridorMean, CorridorStddev, BranchMean, BranchStddev;
    public int generationTries;
    public GameObject Tile;
    private int width, height;
    private Transform[,] layout;
    private PosDir entrance, exit;
    public int[,] tiles;
    public enum GenerationType {Custom, Prim};
    public GenerationType generationType;

    // Start is called before the first frame update
    void Start()
    {
        GetBounds();
        GetEntrance();
        GetExit();
        width++;
        layout = new Transform[width+1, height+1];
        AddToGrid(entrance.pos);
        AddToGrid(exit.pos);
        
        int tries = generationTries;

        switch(generationType)
        {
            case GenerationType.Custom:

                while (tries > 0)
                {
                    ResetMaze();
                    GenerateMaze(entrance);
                    //StartCoroutine(GenerateMazeDelay(entrance));
                    tries--;

                    if (layout[exit.NextPos().x, exit.NextPos().y] != null)
                    {
                        Debug.Log(String.Format("Maze generated in {0} tries", generationTries - tries));
                        break;
                    }
                }

                if (tries == 0)
                {
                    throw new Exception(String.Format("Maze could not be generated in {0} tries", generationTries));
                }
                else
                {
                    MazeSO maze = ScriptableObject.CreateInstance<MazeSO>();
                    maze.Init(width, height, layout);

        #if UNITY_EDITOR
                    AssetDatabase.CreateAsset(maze, "Assets/Maze.asset");
        #endif
                }
                break;

            case GenerationType.Prim:

                /*List<Vector2Int> frontier = new List<Vector2Int>();
                AddFrontier(entrance.pos + Vector2Int.down, frontier);
                AddFrontier(entrance.pos + Vector2Int.up, frontier);
                AddFrontier(entrance.pos + Vector2Int.left, frontier);
                AddFrontier(entrance.pos + Vector2Int.right, frontier);

                while (frontier.Count > 0)
                {
                    Mark(frontier[UnityEngine.Random.Range(0, frontier.Count)], frontier);
                }*/

                StartCoroutine(PrimDelay());

                break;
        }
    }

    IEnumerator PrimDelay()
    {
        List<Vector2Int> frontier = new List<Vector2Int>();
        AddFrontier(entrance.pos + Vector2Int.down, frontier);
        AddFrontier(entrance.pos + Vector2Int.up, frontier);
        AddFrontier(entrance.pos + Vector2Int.left, frontier);
        AddFrontier(entrance.pos + Vector2Int.right, frontier);

        while (frontier.Count > 0)
        {
            int pos = UnityEngine.Random.Range(0, frontier.Count);
            if (AdjacentCount(frontier[pos]) <= 1)
            {
                Mark(frontier[pos], frontier);
            } 
            else
            {
                frontier.Remove(frontier[pos]);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    void AddFrontier(Vector2Int where, List<Vector2Int> frontier)
    {
        if (!OutOfBounds(where) && layout[where.x, where.y] == null && AdjacentCount(where) <= 1)
        {
            frontier.Add(where);
        }
    }

    void Mark(Vector2Int where, List<Vector2Int> frontier)
    {
        frontier.Remove(where);
        AddToGrid(where);
        AddFrontier(where + Vector2Int.down, frontier);
        AddFrontier(where + Vector2Int.up, frontier);
        AddFrontier(where + Vector2Int.left, frontier);
        AddFrontier(where + Vector2Int.right, frontier);
    }

    int AdjacentCount(Vector2Int where)
    {
        int total = 0;

        total += (!OutOfBounds(where + Vector2Int.left) && layout[where.x - 1, where.y] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.right) && layout[where.x + 1, where.y] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.down) && layout[where.x, where.y - 1] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.up) && layout[where.x, where.y + 1] != null) ? 1 : 0;

        /*
        total += (!OutOfBounds(where + Vector2Int.left + Vector2Int.up) && layout[where.x - 1, where.y + 1] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.left + Vector2Int.down) && layout[where.x - 1, where.y - 1] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.right + Vector2Int.up) && layout[where.x + 1, where.y + 1] != null) ? 1 : 0;
        total += (!OutOfBounds(where + Vector2Int.right + Vector2Int.down) && layout[where.x + 1, where.y - 1] != null) ? 1 : 0;
        */

        return total;
    }

    void GenerateMaze(PosDir origin)
    {
        List<PosDir> branches = SpawnCorridor(origin.Advance());

        if (branches != null)
        {
            foreach (PosDir branch in branches)
            {
                GenerateMaze(branch);
            }
        }
    }

    IEnumerator GenerateMazeDelay(PosDir origin)
    {
        List<PosDir> branches = SpawnCorridor(origin.Advance());
        yield return new WaitForSeconds(0.5f);

        if (branches != null)
        {
            foreach (PosDir branch in branches)
            {
                StartCoroutine(GenerateMazeDelay(branch));
            }
        }
    }

    void ResetMaze()
    {
        for (int x = 0; x < layout.GetLength(0); x++)
        {
            for (int y = 0; y < layout.GetLength(1); y++)
            {
                if (layout[x, y] != null)
                {
                    Destroy(layout[x, y].gameObject);
                    layout[x, y] = null;
                }
            }
        }
    }

    IEnumerator DelayedGenerateMaze(PosDir origin)
    {
        yield return new WaitForSeconds(1f);
        List<PosDir> branches = SpawnCorridor(origin.Advance());
        Debug.Log("Spawned corridor");

        if (branches != null)
        {
            foreach (PosDir branch in branches)
            {
                StartCoroutine(DelayedGenerateMaze(branch));
            }
        }
    }

    List<PosDir> SpawnCorridor(PosDir origin)
    {
        if (!ValidMove(origin))
        {
            return null;
        }

        int length = Mathf.FloorToInt(DistributionSampling.NextGaussian(CorridorMean, CorridorStddev, CorridorMin, CorridorMax));
        List<PosDir> branches = new List<PosDir>(); // list of all corridor's branches

        for (int count = 1; ValidMove(origin) && count < length; count++)
        {
            AddToGrid(origin.pos);
            SpawnBranches(origin, branches);
            origin.Advance();
        }

        return branches;
    }

    void SpawnBranches(PosDir origin, List<PosDir> branches)
    {
        int direction = UnityEngine.Random.Range(-1f, 1f) < 0 ? 1 : -1; // randomize first selected side
        if (ValidMove(new PosDir(origin.pos, direction * origin.Rotate90()))
            && DistributionSampling.NextGaussian(BranchMean, BranchStddev) > 1)
        {
            branches.Add(new PosDir(origin.pos, direction * origin.Rotate90()));
        }
        if (ValidMove(new PosDir(origin.pos, direction * origin.Rotate90()))
            && DistributionSampling.NextGaussian(BranchMean, BranchStddev) > 1)
        {
            branches.Add(new PosDir(origin.pos, (-direction) * origin.Rotate90()));
        }
    }

    bool ValidMove(PosDir origin)
    {
        if (OutOfBounds(origin.pos))
        {
            return false;
        }

        Vector2Int[] ahead = {
            origin.NextPos(),                       // Front
            origin.NextPos() + origin.Rotate90(),   // Left front
            origin.NextPos() - origin.Rotate90()    // Right front
        };

        foreach (Vector2Int tile in ahead)
        {
            if (OutOfBounds(tile) || layout[tile.x, tile.y] != null)
            {
                return false;
            }
        }

        return true;
    }

    void GetBounds()
    {
        Vector2Int topRightCorner = Vector2Int.FloorToInt(transform.Find("TopRightCorner").transform.localPosition);
        width = topRightCorner.x;
        height = topRightCorner.y;
    }

    void GetEntrance()
    {
        entrance = new PosDir(Vector2Int.FloorToInt(transform.Find("Entrance").transform.localPosition));

        bool onTopX = entrance.pos.x == 0 || entrance.pos.x == width && entrance.pos.y > 0 && entrance.pos.y < height;
        bool onTopY = entrance.pos.y == 0 || entrance.pos.y == height && entrance.pos.x > 0 && entrance.pos.x < width;
        if (!onTopX ^ onTopY) // XOR to chech if inside corner
        {
            throw new Exception("Entrance is not positioned correctly");
        }

        entrance.dir =
            entrance.pos.x == 0 ? Vector2Int.right :
            entrance.pos.x == width ? Vector2Int.left :
            entrance.pos.y == 0 ? Vector2Int.up :
            Vector2Int.down;
    }

    void GetExit()
    {
        exit = new PosDir(Vector2Int.FloorToInt(transform.Find("Exit").transform.localPosition));

        bool onTopX = exit.pos.x == 0 || exit.pos.x == width && exit.pos.y > 0 && exit.pos.y < height;
        bool onTopY = exit.pos.y == 0 || exit.pos.y == width && exit.pos.x > 0 && exit.pos.x < width;
        if (!onTopX && !onTopY)
        {
            throw new Exception("Exit is not positioned correctly");
        }

        exit.dir =
            exit.pos.x == 0 ? Vector2Int.right :
            exit.pos.x == width ? Vector2Int.left :
            exit.pos.y == 0 ? Vector2Int.up :
            Vector2Int.down;
    }

    void AddToGrid(Vector2Int pos)
    {
        //Debug.Log(String.Format("Pos = ({0},{1}) Bounds = ({2},{3})", pos.x, pos.y, height, width));
        layout[pos.x, pos.y] = Instantiate(Tile, GlobalPos(pos), Quaternion.identity, transform).transform;
    }

    bool OutOfBounds(Vector2Int pos)
    {
        return pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height;
    }

    Vector3 GlobalPos(Vector2Int pos)
    {
        return new Vector3(pos.x + transform.position.x, pos.y + transform.position.y, 0);
    }

    void DrawSquare(Vector2[] corners, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[0], corners[2]);
        Gizmos.DrawLine(corners[1], corners[3]);
        Gizmos.DrawLine(corners[2], corners[3]);
    }

    void OnDrawGizmos()
    {
        GetBounds();
        Vector2[] corners = new Vector2[4];
        corners[0] = transform.position;                                                        // bottom left
        corners[1] = new Vector2(transform.position.x, transform.position.y + height);          // bottom right
        corners[2] = new Vector2(transform.position.x + width, transform.position.y);           // top left
        corners[3] = new Vector2(transform.position.x + width, transform.position.y + height);  // top right
        DrawSquare(corners, Color.blue);

        GetEntrance();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(GlobalPos(entrance.pos), GlobalPos(entrance.pos + entrance.dir * 3));

        GetExit();
        Gizmos.color = Color.red;
        Gizmos.DrawLine(GlobalPos(exit.pos), GlobalPos(exit.pos + exit.dir * 3));
    }
}
