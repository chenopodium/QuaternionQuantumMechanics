using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
    public class Utils {

        public static int phiIndex, phiSector;

        public static void calcPhiComponent(double x, double y) {
            phiSector = 0;
            double val = 0;
            if (x == 0 && y == 0) {
                phiSector = 0;
                phiIndex = 0;
                return;
            }
            if (y >= 0) {
                if (x >= 0) {
                    if (x >= y) {
                        phiSector = 0;
                        val = y / x;
                    }
                    else {
                        phiSector = 1;
                        val = 1 - x / y;
                    }
                }
                else {
                    if (-x <= y) {
                        phiSector = 2;
                        val = -x / y;
                    }
                    else {
                        phiSector = 3;
                        val = 1 + y / x;
                    }
                }
            }
            else {
                if (x <= 0) {
                    if (y >= x) {
                        phiSector = 4;
                        val = y / x;
                    }
                    else {
                        phiSector = 5;
                        val = 1 - x / y;
                    }
                }
                else {
                    if (-y >= x) {
                        phiSector = 6;
                        val = -x / y;
                    }
                    else {
                        phiSector = 7;
                        val = 1 + y / x;
                    }
                }
            }
            phiIndex = (int)(val * Const.dataSize);
        }
    }
}
