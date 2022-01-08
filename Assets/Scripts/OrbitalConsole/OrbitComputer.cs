using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
    class OrbitComputer {

        public double resolution = 2000;
        public int sampleCount;
        public double bestBrightness =1;
        public int[] sampleMult;
        public double scale = 75;
        public Model model;
        //public double[,,] func;
        public int n;
        public int l;
        public int m;
        public int speed = 1;

        private double vmax;
        private double userBrightMult =1;

        public double globalTime;
        private double deltat;
        private double normmult;
        private Stopwatch stopwatch = new Stopwatch();

        public OrbitComputer() {
            n = 2;
            l = 1;
            m = 0;
            init();
        }
        public void Set(int n, int l, int m) {
            this.n = n;
            this.l = l;
            this.m = m;
            this.init();
        }
        public int getDataSize() {
            return Const.dataSize;
        }
        public String ToString() {
            return "n="+n+", l="+l+", m="+m;
        }
        private void init() {
           
            model = Model.model;
            model.setupStates();
            model.getState(n, l, m).set(1, 0);
            model.createOrbitals();
            Const.dataSize = (int)resolution;
            Const.resadj = 50.0 / Const.dataSize;
            model.precomputeAll();
            //      func = new double[gridSizeX,gridSizeY,3];

            this.setupSimpson();
            setScale();
           
            advanceTime();
            testCompute("init");
        }
        public void advanceTime() {
           
            deltat = speed * (0.1 / 16);
            globalTime += deltat;
          
            model.normalize();
            normmult = 0;
            double normmult2 = 0;
            double norm = model.updatePhases(deltat);
            if (norm == 0) {
                normmult2 = 0;
            }
            else {
                normmult2 = 1 / norm;
            }
            setBrightness(normmult2);
            normmult = Math.Sqrt(normmult2);
            initCompute();
        }
        public void initCompute() {
            model.getState(n, l, m).set(1, 0);


            AlternateBasis skipBasis = null;

            //   p("basis count: " + model.basisCount);
            int i;
            for (i = 0; i != model.basisCount; i++) {
                AlternateBasis basis = model.basisList[i];
                if (basis != skipBasis && basis.active) {
                    basis.convertBasisToDerived();
                }
            }

            for (i = 0; i != model.orbCount; i++) {
                //      p("Setup frame " + i+ ", normmult="+normmult);
                model.orbitals[i].setupFrame(normmult);
            }

            vmax = 0;

        }
        public void testCompute(String when) {
            p("TEST " + when+", n="+n+", l="+l+", m="+m);
           
            double size = Const.dataSize/2.0;
            double delta = size/10;
            stopwatch.Start();
            int count = 0;
            OrbValue res = null;
            for (double xx = -size; xx < size; xx += delta) {
                for (double yy = -size; yy < size; yy += delta) {
                    for (double zz = -size; zz < size; zz += delta) {

                        res= computValue(xx, yy, zz, false);
                        count++;
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;

            Console.WriteLine("Elapsed Time is {0:00}h:{1:00}min:{2:00}s.{3}ms for "+count+" points",
                            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            p("Last value: " + res.toString());
            // xx=-876.6546295756763, yy=-1684.9381090427098, zz=623.2665795140357 
            // computValue(-876.6546295756763, -1684.9381090427098, 623.2665795140357);
        }
        public void setScale() {
            
            int i;
            double outer = 0;
            for (i = 0; i != model.orbCount; i++) {
                Orbital orb = model.orbitals[i];
                double r = orb.getScaleRadius();
                if (r > outer) {
                    outer = r;
                }
            }
           
            model.scale = scale;
            model.precomputeAll();
        //    p("Setting model scale to " + scale + ", precomputing all");
            
        }

        public void setBrightness(double normmult) {
            int i;
            double avg = 0;
            double totn = 0;
            double minavg = 1e30;
            for (i = 0; i != model.orbCount; i++) {
                Orbital orb = model.orbitals[i];
                double ass = orb.getBrightness();
                if (ass < minavg) {
                    minavg = ass;
                }
                BasisState st = orb.state;
                double n = st.magSquared() * normmult;
                if (orb.state.m != 0) {
                    n += model.getState(st.nr, st.l, -st.m).magSquared() * normmult;
                }
                totn += n;
                avg += n * ass;
            }
            bestBrightness = 113.9 / (Math.Sqrt(minavg) * totn);
            double mult = bestBrightness * userBrightMult;
            int bvalue = (int)(Math.Log(mult) * 100.0);
    //        p("Brightness bvalue=" + bvalue);
        }

        public OrbValue computValue(double xx, double yy, double zz, bool show) {

            OrbValue res = new OrbValue(xx, yy, zz);

            int dshalf = Const.dataSize / 2;

            Utils.calcPhiComponent(xx, yy);

            // find grid element that contains sampled point
            double r = Math.Sqrt(xx * xx + yy * yy + zz * zz);
            double costh = zz / r;
            int ri = (int)r;
            int costhi = (int)(costh * dshalf + dshalf);
            double fr = 0, fi = 0;

            for (int oi = 0; oi != model.orbCount; oi++) {
                Orbital oo = model.orbitals[oi];
                oo.computePoint(ri, costhi);
                fr += oo.funcr;
                fi += oo.funci;
          //      p("TEST: Orbital " + oi + " of " + model.orbCount + ", fr=" + fr + ", fi=" + fi);
            }

            double fv = fr * fr + fi * fi;
            res.fv = fv;
            fv *= sampleMult[0];
           
            vmax = Math.Max(vmax, fv);
           if (show) p("computeValue: " + (int)xx + "/" + (int)yy + "/" + (int)zz + ", sector=" + Utils.phiSector + " phiInd=" + Utils.phiIndex + " r=" + ri + ", costh=" + costh + ", costhi=" + costhi + ", fv=" + fv + ", fi=" + fi + ", fr=" + fr + ", vmax=" + vmax);
            //   p("              " + res.toString());
            return res;

        }
        public class OrbValue {

            public double xx;
            public double yy;
            public double zz;
            public double simpr = 0;
            public double simpg = 0;
            public double simpb = 0;
            public double fr;
            public double fi;
            public double fv;

            public OrbValue(double xx, double yy, double zz) {
                this.xx = xx;
                this.yy = yy;
                this.zz = zz;
            }

            public String toString() {
                return xx + "/" + yy + "/" + zz + ": fr=" + fr + ", fi=" + fi + ", fv=" + fv;
            }
        }
        private void setupSimpson() {
            sampleCount = 9; // 15;

            // generate table of sample multipliers for efficient Simpson's rule
            sampleMult = new int[sampleCount];
            int i;
            for (i = 1; i < sampleCount; i += 2) {
                sampleMult[i] = 4;
                sampleMult[i + 1] = 2;
            }
            sampleMult[0] = sampleMult[sampleCount - 1] = 1;
        }
        public static void p(String s) {
            Console.WriteLine("OrbitComputer: " + s);
        }
    }
  
}
