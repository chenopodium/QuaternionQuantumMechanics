using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerPath : MonoBehaviour
{

    public bool visible;
    public GameObject marker;
    private int count;
    public int markerCount=200;
    private GameObject[] markers;
    private LineRenderer[] lines;
    public int frameDelta=10;

    public GameObject arrowPrefab;
    private GameObject arrow;

    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        visible = true;
        if (frameDelta < 3) frameDelta = 3;

        Quaternion arrowRotation = Quaternion.LookRotation(Vector3.up);
        arrow = Instantiate(arrowPrefab, transform.position, arrowRotation);
        GameObject onemarker = Instantiate(marker, transform.position, Quaternion.identity);
        mat = onemarker.GetComponent<Renderer>().material;
        mat.SetColor("_Color", Color.red);
        // p("arrow material: " + arrow.GetComponent<Renderer>().material);
        arrow.transform.localScale *= 0.2f;
      
        Material arrowMat = arrow.GetComponent<MeshRenderer>().material;
        arrowMat.SetColor("_Color", Color.red);
        //  p("arrow color. " + arrow.GetComponent<Renderer>().material.color);
        create(); 

    }

    public void destroy() {
        if (markers != null) {
            for (int i = 0; i < markerCount; i++) {
                if (markers[i] != null) Destroy(markers[i]);
                if (lines[i] != null) Destroy(lines[i]);
            }
        }
       // visible = false;
    }
    private void create() {
        destroy();


        if (!visible) {
            p("Not visible, not drawing marker");
            return;
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
    public void setActive(bool b) {
        this.visible = b;
    }
    public void restart() {
        restart(markerCount);
    }
    public void restart(int count) { 
        markerCount = count;
        create();
    }
    private LineRenderer addLine(Color c, Vector3 a, Vector3 b) {
        if (!visible) return null;
        LineRenderer lr = new GameObject("Line").AddComponent<LineRenderer>();

        lr.material = mat;

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
        Debug.Log("------------- MarkerPath: " + s);
    }
    private int next() {
        count++;
        if (count >= markerCount) count = 0;
        return count;
    }
    // Update is called once per frame
    void Update()
    {
        if (!visible) return;
        if (Time.frameCount>0 && Time.frameCount % frameDelta == 0) {
           
          //  p("Updating marker pos " + count);
            GameObject m = markers[count];
            m.transform.position = transform.position;
            if (count > 0) {
                LineRenderer lr = lines[count];
                Vector3 before = markers[count - 1].transform.position;
                Vector3 after = m.transform.position;
                lr.SetPosition(0,before );
                lr.SetPosition(1,after);
                Quaternion targetRotation = Quaternion.LookRotation(after - before);

                arrow.transform.rotation = targetRotation;
              //   Quaternion.Lerp(arrow.transform.rotation, targetRotation, 10f * Time.deltaTime);
                arrow.transform.position = before;

               // arrow.transform.position = Vector3.Lerp(m.transform.position, after, Time.deltaTime * 1.0f);

            }

            next();
        }
    }
}
