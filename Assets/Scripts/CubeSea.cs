using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSea : MonoBehaviour
{
    
   
    public int Width = 3;
    public int Height = 3;
    public int Depth = 3;

   // private Vector3 startpos;

    public GameObject block;


    private GameObject[,,] grid;
    void Start() {

        p("Starting, creating grid ");
        grid = new GameObject[Width * 2 + 3, 2 * Height + 3, Depth * 2 + 3];

       createGrid();
    }

    void createGrid() {

       
        //startpos = transform.position;
        
        for (int x = -Width; x <= Width; ++x) {
            for (int y = -Height; y <= Height; ++y) {
                for (int z = -Depth; z <= Depth; ++z) {
                    
                    Vector3 pos =  new Vector3(x, y , z);
                    p("Creating block " + x +"/ "+y+" / "+z);
                    GameObject gblock = Instantiate(block, pos, Quaternion.identity);
                    gblock.name = "block " + x + "/" + y + "/" + z;
                    grid[x + Width, y+Height, z + Depth] = gblock;
                    p("Created block " + gblock.name);

                }
            }
        }
        for (int x = -Width; x <= Width; x++) {
            for (int y = -Height; y <= Height; y++) {
                for (int z = -Depth; z <= Depth; z++) {
                    GameObject cube  = grid[x + Width , y + Height, z + Depth ];
                    
                    if (cube == null) continue;
                    if (x == Width || y == Height || z == Depth 
                        || x<=-Width || y <= -Height || z<=-Depth) {
                        p("Fixing border " + cube);
                        cube.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                        RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    } 
                       

                }
            }
        }
       
    }
   
    private GameObject get(string name) {
       
        GameObject res= GameObject.Find(name);
    
        return res;
    }
    private void p(string s) {
        Debug.Log("CubeSea: "+s);
    }
    void Update() {
        
    }
}
