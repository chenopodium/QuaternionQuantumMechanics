using UnityEngine;
using System.Collections.Generic;
using System;
using OrbitalConsole;
using static OrbitalConsole.OrbitComputer;
using System.Diagnostics;

public class OrbitalMeshBuilder : MarchingCubesMeshBuilder
{

    public int n = 1;
    public int l = 0;
    public int m = 0;
    
   // private Orbital electron;
    private float maxvalue = 0;
     
    private OrbitComputer computer = new OrbitComputer();

    private int scale = 1000;

    private int count = 0;
    override protected List<Vector3> CalculateVertices() {

        p("Starting calculateVertices");
        computer.Set(n, l, m);
        maxvalue = 0;
        
         Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        count = 0;
        base.CalculateVertices();

        p("n=" + n + ", l=" + l + ", m=" + m + ", max value is " + maxvalue);
        if (this.isolevel > maxvalue) {
            isolevel = 0.9f * maxvalue;
            p("Recomputing using isolevel " + isolevel);
            count = 0;
            base.CalculateVertices();
           // isolevel = 0.5f * isolevel;
          //  p("Adding using half isolevel " + isolevel);

         //   base.CalculateVertices();
        }
    //    stopwatch.Stop();
     //   TimeSpan ts = stopwatch.Elapsed;

     //   Console.WriteLine("Elapsed Time is {0:00}h:{1:00}min:{2:00}s.{3}ms for " + vertices.Count + " points",
     //                   ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        return vertices;
    }

    

    protected void p(string s) {
        //    if (!printDebugInfo) return;
        UnityEngine.Debug.Log("OrbitalMeshBuilder: " + s);
    }
    public override float getValue(float x, float y, float z) {

      //float value =  base.getValue(x, y, z);
         
       OrbValue res = computer.computValue(x*scale, y * scale, z * scale, false);
      // if (vertices.Count % 100==0)  p(computer.ToString()+":"  + res);
       float value = (float)res.fv;
        maxvalue = (float)Math.Max(maxvalue, value);

        if (count % 5000 ==0) {
         //   p(computer.ToString() + ":" + res.toString()+", max so far "+maxvalue);
        }
        count++;
        return value;
    }
    virtual protected Color getColor(Vector3 v) {
        OrbValue res = computer.computValue(v.x * scale, v.y * scale, v.z * scale, false);
       
        return getColor((float)v.x, (float)v.y, (float)res.fv);

    }
    protected Color createColor(float vr, float vy, float v) {
        float from = 0;

        return Color.red;
        /*
        float red = 0;
        if (vy < 0) red = Mathf.Min(1.0f, (-vy - from) / maxgrid + 0.1f);
        float green = 0;
        if (vy > 0) green = Mathf.Min(1.0f, (vy - from) / maxgrid + 0.1f);
        float blue = Mathf.Min(1.0f, (vr - from) / maxgrid + 0.1f);
        float trans = Mathf.Min(1.0f, (Mathf.Abs(v) - from) / maxvalue + 0.1f);

        return new Color(red, green, blue, trans);
        */
    }

}