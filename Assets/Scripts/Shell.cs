using System;
using UnityEngine;

public class Shell
{

	public GameObject shell;
	public float radius;
	public Vector3 scale;
	public float fraction;
	public float minRadius;
	public float maxRadius;

		
	/*
	  GameObject shell = Instantiate(spherePrefab, sphereOrigin, Quaternion.identity);
            shell.transform.parent = this.spheregroup.transform;
            //   shell.transform.rotation = transform.rotation;
            Vector3 sc = new Vector3(2 * r, 2 * r, 2 * r);
            shell.transform.localScale = sc;
            shells[nr] = shell.transform;           
            shellScales[nr] = new Vector3(2 * r, 2 * r, 2 * r);
	*/
	public Shell(float r, float minRadius, float maxRadius,  GameObject shellObject, float pInfluence) {
		scale =  new Vector3( r,  r,  r);
		this.radius = r;
		this.minRadius = minRadius;
		this.maxRadius = maxRadius;
		//float delta = maxRadius - minRadius;
		this.shell = shellObject;
		this.fraction = (float)Math.Min(1.0, Math.Max(0, (maxRadius - r) / maxRadius));
		// minRadius should be 1
		float dr = r - minRadius;
		float f = 0.25f / pInfluence;
		this.fraction = 1.0f / (1.0f + dr * dr* f);
		Vector3 sc = new Vector3( r,  r,  r);
		shell.transform.localScale = sc;
	//	p("Fraction of shell  r=" + r + " is " + fraction + ", min=" + minRadius + ", max=" + maxRadius+ ", pInfluence="+ pInfluence);

	}
	private void p(string s) {
		Debug.Log("Shell: " + s);
	}
	public bool isSmallest() {
		return radius == minRadius;
    }
	public bool isLargest() {
		return radius == maxRadius;
	}
}
