using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSeaScript : MonoBehaviour
{


    public int Size = 3;
    private int Width ;
    private int Height ;
    private int Depth ;

    // private Vector3 startpos;

    public GameObject block;

    public float speed=1;
    public float wavesize = 0.2f;
    public float compressionFactor = 0.2f;

    public int mode=1;

    private GameObject[,,] grid;

    float maxdist;
    private Quaternion quatup = Quaternion.Euler(0, 90, 0);
    void Start() {

        Width = Size;
        Height = Size;
        Depth = Size;

        p("Starting, creating grid ");
        grid = new GameObject[Width * 2 + 3, 2 * Height + 3, Depth * 2 + 3];

        createGrid();
    }

    void createGrid() {

        maxdist = Mathf.Sqrt(3*Size * Size);
        //startpos = transform.position;

        for (int x = -Width; x <= Width; ++x) {
            for (int y = -Height; y <= Height; ++y) {
                for (int z = -Depth; z <= Depth; ++z) {

                    Vector3 pos = new Vector3(x, y, z);
                    p("Creating block " + x + "/ " + y + " / " + z);
                    GameObject gblock = Instantiate(block, pos, Quaternion.identity);
                    gblock.name = "block " + x + "/" + y + "/" + z;
                    grid[x + Width, y + Height, z + Depth] = gblock;
                    p("Created block " + gblock.name);

                }
            }
        }
        for (int x = -Width; x <= Width; x++) {
            for (int y = -Height; y <= Height; y++) {
                for (int z = -Depth; z <= Depth; z++) {
                    GameObject cube = grid[x + Width, y + Height, z + Depth];
                   
                }
            }
        }

    }

    private GameObject get(string name) {

        GameObject res = GameObject.Find(name);

        return res;
    }
    private void p(string s) {
        Debug.Log("CubeSea: " + s);
    }
    private Vector3 computePos(float time, Vector3 pos, int x, int y, int z) {
        float dx = 0;
        float dy = Mathf.Sin(time/100)*y/Height;
        float dz = 0;
        Vector3 newpos =  new Vector3(dx+x, dy+y, dz+z);
        return newpos;
    }
    void Update() {

        float t = Time.realtimeSinceStartup;
    //    p("time " + t);
        for (int y = -Height; y <= Height; y++) {
           
            for (int x = -Width; x <= Width; x++) {
                
                for (int z = -Depth; z <= Depth; z++) {
                    GameObject cube = grid[x + Width, y + Height, z + Depth];
                    float r = Mathf.Sqrt(x * x + y * y + z * z);
                    Vector3 newpos = cube.transform.position;
                    Quaternion newrot = cube.transform.rotation;
                    Quaternion rot = cube.transform.rotation;
                    Vector3 origpos = new Vector3(x, y, z);
                    if (mode == 1) {
                        float dr = Mathf.Cos(t * speed) * compressionFactor;
                        float dy = Mathf.Sin(t * speed) * x * z / (float)Height * wavesize;                        
                        newpos = new Vector3(x, dy + y, z);
                        newpos.Scale(new Vector3(1 + dr, 1 + dr, 1 + dr));
                     
                    }
                    else if (mode == 2) {
                        quatup.SetAxisAngle(Vector3.up, Mathf.Sin(t * speed/Mathf.PI));
                        newrot = quatup ;

                   
                    }
                    else if (mode == 3) {
                        maxdist = Mathf.Sqrt(2 * Size * Size);
                        r = Mathf.Sqrt(x * x + z * z);
                        float da = (maxdist - r)/20;
                        if (da < 0) da = 0;
                        quatup.SetAxisAngle(Vector3.up, Mathf.PI * da*Mathf.Sin(t * speed ));
                        // Quaternion.AngleAxis( da * Mathf.Sin(t * speed), Vector3.up, );
                        newpos = quatup * origpos;

                    }
                    cube.transform.position = newpos;
                    cube.transform.rotation = newrot;
                }
            }
        }
    }
}
