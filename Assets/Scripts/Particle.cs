using System;
using UnityEngine;

public class Particle
{

    private String name;
    private String formula = "<x}x{>";

    private String desc;
	private bool antiMatter;

    public string Formula { get => formula; set => formula = value; }
    public string Name { get => name; set => name = value; }
    public string Desc { get => desc; set => desc = value; }
    public bool AntiMatter { get => antiMatter; set => antiMatter = value; }

    public Particle(String name, String formula, bool anti) {

		this.name = name;
		this.antiMatter =anti;
		this.formula = formula;
		p("Particle "+name+":"+formula);

	}
	public Particle(String name, String desc, String formula, bool anti) {
		this.antiMatter = anti;
		this.name = name;
		this.desc = desc;
		this.formula = formula;
		p("Particle " + name + ":" + formula);

	}
	public String toString() {
		return name + ": " + formula;
    }
	private void p(string s) {
		Debug.Log("Particle: " + s);
	}
	
}
