using System.Collections.Generic;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "MaterialsData", menuName = "Monarchs/Data/MaterialsData", order = 10)]
    public class MaterialsData : ScriptableObject
    {
        public List<MaterialEntry> materials = new List<MaterialEntry>();
        private static MaterialsData _instance;
        public static MaterialsData Get()
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MaterialsData>("MaterialsData");
                if (_instance == null)
                {
                    Debug.LogError("MaterialsData not found in Resources folder");
                }
            }
            return _instance;
        }
        public Material GetMaterial(string id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry.material;
                }
            }
            Debug.LogWarning("Material with id " + id + " not found");
            return null;
        }
        
        [System.Serializable]
        public struct MaterialEntry
        {
            public string id;
            public Material material;
        }
    }
}
