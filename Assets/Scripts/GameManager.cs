using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public delegate void GameListener(string action);

    private static bool _created = false;

    private List<GameListener> listeners;


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
    public bool rotate;

    [SerializeField]
    public bool showSphere;

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
        kernelAngle = 45;
        rotate = true;
        showSphere = false;
        showLines = true;
        showPoints = false;
        flipSecond = false;
        gridSize = 3;
        colorLines = true;
        spinMode = 1;
        particleInfluence = 0.5f;
        useCompression = false;
        showSecondGroup = false;
        speed = 3.0f;
        compressionSpeed = 2.0f;
        compressionMagnitude = 0.8f;
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
        kernelAngle = 45;
        formula = "x";
        nrAxis = 1;
        showLines = true;
        colorLines = false;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("Set simple twist");
    }
   public void setSimpleCompression() {
            Reset();
            useCompression = true;
            speed = 5;
            gridSize = 3;
            spinMode = 1;
            showSecondGroup = false;
            kernelAngle = 0;
            formula = "x";
            nrAxis = 1;
            showLines = true;
            colorLines = false;
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
        kernelAngle = 45;
        formula = "xy";
        nrAxis = 1;
        showLines = true;
        colorLines = false;
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
        nrAxis = 1;
        showLines = true;
        colorLines = false;
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
        preselect = 4;
        showSecondGroup = second;
        kernelAngle = 180;
        formula = "<x>";
        nrAxis = 1;
        showLines = true;
        colorLines = false;
        flipSecond = false;
        particleInfluence = 0.5f;
        p("setComplexSpin");
    }
    public void Init() {
        Reset();
        initUi();
    }
}


