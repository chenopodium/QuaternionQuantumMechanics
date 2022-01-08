using OrbitalConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OrbitalConsole.OrbitComputer;

public class CreateOrbital : MonoBehaviour
{

    public GameObject pointPrefab;
    public Material fieldMaterial;

    public int n=1;
    public int l=0;
    public int m=0;
    public float from = 0.4f;
    public float to = 1.0f;
    private float vmax;
    public float RMAX = 3f;
    private Orbital electron;
    private int scale = 50;
    private static OrbitComputer computer = new OrbitComputer();

    // Start is called before the first frame update
    void Start()
    {
        electron = new Orbital(n, l, m);
        computeOrbital();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void computeOrbital() {
        p("Computing electron cloud...");
        int count = 0;
        vmax = 0;
        for (float x = -RMAX; x < RMAX; x+=0.25f) {
            for (float y = -RMAX;y < RMAX; y += 0.25f) {
                for (float z = -RMAX; z < RMAX; z += 0.25f) {
                    bool debug = true;
                    OrbValue res = computer.computValue(x * scale, y * scale, z * scale, false);
                    float av = Mathf.Abs((float)res.fv);
                        if (av > vmax) vmax = av;
                        if (av>from && av< to && count < 50000) {
                            p("Found "+res+", count="+count);
                            createPoint(res, x, y, z);
                            count++;
                        }
                        
                    }
                
            }
        }
        p("Computing electron cloud done. Max v="+vmax);
    }
    private GameObject createPoint(OrbValue res, float x, float y, float z) {
       
        Vector3 v = new Vector3(x, y, z);
        GameObject gpoint = null;
        gpoint = Instantiate(pointPrefab, v, Quaternion.identity);
        float range = vmax - from;
        float scale = ((Mathf.Abs((float)res.fv) - from) / range + 0.1f)*0.1f ;
        if (fieldMaterial != null) gpoint.GetComponent<Renderer>().material = this.fieldMaterial;
        gpoint.GetComponent<Renderer>().material.SetColor("_Color", createColor((float)res.fi, (float)res.fr, (float)res.fv));
        gpoint.transform.localScale = new Vector3(scale, scale, scale);
        // points[i, j, k] = gpoint;
      //  gpoint.transform.parent = this.transform;
        return gpoint;
    }
    private Color createColor(float vr, float vy, float v) {
        float range = vmax - from;
        float red = 0;
        if (vy<0) red = Mathf.Min(1.0f, (-vy-from)/range+0.1f);
        float green = 0;
        if (vy>0) green = Mathf.Min(1.0f, (vy - from) / range + 0.1f);
        float blue = Mathf.Min(1.0f, (vr - from) / range + 0.1f);
        float trans = Mathf.Min(1.0f, (Mathf.Abs(v) - from) / range + 0.1f);

        return new Color(red, green, blue, trans);
    }
    private void p(string s) {
        Debug.Log("CreateOrbital: " + s);
    }
}
