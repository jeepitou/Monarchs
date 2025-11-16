using Monarchs.Ability;
using TcgEngine.FX;
using UnityEngine;
using Monarchs.Logic.AbilitySystem;
using System.Collections;

namespace Monarchs.Tools
{
    /// <summary>
    /// Container class to hold the result of FX creation
    /// </summary>
    public class FXResult
    {
        public GameObject fxObject;
    }

    /// <summary>
    /// Static functions to spawn FX prefabs
    /// </summary>

    public class FXTool : MonoBehaviour
    {
        public static IEnumerator DoFX(FXData[] fxDatas, int index, Vector3 pos, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoFX(fxDatas[index].FX, pos, result);
            }
        }
        
        public static IEnumerator DoFX(FXData[] fxDatas, int index, Vector3 pos, AbilityArgs abilityArgs, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoFX(fxDatas[index].FX, pos, abilityArgs, result);
            }
        }
        
        public static IEnumerator DoFX(GameObject fxPrefab, Vector3 pos, FXResult result)
        {
            if (fxPrefab != null)
            {
                result.fxObject = Instantiate(fxPrefab, pos, GetFXRotation());
                // Wait for one frame to ensure the FX is properly initialized
                yield return null;
            }
        }

        public static IEnumerator DoFX(GameObject fxPrefab, Vector3 pos, AbilityArgs abilityArgs, FXResult result)
        {
            if (fxPrefab != null)
            {
                result.fxObject = Instantiate(fxPrefab, pos, GetFXRotation());
                
                // Check if the prefab has an AbilityFX component and call SetAbilityData if it exists
                AbilityFX abilityFX = result.fxObject.GetComponent<AbilityFX>();
                if (abilityFX != null && abilityArgs != null)
                {
                    abilityFX.SetAbilityData(abilityArgs);
                }
                
                // Wait for one frame to ensure the FX is properly initialized
                yield return null;
            }
        }

        public static IEnumerator DoSnapFX(GameObject fxPrefab, Transform snapTarget, FXResult result)
        {
            yield return DoSnapFX(fxPrefab, snapTarget, Vector3.zero, result);
        }
        
        public static IEnumerator DoSnapFX(FXData[] fxDatas, int index, Transform snapTarget, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoSnapFX(fxDatas[index].FX, snapTarget, result);
            }
        }
        
        public static IEnumerator DoSnapFX(FXData[] fxDatas, int index, Transform snapTarget, AbilityArgs abilityArgs, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoSnapFX(fxDatas[index].FX, snapTarget, Vector3.zero, abilityArgs, result);
            }
        }

        public static IEnumerator DoSnapFX(FXData[] fxDatas, int index, Transform snapTarget, Vector3 offset, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoSnapFX(fxDatas[index].FX, snapTarget, offset, result);
            }
        }
        
        public static IEnumerator DoSnapFX(FXData[] fxDatas, int index, Transform snapTarget, Vector3 offset, AbilityArgs abilityArgs, FXResult result)
        {
            if (fxDatas.Length > index)
            {
                yield return DoSnapFX(fxDatas[index].FX, snapTarget, offset, abilityArgs, result);
            }
        }

        public static IEnumerator DoSnapFX(GameObject fxPrefab, Transform snapTarget, Vector3 offset, FXResult result)
        {
            if (fxPrefab != null && snapTarget != null)
            {
                result.fxObject = Instantiate(fxPrefab, snapTarget.transform.position + snapTarget.transform.up * .2f, GetFXRotation());
                SnapFX snap = result.fxObject.AddComponent<SnapFX>();
                snap.target = snapTarget;
                snap.offset = offset;
                
                // Wait for one frame to ensure the FX is properly initialized
                yield return null;
            }
        }

        public static IEnumerator DoSnapFX(GameObject fxPrefab, Transform snapTarget, Vector3 offset, AbilityArgs abilityArgs, FXResult result)
        {
            if (fxPrefab != null && snapTarget != null)
            {
                result.fxObject = Instantiate(fxPrefab, snapTarget.transform.position + snapTarget.transform.up * .2f, GetFXRotation());
                SnapFX snap = result.fxObject.AddComponent<SnapFX>();
                snap.target = snapTarget;
                snap.offset = offset;
                
                // Check if the prefab has an AbilityFX component and call SetAbilityData if it exists
                AbilityFX abilityFX = result.fxObject.GetComponent<AbilityFX>();
                if (abilityFX != null && abilityArgs != null)
                {
                    abilityFX.SetAbilityData(abilityArgs);
                    yield return abilityFX.FXEnumerator();
                }
                
                // Wait for one frame to ensure the FX is properly initialized
                yield return null;
            }
        }

        private static Quaternion GetFXRotation()
        {
            Vector3 facing = Vector3.forward;
            return Quaternion.LookRotation(facing, Vector3.up);
        }
    }
}
