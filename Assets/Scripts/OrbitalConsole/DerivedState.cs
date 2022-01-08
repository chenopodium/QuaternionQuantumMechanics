using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
	public class DerivedState : State {
		public int count, m, l, nr, n, nx, ny, nz;
		public AlternateBasis basis;
		public String text;
		public BasisState[] bstates;
		public Complex[] coefs;
		public void convertDerivedToBasis() { basis.convertDerivedToBasis(); }
		public void convertBasisToDerived() { basis.convertBasisToDerived(); }
		public void setBasisActive() { basis.active = true; }
		public override String getText() { return text; }
	}
}
