using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinLabScript : MonoBehaviour
{

    // ======================== EXPOSED ==========================

    public Vector3 TargetSphereAxis = Vector3.up;

    public Transform sphere;
    public Transform spheregroup;
    public MarkerPath markerScript;
    public GameObject arrowPrefab; 
   
    public Texture m_MainTexture, m_Normal, m_Metal;
    // ======================== PRIVATE ==========================

    private bool useGridLines = true;
   
    private GameObject cam;
    private int[] nrGridPointsPerAxis = new int[3];
    // ======================== SHELLS ==========================
    private Dictionary<float, Shell> shellMap = new Dictionary<float, Shell>();

    private Vector3 sphereScale;

    //public int nrAxis = 2;
    private GameObject[,,] points;
    private Vector3[,,] origpoints;

    private GameObject[,,] otherpoints;
    private GameObject centerpoint;

    // ======================== LINES ==========================
    private LineRenderer[,,,,] lines;

    // Start is called before the first frame update
    public GameObject pointPrefab;
    public GameObject spherePrefab;
    public GameObject otherSphere;
    public Material fieldMaterial;

    private bool started;


    private float bigRadius = 5f;
    private float smallRadius = 0.5f;
    private float deltaRadius;
    private float distBetweenPoint = 0.25f;
    private float dalpha = 15f;
    private int gridxleft;
    private int gridxright;
    private int nrpointsPerRing;
    
    private int rings;
    private int spinMode;
    private GameManager _manager;
    private bool modeBracket;

    
    private static int BRAY = 0;
    private static int KETY = 1;
    private static int KERNELX = 2;
    private static int KERNELZ = 3;
    private static int KERNELY = 4;
    private static int BRAZ = 5;
    private static int KETZ = 6;
    private static int BRAX =7;
    private static int KETX = 8;
    private static string DEFAULT_FORMULA = "<x>";
    private int[] sequence;

    private Vector3 spherePosition;
    private Vector3 otherSpherePosition;
    private float direction;
    private float flip;
    private bool leftSide;
    private GameObject canvas;

    private int colorMode = 0;
    private static int colorByAxis = 0;
    private static int colorByStrain = 1;
    private static int colorByTwist = 2;
    private static int colorByCompression = 3;
    private static int colorByY = 4;

  //  private float userPhase = 0;
    private int pointColorMode = 0;

    private int pointColorByLocation = 0;
    private int pointColorByDistance = 1;
    private Gradient gradient;
    // ===================== particles
    private Particles particles;
    private Particle curparticle ;

    // =================== SHELL CACHE ===================
    private Dictionary<(float, float, int), Quaternion> shellCache = new Dictionary<(float, float, int), Quaternion>();
    private Dictionary<(Vector3, Vector3, int), Vector3> midCache = new Dictionary<(Vector3, Vector3, int), Vector3>();
    private int formulaKey;

    // ================== STATS ================
    private float divergence;
    private float curl;
    private float strain;
    private Text txtDiv;
    private Text txtCurl;
    private Text txtStrain;


    private struct shellKey
    {
        float a;
        float b;
        float f;
        
    }
    private void updateStats() {
        return;
        if (txtDiv== null) {
            txtDiv = GameObject.Find("TextDivergence").GetComponent<Text>();
            txtCurl = GameObject.Find("TextCurl").GetComponent<Text>();
            txtStrain = GameObject.Find("TextStrain").GetComponent<Text>();
        }
        txtDiv.text = "" + divergence;
        txtCurl.text = "" + curl;
        txtStrain.text = "" + strain;
        strain = 0;
        divergence = 0;
        curl = 0;

    }

    void Update() {

        float dBracketAngle = _manager.speed / 5.0f;
        float bracketAngle = (float)Time.frameCount * dBracketAngle;
        float phaseCounter = (float)Time.frameCount * dBracketAngle / 180.0f * Mathf.PI;
        if (_manager.userPhase) {
            if (_manager.curUserPhase < 0) _manager.curUserPhase = 0;
            bracketAngle = _manager.curUserPhase;
            phaseCounter = _manager.curUserPhase / 180.0f * Mathf.PI;
        }
       
       
        float compressionPhase = Mathf.Sin(phaseCounter);

        float angleToCompRatio = (float)spinMode;
        if (angleToCompRatio == 0) angleToCompRatio = 1f;
        if (angleToCompRatio < 0) angleToCompRatio = 1.0f / angleToCompRatio;
        float anglePhase = Mathf.Sin(angleToCompRatio * phaseCounter);

        if (bracketAngle > 360) bracketAngle = bracketAngle % 360;

       // bracketAngle= bracketAngle * direction
        if (bracketAngle < 5) updateStats();
        if (modeBracket == false) {
            // use sine function instead, throuh phase
             bracketAngle = 45;// bracketAngle * anglePhase;

        }
        bool debug = false;// Time.frameCount % 50 == 0;
       
        if (debug) {
            //p("angleToCompRatio=" + angleToCompRatio + ", _manager.kernelAngle = " + _manager.kernelAngle + ", anglePhase = " +
            //   anglePhase + ", direction=" + direction + ", oneKernelAngle * anglePhase=" + (_manager.kernelAngle * anglePhase) +
            //  ", modeBracket=" + modeBracket);
        }
        
        foreach (KeyValuePair<float, Shell> entry in shellMap) {
            Shell shell = entry.Value;
            float r = entry.Key;
            float fraction = shell.fraction;
            //if (debug) p("Fraction of r=" + shell.radius + " is " + fraction);

            float oneKernelAngle = fraction * _manager.kernelAngle;
            if (modeBracket == false) {
                //if (debug) p("modeBradket is false");
                oneKernelAngle = oneKernelAngle * anglePhase;
            }

           
            Transform shelltrans = shell.shell.transform;
            rotateOneShell(shelltrans, bracketAngle, oneKernelAngle, debug);
            debug = false;
            if (_manager.useCompression) {
                
                float comp = _manager.kernelAngle / 90.0f;
                //comp = Math.Max(0, comp);
                //comp = Math.Min(1.0f, comp);
                comp = 0.3f;
                float compressionChange = compressionPhase*comp ;
                if (this.curparticle.AntiMatter) compressionChange *= -1;
                float oneCompressionChange = compressionChange * fraction;

                if (Time.frameCount % 100 == 0) {
                //    p("compressionChange=" + compressionChange + ",  _manager.kernelAngle / 90.0=" + comp);
                    //          fraction + ", oneCompressionChange=" + oneCompressionChange + ", direction=" + direction + ", spinMode=" + spinMode+", fraction is "+fraction);
                }
                compressOneShell(shelltrans, shell.scale, 1f + oneCompressionChange, debug);
                if (shell.isSmallest()) compressOneShell(sphere, sphereScale, 1f + oneCompressionChange, debug);
            }
            if (shell.isSmallest()) {
                rotateOneShell(sphere, bracketAngle, oneKernelAngle, debug);
                
            }
            debug = false;
        }

        if (useGridLines) {
            if (leftSide)this.updateGridLines();
        }
        else updateSphericalLines();
        //  Time.timeScale = 0;

    }

    private void compressOneShell(Transform shell, Vector3 origScale, float scale, bool debug) {

        shell.localScale = origScale * scale;
        if (debug) p("compressOneShell: orig " + origScale + ",  scale is " + scale + " -> " + shell.localScale);
    }
    private void rotateOneShell(Transform shell, float bracketAngle, float oneKernelAngle, bool debug) {
       
        if (this.shellCache.ContainsKey((bracketAngle, oneKernelAngle, formulaKey))) {
            // found cached rotation
            shell.rotation = shellCache[(bracketAngle, oneKernelAngle, formulaKey)];
            if (debug) p("Using cache " + bracketAngle+"/" + oneKernelAngle + "/" + formulaKey);
            return;
        }
        else if (debug) p("NOT Using cache " + bracketAngle + "/" + oneKernelAngle + "/" + formulaKey);
        // first is always a time based rotation around the y axis

        Vector3 bracketYVector = new Vector3(0, bracketAngle * direction * flip, 0);
        Vector3 bracketZVector = new Vector3(0, 0, bracketAngle * direction * flip);
        Vector3 bracketXVector = new Vector3(bracketAngle , 0, 0 );

        // this is always a time based LAST rotation around the y axis in exactly the opposite direction
        Vector3 minusBracketYVector = new Vector3(0, -bracketAngle * direction * flip, 0);
        Vector3 minusBracketZVector = new Vector3(0, 0, -bracketAngle * direction * flip);
        Vector3 minusBracketXVector = new Vector3( -bracketAngle , 0,0);

        // in between we can have *any* combination of rotations around any axis
        // the angle here is distance based
        // these vectors define the rotation axis
        Vector3 kernelVectorX = new Vector3(oneKernelAngle , 0, 0);
        Vector3 kernelVectorY = new Vector3(0, oneKernelAngle * direction * flip, 0);
        Vector3 kernelVectorZ = new Vector3(0, 0, oneKernelAngle * direction * flip);

        if(this.leftSide == false && Time.frameCount% 100 == 0) {
            p("Right side, kernelVectorY: " + kernelVectorY);
        }

        // just creating quaternions out of the rotation axis and angles, that's all 
        Quaternion bracketYRotation = Quaternion.Euler(bracketYVector);
        Quaternion bracketZRotation = Quaternion.Euler(bracketZVector);
        Quaternion bracketXRotation = Quaternion.Euler(bracketXVector);
        Quaternion kernelRotationX = Quaternion.Euler(kernelVectorX);
        Quaternion kernelRotationZ = Quaternion.Euler(kernelVectorZ);
        Quaternion kernelRotationY = Quaternion.Euler(kernelVectorY);
        Quaternion minusBracketYRotation = Quaternion.Euler(minusBracketYVector);
        Quaternion minusBracketZRotation = Quaternion.Euler(minusBracketZVector); 
        Quaternion minusBracketXRotation = Quaternion.Euler(minusBracketXVector);

        Quaternion overallRotation =  Quaternion.identity; ;

        int start = 0;
        if (this.modeBracket == false) {
            overallRotation = Quaternion.identity;
          
        }
        if ((_manager == null || _manager.rotate) && oneKernelAngle != 0) {
            for (int i = start; i < sequence.Length; i++) {
                int which = sequence[i];

                if (which == BRAY) overallRotation *=  bracketYRotation;
                else if (which == KETY) overallRotation *= minusBracketYRotation;
                else if (which == KERNELX) overallRotation *=  kernelRotationX;
                else if (which == KERNELY) overallRotation *=  kernelRotationY;
                else if (which == KERNELZ) overallRotation *=  kernelRotationZ;
                else if (which == BRAZ) overallRotation *=  bracketZRotation;
                else if (which == KETZ) overallRotation *=  minusBracketZRotation;
                else if (which == BRAX) overallRotation *=  bracketXRotation;
                else if (which == KETX) overallRotation *=  minusBracketXRotation;
            }

        }
        else {
            overallRotation = bracketXRotation * minusBracketYRotation;
        }
        shell.rotation = overallRotation;
        shellCache[(bracketAngle, oneKernelAngle, formulaKey)] = overallRotation;
    }

    private void createShells() {
        rings = (int)((bigRadius - smallRadius) / distBetweenPoint);

        shellMap = new Dictionary<float, Shell>();

        // add spikes
        
        for (float r = smallRadius; r <= bigRadius; r += distBetweenPoint) {
            GameObject s = Instantiate(spherePrefab, spherePosition, Quaternion.identity);
            Shell shell = new Shell(r, smallRadius, bigRadius, s, _manager.particleInfluence,_manager.dropoff);
            shellMap.Add(r, shell);
        }
        sphereScale = sphere.localScale;


    }

    private void documentation(Transform shell) {

        // this angle changes over TIME (from to 2PI)
        float bracketAngle =0;

        // this angle changes over DISTANCE (from maximum (user defined) 
        //in the center to 0 at infinity or some distance away)
        // example: from 180 degrees, decrease with 1/r^2
        float oneKernelAngle =0; 
        // the angles are computed.... not shown here..

        // first is always a time based rotation around the y axis
        Vector3 bracketVector = new Vector3(0, bracketAngle , 0);
        Vector3 bracketWVector = new Vector3(0, 0, bracketAngle);

        // this is always a time based LAST rotation around the y axis in exactly the opposite direction
        Vector3 minusBracketVector = new Vector3(0, -bracketAngle, 0);
        Vector3 minusBracketWVector = new Vector3(0,  0, -bracketAngle);

        // in between we can have *any* combination of rotations around any axis
        // the angle here is distance based
        // these vectors define the rotation axis
        Vector3 kernelVectorX = new Vector3(oneKernelAngle, 0, 0);
        Vector3 kernelVectorY = new Vector3(0, oneKernelAngle, 0);
        Vector3 kernelVectorZ = new Vector3(0, 0, oneKernelAngle);

        // just creating quaternions out of the rotation axis and angles, that's all 
        Quaternion bracketRotation = Quaternion.Euler(bracketVector);
        Quaternion bracketWRotation = Quaternion.Euler(bracketWVector);
        Quaternion kernelRotationX = Quaternion.Euler(kernelVectorX);
        Quaternion kernelRotationZ = Quaternion.Euler(kernelVectorZ);
        Quaternion kernelRotationY = Quaternion.Euler(kernelVectorY);
        Quaternion minusBracketRotation = Quaternion.Euler(minusBracketVector); 
        Quaternion minusBracketWRotation = Quaternion.Euler(minusBracketWVector);

        // we always start the "opening" bracket rotation arond the y axis (could be any axis!)
        Quaternion overallRotation = bracketRotation;
       
        // sequence is a list of characters that define the spinor
        // < means open bracket
        // x y z mean a kernel rotation for that axis
        // > means closing bracket
        // Example of valid formula:
        // <x> simplest
        // <xxz> a bit more complex
        // <xx><yzzyx>  this is also ok! As long as  we start and end with < and > it is correct
        for (int i = 1; i < sequence.Length; i++) {
            int which = sequence[i];
            if (which == BRAY) overallRotation *= bracketRotation;
            else if (which == KETY) overallRotation  *= minusBracketRotation;
            else if (which == KERNELX) overallRotation *= kernelRotationX;
            else if (which == KERNELY) overallRotation *= kernelRotationY;
            else if (which == KERNELZ) overallRotation *= kernelRotationZ;
        }
        //this rotates the shell - and all points in space attached to it
        shell.rotation = overallRotation; 
        
    }
    public void checkVisibility() {

        bool vis = isVisible() && _manager.showSphere;

        for (int i = 0; i < this.spheregroup.transform.childCount; i++) {
            spheregroup.transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = vis;

        }

        sphere.GetComponent<Renderer>().enabled = vis;
        for (int i = 0; i < this.sphere.transform.childCount; i++) {
            sphere.transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = vis;

        }
    }

    public  GameObject Find( GameObject parent, string name) {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs) {
            if (t.name == name) {
                return t.gameObject;
            }
        }
        return null;
    }
    private bool isVisible() {
        return (direction > 0 || _manager.showSecondGroup);
    }

    private void getVarsFromManager() {

    }
    private void Awake() {
        p("***** Awake direction " + direction + "  ***** frame " + Time.frameCount);

        StartCoroutine(ReallyAwake());
    }
    private void resetCam() {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        float x = 0;
        if (_manager.showSecondGroup == false) x = this.spherePosition.x;
        cam.transform.position = new Vector3(x, 3, -16);
        cam.transform.rotation = Quaternion.Euler(8, 0, 0);
        p("using default cam pos. cam is " + cam.transform.position);

    }
    private System.Collections.IEnumerator ReallyAwake() {
        p("***** ReallyAwake direction " + direction + "  ***** frame " + Time.frameCount + ", manager is " + GameObject.FindObjectOfType<GameManager>());
        if (GameObject.FindObjectOfType<GameManager>() == null) {
          
            yield return null;
        }
        particles = new Particles();
        curparticle = particles.AllParticles[0];
        try {
            Dropdown d = GameObject.Find("ParticleDropdown").GetComponent<Dropdown>();
            List<String> options = new List<String>();
            foreach (Particle p in particles.AllParticles) {
                options.Add(p.Name);
            }
            d.AddOptions(options);            
            d.value = 0;
        }
        catch (Exception e) {
            p("Could not get ParticleDropdown: " + e);
        }
        markerScript.pointColor = new Color(1.0f, 0, 0, 1.0f);
        //   yield return new WaitUntil(() => GameObject.FindObjectOfType<GameManager>()!=null);
        _manager = GameObject.FindObjectOfType<GameManager>();
        //p("GameManager is now " + _manager);
        bigRadius = _manager.gridSize;
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        GameObject panel = Find(canvas, "HelpGroup");

        cam = GameObject.FindGameObjectWithTag("MainCamera");
        if (panel != null) {
            panel.SetActive(false);
        }
        else p("Found no HelpGroup");

        //The following line should work if you stick to having one GameManager in the game

        _manager.addListener((a) => {
            if (a.Equals("togglegroup")) {
                refresh("bla");
            }
        }
        );

        modeBracket = true;
        spherePosition = sphere.transform.position;
        otherSpherePosition = otherSphere.transform.position;
        this.direction = 1;
        this.flip = 1;
        leftSide = true;
        if (spherePosition.x > 0) {
            direction = -1;
            leftSide = false;
            if (_manager.flipSecond) {
                flip = -1;
                p("Second side is flipped");
            }
            else p("Second side is NOT flipped");
        }


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.red;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        updateUiFromManagerValues();
        checkVisibility();


        shellCache.Clear();
        //p("awake: setting speed to " + speed + " and angle to " + nrAxis + ", axis is " + nrAxis+", formula "+formula+ "< _manager.particleInfluence="+ _manager.particleInfluence);


    }
    private void updateUiFromManagerValues() {
        float angle = _manager.kernelAngle;
        int nrAxis = Mathf.Max(1, _manager.nrAxis);
        string formula = _manager.formula;
        float speed = _manager.speed;
        spinMode = System.Math.Max(1, _manager.spinMode);

        //p("Awake: Formula from game manager is " + formula + ", calling parseFormula, visible is " + isVisible());
        cam = GameObject.FindGameObjectWithTag("MainCamera");

        if (Time.frameCount>2000 && _manager.camPosition != null && cam != null &&
             (_manager.camPosition.x+ _manager.camPosition.y + _manager.camPosition.z) != 0) {
            cam.transform.position = _manager.camPosition;
            cam.transform.rotation = _manager.camRotation;
            p("setting camera transform: " + cam.transform);
        }
        else {
            resetCam();

        }
        if (spherePosition.x > 0) {
            direction = -1;
            leftSide = false;
            if (_manager.flipSecond) {
                flip = -1;
                p("Second side is flipped");
            }
            else p("Second side is NOT flipped");
        }
        this.parseFormula(formula);
        InputField fformula = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        fformula.text = formula;
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        int numberOfSteps = (int)aslider.maxValue / 5;
        float range = (angle / aslider.maxValue) * numberOfSteps;
        int ceil = Mathf.CeilToInt(range);
        aslider.value = ceil * 5;

        aslider.SetValueWithoutNotify(angle);
        Text at = GameObject.Find("AngleLabel").GetComponent<Text>();
        at.text = "Kernel angle " + (int)angle;

        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        sslider.SetValueWithoutNotify(speed);
        at = GameObject.Find("SpeedLabel").GetComponent<Text>();
        at.text = "Speed " + (int)speed;


        Slider mslider = GameObject.FindGameObjectWithTag("SpinSlider").GetComponent<Slider>();
        mslider.SetValueWithoutNotify(speed);
        at = GameObject.Find("SpinLabel").GetComponent<Text>();
        at.text = "Spin Mode " + (int)spinMode;

        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();
        xslider.SetValueWithoutNotify(nrAxis);
        at = GameObject.Find("AxisLabel").GetComponent<Text>();
        at.text = "# Axis " + nrAxis;

        Slider pslider = GameObject.FindGameObjectWithTag("ParticleInfluence").GetComponent<Slider>();
        pslider.value = _manager.particleInfluence;

        Slider gslider = GameObject.Find("GridSize").GetComponent<Slider>();
        at = GameObject.Find("GridSizeLabel").GetComponent<Text>();
        gslider.value = _manager.gridSize;
        at.text = "Grid size " + _manager.gridSize;

        try {
            Dropdown preselect = GameObject.Find("CustomDropdown").GetComponent<Dropdown>();
            preselect.value = (_manager.preselect);
        }
        catch (Exception e) {
            p("Could not get dropwidn: " + e);
        }

        try {
            Dropdown drop = GameObject.Find("ColorDrop").GetComponent<Dropdown>();
            drop.value = (_manager.colorBy);
        }
        catch (Exception e) {
            p("Could not get dropwidn: " + e);
        }
        Toggle tgroup = GameObject.FindGameObjectWithTag("ToggleGroup").GetComponent<Toggle>();
        tgroup.isOn = (_manager.showSecondGroup);

        Toggle tcolor = GameObject.Find("ColorLines").GetComponent<Toggle>();
        tcolor.isOn = (_manager.colorLines);

        Toggle tcomp = GameObject.Find("ToggleCompress").GetComponent<Toggle>();
        tcomp.isOn = (_manager.useCompression);


        Toggle tsphere = GameObject.Find("ShowSphere").GetComponent<Toggle>();
        tsphere.isOn = (_manager.showSphere);

        Toggle tpoints = GameObject.Find("ShowPoints").GetComponent<Toggle>();
        tpoints.isOn = (_manager.showPoints);


        Toggle tmag = GameObject.Find("ShowMagnetic").GetComponent<Toggle>();
        tmag.isOn = (_manager.magnet);

        GameObject oflip = GameObject.FindGameObjectWithTag("ToggleFlip");
        if (oflip != null) {
            Toggle tflip = oflip.GetComponent<Toggle>();
            tflip.isOn = (_manager.flipSecond);

            //  oflip.SetActive(_manager.showSecondGroup);
        }
    }
    void Start() {
        p("******** Start **********");
        if (_manager == null) Awake();
        
        deltaRadius = bigRadius - smallRadius;
        rings = (int)(deltaRadius / distBetweenPoint);
        if (sequence == null) parseFormula(DEFAULT_FORMULA);
        createShells();
       
        if (_manager.magnet && leftSide) {
         //   createSpikes();
            createSphericalPoints(true);
        }
        if (useGridLines) {
            this.createGridPoints();
           if (leftSide) this.createGridLines();
        }
        else {
            createSphericalPoints(false);
            createSphericalLines();
        }
        checkVisibility();
        started = true;

    }


    private bool parseFormula(string formula) {
        int bras = 0;
        int kets = 0;
        int brasx = 0;
        int ketsx = 0;
        int brasw = 0;
        int ketsw = 0;
        modeBracket = true;
        p("parseFormula: formula is " + formula);
        if (formula == null || formula.Length < 1) {
                p("Formula " + formula + " is too short, using default");
            return parseFormula(DEFAULT_FORMULA);
        }
      
        char[] chars = formula.Trim().ToCharArray();
        sequence = new int[chars.Length];
        p("Got formula chars length " + sequence.Length);
        if (chars.Length == 0 ) {
           p("Braket formula " + formula + " must start with bra and end with ket: " + formula + ", using default");
            
            return parseFormula(DEFAULT_FORMULA);
        }
        if (chars.Length<1) { 
            modeBracket = false;
        }
        bool hasbrket = false;
        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            int which = -1;
            if (c == '<') {
                which = BRAY;
                bras++;
                hasbrket = true;
            }
            else if (c == '>') {
                which = KETY;
                kets++;
                hasbrket = true;
            }
            else if (c == '{') {
                which = BRAZ;
                brasw++;
            }
            else if (c == '}') {
                which = KETZ;
                ketsw++;
                hasbrket = true;
            }
            else if (c == '[') {
                which = BRAX;
                brasx++;
                hasbrket = true;
            }
            else if (c == ']') {
                which = KETX;
                ketsx++;
                hasbrket = true;
            }
            else if (c == 'x' || c == 'X' || c == 's' || c == 'S') {
                which = KERNELX;
            }
            else if (c == 'y' || c == 'Y') {
                which = KERNELY;
            }
            else {
                which = KERNELZ;
            }

            sequence[i] = which;
            //  p("parseFormula: Setting operation " + i + " to " + which +" for char "+c);
        }
        if (bras != kets) {
            //      p("Unequal number of bras " + bras + " and kets " + kets+": "+formula);
            return parseFormula(DEFAULT_FORMULA);
        }
        if (brasw != ketsw) {
            //      p("Unequal number of bras " + bras + " and kets " + kets+": "+formula);
            return parseFormula(DEFAULT_FORMULA);
        }
        if (brasx != ketsx) {
            //      p("Unequal number of bras " + bras + " and kets " + kets+": "+formula);
            return parseFormula(DEFAULT_FORMULA);
        }
        if (hasbrket == false) modeBracket = false;
        
        p("parseFormula: formula " + formula + " looks ok, sequence is "+sequence.ToString());
        for (int i = 0; i < sequence.Length; i++) {
            //p("Operation "+i+": "+sequence[i]);
        }
        this.formulaKey = sequence.GetHashCode();
        InputField f = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        f.text = formula;
        if (leftSide) _manager.formula = formula;
        return bras == kets;
    }

    private void getOtherPoints() {
        if (otherpoints != null) return;
        if (otherSphere == null) {
            p("getOtherPoints: the object otherSphere is null, cannot get other points");
        }
        else otherpoints = otherSphere.GetComponent<SpinLabScript>().points;
        if (otherpoints == null) {
            p("getOtherPoints: there are NO OTHER points on the other sphere " + otherSphere);
        }
    }
    private Quaternion computeMidTwist(int ax, int i, int j, int k, bool debug) {
        //  if (debug) p("computeMidPoint: point " + i + " / " + j + " / " + k);
        if (getPoint(i, j, k, debug) == null) {
            if (debug) p("computeMidTwist: point " + i + "/" + j + "/" + k + " does not exist, it is null");
            return Quaternion.identity;
        }


        Quaternion a = points[i, j, k].transform.rotation;
        if (_manager.showSecondGroup == false) {
            //   if (debug) p("****************************** second Group NOT showing, NOT computing average ");
            return a;
        }
        if (otherpoints == null) this.getOtherPoints();
        if (otherpoints == null) {
            //    p("computeMidPoint: there are NO OTHER points");
            return Quaternion.identity;
        }

        if (otherpoints[i, j, k] == null) {
            //  if (debug)p("computeMidPoint: otherpoint " + i + "/" + j + "/" + k + " does not exist");
            return Quaternion.identity;
        }

        Quaternion b = otherpoints[i, j, k].transform.rotation;
        //return computeMidPoint(ax, a, b, debug);
        return a;
    }
    private Vector3 computeMidPoint(int ax, int i, int j, int k, bool debug) {
        //  if (debug) p("computeMidPoint: point " + i + " / " + j + " / " + k);
        if (getPoint(i, j, k, debug) == null) {
            if (debug) p("computeMidPoint: point " + i + "/" + j + "/" + k + " does not exist, it is null");
            return Vector3.zero;
        }


        Vector3 a = points[i, j, k].transform.position;
        if (_manager.showSecondGroup == false) {
            //   if (debug) p("****************************** second Group NOT showing, NOT computing average ");
            return a;
        }
        if (otherpoints == null) this.getOtherPoints();
        if (otherpoints == null) {
            //    p("computeMidPoint: there are NO OTHER points");
            return Vector3.zero;
        }

        if (otherpoints[i, j, k] == null) {
            //  if (debug)p("computeMidPoint: otherpoint " + i + "/" + j + "/" + k + " does not exist");
            return Vector3.zero;
        }

        Vector3 b = otherpoints[i, j, k].transform.position;
        return computeMidPoint(ax, a, b, debug);
    }

    private Vector3 computeMidPoint(int axis, Vector3 a, Vector3 b, bool debug) {
        if (axis == 1) {
            // do not take average of x, but use the local one!
            return a;

        }
        if (this.midCache.ContainsKey((a,b, axis))) {
            return midCache[(a, b, axis)];
        }
        float wa = 1.0f;
        float wb = 1.0f;
        float f = 1.0f;
        float ax = a.x;
        float bx = b.x;

        float xme = this.spherePosition.x;
        float xother = this.otherSpherePosition.x;

        float dxa = Mathf.Abs((xme - ax) / xme);
        float dxb = Mathf.Abs((xother - bx) / xother);

        wa = 1.0f / (1.0f + dxa * dxa * f);
        wb = 1.0f / (1.0f + dxb * dxb * f);

        float tot = wa + wb;
        float x = (wa * a.x + wb * b.x) / tot;
        float y = (wa * a.y + wb * b.y) / tot;
        float z = (wa * a.z + wb * b.z) / tot;


        if (debug) {
            p("This point:  " + ax + "/" + a.y + "/" + a.z + "     wa=" + wa);
            p("Other point: " + bx + "/" + b.y + "/" + b.z + "     wb=" + wb);
            p("->Midpoint: " + x + "/" + y + "/" + z + ", total is " + tot);
        }
        Vector3 mid =  new Vector3(x, y, z);
        midCache[(a, b, axis)] = mid;
        return mid;


    }
    private void createGridPoints() {

        nrGridPointsPerAxis = new int[3];
        int minGrid = -_manager.gridSize * 2;
        int maxGrid = _manager.gridSize * 2;
        
        int dGrid = -minGrid + maxGrid;
        int n = (int)(dGrid / distBetweenPoint);

        if (_manager.showSecondGroup ==false) {
            this.nrGridPointsPerAxis[0] = n/2;
        }
        else  this.nrGridPointsPerAxis[0] = n;
        this.nrGridPointsPerAxis[1] = n / 2;
        this.nrGridPointsPerAxis[2] = n / 2;


        points = new GameObject[n + 1, n / 2 + 1, n / 2 + 1];
        origpoints = new Vector3[n + 1, n / 2 + 1, n / 2 + 1];
        Color c = Color.blue;
        if (this.leftSide) c = Color.red;
        // just one axis for now, the zx plane, so y - 0
        //p("***** createPoints for direction " + direction+ ", nrGridPointsPerAxis="+ n+"/"+(n/2)+"/"+(n/2)+ ", _manager.particleInfluence="+ _manager.particleInfluence);

        if (_manager.showPoints) {
            for (int i = 0; i < nrGridPointsPerAxis[0]; i++) {
                int mod = 2;
                float x = minGrid + i * distBetweenPoint;
                if (x == spherePosition.x) this.gridxleft = i;
                else if (x == otherSpherePosition.x) this.gridxright = i;
                for (int j = 0; j < nrGridPointsPerAxis[1]; j++) {
                    for (int k = 0; k < nrGridPointsPerAxis[2]; k++) {
                        bool add = i == gridxleft ||
                            (_manager.nrAxis > 1 && j == nrGridPointsPerAxis[1] / 2) ||
                            (_manager.nrAxis > 2 && k == nrGridPointsPerAxis[2] / 2);
                        if (!add) add = i % mod == 0 || j % mod == 0 || k % mod == 0;
                        if(add) createPoint(minGrid, c, i, j, k);
                    }
                }
            }
        }
        else {
            for (int ax = 0; ax < _manager.nrAxis; ax++) {
                //p("Grid points for axis "+ax);

                if (ax == 0) {
                    for (int i = 0; i < nrGridPointsPerAxis[0]; i++) {
                        float x = minGrid + i * distBetweenPoint;
                        if (x == spherePosition.x) this.gridxleft = i;
                        else if (x == otherSpherePosition.x) this.gridxright = i;
                        int j = nrGridPointsPerAxis[1] / 2;
                        for (int k = 0; k < nrGridPointsPerAxis[2]; k++) {

                            createPoint(minGrid, c, i, j, k);
                        }

                    }
                }
                else if (ax == 1) {
                    int i = gridxleft;
                    if (direction < 0) i = gridxright;
                    //   p("Creating points for axis 1, i is " + i);
                    for (int j = 0; j < nrGridPointsPerAxis[1]; j++) {
                        for (int k = 0; k < nrGridPointsPerAxis[2]; k++) {
                            createPoint(minGrid, c, i, j, k);
                        }
                    }

                }
                else {
                    for (int i = 0; i < n; i++) {
                        for (int j = 0; j < nrGridPointsPerAxis[1]; j++) {
                            int k = nrGridPointsPerAxis[2] / 2;
                            createPoint(minGrid, c, i, j, k);
                        }
                    }
                }
            }
        }

        p("Grid points created, gridxleft="+gridxleft);
    }

    private void addPath(GameObject gpoint, float trans) {
        addPath(gpoint, trans, 100);
    }
        private void addPath(GameObject gpoint, float trans, int count) {
        MarkerPath path = gpoint.AddComponent<MarkerPath>();
        path.frameDelta = Math.Max(1, (int)(2000.0f/_manager.speed/_manager.kernelAngle)) ;
        path.markerCount = count;
        
        path.arrowPrefab = markerScript.arrowPrefab;
        path.marker = this.markerScript.marker;
        path.pointColor = new Color(1.0f, 0, 0, trans);
        path.restart();
    }
  
    private void createPoint(int minGrid, Color c, int i, int j, int k) {
        float x = minGrid + i * distBetweenPoint;
        float z = minGrid / 2 + k * distBetweenPoint;
        float y = minGrid / 2 + j * distBetweenPoint;
        Vector3 v = new Vector3(x, y, z);
        GameObject gpoint = null;
        gpoint = Instantiate(pointPrefab, v, Quaternion.identity);
      
        if (_manager.showPoints) {
            if(fieldMaterial != null) gpoint.GetComponent<Renderer>().material = this.fieldMaterial;
            // 
           
        }
        else gpoint.GetComponent<Renderer>().material.SetColor("_Color", c);
        points[i, j, k] = gpoint;
        Vector3 vec = gpoint.transform.position;
        origpoints[i, j, k] = new Vector3(vec.x, vec.y, vec.z);
        bool vis = this.leftSide && _manager.showPoints;

        if (vis) {

            //only show every 4th
            int mod = 4;
            vis = i % mod == 0 && j % mod == 0 && k % mod == 0;
            if (_manager.isBelt()) {
                vis = i == this.gridxleft || j == this.nrGridPointsPerAxis[2] / 2 || k == this.nrGridPointsPerAxis[3] / 2;
            }
            if (vis) {
                int dGrid = _manager.gridSize * 2;
                float red = Mathf.Min(1.0f, i / dGrid );
                float green = Mathf.Min(1.0f, j / dGrid );
                float blue = Mathf.Min(1.0f, k / dGrid );

                c = new Color(red, green, blue, 0.5f);
                gpoint.transform.localScale *= 4;
                gpoint.GetComponent<Renderer>().material.SetColor("_Color", c);
                
                mod = 8;
                if (i % mod == 0 && j % mod == 0 && k % mod == 0) {
                    float dr = Math.Min(1, Math.Max(0.2f, 1.0f - Math.Abs((vec.magnitude - smallRadius) / bigRadius)));
                    addPath(gpoint, dr);
                }
                
            }
        }
        gpoint.GetComponent<Renderer>().enabled = vis;

        float r = Vector3.Distance(v, this.spherePosition);

        bool debug = false;// (i == 5 && k == 5);
        if (debug) p("Creating point " + i + "/" + j + "/" + k + ", r=" + r);
        Shell shell = null;
        shellMap.TryGetValue(r, out shell);
        if (shell == null) {
            GameObject s = Instantiate(spherePrefab, spherePosition, Quaternion.identity);
            shell = new Shell(r, smallRadius, bigRadius, s, _manager.particleInfluence,_manager.dropoff);

            shellMap.Add(r, shell);
        }
        if (shell != null) {
            gpoint.transform.parent = shell.shell.transform;
        }
        else p("Could not find shell for radius " + r);
    }

    
    private void createSphericalPoints(bool addTrace) {
        p("createSphericalPoints for direction " + direction+" belt. "+_manager.isBelt());
        float curDistBetweenPoint = distBetweenPoint / 2.0f;
        nrpointsPerRing = (int)(360.0f / dalpha) + 1;
        points = new GameObject[_manager.nrAxis, rings, nrpointsPerRing];
        Vector3 pointcoord = spherePosition;
        centerpoint = Instantiate(pointPrefab, pointcoord, Quaternion.identity);
        centerpoint.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        int nrrings = (int)((bigRadius - smallRadius) / curDistBetweenPoint);
        int dring = nrrings / 4;

        int whicha = 0;
        float da = 45;
        float db = 45;
       
        float astart = 0;
        float bstart = 0;
        float danglea = 45;
        float dangleb = 45;
        int minrings = nrrings/2;
        if (_manager.kernelAngle > 90) {
            da = 90;
            db = 45;
            danglea = 90;
            dangleb = 90;
            minrings = nrrings * 4 / 5;

        }
        else if (_manager.kernelAngle > 45) {
            da = 45;
            db = 45;
            danglea = 90;
            dangleb = 90;
            minrings = nrrings * 3/4;
        }
        if(_manager.isBelt()) {
            da = 90;
            db = 180;
            curDistBetweenPoint = 0.1f;
            astart = 90;
            bstart = 90;
            minrings = nrrings * 3/4;
        }
        p("da: " + da + ", db: " + db+", belt: "+_manager.isBelt());

      
        for (float alpha = astart; alpha < 360; alpha += da) {
          
            int whichb = 0;
            for (float beta = bstart; beta < 360; beta += db ) {
                  
                int ring = 0;

                Quaternion orient = Quaternion.Euler(alpha, beta, 0);

                bool showArrow = beta % dangleb == 0 && alpha % danglea == 0;
                if (showArrow) {
                    GameObject arrow = Instantiate(arrowPrefab, spherePosition, orient);
                    arrow.transform.localScale = new Vector3(0.2f, 0.2f, 1.0f);
                }
                
               
                for (float r = smallRadius; r < bigRadius; r += curDistBetweenPoint) {
                    ring++;
                    float dr = Math.Max(0.2f, 1.0f-Math.Abs((r - smallRadius) / bigRadius));
                    Vector3 vr = new Vector3(0, 0, r);
                        Color c = Color.gray;
                    
                        pointcoord = spherePosition + orient * vr;
                     
                        GameObject point = Instantiate(pointPrefab, pointcoord, Quaternion.identity);
                        point.GetComponent<Renderer>().material.SetColor("_Color", c);                     
                        point.GetComponent<Renderer>().enabled = true;

                        float red = Mathf.Min(1.0f, dr);
                        float green = Mathf.Min(1.0f, alpha / 360);
                        float blue = Mathf.Min(1.0f, beta / 360);

                        c = new Color(red, green, blue, 0.5f);
                        if (addTrace) {
                            point.transform.localScale *= 4;
                        if (_manager.isBelt()) {
                            Vector3 s = new Vector3(curDistBetweenPoint * 1.0f,
                                curDistBetweenPoint * 1.0f, curDistBetweenPoint * 5f);
                            point.transform.localScale = s;
                           
                        }

                        point.GetComponent<Renderer>().material.SetColor("_Color", c);
                        if (_manager.isBelt()) {
                            if ((ring % dring ==0  && ring > minrings || r + curDistBetweenPoint >= bigRadius)
                                && showArrow) {
                                addPath(point, dr, 150);
                            }
                        }
                        else if ((ring % dring == 0 && ring>minrings || r+ curDistBetweenPoint >= bigRadius) 
                                && showArrow) {
                                    addPath(point, dr);
                            }
                        }
                    // points[ax, ring, which] = point;
                    Shell shell;
                    shellMap.TryGetValue(r, out shell);
                    if (shell == null) {
                        GameObject s = Instantiate(spherePrefab, spherePosition, Quaternion.identity);
                        shell = new Shell(r, smallRadius, bigRadius, s, _manager.particleInfluence, _manager.dropoff);

                        shellMap.Add(r, shell);
                    }
                    
                            point.transform.parent = shell.shell.transform;
                        
                    }
                whichb++;
            }
            whicha++;

        }
    }
    public GameObject[,,] getPoints() {
        return points;
    }
    private void setTexture(LineRenderer lr) {

        //Make sure to enable the Keywords
        lr.material.EnableKeyword("_NORMALMAP");
        lr.material.EnableKeyword("_METALLICGLOSSMAP");
        lr.SetWidth(3, 3);
        //lr the Texture you assign in the Inspector as the main texture (Or Albedo)
        lr.material.SetTexture("_MainTex", m_MainTexture);
        //Set the Normal map using the Texture you assign in the Inspector
        lr.material.SetTexture("_BumpMap", m_Normal);
        //Set the Metallic Texture as a Texture you assign in the Inspector
        lr.material.SetTexture("_MetallicGlossMap", m_Metal);
    }
    private void createGridLines() {
        p("***** createGridLines for " + (_manager.nrAxis + 1) + " axis");
        //p("Points dimension: " + points.GetLength(0)+", "+points.GetLength(1)+", "+points.GetLength(2));
        lines = new LineRenderer[3, nrGridPointsPerAxis[0], nrGridPointsPerAxis[1], nrGridPointsPerAxis[2], 2];
        // p("Line dims = " + lines.GetLength(0) + ", " + lines.GetLength(1) + ", " + lines.GetLength(2) + ", " + lines.GetLength(3));

        for (int ax = 0; ax < _manager.nrAxis; ax++) {
            if (ax == 0) {
                int j = nrGridPointsPerAxis[1] / 2;
                //       p("axis 0: j is at " + j);
                for (int i = 0; i < points.GetLength(0); i++) {
                    for (int k = 0; k < points.GetLength(2); k++) {
                        // add 2 lines each
                        addGridLine(ax, i, j, k, 0);
                        addGridLine(ax, i, j, k, 1);
                    }

                }
            }
            else if (ax == 1) {

                int i = gridxleft;
                if (direction < 0) i = gridxright;
                //      p("axis 1: i is at " + i+" for direction "+direction);
                for (int j = 0; j < points.GetLength(1); j++) {
                    for (int k = 0; k < points.GetLength(2); k++) {
                        addGridLine(ax, i, j, k, 0);
                        addGridLine(ax, i, j, k, 1);
                    }
                }

            }
            else {
                int k = nrGridPointsPerAxis[2] / 2; ;

                for (int i = 0; i < points.GetLength(0); i++) {
                    for (int j = 0; j < points.GetLength(1); j++) {
                        addGridLine(ax, i, j, k, 0);
                        addGridLine(ax, i, j, k, 1);
                    }
                }
            }
        }

    }
    private void createSphericalLines() {
        p("createSphericalLines");
        lines = new LineRenderer[_manager.nrAxis, rings + 1, nrpointsPerRing, rings, nrpointsPerRing];
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
    private GameObject getPoint(int i, int j, int k, bool debug) {
        if (this.useGridLines == false && (j < 0 || j >= points.GetLength(1))) return centerpoint;
        if (i < 0 || i >= points.GetLength(0)) {
            //  warn("X/ax coord out of bounds: " + i);
            return null;
        }
        if (j < 0 || j >= points.GetLength(1)) {
            //  warn("y/ring coord out of bounds: " + j);
            return null;
        }
        if (k < 0 || k >= points.GetLength(2)) {
            // warn("z/which coord out of bounds: " + k);
            return null;
        }
        if (points[i, j, k] == null) {
            if (debug) warn("point is null:" + i + "/" + j + "/" + k);
        }

        return points[i, j, k];
    }
    private void addGridLine(int axis, int x, int y, int z, int which) {
        Color color = Color.white;
        int x2 = x;
        int y2 = y;
        int z2 = z;
        if (!this.isInBounds(x, y, z)) {
            return;
        }
        if (axis == 1) {
            if (this.leftSide == false) return;
            color = Color.yellow;
            // SAME X
            if (which == 0) y2++;
            else z2++;
        }
        else if (axis == 2) {
            color = new Color(1.0f, 0.5f, 0);
            // same t
            if (which == 0) x2++;
            else y2++;
        }
        else {
            // same y
            if (which == 0) x2++;
            else z2++;
        }
        if (this.isInBounds(x2, y2, z2)) {
            bool debug = false;// ( axis==0 && x == 5 && z == 5);
            if (debug) p("Creating line " + x + "/" + y + "/" + z + " to " + x2 + "/" + y2 + "/" + z2);
            GameObject a = getPoint(x, y, z, debug);
            GameObject b = getPoint(x2, y2, z2, debug);
            LineRenderer lr = addLine(color, a, b);
            if (lr != null) {
                // remember line 
              //  setTexture(lr);
                lines[axis, x, y, z, which] = lr;
            }
            else warn("Could not create line ax " + axis + ", i, i,k=" + x + "/" + y + "/" + z + "-" + x2 + "/" + y2 + "/" + z2);
        }


    }
    private void addLine(int axis, int ring1, int w1, int ring2, int w2) {
        Color c = Color.white;
        if (axis == 1) c = Color.yellow;
        else if (axis == 2) c = new Color(1.0f, 0.5f, 0);
        LineRenderer lr = addLine(c, getPoint(axis, ring1, w1, false), getPoint(axis, ring2, w2, false));
        if (lr != null) {
            // remember line 
            if (ring1 >= 0 && ring2 >= 0) lines[axis, ring1, w1, ring2, w2] = lr;

        }
        else p("Could not create line ax " + axis + ", ring " + ring1 + " which " + w1);


    }
    private LineRenderer addLine(Color c, GameObject a, GameObject b) {
        if (a == null) {
            warn("Cannot add line. a is null");
            return null;
        }
        if (b == null) {
            warn("Cannot add line. b is null");
            return null;
        }
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

        if (_manager.colorLines==true) checkColorOfLine(lr, 0,0, 0, 0, -1);


        return lr;
    }
    private Color getColorValue(float dist, float t) {
        float r = 0.5f;
        float g =0.5f;
        float b = 0.5f;
        float c = 0;
        if (dist != 0) {
            c = Mathf.Min(0.5f, 5.0f / (float)Mathf.Abs(dist));
            if (dist > 0) {
                r = r + c;

            }
            else b = b + c;
        }
        return new Color(r, g, b, t);
    }
    public void Resize(Transform g, float amount, Vector3 direction) {
       // g.position += direction * amount / 2; // Move the object in the direction of scaling, so that the corner on ther side stays in place
        g.localScale = direction * amount; // Scale object in the specified direction
    }
    private void checkColorOfLine(LineRenderer lr, int axis, int i, int j, int k, int which) {
        // in the right sphere, make half the line transparent
        // actually better would be to connect the line
        // to the opposite side
        colorMode = _manager.colorBy;
        // and should happen in each update
        Vector3 a = lr.GetPosition(0);
        Vector3 b = lr.GetPosition(1);
        Color tra = new Color(0, 0, 0, 0);
        Color ca = Color.white;
        Color cb = Color.white;
        float xa = a.x;
        float xb = b.x;
        bool changeColor = false;
        float t = 1.0f;
       // if (Time.frameCount % 10 == 0) p("Color by is " + colorMode);
        if (colorMode == colorByAxis) {
            if (axis == 0) { // hor plane, color if z stays same
                if (k == this.nrGridPointsPerAxis[2] / 2 && which == 0) {
                    ca = Color.blue;
                    cb = Color.blue;
                    changeColor = true;
                }
                else if (i == gridxleft && which == 1) {
                    ca = Color.magenta;
                    cb = Color.magenta;
                    changeColor = true;

                }
            }

            else if (axis == 1) {
                if (k == this.nrGridPointsPerAxis[2] / 2 && which == 0) {
                    ca = Color.red;
                    cb = Color.red;
                    changeColor = true;
                }
                else if (i == gridxleft && which == 1) {
                    ca = new Color(1, 1, 0);
                    cb = new Color(1, 1, 0);
                    changeColor = true;

                }
            }
            else {
                if (k == this.nrGridPointsPerAxis[2] /2 && which == 0) {
                    ca = Color.blue;
                    cb = Color.blue;
                    changeColor = true;
                }
                else if (i == this.gridxleft && which == 1) {
                    ca = Color.red;
                    cb = Color.red;
                    changeColor = true;

                }
            }
        }
        else if (colorMode == colorByStrain) { // rot plus comp
            float d = (Vector3.Distance(a, b) - distBetweenPoint) / distBetweenPoint * 10f;
          //  if (Time.frameCount % 10 == 0) p("colorByCompression. d=" + d);
            if (d > 0) {
                d = Mathf.Min(1.0f, d);
                ca = new Color(1.0f, 1.0f - d, 1.0f - d);
            }
            else {
                d = Mathf.Min(1.0f, -d);
                ca = new Color(1.0f - d, 1.0f - d, d);


            }
            cb = ca;
            changeColor = true;
        }
        else if (colorMode == colorByTwist) { // diff of rotation
            if (points[i, j, k] == null) return;
            GameObject gpoint = points[i,j,k];
            Vector3 orig = origpoints[i, j, k] - this.spherePosition;
            Vector3 now = gpoint.transform.position - this.spherePosition;
            //Quaternion qa =Quaternion.FromToRotation(orig, now );
            Quaternion qa = gpoint.transform.rotation;

            float d = Vector3.AngleBetween(orig, now)*10;
            //float d = Quaternion.Angle(qa, Quaternion.identity)/360.0f*20f;
            
            
          //  Quaternion.
            if (Time.frameCount % 50 == 0 &&  i==0) p("colorByTwist. angle=" + d );

           // Resize(gpoint.transform, 1.0f, new Vector3(1f, d*2, 1f));
            //gpoint.transform.localScale.y = d;
          //  gpoint.transform.rotation = qa;
            if (d > 0) {
                d = Mathf.Min(1.0f, d);
                //ca = new Color(1.0f, 1.0f - d, 1.0f - d);
                ca = gradient.Evaluate(d);
            }
            else {
                d = Mathf.Min(1.0f, -d);
                ca = new Color(1.0f - d, 1.0f - d, d);

            }
            cb = ca;
            Material arrowMat = gpoint.GetComponent<MeshRenderer>().material;
            if (arrowMat != null) arrowMat.SetColor("_Color", ca);
            gpoint.GetComponent<Renderer>().material.SetColor("_Color", ca);
            changeColor = true;
        }
        else if (colorMode == colorByCompression) { // compression = length - grid length
            float d = (Vector3.Distance(a, b)- distBetweenPoint )/ distBetweenPoint * 10f;
            if (Time.frameCount % 10 == 0 && i == 0) p("colorByCompression. d=" +  d);
            if (d>0) {
                d = Mathf.Min(1.0f, d);
                ca = new Color(1.0f, 1.0f - d, 1.0f-d);
            }
            else {
                d = Mathf.Min(1.0f, -d);
                ca = new Color(1.0f-d, 1.0f-d, 1.0f);
                

            }
            cb = ca;
            changeColor = true;
        }
        else if (colorMode == colorByY) {
            float r1 = Vector3.Distance(a, this.spherePosition) / 5.0f;
            float r2 = Vector3.Distance(a, this.otherSpherePosition) / 5.0f;

            float r = Mathf.Max(0, Mathf.Min(r1, r2) / _manager.particleInfluence - 2.0f);
            t = 1.0f / (1.0f + r * r);

           
            if (axis == 0) {
                // up: red 1, 0.5, 0.5
                // down: blue, 0.5, 0.5, 1
                ca = getColorValue(a.y, t);
                // ca = new Color(1.0f, 1.0f, 1.0f, t);
                cb = getColorValue(b.y, t);
                changeColor = true;
            }
            else if (axis == 1) {
                ca = new Color(1.0f, 1.0f, 0.0f, t);
                cb = new Color(1.0f, 1.0f, 0.0f, t);
                changeColor = true;
            }
            else {
                ca = new Color(1.0f, 0.5f, 0.0f, t);
                cb = new Color(1.0f, 0.5f, 0.0f, t);
                changeColor = true;
            }
        }
        if (changeColor == true ) {
            lr.startColor = ca;
            lr.endColor = cb;
        }
        if (!leftSide) {
            // RIGHT side            
            lr.startColor = tra;
            lr.endColor = tra;
        }

    }

    private void setSimpleTwist() {
        _manager.setSimpleTwist();
        refresh("");
    }

    public void refresh(string nada) {
        p("***** refresh *****");
        _manager.camPosition = cam.transform.position;
        _manager.camRotation = cam.transform.rotation;
        p("Setting manager.cameraTransform to " + cam.transform);
        Slider phaseSlider = GameObject.Find("PhaseSlider").GetComponent<Slider>();
        _manager.curUserPhase = phaseSlider.value;

        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();
        Slider sslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        Slider xslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();

        Slider gslider = GameObject.Find("GridSize").GetComponent<Slider>();
        InputField f = GameObject.FindGameObjectWithTag("FormulaField").GetComponent<InputField>();
        Slider pslider = GameObject.FindGameObjectWithTag("ParticleInfluence").GetComponent<Slider>();

        Toggle tsphere = GameObject.Find("ShowSphere").GetComponent<Toggle>();
        Toggle tpoints = GameObject.Find("ShowPoints").GetComponent<Toggle>();
        Toggle tmag = GameObject.Find("ShowMagnetic").GetComponent<Toggle>();
        Toggle tgroup = GameObject.FindGameObjectWithTag("ToggleGroup").GetComponent<Toggle>();
        Toggle tflip = GameObject.FindGameObjectWithTag("ToggleFlip").GetComponent<Toggle>();

        Dropdown preselect = GameObject.Find("CustomDropdown").GetComponent<Dropdown>();
        Dropdown particledrop = GameObject.Find("ParticleDropdown").GetComponent<Dropdown>();
        Dropdown colordrop = GameObject.Find("ColorDrop").GetComponent<Dropdown>();

        Toggle tcolor = GameObject.Find("ColorLines").GetComponent<Toggle>();

        Toggle tcomp = GameObject.Find("ToggleCompress").GetComponent<Toggle>();

        float particleInfluence = (float)pslider.value;
        string formula = f.text;

        int nrAxis = (int)xslider.value;
        float angle = aslider.value;
        int numberOfSteps = (int)aslider.maxValue / 5;
        float range = (angle / aslider.maxValue) * numberOfSteps;
        int ceil = Mathf.CeilToInt(range);
        angle = ceil*5;
        aslider.value = angle;



        float speed = sslider.value;
        this.parseFormula(formula);
        if (leftSide) {
            _manager.preselect = preselect.value;
            _manager.colorBy = colordrop.value;
            _manager.flipSecond = tflip.isOn;
            _manager.colorLines = tcolor.isOn;
            _manager.useCompression = tcomp.isOn;
            p("Compression: " + _manager.useCompression);
            _manager.showSphere = tsphere.isOn;
            _manager.showPoints = tpoints.isOn;
            _manager.magnet = tmag.isOn;
            _manager.showSecondGroup = tgroup.isOn;
            _manager.gridSize = (int)gslider.value;
            _manager.kernelAngle = angle;
            _manager.speed = speed;
            _manager.nrAxis = nrAxis;
            _manager.formula = f.text;
            _manager.particleInfluence = particleInfluence;


            p("refresh: Angle from slider is " + angle + ", speed is " + speed + ", nrAxis is " + nrAxis + ", formula: " + formula + ", particleInfluence=" + particleInfluence);
            restartScene();
        }

    }
    private void restartScene() {
        p(" ************* restarting scene ***********");
      
        Scene scene = SceneManager.GetActiveScene();

        restartMarkers();
        SceneManager.LoadScene(scene.name);
    }

    private bool isInBounds(int i, int j, int k) {
        if (i < this.nrGridPointsPerAxis[0] && j < this.nrGridPointsPerAxis[1] && k < this.nrGridPointsPerAxis[2]) return true;
        else return false;
    }
    void updateGridLine(int ax, int i, int j, int k, int which) {
        if (ax >= lines.GetLength(0)) {
            p("updateGridLine:ax too large: " + ax);
            return;
        }
        bool debug = false;// (ax == 1 && j == 5 && k == 5);
        if (debug) p("-------------------- Updating grid line ax=" + ax + ", ijk=" + i + "/" + j + "/" + k + ", which =" + which);
        Vector3 start = this.computeMidPoint(ax, i, j, k, debug);
        if (start == Vector3.zero) {
            //if (isInBounds(i, j, k)) p("-------------------- Updating grid line ax=" + ax + ", ijk=" + i + "/" + j + "/" + k + ", which =" + which+", but start is ZERO");
            return;
        }

        Vector3 end = Vector3.zero;
        LineRenderer lr = lines[ax, i, j, k, which];


        if (lr == null) {
            if (isInBounds(i, j, k)) {
                //p("updateGridLine: no such line at ijk=" + i + "/" + j + "/" + k + ", which =" + which);
            }
            return;
        }
        if (ax == 0) { // Y SAME
            if (debug) p("Computing mipoint point, y is the same");
            if (which == 0) end = this.computeMidPoint(ax, i + 1, j, k, debug);
            else end = this.computeMidPoint(ax, i, j, k + 1, debug);
        }
        else if (ax == 1) { // X SAME
            if (debug) p("Computing mipoint point, x is the same");
            if (which == 0) end = this.computeMidPoint(ax, i, j + 1, k, debug);
            else end = this.computeMidPoint(ax, i, j, k + 1, debug);
        }
        else { // Z SAME
            if (debug) p("Computing mipoint point, z is the same");
            if (which == 0) end = this.computeMidPoint(ax, i + 1, j, k, debug);
            else end = this.computeMidPoint(ax, i, j + 1, k, debug);
        }
        if (end != Vector3.zero) {
            updateOneGridLine(lr, start, end);
            if (_manager.colorLines==true) this.checkColorOfLine(lr, ax, i, j, k, which);
        }
        //     else p("There was no end point");
    }
    void updateGridLines() {

        //p("Updating grid lines, nr axis "+ _manager.nrAxis);
        this.getOtherPoints();
        for (int ax = 0; ax < _manager.nrAxis; ax++) {
            if (ax == 0) {
                // Y IS SAME
                for (int i = 0; i < points.GetLength(0); i++) {
                    int j = this.nrGridPointsPerAxis[1] / 2;
                    for (int k = 0; k < points.GetLength(2); k++) {
                        updateGridLine(ax, i, j, k, 0);
                        updateGridLine(ax, i, j, k, 1);
                    }
                }
            }
            else if (ax == 1) {

                int i = this.gridxleft;
                if (!this.leftSide) i = this.gridxleft;
                for (int j = 0; j < points.GetLength(1); j++) {
                    for (int k = 0; k < points.GetLength(2); k++) {
                        updateGridLine(ax, i, j, k, 0);
                        updateGridLine(ax, i, j, k, 1);
                    }
                }

            }
            else {
                int k = this.nrGridPointsPerAxis[2] / 2;
                //   for (int i = 0; i < points.GetLength(0); i++) {
                for (int i = 0; i < points.GetLength(0); i++) {
                    for (int j = 0; j < points.GetLength(1); j++) {
                        updateGridLine(ax, i, j, k, 0);
                        updateGridLine(ax, i, j, k, 1);
                    }
                }
            }
        }
    }
    private void updateOneGridLine(LineRenderer lr, Vector3 start, Vector3 end) {
        lr.enabled = _manager.showLines && isVisible();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

    }
    void updateSphericalLines() {

        for (int ax = 0; ax < _manager.nrAxis; ax++) {

            for (int r = 0; r < rings; r++) {
                for (int w = 0; w < nrpointsPerRing; w++) {
                    //   bool show = (r == 0 && w < 2);
                    LineRenderer lr = null;
                    if (r + 1 < rings) {
                        lr = lines[ax, r, w, r + 1, w];
                        // these are the RADIAL LINES
                        if (lr != null) {
                            lr.enabled = _manager.showLines && isVisible();
                            lr.SetPosition(0, points[ax, r, w].transform.position);
                            lr.SetPosition(1, points[ax, r + 1, w].transform.position);
                            // check color of lines
                            if (r > 1) {
                                // the inner shell(s) will never overlap, so just check the outer ones
                                // give it the coordinate of the OTHER point
                                // w is on the OPPOSITE side
                                int otherw = (w + nrpointsPerRing / 2) % nrpointsPerRing;
                                checkColorOfLine(lr, ax, 0,0,0,-1);
                            }
                        }

                    }
                    if (r == 0) {
                        lr = lines[ax, rings, w, r, w];
                        if (lr != null) {
                            lr.enabled = _manager.showLines && isVisible();
                            lr.SetPosition(0, centerpoint.transform.position);
                            lr.SetPosition(1, points[ax, r, w].transform.position);
                        }
                    }
                    if (r < rings) {
                        int v = w - 1;
                        if (v < 0) v = nrpointsPerRing - 1;
                        if (w > 0) {
                            lr = lines[ax, r, v, r, w];
                            if (r > 1) {
                                // these lines are along the sphere
                                // the inner shell(s) will never overlap, so just check the outer ones
                                checkColorOfLine(lr, ax,0, 0, 0, -1);
                            }
                            // these are the circular lines
                            if (lr != null) {
                                lr.enabled = _manager.showLines && isVisible();
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
    private void warn(string s) {
        Debug.Log("WARNING ++++++++++++++++++++++++++++++++ SphereLab: " + s);
    }
    public void setAngleText() {
        Text at = GameObject.Find("AngleLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("AngleSlider").GetComponent<Slider>();

        at.text = "Kernel angle " + (int)aslider.value;
        restartMarkers();
    }

    public void setAxisText() {
        Text at = GameObject.Find("AxisLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("AxisSlider").GetComponent<Slider>();

        at.text = "Axis " + (int)aslider.value;
    }
    public void setUserPhase() {
      
        Slider aslider = GameObject.Find("PhaseSlider").GetComponent<Slider>();
        _manager.curUserPhase = aslider.value;
        if (_manager.curUserPhase < 0) _manager.userPhase = false;
        else _manager.userPhase = true;

        p("_manager.curUserPhase is " + _manager.curUserPhase + ", _manager.userPhase=" + _manager.userPhase);

    }

    public void setSpeedText() {
        Text at = GameObject.Find("SpeedLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        float speed = aslider.value;
        if (leftSide) _manager.speed = speed;

        //  markerScript.markerCount = (int)(150 / speed);
        if (markerScript != null) {
            markerScript.frameDelta = Mathf.Max(1, (int)(5 / speed));
             markerScript.restart();
        }
        else if (this.leftSide) p("There is no marker script");
        at.text = "Speed " + (int)aslider.value;


    }

    public void setSpinMode() {
        Text at = GameObject.Find("SpinLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("SpinSlider").GetComponent<Slider>();
        if (aslider == null) return;
        spinMode = (int)aslider.value;
        if (leftSide) _manager.spinMode = spinMode;
        restartMarkers();
        at.text = "Spin Mode " + (int)aslider.value;
    }

    public void setGridSize() {
        Text at = GameObject.Find("GridSizeLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("GridSize").GetComponent<Slider>();
        if (aslider == null) return;
        int gridSize = (int)aslider.value;
        if (leftSide) _manager.gridSize = gridSize;

        at.text = "Grid Size " + (int)aslider.value;
    }
    private void restartMarkers() {
        //s markerScript.visible = (this.leftSide );
        if (markerScript != null) {
            markerScript.restart();

        }
        else if (this.leftSide) p("There is no marker script");
    }
    public void colorChange(int change) {
         p("colorChange: change=" + change);
        Dropdown preselect = GameObject.Find("ColorDrop").GetComponent<Dropdown>();
        if (preselect == null) {
            p("Could not find dropdown ColorDrop");
            return;
        }
        int which = preselect.value;
        this.colorMode = which;
        _manager.colorBy = which;
        p("===================== colorBy = " + colorMode);
        updateUiFromManagerValues();
        this.refresh("");
    }
    public void particleChange(int change) {
        p("particleChange: change=" + change);
        Dropdown d = GameObject.Find("ParticleDropdown").GetComponent<Dropdown>();
        if (d == null) {
            p("Could not find dropdown ParticleDropdown");
            return;
        }
        int which = d.value;
        curparticle= particles.AllParticles[which];
        _manager.formula = curparticle.Formula;
        p("===================== particleChange = " + curparticle.Name+", "+ curparticle.Formula);
        updateUiFromManagerValues();
        this.refresh("");
    }
    public void selectionChanged(int change) {
        //p("selectionChanged: change=" + change);
        Dropdown preselect = GameObject.Find("CustomDropdown").GetComponent<Dropdown>();
        if (preselect == null) {
            p("Could not find dropdown");
            return;
        }

        int which = preselect.value;
        p("Preselect selectionChanged: " + which);
        _manager.preselect = which;
        _manager.dropoff = GameManager.DROPOFF_RSQUARED;
        if (which == 0) _manager.setSimpleTwist();
        else if (which == 1) _manager.setSimpleCompression();
        else if (which == 2) _manager.setSimpleSpin();
        else if (which == 3) _manager.setComplexSpin(false);
        else if (which == 5) _manager.setDoubleTwist();
        else if (which == 6) _manager.setComplexSpin(true);
        else if (which == 7) _manager.setComplexSpinMax(false);
        //else if (which == 8) _manager.setComplexSpinMax(true);
        else if (which == GameManager.BELT) {
            _manager.setBelt();
        }

        p("Manager preselect is now: " + which);
        updateUiFromManagerValues();
        this.refresh("");
    }

    public void toggleVisible(bool v) {
        if (leftSide) {
            string tag = sphere.tag;
            bool vis = !_manager.showSphere && isVisible();
            p("Toggle visible clicked for tag " + tag + ", vis is " + vis);

            _manager.showSphere = vis;
            GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject t in tagged) {
                t.GetComponent<Renderer>().enabled = vis;
                //  p(t.name + " is now vis: " + vis);
            }
            vis = _manager.showSecondGroup;
            tagged = GameObject.FindGameObjectsWithTag(otherSphere.tag);
            foreach (GameObject t in tagged) {
                t.GetComponent<Renderer>().enabled = vis;
                //  p(t.name + " is now vis: " + vis);
            }


        }

    }
    public void toggleRightGroup(bool v) {
        if (_manager == null) {
            warn("Toggle second group clicked: " + v + ", but _manager is still null");
            return;
        }
        p("Toggle second group clicked: " + v);
        bool enabled = !_manager.showSecondGroup;
        _manager.showSecondGroup = enabled;

        GameObject oflip = GameObject.FindGameObjectWithTag("ToggleFlip");
        if (oflip != null) {
            Toggle tflip = oflip.GetComponent<Toggle>();
            //tflip.enabled = _manager.showSecondGroup;

            //oflip.SetActive(_manager.showSecondGroup);
        }
    }
    public void toggleLines(bool v) {
        if (_manager == null) return;
        p("Toggle lines clicked: " + v);
        if (leftSide) _manager.showLines = !_manager.showLines;


    }
    public void flipSecond(bool v) {
        if (_manager == null) return;
        p("Toggle flipSecond clicked: " + v);
        if (leftSide) {
            _manager.flipSecond = !_manager.flipSecond;
            this.refresh("bla");
        }


    }
    public void toggleCompression(bool v) {
        if (_manager == null || !leftSide) return;
        _manager.useCompression = !_manager.useCompression;
        p("Toggle compression clicked: " + _manager.useCompression);
        restartMarkers();

    }
    public void toggleColor(bool v) {
        if (_manager == null) return;
         _manager.colorLines = !_manager.colorLines;
        p("Toggle color clicked: " + _manager.colorLines);

    }

}
