using System.IO;
using UnityEngine;

namespace Unity.LEGO.Tutorials
{
    /// <summary>
    /// Contains all the callbacks needed for the tutorial steps that check for Asset package imports
    /// </summary>
    [CreateAssetMenu(fileName = "PackageImportedCriteria", menuName = "Tutorials/Microgame/PackageImportedCriteria")]
    class PackageImportedCriteria : ScriptableObject
    {
        public bool PackageFolderExists(string pathRelativeToAssets)
        {
            return Directory.Exists(Path.Combine(Application.dataPath, pathRelativeToAssets));
        }
    }
}
