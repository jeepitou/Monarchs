using UnityEngine;

namespace Monarchs
{
    public static class RecursiveTransparent
    {
        public static void SetTransparency(float alpha, GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    // Standard Shader
                    if (mat.HasProperty("_Mode"))
                        mat.SetFloat("_Mode", 3); // Transparent

                    // URP/HDRP Shader
                    if (mat.HasProperty("_Surface"))
                        mat.SetFloat("_Surface", 1); // 1 = Transparent

                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;

                    if (alpha < 1f)
                    {
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                    else
                    {
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = -1;
                    }
                }
            }
            // UI and Text transparency (unchanged)
            UnityEngine.UI.Image[] images = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (UnityEngine.UI.Image image in images)
            {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
            UnityEngine.UI.Text[] texts = gameObject.GetComponentsInChildren<UnityEngine.UI.Text>();
            foreach (UnityEngine.UI.Text text in texts)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
            TMPro.TextMeshProUGUI[] tmpTexts = gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (TMPro.TextMeshProUGUI text in tmpTexts)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }
    }
}