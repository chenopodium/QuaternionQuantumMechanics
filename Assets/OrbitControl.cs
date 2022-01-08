using OrbitalConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static OrbitalConsole.OrbitComputer;

public class OrbitControl : MonoBehaviour
{
  
    OrbitalMeshBuilder mesh;
    float maxvalue;
    // Start is called before the first frame update
    void Start()
    {
        p("Getting mesh");
         mesh = this.gameObject.GetComponent<OrbitalMeshBuilder>();
        p("Got mesh " + mesh+", using n, l and m "+mesh.n+", "+mesh.l+", "+mesh.m);
      
       // test();
    }
    public void setN(bool val) {
        Slider s = GameObject.Find("NSlider").GetComponent<Slider>();
        p("Got N " + s.value);

        mesh.n = (int)s.value;
        setText("TextN", "N "+ mesh.n);
        mesh.UpdateMesh();

    }
    private void setText(String name,String t) {
        p("Setting text " + name + " to " + t);
        Text at = GameObject.Find(name).GetComponent<Text>();
        if (at != null) at.text = t;
        else p("Text" + name + " not found");
    }
    public void setL(bool val) {
        Slider s = GameObject.Find("LSlider").GetComponent<Slider>();
        p("Got L " + s.value);
        mesh.l = (int)s.value;
        setText("TextL", "L " + mesh.l);
        mesh.UpdateMesh();

    }
    public void setM(bool val) {
        Slider s = GameObject.Find("MSlider").GetComponent<Slider>();
        p("Got M " + s.value);
        mesh.m = (int)s.value;
        setText("TextM", "M " + mesh.m);
        mesh.UpdateMesh();

    }
    public void setIso(bool val) {
        Slider s = GameObject.Find("IsoSlider").GetComponent<Slider>();
        p("Got Iso " + s.value);
        mesh.isolevel = (float)s.value;
        setText("TextIso", "Iso " + mesh.isolevel);
        mesh.UpdateMesh();

    }
    private void test() {
        float maxgrid = mesh.maxgrid;
        float maxvalue=0;
        float gridSize = mesh.gridSize;
        int count = 0;
        p("Computing a few values for maxgrid "+maxgrid+", grid size "+gridSize);
        for (float x = -maxgrid; x <= maxgrid; x += gridSize) {
            for (float y = -maxgrid; y <= maxgrid; y += gridSize) {
                for (float z = -maxgrid; z <= maxgrid; z += gridSize) {
                    float v = getValue(x, y, z);
                    if(count % 100 ==0)p("Got value " + x + "/" + y + "/" + z + "=" + v);
                    maxvalue = Math.Max(maxvalue, Math.Abs(v));
                    count++;
                }
            }
        }
        p("Max value is " + maxvalue);
    }

    // OrbitalMeshBuilder
    // Update is called once per frame
    void Update()
    {
        
    }
    protected  float getValue(float x, float y, float z) {

        //   float value =  base.getValue(x, y, z);

        float value = mesh.getValue(x, y, z);
        // if (vertices.Count % 100==0)  p(computer.ToString()+":"  + res);
       
        maxvalue = (float)Math.Max(maxvalue, value);

        return value;
    }

    protected void p(string s) {
        //    if (!printDebugInfo) return;
        UnityEngine.Debug.Log("OrbitControl: " + s);
    }
}
