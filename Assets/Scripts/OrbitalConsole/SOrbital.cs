using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
	public class SOrbital : Orbital {
	public SOrbital(BasisState bs) :base(bs) {
	
	}
	
	public override void computePoint(int r, int costh) {
		try {
			double v = dataR[r];
			funcr = reMult * v;
			funci = imMult * v;
		//	p("sorbital.computePoint mZero " + r + ", " + costh + ": dataR[r]=" + dataR[r] );
			}
		catch (Exception e) {
				funcr = funci = 0;
				p("sorbital.computePoint bad " + r + " " + costh);
			}
	}
};
}
