using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinCube : MonoBehaviour
{
    public Vector3 RotateDirection;
    public float rotateSpeed;

    public Transform cube;
    private Vector3 AxisRotation = Vector3.right;

    public GameObject block;
    public int Width = 3;
    public int Height = 3;
    public int Depth = 3;

   
    private int dgrid = 2;

   
    void Start() {
        // Start is called before the first frame update
      
        cube = transform.Find("d6").gameObject.transform;
       

    }
   
    private GameObject get(string name) {
       
        GameObject res= GameObject.Find(name);
    
        return res;
    }
    private void p(string s) {
        Debug.Log("SpinCubes: "+s);
    }
    private void SetupSpring(SpringJoint joint, bool isDie) {
        joint.autoConfigureConnectedAnchor = true;
        joint.minDistance = 0.5f;
        joint.maxDistance = 10f;
        joint.spring = 10f;
        joint.damper = 1.0f;
        
        if (isDie) {
            joint.spring = 100f;
            joint.minDistance = 0.1f;
        }
    }
   
    // Update is called once per frame
    void Update() {
        cube.transform.Rotate(RotateDirection, rotateSpeed);
      
        transform.Rotate(AxisRotation, rotateSpeed/2.0f);

    }
}
