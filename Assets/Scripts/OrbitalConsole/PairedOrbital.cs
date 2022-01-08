using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
    public class PairedOrbital : Orbital {


        public BasisState negstate;
        public double f1, f2, f3, f4;

        public PairedOrbital(BasisState bs) : base(bs) {
           
            negstate = model.getState(bs.nr, bs.l, -bs.m);
        }

       
    public override void setupFrame(double mult) {
            double a = state.re * mult;
            double b = state.im * mult;
            double c = negstate.re * mult;
            double d = negstate.im * mult;
            double mphase = Math.Pow(-1, m);
            a *= mphase;
            b *= mphase;
            f1 = (a + c);
            f2 = (d - b);
            f3 = (b + d);
            f4 = (a - c);
        }

       
    public override void computePoint(int r, int costh) {
            try {
                double q = dataR[r] * dataTh[costh];
                double phiValR = dataPhiR[Utils.phiSector, Utils.phiIndex];
                double phiValI = dataPhiI[Utils.phiSector, Utils.phiIndex];
                funcr = q * (f1 * phiValR + f2 * phiValI);
                funci = q * (f3 * phiValR + f4 * phiValI);
            //    p("computePoint Paired " + r + ", " + costh + ": dataR[r]=" + dataR[r] + ", dataTh[costh]=" + dataTh[costh] + ", phiValR=" + phiValR + ", phiValI=" + phiValI);
            }
            catch (Exception e) {
                funcr = funci = 0;
                p("paired orbital, compute Point : bad " + r + " " + costh);
            }
        }
    }
}
