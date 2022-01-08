using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalConsole {
	public abstract class State : Complex {
		public double elevel;
		public void convertDerivedToBasis() { }
		public void convertBasisToDerived() { }
		public void setBasisActive() { }
		public abstract String getText();
	}
}
