using System;

namespace OrbitalConsole {
    public class Model {
        private Random random;
        public AlternateBasis[] basisList;
        public Orbital[] orbitals;
       
        public int basisCount;
        public int orbCount;
        // Phasor phasors[];
        //  int phasorCount;
        public BasisState[] states;
        public int stateCount;
        public AlternateBasis rectBasis, lxBasis, lyBasis;

        public double scale;

        public static Model model = new Model();
    

    public Model() {
        basisList = new AlternateBasis[17];
        random = new Random();
    }

    public bool higherStatesPresent() {
        int i;
        for (i = 0; i != model.stateCount; i++) {
            if (model.states[i].n > 4 && model.states[i].mag > 0) {
                return true;
            }
        }
        return false;
    }

    public void measureL(int axis) {
        if (higherStatesPresent()) {
            return;
        }
        int maxm = 4;
        int pad = 3;
        int ct = (maxm * 2 + 1) * pad;
        double[] ldata = new double[ct];
        int mid = ct / 2;

        normalize();
        AlternateBasis ab = null;
        switch (axis) {
            case 0:
                calcLxy(ab = lxBasis, ldata, ct, maxm, pad, true, false);
                break;
            case 1:
                calcLxy(ab = lyBasis, ldata, ct, maxm, pad, false, false);
                break;
            case 2:
                calcLz(ldata, ct, maxm, pad, false);
                break;
        }

        double n = random.NextDouble();
        int i = 0;
        int pickm = -100;
        for (i = -maxm; i <= maxm; i++) {
            double m = ldata[mid + i * pad];
            m *= m;
            n -= m;
            if (n < 0) {
                pickm = i;
                i = maxm;
                break;
            }
        }
        if (pickm == -100) {
            return;
        }
        switch (axis) {
            case 2: 
                for (i = 0; i != model.stateCount; i++) {
                    BasisState bs = model.states[i];
                    if (bs.m != pickm) {
                        bs.set(0);
                    }
                }
                break;

            default:
            for (i = 0; i != ab.altStateCount; i++) {
                DerivedState ds = ab.altStates[i];
                if (ds.m != pickm) {
                    ds.set(0);
                }
            }
            ab.convertDerivedToBasis();
           break;
            }
        maximize();
        model.createOrbitals();
    }

    public void calcLxy(AlternateBasis ab,
            double[] data, int count, int maxm, int pad, bool xAxis,
            bool square) {
        int i;
        int mid = count / 2;
        for (i = 0; i != count; i++) {
            data[i] = 0;
        }

        if (square) {
            mid = 1;
        }

        // convert to the basis
        ab.convertBasisToDerived();

        int j;
        double qq = 0;
        for (j = 0; j != ab.altStateCount; j++) {
            DerivedState ds = ab.altStates[j];
            if (square) {
                data[mid + ds.m * ds.m * pad] += ds.magSquared();
            }
            else {
                data[mid + ds.m * pad] += ds.magSquared();
            }
        }

        for (i = 0; i != count; i++) {
            data[i] = Math.Sqrt(data[i]);
        }
    }

    public void calcLz(double[] data, int count, int maxm, int pad, bool square) {
        int i;
        int mid = count / 2;
        for (i = 0; i != count; i++) {
            data[i] = 0;
        }
        if (square) {
            mid = 1;
        }
        for (i = 0; i != model.stateCount; i++) {
            BasisState bs = model.states[i];
            if (bs.l <= maxm) {
                if (square) {
                    data[mid + bs.m * bs.m * pad] += bs.magSquared();
                }
                else {
                    data[mid + bs.m * pad] += bs.magSquared();
                }
            }
        }
        for (i = 0; i != count; i++) {
            data[i] = Math.Sqrt(data[i]);
        }
    }

    public void rotateXY(double ang,  bool xAxis) {
        AlternateBasis ab = (xAxis) ? lxBasis : lyBasis;
        // convert to the basis
        ab.convertBasisToDerived();

        // rotate all the  model.states in the basis around the axis
        int j;
        for (j = 0; j != ab.altStateCount; j++) {
            DerivedState ds = ab.altStates[j];
            ds.rotate(ang * ds.m);
        }

        ab.convertDerivedToBasis();

        model.createOrbitals();

    }

    public void doScaled1Gaussian() {
        doClear();
        int i;
        for (i = 0; i != 8; i++) {
            model.getState(i, 0, 0).set(scaledGaussian[i]);
        }
        model.createOrbitals();
    }

    public void doScaled2Gaussian() {
        doClear();
        int i;
        for (i = 0; i != rectBasis.altStateCount; i++) {
            DerivedState ds = rectBasis.altStates[i];
            ds.set(0);
            if ((ds.nx & 1) > 0 || ds.ny > 0 || (ds.nz & 1) > 0) {
                continue;
            }
            int s = (ds.nx / 2) * 3 + ds.nz / 2;
            ds.set(scaled2Gaussian[s]);
        }
        rectBasis.convertDerivedToBasis();
        model.createOrbitals();
    }

    public void doRotatingGaussian() {
        doClear();
        int i;
        for (i = 0; i != rectBasis.altStateCount; i++) {
            DerivedState ds = rectBasis.altStates[i];
            ds.set(0);
            int s = ds.nx * 3 + ds.nz;
            if (ds.ny > 0 || ds.nx > 2 || ds.nz > 2) {
                continue;
            }
            ds.set(rotGaussianR[s], rotGaussianI[s]);
        }
        rectBasis.convertDerivedToBasis();
        model.createOrbitals();
    }

    public void doDispGaussian() {
        doClear();
        int i;
        model.rectBasis.convertBasisToDerived();
        for (i = 0; i != 5; i++) {
            model.rectBasis.altStates[i].set(model.movedGaussian[i], 0);
        }
        model.rectBasis.convertDerivedToBasis();
        model.createOrbitals();
    }

    public void doDispX110() {
        doClear();
        int i;
        for (i = 0; i != rectBasis.altStateCount; i++) {
            DerivedState ds = rectBasis.altStates[i];
            ds.set(0);
            if (ds.nz != 1 || ds.ny != 0) {
                continue;
            }
            ds.set(dispX110Array[ds.nx]);
        }
        rectBasis.convertDerivedToBasis();
        model.createOrbitals();
    }

    public void doDispZ110() {
        doClear();
        int i;
        for (i = 0; i != rectBasis.altStateCount; i++) {
            DerivedState ds = rectBasis.altStates[i];
            ds.set(0);
            //if (ds.nx != 1 || ds.ny != 1)
            if (ds.nx != 1 || ds.ny != 0) {
                continue;
            }
            ds.set(dispZ110Array[ds.nz]);
        }
        rectBasis.convertDerivedToBasis();
        model.createOrbitals();
    }

    public void normalize() {
        double norm = 0;
        int i;
        for (i = 0; i != model.stateCount; i++) {
            norm += model.states[i].magSquared();
        }
        if (norm == 0) {
            return;
        }
        double normmult = 1 / Math.Sqrt(norm);
        for (i = 0; i != model.stateCount; i++) {
            model.states[i].mult(normmult);
        }
    }

    public void rotateZ(double ang) {
        int i;
        for (i = 0; i != model.stateCount; i++) {
            BasisState bs = model.states[i];
            bs.rotate(ang * bs.m);
        }
    }

    public void maximize() {
        int i;
        double maxm = 0;
        for (i = 0; i != model.stateCount; i++) {
            if (model.states[i].mag > maxm) {
                maxm = model.states[i].mag;
            }
        }
        if (maxm == 0) {
            return;
        }
        for (i = 0; i != model.stateCount; i++) {
            model.states[i].mult(1 / maxm);
        }
    }

    public void doClear() {
        int x;
        for (x = 0; x != model.stateCount; x++) {
            model.states[x].set(0);
        }
    }

    public void precomputeAll() {
        int i;
        for (i = 0; i != model.orbitals.Length; i++) {
            Orbital orb = model.orbitals[i];
            orb.precompute();
        }
    }

    public void setupStates() {
        p("setupStates");
        stateCount = (Const.maxnr + 1) * ((Const.maxl + 1) * (Const.maxl + 1));
        int i;
        states = new BasisState[stateCount];
        int nr = 0;
        int l = 0;
        int m = 0;
        for (i = 0; i != stateCount; i++) {
            BasisState bs = states[i] = new BasisState();
            bs.elevel = 2 * nr + l + 1.5;
            bs.nr = nr;
            bs.l = l;
            bs.m = m;
            bs.n = 2 * nr + l;
            if (m < l) {
                m++;
            }
            else {
                l++;
                if (l <= Const.maxl) {
                    m = -l;
                }
                else {
                    nr++;
                    l = m = 0;
                }
            }
        }

        model = Model.model;
        basisCount = 0;

        rectBasis = setupRectBasis();
        lxBasis = initBasis(35, true);
        setupLBasis(lxBasis, 0, 0, true, l0Array);
        setupLBasis(lxBasis, 0, 1, true, l1xArray);
        setupLBasis(lxBasis, 0, 2, true, l2xArray);
        setupLBasis(lxBasis, 0, 3, true, l3xArray);
        setupLBasis(lxBasis, 0, 4, true, l4xArray);
        setupLBasis(lxBasis, 1, 0, true, l0Array);
        setupLBasis(lxBasis, 1, 1, true, l1xArray);
        setupLBasis(lxBasis, 1, 2, true, l2xArray);
        setupLBasis(lxBasis, 2, 0, true, l0Array);
        lyBasis = initBasis(35, true);
        setupLBasis(lyBasis, 0, 0, false, l0Array);
        setupLBasis(lyBasis, 0, 1, false, l1yArray);
        setupLBasis(lyBasis, 0, 2, false, l2yArray);
        setupLBasis(lyBasis, 0, 3, false, l3yArray);
        setupLBasis(lyBasis, 0, 4, false, l4yArray);
        setupLBasis(lyBasis, 1, 0, false, l0Array);
        setupLBasis(lyBasis, 1, 1, false, l1yArray);
        setupLBasis(lyBasis, 1, 2, false, l2yArray);
        setupLBasis(lyBasis, 2, 0, false, l0Array);
        p("setupStates done");
    }

    public AlternateBasis initBasis(int sct, bool xAxis) {
        AlternateBasis basis = new AlternateBasis();
        basis.xAxis = xAxis;
        basis.altStates = new DerivedState[sct];
        basis.altStateCount = 0;
        return basis;
    }

    public void setupLBasis(AlternateBasis basis,
            int nr, int l, bool xAxis, double[] arr) {
        String mtext = (xAxis) ? "mx" : "my";
        int i;
        int lct = l * 2 + 1;
        int ap = 0;
        for (i = 0; i != lct; i++) {
            int sn = basis.altStateCount++;
            DerivedState ds = basis.altStates[sn] = new DerivedState();
            ds.basis = basis;
            ds.count = lct;
            ds.bstates = new BasisState[lct];
            ds.coefs = new Complex[lct];
            ds.m = i - l;
            ds.l = l;
            ds.nr = nr;
            ds.n = 2 * nr + l;
            ds.elevel = 2 * nr + l + 1.5;
            int j;
            for (j = 0; j != lct; j++) {
                ds.bstates[j] = getState(nr, l, j - l);
                ds.coefs[j] = new Complex();
            }
            ds.text = "n = " + ds.n + ", nr = " + nr + ", l = " + l + ", "
                    + mtext + " = " + ds.m;
            for (j = 0; j != lct; j++) {
                ds.coefs[j].set(arr[ap], arr[ap + 1]);
                ap += 2;
            }
        }
    }

    public void createOrbitals() {
        int i;
        int newOrbCount = 0;
        bool newOrbitals = false;
        for (i = 0; i != stateCount; i++) {
            BasisState st = states[i];
            if (st.m == 0) {
                if (st.mag != 0) {
                    newOrbCount++;
                    if (st.orb == null) {
                        newOrbitals = true;
                    }
                }
                else if (st.orb != null) {
                    newOrbitals = true;
                }
            }
            else if (st.m > 0) {
                if (st.mag != 0 || model.getState(st.nr, st.l, -st.m).mag != 0) {
                    newOrbCount++;
                    if (st.orb == null) {
                        newOrbitals = true;
                    }
                }
                else if (st.orb != null) {
                    newOrbitals = true;
                }
            }
        }
        if (!newOrbitals) {
            return;
        }
        orbCount = newOrbCount;
        model.orbitals = new Orbital[orbCount];
        int oi = 0;
        for (i = 0; i != stateCount; i++) {
            BasisState st = states[i];
            if ((st.m == 0 && st.mag != 0)
                    || (st.m > 0 && (st.mag != 0
                    || model.getState(st.nr, st.l, -st.m).mag != 0))) {
                if (st.orb == null) {
                    Orbital orb;
                    if (st.l == 0) {
                        orb = new SOrbital(st);
                    }
                    else if (st.m == 0) {
                        orb = new MZeroOrbital(st);
                    }
                    else {
                        orb = new PairedOrbital(st);
                    }
                    orb.precompute();
                    st.orb = orb;
                }
                model.orbitals[oi++] = st.orb;
            }
            else {
                st.orb = null;
            }
        }
    }

    public double updatePhases(double deltat) {
        // update phases
        int i;
        double norm = 0;
        //    if(framecount==0) p("updateQuantumOsc3d: "+model.stateCount+" states");
        for (i = 0; i != model.stateCount; i++) {
            State st = model.states[i];

            if (st.mag < Const.epsilon) {
                st.set(0);
                //   if(framecount==0) p("mag < epsilon");
                continue;
            }
            if (deltat != 0) {

                st.rotate(-(st.elevel + Const.baseEnergy) * deltat);
            }
            norm += st.magSquared();
        }
        return norm;
    }

    public BasisState getState(int nr, int l, int m) {
        int pre_n_add = nr * (Const.maxl + 1) * (Const.maxl + 1);
        int pre_l_add = l * l;
     
        return states[pre_n_add + pre_l_add + l + m];
    }

    private void p(String s) {
        Console.WriteLine("Model: " + s);
    }

    public AlternateBasis setupRectBasis() {
        p("setup Rect Basis");
        int sct = 35;
        AlternateBasis basis = new AlternateBasis();
        basis.altStates = new DerivedState[sct];
        basis.altStateCount = sct;
        int i;
        int nx = 0, ny = 0, nz = 0;
        int ap = 0;
        for (i = 0; i != sct; i++) {
            int n = nx + ny + nz;
            int n21 = n / 2 + 1;
            int l = ((n & 1) == 0) ? 0 : 1;
            int nr = n / 2;
            int m = -l;
            DerivedState ds = basis.altStates[i] = new DerivedState();
            ds.basis = basis;
            ds.count = (l == 0) ? 2 * n21 * n21 - n21 : 2 * n21 * n21 + n21;
            ds.bstates = new BasisState[sct];
            ds.coefs = new Complex[sct];
            ds.text = "nx = " + nx + ", ny = " + ny + ", nz = " + nz;
            ds.nx = nx;
            ds.ny = ny;
            ds.nz = nz;
            ds.n = n;
            ds.elevel = 2 * nr + l + 1.5;
            int j;
            for (j = 0; j != ds.count; j++) {
                ds.bstates[j] = getState(nr, l, m);
                ds.coefs[j]
                        = new Complex(rectArrayR[ap], rectArrayI[ap]);
                ap++;
                if (m++ == l) {
                    l += 2;
                    nr--;
                    m = -l;
                }
            }
            if (i == sct - 1) {
                break;
            }
            do {
                nz++;
                if (nz > 4) {
                    nz = 0;
                    nx++;
                    if (nx > 4) {
                        nx = 0;
                        ny++;
                    }
                }
            } while (nx + ny + nz > 4);
        }
        return basis;
    }

    public void measureE() {
        normalize();
        double n = random.NextDouble();
        int i = 0;
        int picki = -1;
        for (i = 0; i != model.stateCount; i++) {
            double m = model.states[i].magSquared();
            n -= m;
            if (n < 0) {
                picki = i;
                i = model.stateCount;
                break;
            }
        }
        if (picki == -1) {
            return;
        }
        for (i = 0; i != model.stateCount; i++) {
            State st = model.states[i];
            if (st.elevel != model.states[picki].elevel) {
                st.set(0);
            }
        }
        normalize();
    }

    double[] l0Array = { 1, 0 };
    double[] l1xArray = {.5, 0, -Const.root2inv, 0, .5, 0, Const.root2inv, 0, 0, 0,
        -Const.root2inv, 0, .5, 0, Const.root2inv, 0, .5, 0};
    double[] l1yArray = {.5, 0, 0, -Const.root2inv, -.5, 0, 0, -Const.root2inv,
        0, 0, 0, -Const.root2inv, .5, 0, 0, Const.root2inv, -.5, 0};
    static  double root6by4 = .61237243569579452454;
    double[] l2xArray= {
        1 / 4.0, 0, -1 / 2.0, 0, root6by4, 0, -1 / 2.0, 0, 1 / 4.0, 0,
        -.5, 0, .5, 0, 0, 0, -.5, 0, .5, 0,
        root6by4, 0, 0, 0, -.5, 0, 0, 0, root6by4, 0,
        -.5, 0, -.5, 0, 0, 0, .5, 0, .5, 0,
        1 / 4.0, 0, 1 / 2.0, 0, root6by4, 0, 1 / 2.0, 0, 1 / 4.0, 0
    };
    double[] l2yArray = {
        1 / 4.0, 0, 0, -1 / 2.0, -root6by4, 0, 0, 1 / 2.0, 1 / 4.0, 0,
        -.5, 0, 0, .5, 0, 0, 0, .5, .5, 0,
        -root6by4, 0, 0, 0, -.5, 0, 0, 0, -root6by4, 0,
        -.5, 0, 0, -.5, 0, 0, 0, -.5, .5, 0,
        1 / 4.0, 0, 0, 1 / 2.0, -root6by4, 0, 0, -1 / 2.0, 1 / 4.0, 0
    };
    double[] l3xArray = {
        0.125, 0, -0.306186, 0, 0.484123, 0, -0.559017, 0,
        0.484123, 0, -0.306186, 0, 0.125, 0,
        -0.306186, 0, 0.5, 0, -0.395285, 0, 0.0, 0,
        0.395285, 0, -0.5, 0, 0.306186, 0,
        0.484123, 0, -0.395285, 0, -0.125, 0, 0.433013, 0,
        -0.125, 0, -0.395285, 0, 0.4841230, 0,
        0.559017, 0, 0.0, 0, -0.433013, 0, 0.0, 0,
        0.433013, 0, 0.0, 0, -0.559017, 0,
        0.484123, 0, 0.395285, 0, -0.125, 0, -0.433013, 0,
        -0.125, 0, 0.395285, 0, 0.484123, 0,
        -0.306186, 0, -0.5, 0, -0.395285, 0, 0.0, 0,
        0.395285, 0, 0.5, 0, 0.306186, 0,
        0.125, 0, 0.306186, 0, 0.484123, 0, 0.559017, 0,
        0.484123, 0, 0.306186, 0, 0.125, 0
    };
    double[] l3yArray = {
        -0.125, 0, 0, 0.306186, 0.484123, 0, 0, -0.559017,
        -0.484123, 0, 0, 0.306186, 0.125, 0,
        0.306186, 0, 0, -0.5, -0.395285, 0, 0.0, 0,
        -0.395285, 0, 0, 0.5, 0.306186, 0,
        -0.484123, 0, 0, 0.395285, -0.125, 0, 0, 0.433013,
        0.125, 0, 0, 0.395285, 0.484123, 0,
        0, 0.559017, 0.0, 0, 0, 0.433013, 0.0, 0,
        0, 0.433013, 0.0, 0, 0, 0.559017,
        -0.484123, 0, 0, -0.395285, -0.125, 0, 0, -0.433013,
        0.125, 0, 0, -0.395285, 0.484123, 0,
        0.306186, 0, 0, +0.5, -0.395285, 0, 0.0, 0, -0.395285, 0,
        0, -0.5, 0.306186, 0,
        -0.125, 0, 0, -0.306186, 0.484123, 0, 0, +0.559017,
        -0.484123, 0, 0, -0.306186, 0.125, 0
    };
    double[] l4xArray = {
        0.0625, 0.0, -0.176777, 0.0, 0.330719, 0.0, -0.467707,
        0.0, 0.522913, 0.0, -0.467707, 0.0, 0.330719, 0.0, -0.176777,
        0.0, 0.0625, 0.0, -0.176777, 0.0, 0.375, 0.0, -0.467707, 0.0,
        0.330719, 0.0, 0.0, 0.0, -0.330719, 0.0, 0.467707, 0.0, -0.375,
        0.0, 0.176777, 0.0, 0.330719, 0.0, -0.467707, 0.0, 0.25,
        0.0, 0.176777, 0.0, -0.395285, 0.0, 0.176777, 0.0, 0.25, 0.0,
        -0.467707, 0.0, 0.330719, 0.0, -0.467707, 0.0, 0.330719, 0.0,
        0.176777, 0.0, -0.375, 0.0, 0.0, 0.0, 0.375, 0.0, -0.176777,
        0.0, -0.330719, 0.0, 0.467707, 0.0, 0.522913, 0.0, 0.0, 0.0,
        -0.395285, 0.0, 0.0, 0.0, 0.375, 0.0, 0.0, 0.0, -0.395285,
        0.0, 0.0, 0.0, 0.522913, 0.0, -0.467707, 0.0, -0.330719, 0.0,
        0.176777, 0.0, 0.375, 0.0, 0.0, 0.0, -0.375, 0.0, -0.176777,
        0.0, 0.330719, 0.0, 0.467707, 0.0, 0.330719, 0.0, 0.467707,
        0.0, 0.25, 0.0, -0.176777, 0.0, -0.395285, 0.0, -0.176777,
        0.0, 0.25, 0.0, 0.467707, 0.0, 0.330719, 0.0, -0.176777, 0.0,
        -0.375, 0.0, -0.467707, 0.0, -0.330719, 0.0, 0.0, 0.0, 0.330719,
        0.0, 0.467707, 0.0, 0.375, 0.0, 0.176777, 0.0, 0.0625, 0.0,
        0.176777, 0.0, 0.330719, 0.0, 0.467707, 0.0, 0.522913, 0.0,
        0.467707, 0.0, 0.330719, 0.0, 0.176777, 0.0, 0.0625, 0.0
    };
    double[] l4yArray = {
        0.0625, 0.0, 0.0, -0.176777, -0.330719, 0.0, 0.0, 0.467707,
        0.522913, 0.0, 0.0, -0.467707, -0.330719, 0.0, 0.0, 0.176777,
        0.0625, 0.0, -0.176777, 0.0, 0.0, 0.375, 0.467707, 0.0, 0.0,
        -0.330719, 0.0, 0.0, 0.0, -0.330719, -0.467707, 0.0, 0.0,
        0.375, 0.176777, 0.0, 0.330719, 0.0, 0.0, -0.467707, -0.25,
        0.0, 0.0, -0.176777, -0.395285, 0.0, 0.0, 0.176777, -0.25, 0.0,
        0.0, 0.467707, 0.330719, 0.0, -0.467707, 0.0, 0.0, 0.330719,
        -0.176777, 0.0, 0.0, 0.375, 0.0, 0.0, 0.0, 0.375, 0.176777,
        0.0, 0.0, 0.330719, 0.467707, 0.0, 0.522913, 0.0, 0.0, 0.0,
        0.395285, 0.0, 0.0, 0.0, 0.375, 0.0, 0.0, 0.0, 0.395285, 0.0, 0.0,
        0.0, 0.522913, 0.0, -0.467707, 0.0, 0.0, -0.330719, -0.176777,
        0.0, 0.0, -0.375, 0.0, 0.0, 0.0, -0.375, 0.176777, 0.0, 0.0,
        -0.330719, 0.467707, 0.0, 0.330719, 0.0, 0.0, 0.467707,
        -0.25, 0.0, 0.0, 0.176777, -0.395285, 0.0, 0.0, -0.176777,
        -0.25, 0.0, 0.0, -0.467707, 0.330719, 0.0, -0.176777, 0.0, 0.0,
        -0.375, 0.467707, 0.0, 0.0, 0.330719, 0.0, 0.0, 0.0, 0.330719,
        -0.467707, 0.0, 0.0, -0.375, 0.176777, 0.0, 0.0625, 0.0, 0.0,
        0.176777, -0.330719, 0.0, 0.0, -0.467707, 0.522913, 0.0, 0.0,
        0.467707, -0.330719, 0.0, 0.0, -0.176777, 0.0625, 0.0
    };

    // precomputed using brute-force mathematica code
    double[] rectArrayR = {
        1.0, 0.0, 1.0, 0.0, -0.57735, 0.0, 0.0, 0.816497, 0.0, 0.0, 0.0, -0.774597, 0.0, 0.0, 0.0,
        0.0, 0.632456, 0.0, 0.0, 0.0, 0.447214, 0.0, 0.0, -0.755929, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.478091, 0.0, 0.0, 0.0, 0.0, 0.707107, 0.0, -0.707107, 0.0, 0.0, 0.707107, 0.0,
        -0.707107, 0.0, -0.316228, 0.0, 0.316228, 0.0, 0.0, 0.632456, 0.0, -0.632456,
        0.0, 0.0, 0.0, 0.0, -0.46291, 0.0, 0.46291, 0.0, 0.0, 0.0, 0.0, 0.534522, 0.0,
        -0.534522, 0.0, 0.0, 0.0, -0.57735, 0.5, 0.0, -0.408248, 0.0, 0.5, 0.0,
        -0.447214, 0.0, 0.0, 0.5, 0.0, -0.547723, 0.0, 0.5, 0.0, 0.365148, -0.188982, 0.0,
        -0.154303, 0.0, -0.188982, 0.0, 0.0, 0.46291, 0.0, -0.58554, 0.0, 0.46291, 0.0, 0.0,
        -0.547723, 0.0, 0.547723, 0.353553, 0.0, -0.273861, 0.0, 0.273861, 0.0,
        -0.353553, 0.0, 0.0, -0.46291, 0.0, 0.46291, 0.0, 0.0, 0.353553, 0.0, -0.400892,
        0.0, 0.400892, 0.0, -0.353553, 0.0, 0.447214, -0.46291, 0.0, 0.377964, 0.0,
        -0.46291, 0.25, 0.0, -0.188982, 0.0, 0.179284, 0.0, -0.188982, 0.0, 0.25, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        -0.57735, -0.5, 0.0, -0.408248, 0.0, -0.5, 0.0, -0.447214, 0.0, 0.0, -0.5, 0.0,
        -0.547723, 0.0, -0.5, 0.0, 0.365148, 0.188982, 0.0, -0.154303, 0.0, 0.188982,
        0.0, 0.0, -0.46291, 0.0, -0.58554, 0.0, -0.46291, 0.0, 0.0, -0.316228, 0.0, 0.316228,
        -0.612372, 0.0, -0.158114, 0.0, 0.158114, 0.0, 0.612372, 0.0, 0.0,
        -0.267261, 0.0, 0.267261, 0.0, 0.0, -0.612372, 0.0, -0.231455, 0.0,
        0.231455, 0.0, 0.612372, 0.0, 0.365148, 0.0, 0.0, 0.308607, 0.0, 0.0,
        -0.612372, 0.0, 0.0, 0.0, 0.146385, 0.0, 0.0, 0.0, -0.612372, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.447214, 0.46291, 0.0,
        0.377964, 0.0, 0.46291, 0.25, 0.0, 0.188982, 0.0, 0.179284, 0.0, 0.188982, 0.0, 0.25
    };

    double[] rectArrayI = {
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.707107, 0.0, -0.707107, 0.0, 0.0, -0.707107, 0.0,
        -0.707107, 0.0, 0.316228, 0.0, 0.316228, 0.0, 0.0, -0.632456, 0.0, -0.632456, 0.0,
        0.0, 0.0, 0.0, 0.46291, 0.0, 0.46291, 0.0, 0.0, 0.0, 0.0, -0.534522, 0.0, -0.534522, 0.0,
        0.0, 0.0, 0.0, -0.707107, 0.0, 0.0, 0.0, 0.707107, 0.0, 0.0, 0.0, 0.0, -0.707107, 0.0, 0.0, 0.0,
        0.707107, 0.0, 0.0, 0.267261, 0.0, 0.0, 0.0, -0.267261, 0.0, 0.0, -0.654654, 0.0, 0.0, 0.0,
        0.654654, 0.0, 0.0, 0.316228, 0.0, 0.316228, -0.612372, 0.0, 0.158114, 0.0,
        0.158114, 0.0, -0.612372, 0.0, 0.0, 0.267261, 0.0, 0.267261, 0.0, 0.0, -0.612372,
        0.0, 0.231455, 0.0, 0.231455, 0.0, -0.612372, 0.0, 0.0, 0.46291, 0.0, 0.0, 0.0,
        -0.46291, -0.5, 0.0, 0.188982, 0.0, 0.0, 0.0, -0.188982, 0.0, 0.5, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.547723, 0.0, 0.547723, 0.353553, 0.0, 0.273861, 0.0, 0.273861, 0.0, 0.353553,
        0.0, 0.0, 0.46291, 0.0, 0.46291, 0.0, 0.0, 0.353553, 0.0, 0.400892, 0.0, 0.400892, 0.0,
        0.353553, 0.0, 0.0, 0.46291, 0.0, 0.0, 0.0, -0.46291, 0.5, 0.0, 0.188982, 0.0, 0.0, 0.0,
        -0.188982, 0.0, -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
    };

    public double[] movedGaussian = {
        // 0.9394130628134758, 0.33213267352531645, 0.08303316838132911
        .778801, .550695, .275348, .11241, .039743
    };

    public double[] scaledGaussian = {
        0.252982, 0.185903, 0.124708,
        0.0808198, 0.0514334, 0.0323663,
        0.0202127, 0.0125533
    };

    public double[] scaled2Gaussian = {
        0.923077, 0.251044, 0.0836194,
        -0.251044, -0.0682749, -0.0227415,
        0.0836194, 0.0227415, 0.00757488
    };

    public double[] rotGaussianR = {
        0.75484, 0.0, -0.150118, 0.400314, 0.0, -0.079612, 0.150118, 0.0, 0
    };
    public double[] rotGaussianI = {
        0, 0.400314, 0, 0, 0.212299, 0, 0, 0.079612, 0
    };

    public double[] dispX110Array = {
        // -.174036, .953731, .242278, 0, 0
        -0.332133, 0.821986, 0.44035, 0.137825, 0
    // -0.460759,0.624461,0.559978,0.271215,0.0983688
    };

    public double[] dispZ110Array = {
        0.778801, 0.550695, 0.275348, 0.11241, 0.039743
    };

}
}