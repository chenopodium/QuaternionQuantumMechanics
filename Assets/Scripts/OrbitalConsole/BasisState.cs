using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole
{
    public class BasisState : State {
        public int nr, l, m, n;
        public Orbital orb;

        public override String getText() {
            return "n = " + n + ", nr = " + nr + ", l = " + l + ", m = " + m;
        }
    }
}
