using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace OrbitalConsole {
    public class Rot {
        public double[] rotmatrix;
        public Rot() {
            rotmatrix = new double[9];
            rotmatrix[0] = rotmatrix[4] = rotmatrix[8] = 1;
            rotate(0, -Const.pi / 2);
        }

        // multiply rotation matrix by rotations through angle1 and angle2
        public void rotate(double angle1, double angle2) {
            double r1cos = Math.Cos(angle1);
            double r1sin = Math.Sin(angle1);
            double r2cos = Math.Cos(angle2);
            double r2sin = Math.Sin(angle2);
            double[] rotm2 = new double[9];

            // angle1 is angle about y axis, angle2 is angle about x axis
            rotm2[0] = r1cos;
            rotm2[1] = -r1sin * r2sin;
            rotm2[2] = r2cos * r1sin;

            rotm2[3] = 0;
            rotm2[4] = r2cos;
            rotm2[5] = r2sin;

            rotm2[6] = -r1sin;
            rotm2[7] = -r1cos * r2sin;
            rotm2[8] = r1cos * r2cos;

            double[] rotm1 = rotmatrix;
            rotmatrix = new double[9];

            int i, j, k;
            for (j = 0; j != 3; j++) {
                for (i = 0; i != 3; i++) {
                    double v = 0;
                    for (k = 0; k != 3; k++) {
                        v += rotm1[k + j * 3] * rotm2[i + k * 3];
                    }
                    rotmatrix[i + j * 3] = v;
                }
            }
        }

    }
}
