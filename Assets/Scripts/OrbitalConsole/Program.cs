using System;

namespace OrbitalConsole
{
    class Program
    {
        static void Main(string[] args) {
            p("Orbital Test");

            OrbitComputer c = new OrbitComputer();
            c.testCompute("1");

            int x = 100;
            int y = 100;
            int z = 100;
            for (int i = 0;i< 10; i++) {
                p("Time is " + c.globalTime);
                c.advanceTime();
                p(c.computValue(x, y, z, false).toString());
            }
           
           
        }
        public static void p(String s) {
            Console.WriteLine("Program: " + s);
        }
    }
}
