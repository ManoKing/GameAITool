using Unity.LEGO.Minifig;
using UnityEditor;
using UnityEngine;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(OneButtonMinifigController))]
    public class OneButtonMinifigControllerEditor : MinifigControllerEditor
    {
        SerializedProperty startMovementProp;
        SerializedProperty startRotationProp;
        SerializedProperty pressedMovementProp;
        SerializedProperty pressedRotationProp;
        SerializedProperty releasedMovementProp;
        SerializedProperty releasedRotationProp;

        SerializedProperty startSettingProp;
        SerializedProperty pressedSettingProp;
        SerializedProperty releasedSettingProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            startMovementProp = serializedObject.FindProperty("startMovementType");
            startRotationProp = serializedObject.FindProperty("startRotationType");
            pressedMovementProp = serializedObject.FindProperty("pressedMovementType");
            pressedRotationProp = serializedObject.FindProperty("pressedRotationType");
            releasedMovementProp = serializedObject.FindProperty("releasedMovementType");
            releasedRotationProp = serializedObject.FindProperty("releasedRotationType");

            startSettingProp = serializedObject.FindProperty("startSetting");
            pressedSettingProp = serializedObject.FindProperty("pressedSetting");
            releasedSettingProp = serializedObject.FindProperty("releasedSetting");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (inputEnabledProp.boolValue)
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

                // On Start Game.
                EditorGUILayout.LabelField("On Start Game", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(startMovementProp, new GUIContent("Move"));

                var startMovementType = (OneButtonMinifigController.StartMovementType)startMovementProp.enumValueIndex;
                if (startMovementType == OneButtonMinifigController.StartMovementType.Continuously || startMovementType == OneButtonMinifigController.StartMovementType.Distance)
                {
                    CreateMovementGUI(startSettingProp, startMovementType == OneButtonMinifigController.StartMovementType.Continuously ?
                        OneButtonMinifigController.MovementType.Continuously : OneButtonMinifigController.MovementType.Distance);
                }

                EditorGUILayout.Space(); // Insert space.

                EditorGUILayout.PropertyField(startRotationProp, new GUIContent("Rotate"));

                var startRotationType = (OneButtonMinifigController.StartRotationType)startRotationProp.enumValueIndex;
                if (startRotationType == OneButtonMinifigController.StartRotationType.Continuously || startRotationType == OneButtonMinifigController.StartRotationType.Angle)
                {
                    CreateRotationGUI(startSettingProp, startRotationType == OneButtonMinifigController.StartRotationType.Continuously ?
                        OneButtonMinifigController.RotationType.Continuously : OneButtonMinifigController.RotationType.Angle);
                }

                EditorGUILayout.Space(); // Insert space.

                // When Pressed.
                EditorGUILayout.LabelField("When Pressed", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(pressedMovementProp, new GUIContent("Move"));

                var pressedMovementType = (OneButtonMinifigController.MovementType)pressedMovementProp.enumValueIndex;
                if (pressedMovementType != OneButtonMinifigController.MovementType.NoChange)
                {
                    CreateMovementGUI(pressedSettingProp, pressedMovementType);

                    if (pressedMovementType != OneButtonMinifigController.MovementType.Stop)
                    {
                        EditorGUILayout.PropertyField(pressedSettingProp.FindPropertyRelative("moveAlternate"), new GUIContent("Alternate"));
                    }
                }

                EditorGUILayout.Space(); // Insert space.

                EditorGUILayout.PropertyField(pressedRotationProp, new GUIContent("Rotate"));

                var pressedRotationType = (OneButtonMinifigController.RotationType)pressedRotationProp.enumValueIndex;
                if (pressedRotationType != OneButtonMinifigController.RotationType.NoChange)
                {
                    CreateRotationGUI(pressedSettingProp, pressedRotationType);

                    if (pressedRotationType != OneButtonMinifigController.RotationType.Stop)
                    {
                        EditorGUILayout.PropertyField(pressedSettingProp.FindPropertyRelative("rotateAlternate"), new GUIContent("Alternate"));
                    }
                }

                EditorGUILayout.Space(); // Insert space.

                var pressedJumpProp = pressedSettingProp.FindPropertyRelative("jump");
                EditorGUILayout.PropertyField(pressedJumpProp, new GUIContent("Jump"));

                EditorGUILayout.Space(); // Insert space.

                // When Released.
                EditorGUILayout.LabelField("When Released", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(releasedMovementProp, new GUIContent("Move"));

                var releasedMovementType = (OneButtonMinifigController.MovementType)releasedMovementProp.enumValueIndex;
                if (releasedMovementType != OneButtonMinifigController.MovementType.NoChange)
                {
                    CreateMovementGUI(releasedSettingProp, releasedMovementType);

                    if (releasedMovementType != OneButtonMinifigController.MovementType.Stop)
                    {
                        EditorGUILayout.PropertyField(releasedSettingProp.FindPropertyRelative("moveAlternate"), new GUIContent("Alternate"));
                    }
                }

                EditorGUILayout.Space(); // Insert space.

                EditorGUILayout.PropertyField(releasedRotationProp, new GUIContent("Rotate"));

                var releasedRotationType = (OneButtonMinifigController.RotationType)releasedRotationProp.enumValueIndex;
                if (releasedRotationType != OneButtonMinifigController.RotationType.NoChange)
                {
                    CreateRotationGUI(releasedSettingProp, releasedRotationType);

                    if (releasedRotationType != OneButtonMinifigController.RotationType.Stop)
                    {
                        EditorGUILayout.PropertyField(releasedSettingProp.FindPropertyRelative("rotateAlternate"), new GUIContent("Alternate"));
                    }
                }

                EditorGUILayout.Space(); // Insert space.

                var releasedJumpProp = releasedSettingProp.FindPropertyRelative("jump");
                EditorGUILayout.PropertyField(releasedJumpProp, new GUIContent("Jump"));

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(); // Insert space.

                // Audio
                EditorGUILayout.PropertyField(stepAudioClipsProp);
                EditorGUILayout.PropertyField(jumpAudioClipProp);
                EditorGUILayout.PropertyField(doubleJumpAudioClipProp);
                EditorGUILayout.PropertyField(landAudioClipProp);
                EditorGUILayout.PropertyField(explodeAudioClipProp);

                EditorGUILayout.Space(); // Insert space.

                EditorGUILayout.LabelField("Miscellaneous", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(inputEnabledProp);
                if (releasedJumpProp.boolValue || pressedJumpProp.boolValue)
                {
                    EditorGUILayout.PropertyField(maxJumpsInAirProp);
                    EditorGUILayout.PropertyField(jumpSpeedProp);
                }
                EditorGUILayout.PropertyField(gravityProp);
            }
            else
            {
                base.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void CreateMovementGUI(SerializedProperty property, OneButtonMinifigController.MovementType type)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("moveDirection"), new GUIContent("Direction"));

            if (type != OneButtonMinifigController.MovementType.Stop)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("moveMaxSpeed"), new GUIContent("Max Speed"));

                EditorGUILayout.PropertyField(property.FindPropertyRelative("moveAcceleration"), new GUIContent("Acceleration"));

                if (type == OneButtonMinifigController.MovementType.Distance)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("moveDistance"), new GUIContent("Distance"));
                }
            }
        }

        void CreateRotationGUI(SerializedProperty property, OneButtonMinifigController.RotationType type)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("rotateDirection"), new GUIContent("Direction"));

            if (type != OneButtonMinifigController.RotationType.Stop)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("rotateSpeed"), new GUIContent("Speed"));

                if (type == OneButtonMinifigController.RotationType.Angle)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("rotateAngle"), new GUIContent("Angle"));
                }
            }
        }
    }
}
