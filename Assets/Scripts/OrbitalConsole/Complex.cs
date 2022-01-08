using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
   public class Complex {

		public double re, im, mag, phase;
		public Complex() { re = im = mag = phase = 0; }
		public Complex(double r, double i) {
			set(r, i);
		}
		public double magSquared() { return mag * mag; }
		public void set(double aa, double bb) {
			re = aa; im = bb;
			setMagPhase();
		}
		public void set(double aa) {
			re = aa; im = 0;
			setMagPhase();
		}
		public void set(Complex c) {
			re = c.re;
			im = c.im;
			mag = c.mag;
			phase = c.phase;
		}
		public void add(double r) {
			re += r;
			setMagPhase();
		}
		public void add(double r, double i) {
			re += r; im += i;
			setMagPhase();
		}
		public void add(Complex c) {
			re += c.re;
			im += c.im;
			setMagPhase();
		}
		public void square() {
			set(re * re - im * im, 2 * re * im);
		}
		public void mult(double c, double d) {
			set(re * c - im * d, re * d + im * c);
		}
		public void mult(double c) {
			re *= c; im *= c;
			mag *= c;
		}
		public void mult(Complex c) {
			mult(c.re, c.im);
		}
		public void setMagPhase() {
			mag = Math.Sqrt(re * re + im * im);
			phase = Math.Atan2(im, re);
		}
		public void setMagPhase(double m, double ph) {
			mag = m;
			phase = ph;
			re = m * Math.Cos(ph);
			im = m * Math.Sin(ph);
		}
		public void rotate(double a) {
			setMagPhase(mag, (phase + a) % (2 * Math.PI));
		}
		public void conjugate() {
			im = -im;
			phase = -phase;
		}
	}
}
