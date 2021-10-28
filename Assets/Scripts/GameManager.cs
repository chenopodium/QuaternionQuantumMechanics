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
    public void Init() {
        kernelAngle = 45;
        rotate = true;
        showSphere = false;
        showLines = true;
        showPoints = false;
        flipSecond = false;
        gridSize = 6;
        spinMode = 1;
        particleInfluence = 0.5f;
        useCompression = false;
        showSecondGroup = false;
        speed = 5.0f;
        compressionSpeed = 2.0f;
        compressionMagnitude = 0.8f;
        nrAxis = 1;
        formula = "<x>";
        initUi();
    }
}

