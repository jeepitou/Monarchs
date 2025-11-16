using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class InvertedMask : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material material = new Material(base.materialForRendering);
                material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.NotEqual);
                return material;
            }
        }
    }
}
