using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class GenVertsOp : MonoBehaviour
{
    private enum MapNodeState : byte
    {
        None = 0,
        Available,
        DownHill,
        Unavailable
    }
    
    private struct MapMoveableV3
    {
        public Vector3 Pos;
        public MapNodeState state;
    }

    private struct PosNode
    {
        public PosNode(int x, int y)
        {
            row = x;
            col = y;
        }
        public int row;
        public int col;
    }
    
    public Terrain terrian;

    [Range (0, 10)]
    public float MaxHeight;

    [Range (0, 360)]
    public float MaxAngle = 45f;

    [Range(1, 5000)]
    public float RegionSize = 100;

    public bool SRAvailable = false;

    private bool init = false;

    private float PlainAngle = 10f;

    private MapMoveableV3[,] array;

    private int arrRow = 0;

    private int arrCol = 0;

    // Use this for initialization
    void Start ()
    {
        this.initMapNdoeData();

        this.init = true;
    }

    /// <summary>
    /// 初始化地图数据，默认边界为可到达
    /// </summary>
    private void initMapNdoeData()
    {
        TerrainData data = terrian.terrainData;

        int mapz = (int)(data.size.x / data.heightmapScale.x);
        int mapx = (int)(data.size.z / data.heightmapScale.z);

        this.arrRow = Math.Min(data.heightmapWidth, mapz);
        this.arrCol = Math.Min(data.heightmapHeight, mapx);

        float[,] heightPosArray = data.GetHeights(0, 0, this.arrRow, this.arrCol);

        this.array = new MapMoveableV3[this.arrRow, this.arrCol];

        for (int i = 0; i < this.arrRow; ++i)
        {
            for (int j = 0; j < this.arrCol; ++j)
            {
                this.array[i, j].Pos = new Vector3(j * data.heightmapScale.x, heightPosArray[i, j] * data.heightmapScale.y, i * data.heightmapScale.z);
                if (this.isMapEdge(i, j))
                {
                    this.array[i, j].state = MapNodeState.Available;
                }
                else
                {
                    this.array[i, j].state = MapNodeState.None;
                }

            }
        }

        this.autoCaculateMapNodeData();
    }

    /// <summary>
    /// 自动计算地图数据
    /// 1.根据PlainAngle数值计算平原角度，平原为可行走区域
    /// 2.根据SRAvailable标识，判断不可到达的平原是否定义为可行走区域
    /// </summary>
    private void autoCaculateMapNodeData()
    {
        for (int i = 0; i < this.arrRow; ++i)
        {
            for (int j = 0; j < this.arrCol; ++j)
            {
                if (this.array[i, j].state == MapNodeState.None)
                {
                    MapNodeState state = checkIsPlain(i, j);
                    this.array[i, j].state = state;
                }
            }
        }

        if (!this.SRAvailable)
        {
            this.surroundedRegionsForbidden();
        }

        this.setDownHillRegion();
    }

    /// <summary>
    /// 设置包围区域是否可行
    /// </summary>
    private void surroundedRegionsForbidden()
    {
        bool[,] flag = new bool[this.arrRow, this.arrCol];

        for (int i = 0; i < this.arrRow; i++)
        {
            for (int j = 0; j < this.arrCol; j++)
            {
                if (!flag[i, j] && this.array[i, j].state == MapNodeState.Available)
                {
                    this.checkRegionSize(i, j, flag);
                }
            }
        }
    }

    /// <summary>
    /// 设置下坡区域
    /// </summary>
    private void setDownHillRegion()
    {
        bool[,] flag = new bool[this.arrRow, this.arrCol];
        for (int i = 1; i < this.arrRow; i++)
        {
            for (int j = 1; j < this.arrCol; j++)
            {
                if (!flag[i, j] && this.array[i, j].state == MapNodeState.Unavailable)
                {
                    this.checkIsDownHillRegion(i, j, flag);
                }
            }
        }
    }

    /// <summary>
    /// 是否是地图边界
    /// </summary>
    private bool isMapEdge(int row, int col)
    {
        if (row == 0 || row == this.arrRow - 1 || col == 0 || col == this.arrCol - 1)
            return true;
        return false;
    }
    
    /// <summary>
    /// 简易判断方法，边界情况会被判定为Unavailable，可优化
    /// </summary>
    private MapNodeState checkIsPlain(int i, int j)
    {
        bool lf_enable = this.getmapDirenable(i, j, Vector3.left, false);
        bool rh_enable = this.getmapDirenable(i, j, Vector3.right, false);
        bool up_enable = this.getmapDirenable(i, j, Vector3.up, false);
        bool down_enable = this.getmapDirenable(i, j, Vector3.down, false);
        if (lf_enable && rh_enable && up_enable && down_enable)
        {
            return MapNodeState.Available;
        }
        return MapNodeState.Unavailable;
    }

    private bool getmapDirenable(int x, int y, Vector3 dir, bool isDownHill, int dis = 1)
    {
        int nextx = 0;
        int nexty = 0;
        if (dir == Vector3.left)
        {
            nextx = x - dis;
            nexty = y;
        }
        else if (dir == Vector3.right)
        {
            nextx = x + dis;
            nexty = y;
        }
        else if (dir == Vector3.up)
        {
            nextx = x;
            nexty = y + dis;
        }
        else if (dir == Vector3.down)
        {
            nextx = x;
            nexty = y - dis;
        }
        else
        {
            throw new FrameWorkException("dir error");
        }

        if (isDownHill)
        {
            if (nextx < this.arrRow && nextx >= 0 && nexty < this.arrCol && nexty >= 0 && this.array[nextx, nexty].state != MapNodeState.Unavailable)
            {
                return this.array[x, y].Pos.y < this.array[nextx, nexty].Pos.y;
            }
        }
        else {
            if (nextx < this.arrRow && nextx >= 0 && nexty < this.arrCol && nexty >= 0)
            {
                float angle = this.getMapAngle(this.array[x, y], this.array[nextx, nexty]);
                return angle < this.PlainAngle;
            }
        }
       

        return false;
    }

    /// <summary>
    /// 计算角度
    /// </summary>
    private float getMapAngle(MapMoveableV3 current, MapMoveableV3 next)
    {
        Vector3 delta = next.Pos - current.Pos;
        float w = Mathf.Sqrt(delta.x * delta.x + delta.z * delta.z);
        float angle = Mathf.Atan2(delta.y, w) * Mathf.Rad2Deg;
        return angle;
    }
    
    /// <summary>
    /// 检测可行区域范围大小是否满足
    /// </summary>
    private bool checkRegionSize(int i, int j, bool[,] flag)
    {
        bool isEdge = this.isMapEdge(i, j);
        Stack<PosNode> stack = new Stack<PosNode>();
        stack.Push(new PosNode(i, j));
        if (isEdge)
        {
            while (stack.Count > 0)
            {
                PosNode node = stack.Pop();
                int row = node.row;
                int col = node.col;
                flag[row, col] = true;
                if (this.checkLocation(row, col + 1, flag))
                {
                    stack.Push(new PosNode(row, col + 1));
                }
                if (this.checkLocation(row, col - 1, flag))
                {
                    stack.Push(new PosNode(row, col - 1));
                }
                if (this.checkLocation(row + 1, col, flag))
                {
                    stack.Push(new PosNode(row + 1, col));
                }
                if (this.checkLocation(row - 1, col, flag))
                {
                    stack.Push(new PosNode(row - 1, col));
                }
            }
        }
        else
        {
            int rSize = 0;
            List<PosNode> list = new List<PosNode>();
            while (stack.Count > 0)
            {
                PosNode node = stack.Pop();
                int row = node.row;
                int col = node.col;
                flag[row, col] = true;
                list.Add(node);
                if (this.checkLocation(row, col + 1, flag))
                {
                    stack.Push(new PosNode(row, col + 1));
                    ++rSize;
                }
                if (this.checkLocation(row, col - 1, flag))
                {
                    stack.Push(new PosNode(row, col - 1));
                    ++rSize;
                }
                if (this.checkLocation(row + 1, col, flag))
                {
                    stack.Push(new PosNode(row + 1, col));
                    ++rSize;
                }
                if (this.checkLocation(row - 1, col, flag))
                {
                    stack.Push(new PosNode(row - 1, col));
                    ++rSize;
                }
            }
            if (rSize < this.RegionSize)
            {
                for (int t = 0; t < list.Count; ++t)
                {
                    this.array[list[t].row, list[t].col].state = MapNodeState.Unavailable;
                }
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检测是否是下坡区域
    /// </summary>
    private bool checkIsDownHillRegion(int i, int j, bool[,] flag)
    {
        if (this.checkDownHillConditions(i, j))
        {
            Stack<PosNode> stack = new Stack<PosNode>();
            stack.Push(new PosNode(i, j));
            while (stack.Count > 0)
            {
                PosNode node = stack.Pop();
                int row = node.row;
                int col = node.col;
                flag[row, col] = true;
                if (this.checkDownHillConditions(row, col + 1))
                {
                    stack.Push(new PosNode(row, col + 1));
                }
                if (this.checkDownHillConditions(row, col - 1))
                {
                    stack.Push(new PosNode(row, col - 1));
                }
                if (this.checkDownHillConditions(row + 1, col))
                {
                    stack.Push(new PosNode(row + 1, col));
                }
                if (this.checkDownHillConditions(row - 1, col))
                {
                    stack.Push(new PosNode(row - 1, col));
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 是否已经访问
    /// </summary>
    private bool checkLocation(int row, int col, bool[,] flag)
    {
        if (row >= 0 && row < this.arrRow && col >= 0 && col < this.arrCol)
        {
            if (this.array[row, col].state == MapNodeState.Available && !flag[row, col])
                return true;
        }
        return false;
    }

    /// <summary>
    /// 检测是否满足下坡条件
    /// </summary>
    private bool checkDownHillConditions(int i, int j)
    {
        if (i >= 0 && i < this.arrRow && j >= 0 && j < this.arrCol && this.array[i, j].state == MapNodeState.Unavailable)
        {
            bool lf_enable = this.getmapDirenable(i, j, Vector3.left, true);
            bool rh_enable = this.getmapDirenable(i, j, Vector3.right, true);
            bool up_enable = this.getmapDirenable(i, j, Vector3.up, true);
            bool down_enable = this.getmapDirenable(i, j, Vector3.down, true);
            if (lf_enable || rh_enable || up_enable || down_enable)
            {
                if (this.array[i, j].state == MapNodeState.Unavailable)
                {
                    this.array[i, j].state = MapNodeState.DownHill;
                }
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update ()
    {

        if (init) {

            for (int i = 0; i < this.array.GetLength(0); ++i) {
                for (int j = 0; j < this.array.GetLength(1) ; ++j) {
                    var posdata = this.array[i,j];
                    Vector3 pos = this.transform.position+ this.array[i,j].Pos + new Vector3 (0.5f, 0, 0);
                    if (posdata.state == MapNodeState.Available)
                    {
                        Debug.DrawLine(this.transform.position + this.array[i, j].Pos, pos, Color.green);
                    }
                    else if (posdata.state == MapNodeState.DownHill)
                    {
                        Debug.DrawLine(this.transform.position + this.array[i, j].Pos, pos, Color.yellow);
                    }
                    else if (posdata.state == MapNodeState.Unavailable)
                    {
                        Debug.DrawLine(this.transform.position + this.array[i, j].Pos, pos, Color.red);
                    }
                    else
                    {
                        Debug.DrawLine(this.transform.position + this.array[i , j].Pos, pos, Color.grey);
                    }

                }
            }

        }

    }
}
