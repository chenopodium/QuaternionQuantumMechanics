using UnityEngine;
using System.Collections.Generic;
using System;

public class SphericMeshBuilder : MarchingCubesMeshBuilder
{

    public override float getValue(float x, float y, float z) {

        float res =  Mathf.Sqrt(x * x + y * y + z * z);
     //  p("     Value " + x + "/" + y + "/" + z + " = " + res);
        return res;
    }
   
}