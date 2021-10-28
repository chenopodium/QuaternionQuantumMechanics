using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerPath : MonoBehaviour
{

    public GameObject marker;
    private int count;
    public int markerCount=200;
    private GameObject[] markers;
    private LineRenderer[] lines;
    public int frameDelta=50;
    // Start is called before the first frame update
    void Start()
    {
        if (frameDelta < 5) frameDelta = 50;

        create();

    }
    private void create() {

        if (markers != null) {
            for (int i = 0; i < markerCount; i++) {
                if (markers[i] != null) Destroy(markers[i]);
                if (lines[i] != null) Destroy(lines[i]);
            }
        }
        // p("About to create markers");
        markers = new GameObject[markerCount];
        lines = new LineRenderer[markerCount];

        for (int i = 0; i < markerCount; i++) {
            //     p("Creating markar " + i);
            GameObject m = Instantiate(marker, transform.position, Quaternion.identity);
            markers[i] = m;
            m.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            if (i > 0) {
                LineRenderer lr = addLine(Color.red, markers[i - 1].transform.position, m.transform.position);
                lines[i] = lr;
            }
        }
    }
    public void restart() {
        restart(markerCount);
    }
    public void restart(int count) { 
        markerCount = count;
        create();
    }
    private LineRenderer addLine(Color c, Vector3 a, Vector3 b) {
        LineRenderer lr = new GameObject("Line").AddComponent<LineRenderer>();

        //  Debug.DrawLine(a,b, Color.yellow,20f);
        Material m = new Material(Shader.Find("Sprites/Default"));
        // Material m = new Material(Shader.Find("Unlit/Texture"));
        lr.material = m;

        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetColors(c, c);

        //For drawing line in the world space, provide the x,y,z values
        lr.SetPosition(0, a); //x,y and z position of the starting point of the line
        lr.SetPosition(1, b);
        //lr.transform.parent = sphere.transform;
        return lr;
    }

    private void p(string s) {
        Debug.Log("MarkerPath: " + s);
    }
    private int next() {
        count++;
        if (count >= markerCount) count = 0;
        return count;
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % frameDelta == 0) {
            
            next();
          //  p("Updating marker pos " + count);
            GameObject m = markers[count];
            m.transform.position = transform.position;
            if (count > 0) {
                LineRenderer lr = lines[count];
                lr.SetPosition(0, markers[count - 1].transform.position);
                lr.SetPosition(1,m.transform.position);
            }

        }
    }
}