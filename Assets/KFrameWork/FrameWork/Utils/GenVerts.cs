using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

public class GenVerts : MonoBehaviour
{
    private struct MapMoveableV3
    {
        public Vector3 Pos;
        public bool visiable ;
    }
    
    public Terrain terrian;

    [Range (0, 10)]
    public float MaxHeight;

    [Range (0, 360)]
    public float MaxAngle =45f;

    private bool init = false;

    private MapMoveableV3[,] array;
    // Use this for initialization
    void Start ()
    {
	
        TerrainData data = terrian.terrainData;

        int mapz =(int)(data.size.x / data.heightmapScale.x );
        int mapx=  (int)(data.size.z / data.heightmapScale.z);

        int wid = Math.Min (data.heightmapWidth, mapz);
        int height = Math.Min (data.heightmapHeight, mapx);

        float[,] heightPosArray = data.GetHeights (0, 0, wid, height);

        this.array = new MapMoveableV3[wid ,height];

        for (int i = 0; i < wid; ++i) {
            for (int j = 0; j < height; ++j) {
                this.array [i,j].Pos = new Vector3 (j * data.heightmapScale.x, heightPosArray [i, j] * data.heightmapScale.y, i * data.heightmapScale.z);
            }
        }
        for (int i = 0; i < mapz; ++i) {
            for (int j = 0; j < mapx ; ++j) {

                bool able = checknearst(i,j,1);
                this.array[i,j].visiable =!able;
            }
        }


        this.init = true;
    }

    private float getMapAngle(MapMoveableV3 current,MapMoveableV3 next)
    {
        Vector3 delta =next.Pos - current.Pos;
        float w =Mathf.Sqrt(delta.x * delta.x + delta.z* delta.z);
        float angle = Mathf.Atan2(w,delta.y)*Mathf.Rad2Deg ;

        return angle;
    }

    private bool getmapDirenable(int x,int y,Vector3 dir,int dis =1)
    {
        int wid = this.array.GetLength(0);
        int height = this.array.GetLength(1);
        int nextx =0;
        int nexty =0;
        if(dir == Vector3.left)
        {
            nextx = x -dis;
            nexty = y;
        }
        else if(dir == Vector3.right)
        {
            nextx = x + dis;
            nexty = y;
        }
        else if(dir == Vector3.up)
        {
            nextx = x ;
            nexty = y+dis;
        }
        else if(dir == Vector3.down)
        {
            nextx = x ;
            nexty = y -dis;
        }
        else
        {
            throw new FrameWorkException("dir error");
        }

        if(nextx <  wid && nextx >=0 && nexty < height && nexty >=0)
        {
            float angle = this.getMapAngle(this.array[x,y],this.array[nextx,nexty]);
            return  angle< this.MaxAngle;
        }

        return false;
    }

    private bool checknearst(int i,int j,int count)
    {
        for(int k =0; k < count;++k)
        {
            bool lf_enable = this.getmapDirenable(i,j,Vector3.left,k+1);
            bool rh_enable = this.getmapDirenable(i,j,Vector3.right,k+1);
            bool up_enable = this.getmapDirenable(i,j,Vector3.up,k+1);
            bool down_enable = this.getmapDirenable(i,j,Vector3.down,k+1);
            if(!lf_enable && !rh_enable && !up_enable && !down_enable)
            {
                return false;
            }
        }
        return true;
    }
	
    // Update is called once per frame
    void Update ()
    {

        if (init) {
            //TerrainData data = terrian.terrainData;
//            int mapz =(int)(data.size.x / data.heightmapScale.x );
//            int mapx=  (int)(data.size.z / data.heightmapScale.z);
//

            for (int i = 0; i < array.GetLength(0); ++i) {
                for (int j = 0; j < array.GetLength(1) ; ++j) {
                    var posdata = this.array[i,j];
                    Vector3 pos = this.transform.position+ array[i,j].Pos + new Vector3 (0.5f, 0, 0);
                    if(posdata.visiable)
                    {
                        Debug.DrawLine(this.transform.position+ array[i,j].Pos,pos,Color.green) ;
                    }
                    else
                    {
                        Debug.DrawLine(this.transform.position+ array[i,j].Pos,pos,Color.red) ;
                    }


                }
            }

        }

    }
}
