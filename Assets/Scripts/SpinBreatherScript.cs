using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinBreatherScript : MonoBehaviour
{

    public Vector3 TargetSphereAxis = Vector3.up;

    public Transform sphere;
    public Transform spheregroup;
    public Vector3 bracketAxis = Vector3.up;
    public Vector3 kernelAxis = Vector3.right;
    

    private Transform[] shells;

    //public int nrAxis = 2;
    private GameObject[,,] points;
    private GameObject centerpoint;
    private LineRenderer[,,,,] lines;

    // Start is called before the first frame update
    public GameObject pointPrefab;
    public GameObject spherePrefab;

    private float bigRadius = 8f;
    private float smallRadius = 0.5f;
    private float deltaRadius;
    private float distBetweenPoints = 0.25f;
    private float dalpha = 10f;

    private int nrpointsPerRing;

    private int rings;
    private bool showLines;
    private GameManager _manager;

    private static int BRA = 0;
    private static int KET = 1;
    private static int KERNELX = 2;
    private static int KERNELZ = 3;
    private static string DEFAULT_FORMULA = "<x>";
    private int[] sequence;

    void Update() {

        float dBracketAngle = _manager.speed / 20.0f;
        float bracketAngle = (float)Time.frameCount * dBracketAngle;
        if (bracketAngle > 360) bracketAngle = bracketAngle % 360;

        for (int i = 0; i < rings; i++) {
            Transform shell = shells[i];
            float dr = rings - i-1;
            
            float fraction = dr / (rings-1);
            // fraction is 0 for large i 1 at the beginning
            float oneKernelAngle = fraction * _manager.kernelAngle;
           
           rotateOneShell(shell, bracketAngle, oneKernelAngle);
            compressOneShell(shell, 1);

            if (i == 0) {
                rotateOneShell(sphere, bracketAngle, oneKernelAngle);
                compressOneShell(sphere, 1);
            }
          

           
        }
         
        if (Time.frameCount % 20 == 0) {
            Text txt = GameObject.Find("CurrentAngleLabel").GetComponent<Text>();

            txt.text = "Bracket " + (int)bracketAngle;
        }

        updateLines();

    }
    private void compressOneShell(Transform shell, float scale) {
        shell.localScale = new Vector3(scale, scale, scale);
    }
        private void rotateOneShell(Transform shell, float bracketAngle, float oneKernelAngle) {
        
        Vector3 bracketVector = new Vector3(0, bracketAngle, 0);
        Vector3 minusBracketVector = new Vector3(0, -bracketAngle, 0);
        Vector3 kernelVectorX = new Vector3(oneKernelAngle, 0, 0);
        Vector3 kernelVectorZ = new Vector3(0, 0, oneKernelAngle);

        Quaternion bracketRotation = Quaternion.Euler(bracketVector);
        Quaternion kernelRotationX = Quaternion.Euler(kernelVectorX);
        Quaternion kernelRotationZ = Quaternion.Euler(kernelVectorZ);
        Quaternion minusBracketRotation = Quaternion.Euler(minusBracketVector);

        Quaternion overallRotation = bracketRotation;
        if ((_manager == null || _manager.rotate) && oneKernelAngle != 0) {
            for (int i = 1; i<sequence.Length; i++) {
                int which = sequence[i];
                if (which == BRA) overallRotation = overallRotation * bracketRotation;
                else if (which == KET) overallRotation = overallRotation * minusBracketRotation;
                else if (which == KERNELX) overallRotation = overallRotation * kernelRotationX;
                else if (which == KERNELZ) overallRotation = overallRotation * kernelRotationZ;
            }
           
        }
        else {
            overallRotation = bracketRotation  * minusBracketRotation;
        }
        shell.rotation = overallRotation;
    }

    private void createShells() {
        rings = (int)((bigRadius - smallRadius) / distBetweenPoints);

        shells = new Transform[rings];
        int nr = 0;
        for (float r = smallRadius; r < bigRadius; r += distBetweenPoints) {
           
            GameObject shell = Instantiate(spherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            shell.transform.parent = this.spheregroup.transform;
            //   shell.transform.rotation = transform.rotation;
            shell.transform.localScale = new Vector3(2 * r, 2 * r, 2 * r);
            shells[nr] = shell.transform;
            nr++;
        }

    }
   
    private void Awake() {
        p("Awake");
        //The following line should work if you stick to having one GameManager in the game
        _manager = GameObject.FindObjectOfType<GameManager>();
        float angle = _manager.kernelAngle;
        float speed = _manager.speed;
        int nrAxis = _manager.nrAxis;
        string formula = _manager.formula;
        p("Awake: Formula from game manager is " + formula+", calling parseFormula");
        this.parseFormula(formula);
        InputField fformula = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        fformula.text = formula;
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        aslider.SetValueWithoutNotify(angle);
        Text at = GameObject.Find("AngleLabel").GetComponent<Text>();
        at.text = "Kernel angle " + (int)angle;

        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        sslider.SetValueWithoutNotify(speed);
        at = GameObject.Find("SpeedLabel").GetComponent<Text>();
        at.text = "Speed " + (int)speed;

        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
        xslider.SetValueWithoutNotify(nrAxis);
        at = GameObject.Find("AxisLabel").GetComponent<Text>();
        at.text = "# Axis " + nrAxis;

        p("awake: setting speed to " + speed + " and angle to " + nrAxis + ", axis is " + nrAxis+", formula "+formula);
      
    }
    void Start() {
        p("Start");
        showLines = true;
        deltaRadius = bigRadius - smallRadius;
        rings = (int)(deltaRadius / distBetweenPoints);
        if (sequence == null) parseFormula(DEFAULT_FORMULA);
        createShells();
        createPoints();
        createLines();
      
    }

   
    private bool parseFormula(string formula) {
        int bras = 0;
        int kets = 0;
        p("parseFormula: formula is " + formula);
        if (formula == null || formula.Length < 2) {
            p("Formula " + formula + " is too short, using default");
            return parseFormula(DEFAULT_FORMULA);
        }
        char[] chars = formula.Trim().ToCharArray();
        sequence = new int[chars.Length];
        if (chars[0] !=  '<' || chars[chars.Length-1] !='>') {
            p("Formula " + formula + " must start with bra and end with ket: "+formula);
            return parseFormula(DEFAULT_FORMULA);
        }
        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            int which = -1;
            if (c == '<') {
                which = BRA;
                bras++;
            }
            else if (c == '>') {
                which = KET;
                kets++;
            }
            else if (c == 'x' || c == 'X') {
                which = KERNELX;
            }
            else  {
                which = KERNELZ;
            }
           
            sequence[i] = which;
            p("parseFormula: Setting operation " + i + " to " + which);
        }
        if (bras != kets || bras < 1) {
            p("Unequal number of bras " + bras + " and kets " + kets+": "+formula);
            return parseFormula(DEFAULT_FORMULA);
        }
        p("parseFormula: formula " + formula + " looks ok, seuence is "+sequence.ToString());
        InputField f = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        f.text = formula;
        _manager.formula = formula;
        return bras == kets;
    }
        private void createPoints() {
        p("createPoints");

        nrpointsPerRing = (int)(360.0f / dalpha) + 1;
        points = new GameObject[_manager.nrAxis, rings, nrpointsPerRing];
        Vector3 pointcoord = new Vector3(0, 0, 0);
        centerpoint = Instantiate(pointPrefab, pointcoord, Quaternion.identity);
        centerpoint.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        lines = new LineRenderer[_manager.nrAxis, rings + 1, nrpointsPerRing, rings, nrpointsPerRing];
        for (int ax = 0; ax < _manager.nrAxis; ax++) {
           
            int ring = 0;
            for (float r = smallRadius; r < bigRadius; r += distBetweenPoints) {

                int which = 0;
                for (float alpha = 0; alpha <= 360; alpha += dalpha) {
                    Vector3 vr;
                    Color c = Color.gray;
                    if (ax == 0) {
                       
                        vr = new Vector3(r, 0, 0);
                        pointcoord = Quaternion.Euler(0, alpha, 0) * vr;
                    }
                    else if (ax == 1) {
                      
                        vr = new Vector3(0, r, 0);
                      
                        pointcoord = Quaternion.Euler(alpha, 0, 0) * vr;
                    }
                    else if (ax == 2) {
                        vr = new Vector3(0, r, 0);
                     
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
                        addLine(ax, ring, w, ring + 1, w);
                    }
                    if (ring > 0) {
                        if (w > 0) addLine(ax, ring, w - 1, ring, w);
                        else addLine(ax, ring, nrpointsPerRing - 1, ring, w);
                    }
                    else {
                       
                        addLine(ax, rings, w, ring, w);
                    }
                }
            }
        }
    }
    private GameObject getPoint(int ax, int ring, int which) {
        if (ring < 0 || ring >= rings) return centerpoint;
        return points[ax, ring, which];
    }
    private void addLine(int ax, int r1, int w1, int r2, int w2) {
        Color c = Color.white;
        if (ax == 1) c = Color.yellow;
        else if (ax == 2) c = new Color(1.0f, 0.5f, 0);
        LineRenderer lr = addLine(c, getPoint(ax, r1, w1), getPoint(ax, r2, w2));
        if (lr != null) {
            // remember line and onts
            if (r1 >= 0 && r2 >= 0) lines[ax, r1, w1, r2, w2] = lr;
           
        }
        else p("Could not create line ax " + ax + ", ring " + r1 + " which " + w1);

    }
    private LineRenderer addLine(Color c, GameObject a, GameObject b) {
        if (a == null || b == null) return null;

        LineRenderer lr = addLine(c, a.transform.position, b.transform.position);
        return lr;
    }
    private LineRenderer addLine(Color c, Vector3 a, Vector3 b) {
        LineRenderer lr = new GameObject("Line").AddComponent<LineRenderer>();

        Material m = new Material(Shader.Find("Sprites/Default"));
       
        lr.material = m;

        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetColors(c, c);

       
        lr.SetPosition(0, a); //x,y and z position of the starting point of the line
        lr.SetPosition(1, b);
       
        return lr;
    }

    public void refresh(string whatever) {
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
        InputField f = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        string formula = f.text;

        int nrAxis = (int)xslider.value;
        float angle = aslider.value;
        float speed = sslider.value;
        this.parseFormula(formula);
        _manager.kernelAngle = angle;
        _manager.speed = speed;
        _manager.nrAxis = nrAxis;
        _manager.formula = f.text;
        p("refresh: Angle from slider is " + angle + ", speed is " + speed + ", nrAxis is " + nrAxis+", formula: "+formula);
        restartScene();
        
    }
    private void restartScene() {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }


    void updateLines() {
       
        for (int ax = 0; ax < _manager.nrAxis; ax++) {

            for (int r = 0; r < rings; r++) {
                for (int w = 0; w < nrpointsPerRing; w++) {
                    bool show = (r == 0 && w < 2);
                    if (r + 1 < rings) {
                        LineRenderer lr = lines[ax, r, w, r + 1, w];
                       
                        if (lr != null) {
                            lr.enabled = showLines;
                            lr.SetPosition(0, points[ax, r, w].transform.position);
                            lr.SetPosition(1, points[ax, r + 1, w].transform.position);
                        }
                        
                    }
                    if (r == 0) {
                        LineRenderer lr = lines[ax, rings, w, r, w];
                        if (lr != null) {
                            lr.enabled = showLines;
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
                                lr.enabled = showLines;
                                lr.SetPosition(0, points[ax, r, v].transform.position);
                                lr.SetPosition(1, points[ax, r, w].transform.position);
                            }
                           
                        }
                    }

                }
            }
        }
    }

    private void p(string s) {
        Debug.Log("SphereLab: " + s);
    }
    public void setAngleText() {
        Text at = GameObject.Find("AngleLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
       
        at.text = "Kernel angle " + (int)aslider.value;
    }

    public void setAxisText() {
        Text at = GameObject.Find("AxisLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
       
        at.text = "Axis " + (int)aslider.value;
    }

    public void setSpeedText() {
        Text at = GameObject.Find("SpeedLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
          
        at.text = "Speed " + (int)aslider.value;
    }

    public void toggleVisible(bool v) {
        p("Toggle visible clicked: " + v);
        sphere.GetComponent<Renderer>().enabled = !sphere.GetComponent<Renderer>().enabled;
    

    }
    public void toggleLines(bool v) {
        p("Toggle lines clicked: " + v);
        this.showLines = !showLines;

    }

}
