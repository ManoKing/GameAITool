using Unity.LEGO.Behaviours;
using Cinemachine;
using LEGOModelImporter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.LEGO.EditorExt
{
    public static class EditorUtilities
    {
        public static void FocusCamera(LEGOBehaviour behaviour)
        {
            var cinemachine = StageUtility.GetStageHandle(behaviour.gameObject).FindComponentOfType<CinemachineFreeLook>();

            if (cinemachine)
            {
                var serializedCinemachine = new SerializedObject(cinemachine);

                var modelGroup = behaviour.GetComponentInParent<ModelGroup>();

                if (modelGroup)
                {
                    serializedCinemachine.FindProperty("m_LookAt").objectReferenceValue = modelGroup.transform;
                    serializedCinemachine.FindProperty("m_Follow").objectReferenceValue = modelGroup.transform;

                    var scopedBricks = behaviour.GetScopedBricks();
                    var scopedBounds = behaviour.GetScopedBounds(scopedBricks, out _, out _);

                    var radius = scopedBounds.extents.magnitude;

                    if (!cinemachine.m_Lens.Orthographic)
                    {
                        var cameraVerticalFOV = cinemachine.m_Lens.FieldOfView;
                        var cameraHorizontalFOV = Camera.VerticalToHorizontalFieldOfView(cameraVerticalFOV, cinemachine.m_Lens.Aspect);

                        var fov = Mathf.Min(cameraHorizontalFOV, cameraVerticalFOV) * 0.5f;
                        var distance = radius / Mathf.Tan(fov * Mathf.Deg2Rad) + radius;

                        serializedCinemachine.FindProperty("m_Orbits").GetArrayElementAtIndex(1).FindPropertyRelative("m_Radius").floatValue = distance;
                    }
                    else
                    {
                        serializedCinemachine.FindProperty("m_Lens").FindPropertyRelative("OrthographicSize").floatValue = radius;
                    }
                }

                serializedCinemachine.ApplyModifiedProperties();
            }
            else
            {
                EditorUtility.DisplayDialog("Cinemachine Free Look Camera Not Found", "Focus camera only supports Cinemachine Free Look camera.", "OK");
            }
        }
    }
}
