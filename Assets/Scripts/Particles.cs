using System;
using UnityEngine;
using System.Collections.Generic;
public class Particles
{

    private List<Particle> particles;

    public Particles() {
		particles = new List<Particle>();
		particles.Add(new Particle("1/2 up", "<s>", false));
		particles.Add(new Particle("1/2 down", ">s<", false));

		particles.Add(new Particle("1/2 up hor", "[z]", false));
		particles.Add(new Particle("1/2 down hor", "]z[", false));

		particles.Add(new Particle("Complex spin 1", "<s}s{>", false));
		particles.Add(new Particle("Complex spin 2", "<s{s}>", false));

		particles.Add(new Particle("Complex spin 3", "<s{s[x]s}s>", false));
		particles.Add(new Particle("Complex spin 4", "<s{s]x[s}s>", false));

	}

    public List<Particle> AllParticles { get => particles; set => particles = value; }

    private static void p(string s) {
		Debug.Log("Particles: " + s);
	}
	
}
