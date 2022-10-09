using Unity.LEGO.Behaviours;
using Unity.LEGO.Behaviours.Controls;
using UnityEditor;
using UnityEngine;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(OneButtonAction), true)]
    public class OneButtonActionEditor : MovementActionEditor
    {
        OneButtonAction m_OneButtonAction;

        SerializedProperty m_StartMovementProp;
        SerializedProperty m_StartRotationProp;
        SerializedProperty m_PressedMovementProp;
        SerializedProperty m_PressedRotationProp;
        SerializedProperty m_ReleasedMovementProp;
        SerializedProperty m_ReleasedRotationProp;

        SerializedProperty m_StartSettingProp;
        SerializedProperty m_PressedSettingProp;
        SerializedProperty m_ReleasedSettingProp;
        SerializedProperty m_IsPlayerProp;
        SerializedProperty m_GravityProp;
        SerializedProperty m_GravityValueProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_OneButtonAction = (OneButtonAction)m_Action;

            m_StartMovementProp = serializedObject.FindProperty("m_StartMovementType");
            m_StartRotationProp = serializedObject.FindProperty("m_StartRotationType");
            m_PressedMovementProp = serializedObject.FindProperty("m_PressedMovementType");
            m_PressedRotationProp = serializedObject.FindProperty("m_PressedRotationType");
            m_ReleasedMovementProp = serializedObject.FindProperty("m_ReleasedMovementType");
            m_ReleasedRotationProp = serializedObject.FindProperty("m_ReleasedRotationType");

            m_IsPlayerProp = serializedObject.FindProperty("m_IsPlayer");
            m_GravityProp = serializedObject.FindProperty("m_UseGravity");
            m_GravityValueProp = serializedObject.FindProperty("m_Gravity");
            m_StartSettingProp = serializedObject.FindProperty("m_StartSetting");
            m_PressedSettingProp = serializedObject.FindProperty("m_PressedSetting");
            m_ReleasedSettingProp = serializedObject.FindProperty("m_ReleasedSetting");
        }

        protected override void CreateGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            // On Start Game.
            EditorGUILayout.LabelField("On Start Game", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_StartMovementProp, new GUIContent("Move"));

            var startMovementType = (OneButtonAction.StartMovementType) m_StartMovementProp.enumValueIndex;
            if (startMovementType == OneButtonAction.StartMovementType.Continuously || startMovementType == OneButtonAction.StartMovementType.Distance)
            {
                CreateMovementGUI(m_StartSettingProp, startMovementType == OneButtonAction.StartMovementType.Continuously ? OneButtonAction.MovementType.Continuously : OneButtonAction.MovementType.Distance);
            }

            EditorGUILayout.PropertyField(m_StartRotationProp, new GUIContent("Rotate"));

            var startRotationType = (OneButtonAction.StartRotationType) m_StartRotationProp.enumValueIndex;
            if (startRotationType == OneButtonAction.StartRotationType.Continuously || startRotationType == OneButtonAction.StartRotationType.Angle)
            {
                CreateRotationGUI(m_StartSettingProp, startRotationType == OneButtonAction.StartRotationType.Continuously ? OneButtonAction.RotationType.Continuously : OneButtonAction.RotationType.Angle);
            }

            EditorGUILayout.Space(); // Insert space.

            // When Pressed.
            EditorGUILayout.LabelField("When Pressed", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_PressedMovementProp, new GUIContent("Move"));

            var pressedMovementType = (OneButtonAction.MovementType) m_PressedMovementProp.enumValueIndex;
            if (pressedMovementType != OneButtonAction.MovementType.NoChange)
            {
                CreateMovementGUI(m_PressedSettingProp, pressedMovementType);

                if (pressedMovementType != OneButtonAction.MovementType.Stop)
                {
                    EditorGUILayout.PropertyField(m_PressedSettingProp.FindPropertyRelative("MoveAlternate"), new GUIContent("Alternate"));
                }
            }

            EditorGUILayout.PropertyField(m_PressedRotationProp, new GUIContent("Rotate"));

            var pressedRotationType = (OneButtonAction.RotationType) m_PressedRotationProp.enumValueIndex;
            if (pressedRotationType != OneButtonAction.RotationType.NoChange)
            {
                CreateRotationGUI(m_PressedSettingProp, pressedRotationType);

                if (pressedRotationType != OneButtonAction.RotationType.Stop)
                {
                    EditorGUILayout.PropertyField(m_PressedSettingProp.FindPropertyRelative("RotateAlternate"), new GUIContent("Alternate"));
                }
            }

            if (!m_OneButtonAction.IsVerticalMovementEnabled())
            {
                var pressedJumpProp = m_PressedSettingProp.FindPropertyRelative("Jump");
                EditorGUILayout.PropertyField(pressedJumpProp, new GUIContent("Jump"));

                if (pressedJumpProp.boolValue)
                {
                    EditorGUILayout.PropertyField(m_PressedSettingProp.FindPropertyRelative("JumpSpeed"));
                    EditorGUILayout.PropertyField(m_PressedSettingProp.FindPropertyRelative("MaxJumpsInAir"));
                }
            }

            EditorGUILayout.Space(); // Insert space.

            // When Released.
            EditorGUILayout.LabelField("When Released", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_ReleasedMovementProp, new GUIContent("Move"));

            var releasedMovementType = (OneButtonAction.MovementType) m_ReleasedMovementProp.enumValueIndex;
            if (releasedMovementType != OneButtonAction.MovementType.NoChange)
            {
                CreateMovementGUI(m_ReleasedSettingProp, releasedMovementType);

                if (releasedMovementType != OneButtonAction.MovementType.Stop)
                {
                    EditorGUILayout.PropertyField(m_ReleasedSettingProp.FindPropertyRelative("MoveAlternate"), new GUIContent("Alternate"));
                }
            }

            EditorGUILayout.PropertyField(m_ReleasedRotationProp, new GUIContent("Rotate"));

            var releasedRotationType = (OneButtonAction.RotationType) m_ReleasedRotationProp.enumValueIndex;
            if (releasedRotationType != OneButtonAction.RotationType.NoChange)
            {
                CreateRotationGUI(m_ReleasedSettingProp, releasedRotationType);

                if (releasedRotationType != OneButtonAction.RotationType.Stop)
                {
                    EditorGUILayout.PropertyField(m_ReleasedSettingProp.FindPropertyRelative("RotateAlternate"), new GUIContent("Alternate"));
                }
            }

            if (!m_OneButtonAction.IsVerticalMovementEnabled())
            {
                var releasedJumpProp = m_ReleasedSettingProp.FindPropertyRelative("Jump");
                EditorGUILayout.PropertyField(releasedJumpProp, new GUIContent("Jump"));

                if (releasedJumpProp.boolValue)
                {
                    EditorGUILayout.PropertyField(m_ReleasedSettingProp.FindPropertyRelative("JumpSpeed"));
                    EditorGUILayout.PropertyField(m_ReleasedSettingProp.FindPropertyRelative("MaxJumpsInAir"));
                }
            }

            EditorGUILayout.Space(); // Insert space.

            // Miscellaneous.
            EditorGUILayout.LabelField("Miscellaneous", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_IsPlayerProp);
            EditorGUILayout.PropertyField(m_CollideProp);

            if (!m_OneButtonAction.IsVerticalMovementEnabled())
            {
                EditorGUILayout.PropertyField(m_GravityProp);

                if (m_GravityProp.boolValue)
                {
                    EditorGUILayout.PropertyField(m_GravityValueProp);
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!m_OneButtonAction.IsPlacedOnBrick());

            if (GUILayout.Button("Focus Camera"))
            {
                EditorUtilities.FocusCamera(m_OneButtonAction);
            }

            EditorGUI.EndDisabledGroup();
        }

        void CreateMovementGUI(SerializedProperty property, OneButtonAction.MovementType type)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("MoveDirection"), new GUIContent("Direction"));

            if (type != OneButtonAction.MovementType.Stop)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("MoveSpeed"), new GUIContent("Speed"));

                if (type == OneButtonAction.MovementType.Distance)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("MoveDistance"), new GUIContent("Distance"));
                }
            }
        }

        void CreateRotationGUI(SerializedProperty property, OneButtonAction.RotationType type)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("RotateDirection"), new GUIContent("Direction"));

            if (type != OneButtonAction.RotationType.Stop)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("RotateSpeed"), new GUIContent("Speed"));

                if (type == OneButtonAction.RotationType.Angle)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("RotateAngle"), new GUIContent("Angle"));
                }
            }
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (Event.current.type == EventType.Repaint)
            {
                if (m_OneButtonAction && m_OneButtonAction.IsPlacedOnBrick())
                {
                    Handles.color = Color.green;

                    var startMovementType = (OneButtonAction.StartMovementType) m_StartMovementProp.enumValueIndex;
                    if (startMovementType != OneButtonAction.StartMovementType.No)
                    {
                        DrawMovementGizmo(m_OneButtonAction.GetStartSetting(), startMovementType == OneButtonAction.StartMovementType.Continuously ? OneButtonAction.MovementType.Continuously : OneButtonAction.MovementType.Distance);
                    }

                    var startRotationType = (OneButtonAction.StartRotationType) m_StartRotationProp.enumValueIndex;
                    if (startRotationType != OneButtonAction.StartRotationType.No)
                    {
                        DrawRotationGizmo(m_OneButtonAction.GetStartSetting(), startRotationType == OneButtonAction.StartRotationType.Continuously ? OneButtonAction.RotationType.Continuously : OneButtonAction.RotationType.Angle);
                    }

                    var pressedMovementType = (OneButtonAction.MovementType) m_PressedMovementProp.enumValueIndex;
                    if (pressedMovementType != OneButtonAction.MovementType.NoChange && pressedMovementType != OneButtonAction.MovementType.Stop)
                    {
                        DrawMovementGizmo(m_OneButtonAction.GetPressedSetting(), pressedMovementType);
                    }

                    var pressedRotationType = (OneButtonAction.RotationType) m_PressedRotationProp.enumValueIndex;
                    if (pressedRotationType != OneButtonAction.RotationType.NoChange && pressedRotationType != OneButtonAction.RotationType.Stop)
                    {
                        DrawRotationGizmo(m_OneButtonAction.GetPressedSetting(), pressedRotationType);
                    }

                    var releasedMovementType = (OneButtonAction.MovementType) m_ReleasedMovementProp.enumValueIndex;
                    if (releasedMovementType != OneButtonAction.MovementType.NoChange && releasedMovementType != OneButtonAction.MovementType.Stop)
                    {
                        DrawMovementGizmo(m_OneButtonAction.GetReleasedSetting(), releasedMovementType);
                    }

                    var releasedRotationType = (OneButtonAction.RotationType) m_ReleasedRotationProp.enumValueIndex;
                    if (releasedRotationType != OneButtonAction.RotationType.NoChange && releasedRotationType != OneButtonAction.RotationType.Stop)
                    {
                        DrawRotationGizmo(m_OneButtonAction.GetReleasedSetting(), releasedRotationType);
                    }
                }
            }
        }

        void DrawMovementGizmo(OneButtonAction.MotionSetting setting, OneButtonAction.MovementType type)
        {
            var center = m_OneButtonAction.GetBrickCenter();

            var remainingDistance = m_OneButtonAction.GetRemainingDistance(setting);
            var direction = m_OneButtonAction.GetBrickRotation() * GetMovementAxis(setting.MoveDirection);

            var LEGOModule = setting.MoveDirection == OneButtonAction.MovementDirection.Up || setting.MoveDirection == OneButtonAction.MovementDirection.Down
                ? LEGOBehaviour.LEGOVerticalModule : LEGOBehaviour.LEGOHorizontalModule;
            var movementDistance = type == OneButtonAction.MovementType.Continuously ? setting.MoveSpeed : remainingDistance;
            var end = center + direction * movementDistance * LEGOModule;

            if (movementDistance > 0.0f)
            {
                Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);
                Handles.DrawLine(center, end);
            }
        }

        void DrawRotationGizmo(OneButtonAction.MotionSetting setting, OneButtonAction.RotationType type)
        {
            var rotationAxis = GetRotationAxis(setting.RotateDirection);
            var direction = setting.RotateDirection == OneButtonAction.RotationDirection.RollRight || setting.RotateDirection == OneButtonAction.RotationDirection.RollLeft ? Vector3.up : Vector3.forward;
            var forward = m_OneButtonAction.GetBrickRotation() * direction;
            var center = m_OneButtonAction.GetBrickCenter();
            var angle = type == OneButtonAction.RotationType.Angle ? m_OneButtonAction.GetRemainingAngle(setting) : (float) setting.RotateSpeed;
            var end = m_OneButtonAction.GetBrickRotation() * Quaternion.Euler(rotationAxis * angle) * direction * 3.2f + center;

            if (angle > 0.0f)
            {
                Handles.DrawWireArc(center, m_OneButtonAction.GetBrickRotation() * rotationAxis, forward, Mathf.Clamp(angle, -360.0f, 360.0f), 3.2f);
                Handles.DrawSolidDisc(end, Camera.current.transform.forward, 0.16f);

                var fullRounds = Mathf.FloorToInt(Mathf.Abs(angle) / 360);
                if (fullRounds > 0)
                {
                    for (var i = 0; i < fullRounds; ++i)
                    {
                        Handles.DrawWireDisc(end, Camera.current.transform.forward, 0.16f + (i + 1) * 0.08f);
                    }
                }
            }
        }

        Vector3 GetMovementAxis(OneButtonAction.MovementDirection moveDirection)
        {
            switch (moveDirection)
            {
                case OneButtonAction.MovementDirection.Forward:
                    return Vector3.forward;
                case OneButtonAction.MovementDirection.Back:
                    return Vector3.back;
                case OneButtonAction.MovementDirection.Right:
                    return Vector3.right;
                case OneButtonAction.MovementDirection.Left:
                    return Vector3.left;
                case OneButtonAction.MovementDirection.Up:
                    return Vector3.up;
                case OneButtonAction.MovementDirection.Down:
                    return Vector3.down;
                default:
                    return Vector3.zero;
            }
        }

        Vector3 GetRotationAxis(OneButtonAction.RotationDirection rotateAxis)
        {
            switch (rotateAxis)
            {
                case OneButtonAction.RotationDirection.RollRight:
                    return Vector3.back;
                case OneButtonAction.RotationDirection.RollLeft:
                    return Vector3.forward;
                case OneButtonAction.RotationDirection.PitchUp:
                    return Vector3.left;
                case OneButtonAction.RotationDirection.PitchDown:
                    return Vector3.right;
                case OneButtonAction.RotationDirection.TurnRight:
                    return Vector3.up;
                case OneButtonAction.RotationDirection.TurnLeft:
                    return Vector3.down;
                default:
                    return Vector3.zero;
            }
        }
    }
}
