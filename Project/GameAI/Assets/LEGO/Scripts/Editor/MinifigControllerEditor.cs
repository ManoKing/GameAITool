using Unity.LEGO.Minifig;
using UnityEditor;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(MinifigController))]
    public class MinifigControllerEditor : Editor
    {
        protected SerializedProperty maxForwardSpeedProp;
        protected SerializedProperty maxBackwardSpeedProp;
        protected SerializedProperty accelerationProp;
        protected SerializedProperty maxRotateSpeedProp;
        protected SerializedProperty rotateAccelerationProp;
        protected SerializedProperty jumpSpeedProp;
        protected SerializedProperty gravityProp;

        protected SerializedProperty inputTypeProp;
        protected SerializedProperty inputEnabledProp;
        protected SerializedProperty maxJumpsInAirProp;

        protected SerializedProperty stepAudioClipsProp;
        protected SerializedProperty jumpAudioClipProp;
        protected SerializedProperty doubleJumpAudioClipProp;
        protected SerializedProperty landAudioClipProp;
        protected SerializedProperty explodeAudioClipProp;

        protected virtual void OnEnable()
        {
            maxForwardSpeedProp = serializedObject.FindProperty("maxForwardSpeed");
            maxBackwardSpeedProp = serializedObject.FindProperty("maxBackwardSpeed");
            accelerationProp = serializedObject.FindProperty("acceleration");
            maxRotateSpeedProp = serializedObject.FindProperty("maxRotateSpeed");
            rotateAccelerationProp = serializedObject.FindProperty("rotateAcceleration");
            jumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            gravityProp = serializedObject.FindProperty("gravity");

            inputTypeProp = serializedObject.FindProperty("inputType");
            inputEnabledProp = serializedObject.FindProperty("inputEnabled");
            maxJumpsInAirProp = serializedObject.FindProperty("maxJumpsInAir");

            stepAudioClipsProp = serializedObject.FindProperty("stepAudioClips");
            jumpAudioClipProp = serializedObject.FindProperty("jumpAudioClip");
            doubleJumpAudioClipProp = serializedObject.FindProperty("doubleJumpAudioClip");
            landAudioClipProp = serializedObject.FindProperty("landAudioClip");
            explodeAudioClipProp = serializedObject.FindProperty("explodeAudioClip");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(maxForwardSpeedProp);
            EditorGUILayout.PropertyField(maxBackwardSpeedProp);
            EditorGUILayout.PropertyField(accelerationProp);
            EditorGUILayout.PropertyField(maxRotateSpeedProp);
            EditorGUILayout.PropertyField(rotateAccelerationProp);
            EditorGUILayout.PropertyField(jumpSpeedProp);
            EditorGUILayout.PropertyField(gravityProp);

            EditorGUILayout.Space(); // Insert space.

            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(stepAudioClipsProp);
            EditorGUILayout.PropertyField(jumpAudioClipProp);
            EditorGUILayout.PropertyField(doubleJumpAudioClipProp);
            EditorGUILayout.PropertyField(landAudioClipProp);
            EditorGUILayout.PropertyField(explodeAudioClipProp);

            EditorGUILayout.Space(); // Insert space.

            EditorGUILayout.LabelField("Miscellaneous", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(inputEnabledProp);

            if (inputEnabledProp.boolValue)
            {
                EditorGUILayout.PropertyField(inputTypeProp);
            }
            
            EditorGUILayout.PropertyField(maxJumpsInAirProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
