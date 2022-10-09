using LEGOModelImporter;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.LEGO.Tutorials
{
    /// <summary>
    /// Contains all the methods needed to check that an asset from a mod has been instantiated.
    /// We can't use the standard InstantiateCriterion for these assets, as they're not available in the project at authoring time.
    /// </summary>
    [CreateAssetMenu(fileName = "ModAssetInstantiatedCriteria", menuName = "Tutorials/Microgame/ModAssetInstantiatedCriteria")]
    class ModAssetInstantiatedCriteria : ScriptableObject
    {
        public string assetName = "Short Bridge";
        IEnumerable<Model> existingInstances;
        bool instantiated;

        public void ResetInstancesCount()
        {
            instantiated = false;

            //startsWith as we want to ensure that even clones are detected, and they are often named "<asset name> (<number>)"
            existingInstances = GameObject.FindObjectsOfType<Model>()
                .Where(go => go.name.StartsWith(assetName));


            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public bool AutoComplete()
        {
            instantiated = true;
            Selection.selectionChanged -= OnSelectionChanged;
            return true;
        }

        void OnSelectionChanged()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                if (!gameObject.name.StartsWith(assetName)) { continue; }

                var modelComponent = gameObject.GetComponent<Model>();
                if (!modelComponent || existingInstances.Contains(modelComponent)) { continue; }

                instantiated = true;
                return;
            }
        }

        public bool ShortBridgeInstantiated()
        {
            if (instantiated)
            {
                Selection.selectionChanged -= OnSelectionChanged;
            }
            return instantiated;
        }
    }
}
