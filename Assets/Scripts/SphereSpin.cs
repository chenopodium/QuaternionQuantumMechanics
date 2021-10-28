using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SphereSpin : MonoBehaviour
{
    
    public Vector3 TargetSphereAxis = Vector3.up;
   
    
    public float wind = 0.1f;

    public Transform sphere;
    public Transform spheregroup;
    public Transform SphereAxisObject;

    private Transform[] shells;
    
   
    private GameObject[,,] points;
    private GameObject centerpoint;
    private LineRenderer[,,,,] lines;

    
    public GameObject pointPrefab;
    public GameObject spherePrefab;

    private float bigRadius =6.5f;
    private float smallRadius = 0.5f;
    private float distBetweenPoints = 0.5f;
    private float dalpha =45.0f;
    
    private int nrpointsPerRing;
    
    private int rings;
    private float absAngle;

    private GameManager _manager;

    void Update() {

        float angle = _manager.speed / 50.0f;

        if (_manager == null || _manager.rotate) {           
            sphere.Rotate(Vector3.up, angle);
            spheregroup.Rotate(Vector3.up, -angle, Space.World);

            // rotate also shells!
            for (int i = 0; i < rings; i++) {
          
                shells[i].Rotate(Vector3.up, angle, Space.Self);
            }
            absAngle = absAngle + angle;
            if (absAngle > 360) absAngle = absAngle - 360;
           
            if (Time.frameCount % 20 == 0) {
                Text txt = GameObject.Find("CurrentAngleLabel").GetComponent<Text>();
                
                txt.text = "Current " + (int)absAngle;
            }

        }
        //  Debug.Break();
   //     updatePoints();
        updateLines();

        //  Time.deltaTime = 0f;

    }

    private void createShells() {
        rings = (int)((bigRadius - smallRadius) / distBetweenPoints);

        shells = new Transform[rings];
        int nr = 0;
        for (float r = smallRadius; r < bigRadius; r += distBetweenPoints) {
            p("Creating shell with radius " + r);
     
            GameObject shell= Instantiate(spherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            shell.transform.parent = this.spheregroup.transform;
         //   shell.transform.rotation = transform.rotation;
            shell.transform.localScale = new Vector3(2*r,2* r,2* r);
            shells[nr] = shell.transform;              
            nr++;
        }
       
    }
    public void ChangeAngleOfRotationAxis(float angle) {
      
        _manager.kernelAngle = angle;
        ChangeAngleOfRotationAxis(angle, TargetSphereAxis);
        changeAngleOfShellsAndRotateThem(angle);



    }
    private void changeAngleOfShellsAndRotateThem(float angle) {
        if (rings > 0) {
            p("Changing angle of shellsf or " + rings + " shells");
            float delta = (1 / (float)(rings));
           
            for (int i = 0; i < rings; i++) {
                float ratio = (float)(rings - i - 1)*delta;
                float dangle = (float)(ratio) * angle;
                Vector3 target = Vector3.up;
                target = ChangeAngleOfRotationAxis(dangle, target);
                // now look thre
                rotateShell(i, shells[i].transform, target);
            }
        }
        else p("Could not change angle of shells");
    }
    private void rotateShell(int nr, Transform shelltransform, Vector3 targetangle) {

        Quaternion OriginalShellAxis = shelltransform.rotation;
        Quaternion rotleft = Quaternion.LookRotation(new Vector3(-1, 0, 0));
        Quaternion rotation = Quaternion.LookRotation(targetangle);
        rotation *= rotleft;       
        rotation *= OriginalShellAxis;
        shelltransform.rotation = rotation;
        p("Rotated shell "+nr+" to axis " + targetangle);
    }
    public Vector3 ChangeAngleOfRotationAxis(float angle, Vector3 target) {
         float s = Mathf.Sin(Mathf.Deg2Rad * angle);
        float c = Mathf.Cos(Mathf.Deg2Rad * angle);
        target.x = c;
        target.y = s;
        p("Setting angle " + angle + ", target rotation axis is now "+target);
        return target;

    }

    private void rotateSphereAndGroup() {

        Quaternion OriginalSphereAxis = transform.rotation;
        Quaternion rotleft = Quaternion.LookRotation(new Vector3(-1, 0, 0));
        Quaternion rotation = Quaternion.LookRotation(TargetSphereAxis);
        rotation *= rotleft;
        
        rotation *= OriginalSphereAxis;
        transform.rotation = rotation;

    }

    private void Awake() {
        p("Awake");
        //The following line should work if you stick to having one GameManager in the game
        _manager = GameObject.FindObjectOfType<GameManager>();
        float angle = _manager.kernelAngle;
        float speed = _manager.speed;
        int nrAxis = _manager.nrAxis;
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        aslider.SetValueWithoutNotify(angle);
        Text at = GameObject.Find("AngleLabel").GetComponent<Text>();
        at.text = "Angle " + (int)angle;

        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        sslider.SetValueWithoutNotify(speed);
        at = GameObject.Find("SpeedLabel").GetComponent<Text>();
        at.text = "Speed " + (int)speed;

        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
        xslider.SetValueWithoutNotify(nrAxis);
        at = GameObject.Find("AxisLabel").GetComponent<Text>();
        at.text = "# Axis " + nrAxis;

        p("awake: setting speed to " + speed + " and angle to " + nrAxis + ", axis is "+nrAxis);
        ChangeAngleOfRotationAxis(angle);
        
    }
    void Start() {
        p("Start");
        rings = (int)((bigRadius - smallRadius) / distBetweenPoints);

        createShells();
        createPoints();
        createLines();
        changeAngleOfShellsAndRotateThem(_manager.kernelAngle);
        rotateSphereAndGroup();
       // toggleVisible(false);
    }
   

    private void createPoints() {
        p("createPoints");
       
        nrpointsPerRing = (int)(360.0f / dalpha)+1;
        points = new GameObject[ _manager.nrAxis, rings, nrpointsPerRing];
        Vector3 pointcoord = new Vector3(0, 0, 0);
        centerpoint = Instantiate(pointPrefab, pointcoord, Quaternion.identity);
        centerpoint.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        lines = new LineRenderer[_manager.nrAxis, rings +1, nrpointsPerRing, rings, nrpointsPerRing];
        for (int ax = 0; ax < _manager.nrAxis; ax++) {
          //  p("Creating points for axis " + ax);
            int ring = 0;
            for (float r = smallRadius; r < bigRadius; r += distBetweenPoints) {
          
         //       p("creating ax "+ax+" ring "+ring+", radius " +r);
                int which = 0;
                for (float alpha = 0; alpha <= 360; alpha += dalpha) {
                    Vector3 vr;
                    Color c = Color.gray;
                    if (ax == 0) {
                      //  p("Changing alpha around y, vr in x direction");
                      //  c = Color.gray;
                        vr = new Vector3(r, 0, 0);
                        pointcoord = Quaternion.Euler(0, alpha, 0) * vr;
                    }
                    else if (ax == 1) {
                     //   p("Changing alpha around x, vr in z direction");
                        vr = new Vector3(0,r, 0);
                     //   c = Color.red;
                        pointcoord = Quaternion.Euler(alpha, 0, 0) * vr;
                    }
                    else if (ax == 2) {
                        vr = new Vector3(0, r, 0);
                       // c = Color.blue;
                     //   p("Changing alpha around z, vr in z direction");
                        pointcoord = Quaternion.Euler(0, 0, alpha) * vr;
                    }
                    GameObject point = Instantiate(pointPrefab, pointcoord, Quaternion.identity);
                    point.GetComponent<Renderer>().material.SetColor("_Color", c);
                    points[ax, ring, which] = point;               
                    point.transform.parent = shells[ring];
                    which++;
                }
                ring++;
               
            }
            
        }
    }
    private void createLines() {
        p("createLines");
        for (int ax = 0; ax < _manager.nrAxis; ax++) {
           
            for (int ring = 0; ring < rings; ring++) {
            //    p("Creating lines for axis " + ax+", ring "+ring);
                for (int w = 0; w < nrpointsPerRing; w++) {
                    if (ring + 1 < rings) {
                        addLine(ax,ring, w, ring + 1, w);
                    }
                    if (ring > 0) {
                        if (w > 0) addLine(ax, ring, w - 1, ring, w);
                        else addLine(ax, ring, nrpointsPerRing - 1, ring, w);
                    }
                    else {
                        // lines to center
                        //   p("Adding line to center "+centerpoint);
                        addLine(ax, rings, w, ring, w);
                    }
                }
            }
        }
    }
    private GameObject getPoint(int ax, int ring, int which) {
        if (ring < 0 || ring >=rings) return centerpoint;
        return points[ax, ring, which];
    }
    private void addLine(int ax, int r1, int w1, int r2, int w2) {
        Color c = Color.white;
        if (ax == 1) c = Color.yellow;
        else if (ax == 2) c = new Color(1.0f, 0.5f, 0);
        LineRenderer lr = addLine(c, getPoint(ax,r1, w1), getPoint(ax,r2, w2));
        if (lr != null) {
            // remember line and onts
            if (r1 >= 0 && r2 >= 0) lines[ax, r1, w1, r2, w2] = lr;
            /*
                        if (r1==0) lr.startColor = Color.yellow;
                        if (r2==0) lr.endColor = Color.yellow;

                        if (r1+1 ==rings) lr.startColor = Color.red;
                        if (r2+1 == rings) lr.endColor = Color.red;
            */
        }
        else p("Could not create line ax " + ax + ", ring " + r1 + " which " + w1);
        
    }
    private LineRenderer addLine(Color c, GameObject a, GameObject b) {
        if (a == null || b == null) return null;
        
        LineRenderer lr = addLine(c,a.transform.position, b.transform.position);
        return lr;
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
        lr.SetColors(c,c);
       
        //For drawing line in the world space, provide the x,y,z values
        lr.SetPosition(0, a); //x,y and z position of the starting point of the line
        lr.SetPosition(1, b);
        //lr.transform.parent = sphere.transform;
        return lr;
    }
   
    public void refresh(string whatever) {
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
        int nrAxis =(int) xslider.value;
        float angle =aslider.value;
        float speed = sslider.value;
        _manager.kernelAngle = angle;
        _manager.speed = speed;
        _manager.nrAxis = nrAxis;
        p("refresh: Angle from slider is " + angle+", speed is "+speed+", nrAxis is "+nrAxis);
        restartScene();
        rotateSphereAndGroup();
    }
    private void restartScene() {
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }
   

    void updateLines() {
        //     p("Updating lines for " + rings + "  rings and "+ nrpointsPerRing+"  points per ring");
        for (int ax = 0; ax < _manager.nrAxis; ax++) {

            for (int r = 0; r < rings; r++) {
                for (int w = 0; w < nrpointsPerRing; w++) {
                    bool show = (r == 0 && w < 2);
                    if (r + 1 < rings) {
                        LineRenderer lr = lines[ax, r, w, r + 1, w];
                        if (lr != null) {
                            //       if (show) p("Updating  ring line for " + r + "/" + w + ": " + points[r, w].transform.position);
                            lr.SetPosition(0, points[ax, r, w].transform.position);
                            lr.SetPosition(1, points[ax, r + 1, w].transform.position);
                        }
                        //  else if (show) p("No LR to next ring for " + r + "/" + w);
                    }
                    if (r == 0) {
                        LineRenderer lr = lines[ax, rings, w, r, w];
                        if (lr != null) {
                            //       if (show) p("Updating  ring line for " + r + "/" + w + ": " + points[r, w].transform.position);
                            lr.SetPosition(0, centerpoint.transform.position);
                            lr.SetPosition(1, points[ax, r, w].transform.position);
                        }
                    }
                    if (r < rings) {
                        int v = w - 1;
                        if (v < 0) v = nrpointsPerRing - 1;
                        if (w > 0) {
                            LineRenderer lr = lines[ax, r, v, r, w];
                            if (lr != null) {
                                //      if (show) p("Updating segment line for " + r + "/" + w + ": " + points[r, w].transform.position);

                                lr.SetPosition(0, points[ax, r, v].transform.position);
                                lr.SetPosition(1, points[ax, r, w].transform.position);
                            }
                            //   else if (show)  p("No LR for prev segment  " + r + "/" + w);
                        }
                    }

                }
            }
        }
    }
   
    private void p(string s) {
        Debug.Log("SphereSpin: " + s);
    }
    public void toggleVisible(bool v) {
        p("Toggle visible clicked: " + v);
        sphere.GetComponent<Renderer>().enabled = !sphere.GetComponent<Renderer>().enabled;
        SphereAxisObject.GetComponent<Renderer>().enabled = !SphereAxisObject.GetComponent<Renderer>().enabled;
       
    }
   
}
