using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
	public class MZeroOrbital : Orbital {
	public MZeroOrbital(BasisState bs) : base(bs){
		
	}

		public override void computePoint(int r, int costh) {
			
			try {
				double v = dataR[r] * dataTh[costh];

			//	p("mzero.computePoint  " + r + ", " + costh+": dataR[r]="+dataR[r]+", dataTh[costh]="+dataTh[costh]);
				funcr = v * reMult;
				funci = v * imMult;
			}
			catch (Exception e) {
				funcr = funci = 0;
			//	p("mzero.computePoint bad r=" + r + " costh=" + costh+":"+e.Message);
			}
		}
	}
}
