namespace OrbitalConsole {
    public class AlternateBasis {
		public DerivedState[] altStates;
		public int altStateCount;
		public bool active;
		public int n, l;
		public bool xAxis;
		public Model model = Model.model;
		public AlternateBasis() {
			Model.model.basisList[model.basisCount++] = this;
		}
		public void convertDerivedToBasis() { convertDerivedToBasis(true); }
		public void convertDerivedToBasis(bool clear) {
			int i, j;
			if (clear)
				for (i = 0; i != model.stateCount; i++)
					model.states[i].set(0);
			Complex c = new Complex();
			for (i = 0; i != altStateCount; i++) {
				DerivedState ds = altStates[i];
				for (j = 0; j != ds.count; j++) {
					c.set(ds.coefs[j]);
					c.conjugate();
					c.mult(ds);
					ds.bstates[j].add(c);
				}
			}
			double maxm = 0;
			for (i = 0; i != model.stateCount; i++)
				if (model.states[i].mag > maxm)
					maxm = model.states[i].mag;
			if (maxm > 1) {
				double mult = 1 / maxm;
				for (i = 0; i != model.stateCount; i++)
					model.states[i].mult(mult);
			}
		}

		public void convertBasisToDerived() {
			int i, j;
			Complex c1 = new Complex();
			Complex c2 = new Complex();
			double maxm = 0;
			for (i = 0; i != altStateCount; i++) {
				DerivedState ds = altStates[i];
				c1.set(0);
				try {
					for (j = 0; j != ds.count; j++) {
						c2.set(ds.coefs[j]);
						c2.mult(ds.bstates[j]);
						c1.add(c2);
					}
				}
				catch (System.Exception e) {
					
				}
				if (c1.mag < Const.epsilon)
					c1.set(0);
				ds.set(c1);
				if (c1.mag > maxm)
					maxm = ds.mag;
			}
			if (maxm > 1) {
				double mult = 1 / maxm;
				for (i = 0; i != altStateCount; i++)
					altStates[i].mult(mult);
			}
		}
	}
}