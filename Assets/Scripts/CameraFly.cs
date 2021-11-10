using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFly : MonoBehaviour
{
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public Vector2 sensitivity = new Vector2(0.1f, 0.02f);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;
    public Vector2 targetCharacterDirection;

   
    public float flySpeed = 0.2f;
    public GameObject defaultCamera;
    private Camera cam;
    private Canvas canvas;
    private float X;
    private float Y;
    private float timeScale;
    public float Sensitivity;
   
    private Transform startingTransform;
    
    private bool modeFly;
    private int lastChange;

    void Start() {
      //  p("starting camerafly");
        // Set target direction to the camera's initial orientation.
        targetDirection = transform.localRotation.eulerAngles;
        startingTransform = transform;
        modeFly = false;
        timeScale = Time.timeScale;
        Vector3 euler = transform.rotation.eulerAngles;
        X = euler.x;
        Y = euler.y;
        Cursor.visible = true;


        // move canvas 
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        
        cam = GameObject.FindObjectOfType<Camera>();
        float screenx = Screen.width/2;
        float screeny = Screen.height / 2;
        
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(screenx, screeny, cam.nearClipPlane));
   //     p("Moving canvas to " + point);
        canvas.transform.position = point;


    }
    public GameObject Find(GameObject parent, string name) {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs) {
            if (t.name == name) {
                return t.gameObject;
            }
        }
        return null;
    }
    void Update() {

        if (modeFly) {
            const float MIN_X = 0.0f;
            const float MAX_X = 360.0f;
            const float MIN_Y = -90.0f;
            const float MAX_Y = 90.0f;

            X += Input.GetAxis("Mouse X") * (Sensitivity * Time.deltaTime);
            if (X < MIN_X) X += MAX_X;
            else if (X > MAX_X) X -= MAX_X;
            Y -= Input.GetAxis("Mouse Y") * (Sensitivity * Time.deltaTime);
            if (Y < MIN_Y) Y = MIN_Y;
            else if (Y > MAX_Y) Y = MAX_Y;

            transform.rotation = Quaternion.Euler(Y, X, 0.0f);

            //ensure these stay this way
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation * targetOrientation;


            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
        if (Input.GetKey(KeyCode.S)) {
            setModeFly(true);
           
            Vector3 dir = defaultCamera.transform.forward * -1;
            transform.Translate(dir* flySpeed , Space.World);
        }
        else if (Input.GetKey(KeyCode.W)) {
            setModeFly(true);
           
            Vector3 dir = defaultCamera.transform.forward;
            transform.Translate(dir * flySpeed, Space.World);
        }
        else if (Input.GetKey(KeyCode.A)) {
            setModeFly(true);
           
            Vector3 dir = defaultCamera.transform.right * -1;
            transform.Translate(dir * flySpeed , Space.World);
       
        }
        else if (Input.GetKey(KeyCode.D)) {
            setModeFly(true);
           
            Vector3 dir = defaultCamera.transform.right ;
            transform.Translate(dir * flySpeed , Space.World);
        }
        else  if (Input.GetKey(KeyCode.E)) {
            setModeFly(true);
            transform.Translate(Vector3.up * flySpeed * 0.5f, Space.World);
        }
        else if (Input.GetKey(KeyCode.Q)) {
            setModeFly(true);
            transform.Translate(-Vector3.up * flySpeed * 0.5f, Space.World);
        }
        else if (Input.GetKey(KeyCode.Escape)  || Input.GetKey(KeyCode.X) ||
            (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.C)) ) {
            Application.Quit();
        }
        else if (Input.GetKey(KeyCode.H)) {
            setModeFly(false);
            int delta = Time.frameCount- lastChange;
            if (delta > 10) {
                GameObject panel = Find(GameObject.FindGameObjectWithTag("Canvas"), "HelpGroup");
                if (panel != null) {
                    panel.SetActive(!panel.activeSelf);

                }
                else p("Coulld not find HelpGroup");
                lastChange = Time.frameCount;
            }
           

        }
        else if (Input.GetKey(KeyCode.P)) {
            if (Time.timeScale==0) Time.timeScale = timeScale;
            else Time.timeScale = 0;
            p("Pause pressed, time scale is "+Time.timeScale);
            setModeFly(false);
        }

        else if (Input.GetKey(KeyCode.F1)
            ||  Input.GetKey(KeyCode.Space) 
              || Input.GetKey(KeyCode.Alpha1) 
                  || Input.GetKey(KeyCode.F12) ||
            Input.GetKey(KeyCode.F2)) {
            setModeFly(false);
            
           
            //p("User entered ESC or F1, F2 or X (mode fly =false)");
        }
    }
    private void setModeFly(bool fly) {
        if (modeFly == fly) return;
        modeFly = fly;
      
        p("mode fly "+fly);
        if (fly == false) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            transform.position = startingTransform.position;
            transform.rotation = startingTransform.rotation;
        }

        canvas.gameObject.SetActive( !modeFly);
    }
    private void p(string s) {
        Debug.Log("CameraFly: " + s);
    }
}
