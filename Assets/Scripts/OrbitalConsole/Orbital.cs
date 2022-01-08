using System;

namespace OrbitalConsole {
    public abstract class Orbital {
        public BasisState state;
        public int n, nr, l, m;
        public double reMult, imMult;
        public double funcr, funci;
        public double[] dataR, dataTh;
        public double [,] dataPhiR, dataPhiI;
        public int dshalf;
        public double brightnessCache;
        public int distmult = 4;

        public Model model = Model.model;

        public Model Model { get => model; set => model = value; }

        public Orbital(BasisState bs) {
            nr = bs.nr;
            l = bs.l;
            m = bs.m;
            n = nr * 2 + l;
            state = bs;
        }

        public virtual void setupFrame(double mult) {
            reMult = state.re * mult;
            imMult = state.im * mult;
        }

        public double getBoundRadius(double bright) {
            int i;
            int outer = 1;

            // we need to divide the spherical harmonic norm out of
            // dataR[] to get just the radial function.  (The spherical
            // norm gets multiplied into dataR[] for efficiency.)
            int mpos = (m < 0) ? -m : m;
            double norm1 = 1 / sphericalNorm(l, mpos);
            //norm1 *= maxThData;
            norm1 *= norm1;
            norm1 *= bright;

            for (i = 0; i != Const.dataSize; i++) { // XXX
                double v = dataR[i] * dataR[i] * norm1;
                if (v > 32) {
                    outer = i;
                }
            }
            //System.out.println(maxThData + " " + outer);
            return outer / (Const.dataSize / 2.0);
        }

        public double getScaleRadius() {
            // set scale by solving equation Veff(r) = E, assuming m=0
            // r^2/2 = (n+3/2)
            return Math.Sqrt(2 * (n + 1.5));
        }

        double hypser(double a, double c, double z) {
            int n;
            double fac = 1;
            double result = 1;
            for (n = 1; n <= 1000; n++) {
                fac *= a * z / ((double)n * c);
                //System.out.print("fac " + n + " " + fac + " " + z + "\n");
                if (fac == 0) {
                    return result;
                }
                result += fac;
                a++;
                c++;
            }
            //System.out.print("convergence failure in hypser\n");
            return 0;
        }

        double plgndr(int l, int m, double x) {
            double fact, pll = 0, pmm, pmmp1, somx2;
            int i, ll;

            if (m < 0 || m > l || Math.Abs(x) > 1.0) {
                //System.out.print("bad arguments in plgndr\n");
            }
            pmm = 1.0;
            if (m > 0) {
                somx2 = Math.Sqrt((1.0 - x) * (1.0 + x));
                fact = 1.0;
                for (i = 1; i <= m; i++) {
                    pmm *= -fact * somx2;
                    fact += 2.0;
                }
            }
            if (l == m) {
                return pmm;
            }
            else {
                pmmp1 = x * (2 * m + 1) * pmm;
                if (l == (m + 1)) {
                    return pmmp1;
                }
                else {
                    for (ll = (m + 2); ll <= l; ll++) {
                        pll = (x * (2 * ll - 1) * pmmp1 - (ll + m - 1) * pmm) / (ll - m);
                        pmm = pmmp1;
                        pmmp1 = pll;
                    }
                    return pll;
                }
            }
        }
        public static void p(String s) {
            Console.WriteLine("Orbital: " + s);
        }
        public void precompute() {
            int x, y;
            
            dshalf = Const.dataSize / 2;
            double mult = Model.scale / 2500.0;

            int mpos = (m < 0) ? -m : m;
            double lgcorrect = Math.Pow(-1, m);
            double norm = radialNorm(nr, l) * sphericalNorm(l, mpos);

  //          p("Precompute, dataSize is " + Const.dataSize+", scale is "+Model.scale);

  //          p("Precompute: norm is " + norm);
            dataR = new double[Const.dataSize];
            for (x = 0; x != Const.dataSize; x++) {
                double r = x * Const.resadj * mult + .00000001;
                double rl = Math.Pow(r, l) * norm;
                dataR[x] = Math.Exp(-r * r / 2) * rl
                        * hypser(-nr, l + 1.5, r * r);
            }

            if (l > 0) {
                dataTh = new double[Const.dataSize + 1];
                for (x = 0; x != Const.dataSize + 1; x++) {
                    double th = (x - dshalf) / (double)dshalf;
                    // we multiply in lgcorrect because plgndr() uses a
                    // different sign convention than Bransden
                    dataTh[x] = lgcorrect * plgndr(l, mpos, th);
                }
            }

            if (m != 0) {
                dataPhiR = new double[8,Const.dataSize + 1];
                dataPhiI = new double[8,Const.dataSize + 1];
                for (x = 0; x != 8; x++) {
                    for (y = 0; y <= Const.dataSize; y++) {
                        double phi = x * Const.pi / 4 + y * (Const.pi / 4) / Const.dataSize;
                        dataPhiR[x,y] = Math.Cos(phi * mpos);
                        dataPhiI[x,y] = Math.Sin(phi * mpos);
                    }
                }
            }

            brightnessCache = 0;
        }

        public double getBrightness() {
            if (brightnessCache != 0) {
                return brightnessCache;
            }
            int x;
            double avgsq = 0;
            double vol = 0;

            // we need to divide the spherical harmonic norm out of
            // dataR[] to get just the radial function.  (The spherical
            // norm gets multiplied into dataR[] for efficiency.)
            int mpos = (m < 0) ? -m : m;
            double norm1 = 1 / sphericalNorm(l, mpos);

            for (x = 0; x != Const.dataSize; x++) {
                double val = dataR[x] * norm1;
                val *= val;
                avgsq += val * val * x * x;
                vol += x * x;
            }

            brightnessCache = avgsq / vol;
            return brightnessCache;
        }

        public double radialNorm(int nr, int l) {
            return Math.Sqrt(2 * factorial(nr) / fracfactorial(l + nr + .5))
                    * pochhammer(l + 1.5, nr) / factorial(nr);
        }

        public double sphericalNorm(int l, int m) {
            return Math.Sqrt((2 * l + 1) * factorial(l - m)
                    / (4 * Const.pi * factorial(l + m)));
        }

        public double factorial(int f) {
            double res = 1;
            while (f > 1) {
                res *= f--;
            }
            return res;
        }

        public double fracfactorial(double f) {
            double res = Math.Sqrt(Const.pi);
            while (f > 0) {
                res *= f--;
            }
            return res;
        }

        public double pochhammer(double f, int n) {
            double res = 1;
            for (; n > 0; n--) {
                res *= f;
                f += 1;
            }
            return res;
        }

        public abstract void computePoint(int r, int costh);
    }
    
}