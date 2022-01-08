using System;
using UnityEngine;


/*
 * https://chemistry.tcd.ie/staff/people/sd/lectures/MO_lecture_course-2.pdf
 http://laussy.org/wiki/WLP_XI/HWavefunctions
 */
public class Orbital {
	int n;
	int l;
	int m;

	static double PI = Math.PI;
	static String[] names = { "s", "p", "d", "f", "g", "h", "i", "j" };
	String name;

	public Orbital(int n, int l,int m) {
		this.n = n;
		this.m =m;
		this.l = Math.Min(l, n-1);
		name = n + names[l] + m;

		p("Orbital " + toString());
	}
	public String toString() {
		return n + "/" + l+"/" + m+": "+name+" ";
    }
	private void p(string s) {
		Debug.Log("Orbital: " + s);
	}
	// Function of n and l
	public double R(double r, bool debug) { 
		// 1 s
		double v = 0;
		if (l == 0) {  // S Orbitals
			if (debug) p("Computing R S for " + toString() + ":r=" + r);
			if (n == 1) v  = 2 * e(-r);
			else if (n == 2) {
				v = 1 / (2 * sqrt(2)) * (2 - r) * e(-r / 2);
			}
			else if (n == 3) {
				v = 1 / (9 * sqrt(3)) * (6 - 4 * r + pow2(2 / 3*r))*e(-r / 3);
			}
		}
		else if (l == 1) { // P Orbitals
			if (debug) p("Computing R P for " + toString() + ":r=" + r);
			if (n ==2) {
				v = 1 / (2 * sqrt(6)) * r * e(-r / 2);
            }
			else if(n==3) {
				v = 1 / (9 * sqrt(6)) * (4 - 2 * r / 3) * 2 * r / 3 * e(-2 * r / 3);
            }
		}
		else {
			if (debug) p("Don't know how to compute R  " + toString() + ":r=" + r);
		}
		return v;
    }
	
	// Function of l and m
	public double V(double phid, double thetad, bool debug) {
		double v = 0;
		double phi = phid / 180.0f * PI;
		double theta = thetad / 180.0f * PI;
		if (debug) p("Computing V for " + toString() + ":phid=" + phi+", theta="+thetad);
		if (l == 0) { // S Orbitals		
			
			v = 1 / (2 * sqrt(PI));
			if (debug) p("		S: vy="+v);
		}
		else if (l == 1) { // P Orbitals
			
			v = sqrt(3 / (4 * PI));
			if (m == -1) {
				v = v * c(theta);
            }
			else if (m==0) {
				v = v * s(theta) * c(phi);
			}
			else {
				v = v * s(theta) * s(phi);
			}
			if (debug) p("		P: vy=" + v);
		}
		else if(l ==2) { // D Orbitals
			if (debug) p("D orbital" + toString());
			v = 0.25;
			if (m == 0) {
				double ct = c(theta);
				v = v * sqrt(5 / PI) * 3 * ct * ct - 1;
			}
			else if (m == -1) {
				v = v * sqrt(15 / (2 * PI)) * c(theta) * s(theta) * c(phi);
            }
			else if (m == 1) {
				v = v * sqrt(15 / (2 * PI)) * c(theta) * s(theta) * s(phi);
			
			}
			else if (m == -2) {
				v = v * sqrt(15 / (2 * PI)) * s(theta) * s(theta) * s(phi);
			}
			else if (m == 2) {
				v = v * sqrt(15 / (2 * PI)) * s(theta) * s(theta) * s(phi);
			}
		}
		return v;
    }

// ===================== UTILITY ===================
	private double e(double x) {
		return Math.Exp(x);
	}
	private double sqrt(double x) {
		return Math.Sqrt(x);
	}
	public double pow2(double x) {
		return Math.Pow(x, 2);
	}
	public double c(double x) {
		return Math.Cos(x);

	}
	public double s(double x) {
		return Math.Sin(x);
	}
	public double toR(double x, double y, double z) {
		return sqrt(x * x + y * y + z * z);
    }
	public double toTheta(double x, double y, double z) {
		return (double)Math.Acos(-Math.Abs(z) / toR(x, y, z));// * (z < 0 ? -1 : 1);

	}
	public double toPhi(double x, double y, double z) {
		return (double)Math.Acos(x / Math.Sqrt(x * x + y * y)) * (y < 0 ? -1 : 1);
	}
	public double toX(double r, double phid, double thetad) {
		double phi = phid / 180.0f * PI;
		double theta = thetad / 180.0f * PI;
		return r * s(theta) * c(phi);
    }
	public double toY(double r, double phid, double thetad) {
		double phi = phid / 180.0f * PI;
		double theta = thetad / 180.0f * PI;
		return r * s(theta) * s(phi);
	}
	public double toZ(double r, double phid, double thetad) {
		
		double theta = thetad / 180.0f * PI;
		return r * c(theta) ;
	}
}
