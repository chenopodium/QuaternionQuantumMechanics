using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public delegate void GameListener(string action);

    private static bool _created = false;

    public static int BELT = 8;

    public static int DROPOFF_FIXED = 1;
    public static int DROPOFF_RSQUARED = 0; 
    public static int DROPOFF_WAVE = 2;

    private List<GameListener> listeners;

    [SerializeField]
    public int dropoff = DROPOFF_RSQUARED;


    [SerializeField]
    public bool magnet = true;

    [SerializeField]
    public bool wave=false;

    [SerializeField]
    public Vector3 camPosition;

    [SerializeField]
    public Quaternion camRotation;

    [SerializeField]
    public Vector3 panelPosition;

    [SerializeField]
    public float kernelAngle;

    [SerializeField]
    public int spinMode;

    [SerializeField]
    public int gridSize;

    [SerializeField]
    public string formula;

    [SerializeField]
    public float speed;

    [SerializeField]
    public float particleInfluence;

    [SerializeField]
    public int preselect;

    [SerializeField]
    public int colorBy;

    [SerializeField]
    public bool rotate;

    [SerializeField]
    public bool showSphere;

    [SerializeField]
    public bool userPhase;

    [SerializeField]
    public float curUserPhase;

    [SerializeField]
    public bool showLines;

    [SerializeField]
    public bool showPoints;

    [SerializeField]
    public bool showSecondGroup;

    [SerializeField]
    public bool colorLines;

    [SerializeField]
    public bool flipSecond;

    [SerializeField]
    public int nrAxis;

    [SerializeField]
    public float compressionMagnitude;

    [SerializeField]
    public float compressionSpeed;

    [SerializeField]
    public bool useCompression;

    private void Awake() {
        if (!_created) {
            DontDestroyOnLoad(this.gameObject);
            _created = true;
            Init();
        }
    }
    public void addListener(GameListener li) {
        if (listeners == null) listeners = new List<GameListener>();
        listeners.Remove(li); // just in case it was already in there
        listeners.Add(li);
    }
    public void notifyListeners(string action) {
        if (listeners == null) return;
        foreach (GameListener li in listeners) {
            li.Invoke(action);
        }
    }
    public void toggleRightGroup(bool v) {

        p("Toggle second group clicked: " + v);
        bool enabled = !showSecondGroup;
        showSecondGroup = enabled;
        notifyListeners("togglegroup");
    }
    private void p(string s) {
        Debug.Log("GameManager: " + s);
    }
    private void warn(string s) {
        Debug.Log("GameManager WARNING: " + s);
    }
    public void setParticleInfluence() {
        
        UnityEngine.UI.Text at = GameObject.Find("ParticleInfluenceLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("ParticleInfluence").GetComponent<Slider>();
        particleInfluence = (float)aslider.value;
    //    p("setParticleInfluence to " + particleInfluence);
        if (at != null) at.text = "Particle influence is " + particleInfluence;
        //   else p("Could not find ParticleInfluenceLabel");
        notifyListeners("particleinfluence");
    }
    private void initUi() {
        UnityEngine.UI.Text at = GameObject.Find("ParticleInfluenceLabel").GetComponent<Text>();
        Slider aslider = GameObject.FindGameObjectWithTag("ParticleInfluence").GetComponent<Slider>();
        aslider.value = particleInfluence;
        if (at != null)  at.text = "Particle influence is " + particleInfluence;
        else p("Could not find ParticleInfluenceLabel");
    }
    public void Reset() {
        kernelAngle = 10;
        colorBy = 0;
        userPhase = true;
        this.dropoff = DROPOFF_RSQUARED;
        wave = false;
        magnet = true;
        rotate = true;
        preselect = BELT;
        showSphere = true;
        showLines = false;
        showPoints = false;
        flipSecond = false;
        gridSize = 4;
        colorLines = true;
        spinMode = 1;
        particleInfluence = 0.3f;
        useCompression = false;
        showSecondGroup = false;
        speed = 2.0f;
        compressionSpeed = 2.0f;
        compressionMagnitude = 0.5f;
        nrAxis = 1;
        formula = "<x>";
    }
    public void setSimpleTwist() {
        Reset();
        useCompression = false;
        speed = 5;
        gridSize = 3;
        spinMode = 1;
        showSecondGroup = false;
        preselect = 0;
        showPoints = true;
        kernelAngle = 45;
        formula = "x";
        nrAxis = 1;
        showLines = true;
        colorLines = true;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("Set simple twist");
    }
   public void setSimpleCompression() {
            Reset();
        showPoints = true;
            useCompression = true;
            speed = 5;
            gridSize = 3;
            spinMode = 1;
            showSecondGroup = false;
            kernelAngle = 0;
            formula = "x";
        preselect = 1;
        nrAxis = 1;
            showLines = true;
            colorLines = true;
            flipSecond = false;
            particleInfluence = 0.5f;
         p("Set setSimpleCompression");
    }
       public void setDoubleTwist() {
        Reset();
        useCompression = false;
        speed = 3;
        gridSize = 3;
        spinMode = 1;
        showSecondGroup = false;
        preselect = 5;
        kernelAngle = 45;
        formula = "xy";
        nrAxis = 1;
        showLines = true;
        colorLines = true;
        this.magnet = false;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("Set setDoubleTwist");
    }
    public void setSimpleSpin() {
        Reset();
        useCompression = false;
        speed = 3;
        gridSize = 3;
        spinMode = 1;
        showSecondGroup = false;
        kernelAngle = 45;
        formula = "<x>";
        preselect = 2;
        nrAxis = 1;
        showLines = true;
        colorLines = true;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("setSimpleSpin");
    }
    public void setComplexSpin(bool second) {
        Reset();
        useCompression = false;
        speed = 5;
        gridSize = 3;
        spinMode = 1;
        preselect = 3;
        if (second) preselect = 6;
        showSecondGroup = second;
        kernelAngle = 180;
        formula = "<x>";
        nrAxis = 1;
        showLines = true;
        colorLines = true;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("setComplexSpin");
   
    }
    public void setBelt() {
        Reset();
        useCompression = false;
        speed = 2;
        gridSize = 4;
        spinMode = 1;
        preselect = BELT;
        dropoff = DROPOFF_FIXED;
        showSecondGroup = false;
        kernelAngle = 180;
        formula = "<x>";
        nrAxis = 1;
        showLines = false;
        colorLines = true;
        dropoff = DROPOFF_FIXED;
        flipSecond = false;
        particleInfluence = 0.3f;
        p("setBelt, preselect is " + preselect);

    }
    public bool isBelt() {
        return preselect == BELT;
    }
    public void setComplexSpinMax(bool second) {
        Reset();
        useCompression = false;
        speed = 7;
        gridSize = 6;
        spinMode = 1;
        preselect = 7;
     //   if (second) preselect = 8;
        showSecondGroup = second;
        kernelAngle = 180;
        formula = "<x>";
        nrAxis = 1;
        showLines = true;
        colorLines = true;
        flipSecond = false;
        particleInfluence = 1.0f;
        p("setComplexSpinMax");
    }
    public void Init() {
        Reset();
        initUi();
    }
}


