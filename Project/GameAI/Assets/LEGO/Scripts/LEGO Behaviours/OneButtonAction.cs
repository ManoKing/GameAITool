using Unity.LEGO.Behaviours.Actions;
using Unity.LEGO.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using LEGOModelImporter;
using UnityEngine;

namespace Unity.LEGO.Behaviours.Controls
{
    public class OneButtonAction : MovementAction
    {
        const float k_BumpEpsilon = 0.01f;
        const float k_MaxBumpHeight = 1.2f;
        const float k_GroundingPointThreshold = 1.0f;

        public enum StartMovementType
        {
            No,
            Continuously,
            Distance
        }

        [SerializeField, Tooltip("The movement type to activate on start.")]
        StartMovementType m_StartMovementType = StartMovementType.No;

        public enum StartRotationType
        {
            No,
            Continuously,
            Angle
        }

        [SerializeField, Tooltip("The rotation type to activate on start.")]
        StartRotationType m_StartRotationType = StartRotationType.No;

        public enum MovementType
        {
            NoChange,
            Continuously,
            Distance,
            Stop
        }

        [SerializeField, Tooltip("The movement type to activate when a button is pressed.")]
        MovementType m_PressedMovementType = MovementType.NoChange;

        [SerializeField, Tooltip("The movement type to activate when a button is released.")]
        MovementType m_ReleasedMovementType = MovementType.NoChange;

        public enum RotationType
        {
            NoChange,
            Continuously,
            Angle,
            Stop
        }

        [SerializeField, Tooltip("The rotation type to activate when a button is pressed.")]
        RotationType m_PressedRotationType = RotationType.NoChange;

        [SerializeField, Tooltip("The rotation type to activate when a button is released.")]
        RotationType m_ReleasedRotationType = RotationType.NoChange;

        public enum MovementDirection
        {
            Forward,
            Back,
            Right,
            Left,
            Up,
            Down
        }

        public enum RotationDirection
        {
            TurnRight,
            TurnLeft,
            PitchUp,
            PitchDown,
            RollRight,
            RollLeft
        }

        [SerializeField, Tooltip("Make other bricks behave as if this is the player.")]
        bool m_IsPlayer = true;

        [SerializeField, Tooltip("Applies gravity to the bricks.")]
        bool m_UseGravity = false;

        [SerializeField, Range(0.0f, 80.0f)]
        float m_Gravity = 40.0f;

        [Serializable]
        public class MotionSetting
        {
            [Tooltip("The direction of movement.")]
            public MovementDirection MoveDirection = MovementDirection.Forward;
            [Tooltip("The direction to rotate.")]
            public RotationDirection RotateDirection = RotationDirection.TurnRight;

            [Range(0.0f, 50.0f), Tooltip("The speed in LEGO modules per second.")]
            public uint MoveSpeed = 10;
            [Tooltip("The distance in LEGO modules.")]
            public uint MoveDistance = 10;
            [Range(0.0f, 720.0f), Tooltip("The speed in degrees per second.")]
            public uint RotateSpeed = 180;
            [Tooltip("The angle in degrees.")]
            public uint RotateAngle = 90;
            [Range(0.0f, 50.0f)]
            public uint JumpSpeed = 20;
            public uint MaxJumpsInAir = 0;

            public bool Jump = false;
            [Tooltip("Alternate to opposite movement direction.")]
            public bool MoveAlternate = false;
            [Tooltip("Alternate to opposite rotation direction.")]
            public bool RotateAlternate = false;

            public float PreviousMoveTarget;
            public float PreviousRotateTarget;
        }

        [SerializeField]
        MotionSetting m_StartSetting = new MotionSetting(), m_PressedSetting = new MotionSetting(), m_ReleasedSetting = new MotionSetting();

        class Axis
        {
            public MotionSetting ActiveMovementSetting;
            public MotionSetting ActiveRotationSetting;

            public float MovementDuration;
            public float MovementTime;
            public float MovementSpeed;
            public float RotationDuration;
            public float RotationTime;
            public float RotationSpeed;
        }

        Axis m_XAxis = new Axis(), m_YAxis = new Axis(), m_ZAxis = new Axis();

        enum GroundingState
        {
            Bump,
            Jump,
            Falling,
        }

        LayerMask m_LayerMask;

        List<Vector3> m_LocalGroundingCheckPoints = new List<Vector3>();
        List<Vector3> m_WorldGroundingCheckPoints = new List<Vector3>();
        List<Vector3> m_ActiveGroundingCheckPoints = new List<Vector3>();

        Collider[] m_GroundingColliders = new Collider[32];
        RaycastHit[] m_GroundingRaycastHits = new RaycastHit[32];
        Transform m_GroundedTransform;
        
        Vector3 m_Velocity;
        Vector3 m_FallVelocity;
        float m_RemainingFallDistance;
        int m_JumpCount;
        bool m_InputHeld;

        public MotionSetting GetStartSetting()
        {
            return m_StartSetting;
        }

        public MotionSetting GetPressedSetting()
        {
            return m_PressedSetting;
        }

        public MotionSetting GetReleasedSetting()
        {
            return m_ReleasedSetting;
        }

        public float GetRemainingDistance(MotionSetting setting)
        {
            var axis = m_XAxis;
            if (setting.MoveDirection == MovementDirection.Up || setting.MoveDirection == MovementDirection.Down)
            {
                axis = m_YAxis;
            }
            else if (setting.MoveDirection == MovementDirection.Forward || setting.MoveDirection == MovementDirection.Back)
            {
                axis = m_ZAxis;
            }

            if (axis.ActiveMovementSetting == null || axis.ActiveMovementSetting != setting)
            {
                return setting.MoveDistance;
            }
            return setting.PreviousMoveTarget / axis.MovementDuration * Mathf.Max(0.0f, axis.MovementDuration - axis.MovementTime);
        }

        public float GetRemainingAngle(MotionSetting setting)
        {
            var axis = m_XAxis;
            if (setting.RotateDirection == RotationDirection.TurnRight || setting.RotateDirection == RotationDirection.TurnLeft)
            {
                axis = m_YAxis;
            }
            else if (setting.RotateDirection == RotationDirection.RollRight || setting.RotateDirection == RotationDirection.RollLeft)
            {
                axis = m_ZAxis;
            }

            if (axis.ActiveRotationSetting == null || axis.ActiveRotationSetting != setting)
            {
                return setting.RotateAngle;
            }
            return setting.PreviousRotateTarget / axis.RotationDuration * Mathf.Max(0.0f, axis.RotationDuration - axis.RotationTime);
        }

        public bool IsVerticalMovementEnabled()
        {
            return m_StartSetting.MoveDirection >= MovementDirection.Up && (m_StartMovementType == StartMovementType.Continuously || m_StartMovementType == StartMovementType.Distance) ||
                   m_PressedSetting.MoveDirection >= MovementDirection.Up && (m_PressedMovementType == MovementType.Continuously || m_PressedMovementType == MovementType.Distance) ||
                   m_ReleasedSetting.MoveDirection >= MovementDirection.Up && (m_ReleasedMovementType == MovementType.Continuously || m_ReleasedMovementType == MovementType.Distance);
        }

        protected override void Reset()
        {
            base.Reset();

            m_Repeat = false;
            m_IconPath = "Assets/LEGO/Gizmos/LEGO Behaviour Icons/One Button Action.png";
        }

        protected override void Start()
        {
            base.Start();

            m_LayerMask = LayerMask.GetMask("Environment") | LayerMask.GetMask("Default");

            FindLocalGroundingCheckPoints();

            if (IsPlacedOnBrick() && m_IsPlayer)
            {
                // If is player, set entire scope of colliders to player tag and layer.
                ControlMovementUtilities.SetScopeToPlayer(m_ScopedBricks);
            }

            // Update movement to its start setting.
            var movementType = MovementType.NoChange;
            if (m_StartMovementType != StartMovementType.No)
            {
                movementType = m_StartMovementType == StartMovementType.Continuously ? MovementType.Continuously : MovementType.Distance;
            }
            UpdateMovement(m_StartSetting, movementType);

            // Update rotation to its start setting.
            var rotationType = RotationType.NoChange;
            if (m_StartRotationType != StartRotationType.No)
            {
                rotationType = m_StartRotationType == StartRotationType.Continuously ? RotationType.Continuously : RotationType.Angle;
            }
            UpdateRotation(m_StartSetting, rotationType);

            UpdateGroundingCheckPoints(false);
            UpdateGrounding(k_MaxBumpHeight, GroundingState.Bump);
        }

        void Update()
        {
            if (m_Active)
            {
                // Handle inputs and update control values accordingly.
                HandleInput();

                // Find brick center world pivot.
                var worldPivot = transform.position + transform.TransformVector(m_BrickPivotOffset);

                // Rotation.
                Rotation(m_XAxis, transform.right, worldPivot); // Around X-axis.
                Rotation(m_YAxis, transform.up, worldPivot); // Around Y-axis.
                Rotation(m_ZAxis, transform.forward, worldPivot); // Around Z-axis.

                // Movement.
                var targetVelocity = Movement(m_XAxis, transform.right, true); // On X-axis.
                targetVelocity += Movement(m_YAxis, transform.up, false); // On Y-axis.
                targetVelocity += Movement(m_ZAxis, transform.forward, true); // On Z-axis.

                // Performing jump if fall velocity is positive.
                var isJumping = m_FallVelocity.y > 0.0f;

                // If jumping and colliding, stop jump.
                if (isJumping)
                {
                    if (IsMovementColliding(m_FallVelocity))
                    {
                        m_FallVelocity = Vector3.zero;
                    }
                }

                // Calculate velocity. If remaining fall distance, override velocity to fall with exactly the remaining distance.
                var velocity = (targetVelocity + m_FallVelocity) * Time.deltaTime;
                if (m_RemainingFallDistance > 0.0f)
                {
                    var remainingFallDistance = isJumping ? Vector3.up * m_RemainingFallDistance  : Vector3.down * m_RemainingFallDistance;
                    velocity = targetVelocity * Time.deltaTime + remainingFallDistance;
                    m_RemainingFallDistance = 0.0f;
                }

                // Move bricks. 
                m_Group.transform.position += velocity;

                if (!IsVerticalMovementEnabled() && (m_UseGravity || isJumping))
                {
                    // Update points used to perform grounding checks.
                    UpdateGroundingCheckPoints(isJumping);

                    if (m_UseGravity && !m_GroundedTransform)
                    {
                        m_FallVelocity += Vector3.down * (m_Gravity * Time.deltaTime);
                    }

                    // Update grounding.
                    if (isJumping)
                    {
                        UpdateGrounding(m_FallVelocity.y * Time.deltaTime + Mathf.Epsilon, GroundingState.Jump);
                    }
                    else if (m_UseGravity && !m_GroundedTransform)
                    {
                        UpdateGrounding(-m_FallVelocity.y * Time.deltaTime + Mathf.Epsilon, GroundingState.Falling);

                        if (m_GroundedTransform)
                        {
                            m_FallVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        m_JumpCount = 0;
                        UpdateGrounding(k_MaxBumpHeight, GroundingState.Bump);
                    }
                }

                // Update model position.
                m_MovementTracker.UpdateModelPosition();
            }
        }

        Vector3 Movement(Axis axis, Vector3 direction, bool isHorizontal)
        {
            var velocity = direction * axis.MovementSpeed;
            var absoluteSpeed = Mathf.Abs(axis.MovementSpeed);

            if (axis.MovementDuration > 0.0f && absoluteSpeed > 0.0f) // Continuous if movement duration is 0.
            {
                var targetDistance = absoluteSpeed * axis.MovementDuration;

                if (absoluteSpeed * (axis.MovementTime + Time.deltaTime) > targetDistance)
                {
                    var moveDistance = targetDistance - axis.MovementTime * absoluteSpeed;
                    velocity = Mathf.Sign(axis.MovementSpeed) * (direction * (moveDistance / Time.deltaTime));

                    axis.MovementSpeed = 0.0f;
                    axis.ActiveMovementSetting.PreviousMoveTarget = 0.0f;

                    if (axis.ActiveMovementSetting.MoveAlternate)
                    {
                        axis.ActiveMovementSetting.MoveDirection = GetOppositeMovementDirection(axis.ActiveMovementSetting.MoveDirection);
                    }
                }
            }

            velocity *= isHorizontal ? LEGOHorizontalModule : LEGOVerticalModule;

            if (!IsMovementColliding(velocity.normalized))
            {
                if (absoluteSpeed > 0.0f)
                {
                    axis.MovementTime += Time.deltaTime;
                }

                return velocity;
            }

            return Vector3.zero;
        }

        void Rotation(Axis axis, Vector3 targetAxis, Vector3 worldPivot)
        {
            var angleDelta = axis.RotationSpeed;
            var absoluteSpeed = Mathf.Abs(axis.RotationSpeed);

            if (axis.RotationDuration > 0.0f && absoluteSpeed > 0.0f) // Continuous if rotate duration is 0.
            {
                var targetAngle = absoluteSpeed * axis.RotationDuration;

                if (absoluteSpeed * (axis.RotationTime + Time.deltaTime) > targetAngle)
                {
                    var angle = targetAngle - axis.RotationTime * absoluteSpeed;
                    angleDelta = Mathf.Sign(axis.RotationSpeed) * (angle / Time.deltaTime);

                    axis.RotationSpeed = 0.0f;
                    axis.ActiveRotationSetting.PreviousRotateTarget = 0.0f;

                    if (axis.ActiveRotationSetting.RotateAlternate)
                    {
                        axis.ActiveRotationSetting.RotateDirection = GetOppositeRotationDirection(axis.ActiveRotationSetting.RotateDirection);
                    }
                }
            }

            if (!IsRotationColliding(axis.RotationSpeed, targetAxis))
            {
                // Rotate bricks.
                m_Group.transform.RotateAround(worldPivot, targetAxis, angleDelta * Time.deltaTime);

                // Update axis rotation time.
                if (absoluteSpeed > 0.0f)
                {
                    axis.RotationTime += Time.deltaTime;
                }
            }
        }

        void HandleInput()
        {
            if (Input.anyKeyDown)
            {
                UpdateMovement(m_PressedSetting, m_PressedMovementType);
                UpdateRotation(m_PressedSetting, m_PressedRotationType);

                if (!IsVerticalMovementEnabled() && m_PressedSetting.Jump && m_JumpCount <= m_PressedSetting.MaxJumpsInAir)
                {
                    if (m_PressedSetting.MaxJumpsInAir > 0 || m_GroundedTransform)
                    {
                        m_FallVelocity = Vector3.up * m_PressedSetting.JumpSpeed;
                        m_JumpCount++;
                        m_GroundedTransform = null;
                    }
                }
            }
            else if (!Input.anyKey && m_InputHeld)
            {
                UpdateMovement(m_ReleasedSetting, m_ReleasedMovementType);
                UpdateRotation(m_ReleasedSetting, m_ReleasedRotationType);

                if (!IsVerticalMovementEnabled() && m_ReleasedSetting.Jump && m_JumpCount <= m_ReleasedSetting.MaxJumpsInAir)
                {
                    if (m_ReleasedSetting.MaxJumpsInAir > 0 || m_GroundedTransform)
                    {
                        m_FallVelocity = Vector3.up * m_ReleasedSetting.JumpSpeed;
                        m_JumpCount++;
                        m_GroundedTransform = null;
                    }
                }
            }

            // Check if input key is held down.
            m_InputHeld = Input.anyKey;
        }

        void UpdateMovement(MotionSetting setting, MovementType type)
        {
            if (type != MovementType.NoChange)
            {
                switch (setting.MoveDirection)
                {
                    case MovementDirection.Right: // X-axis
                    case MovementDirection.Left:
                    {
                        if (type == MovementType.Stop)
                        {
                            m_XAxis.MovementSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_XAxis.ActiveMovementSetting;
                            m_XAxis.ActiveMovementSetting = setting;
                            SetMovementAxis(m_XAxis, previousSetting, type, MovementDirection.Right);
                        }
                        break;
                    }
                    case MovementDirection.Up: // Y-axis
                    case MovementDirection.Down:
                    {
                        if (type == MovementType.Stop)
                        {
                            m_YAxis.MovementSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_YAxis.ActiveMovementSetting;
                            m_YAxis.ActiveMovementSetting = setting;
                            SetMovementAxis(m_YAxis, previousSetting, type, MovementDirection.Up);
                        }
                        break;
                    }
                    case MovementDirection.Forward: // Z-axis
                    case MovementDirection.Back:
                    {
                        if (type == MovementType.Stop)
                        {
                            m_ZAxis.MovementSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_ZAxis.ActiveMovementSetting;
                            m_ZAxis.ActiveMovementSetting = setting;
                            SetMovementAxis(m_ZAxis, previousSetting, type, MovementDirection.Forward);
                        }
                        break;
                    }
                }
            }
        }

        static void SetMovementAxis(Axis axis, MotionSetting previousSetting, MovementType type, MovementDirection positiveDirection)
        {
            var newSetting = axis.ActiveMovementSetting;

            var moveDuration = (float)newSetting.MoveDistance / newSetting.MoveSpeed;

            if (newSetting.MoveAlternate)
            {
                var previouslyMoved = newSetting.PreviousMoveTarget > 0.0f;

                if (newSetting != previousSetting && previousSetting != null)
                {
                    if (previousSetting.MoveAlternate)
                    {
                        previousSetting.MoveDirection = GetOppositeMovementDirection(previousSetting.MoveDirection);
                    }
                }
                else
                {
                    newSetting.MoveDirection = previouslyMoved ? GetOppositeMovementDirection(newSetting.MoveDirection) : newSetting.MoveDirection;
                }

                if (type == MovementType.Distance)
                {
                    // If MovementRemaining > 0, the movement was interrupted before reaching goal.
                    var remainingDistance = previouslyMoved ? newSetting.PreviousMoveTarget - axis.MovementTime * newSetting.MoveSpeed : newSetting.MoveDistance;
                    var moveDistance = newSetting.MoveDistance - remainingDistance;

                    var movementInterrupted = moveDistance > 0.0f && newSetting == previousSetting;
                    axis.MovementDuration = movementInterrupted ? moveDistance / newSetting.MoveSpeed : moveDuration;
                    newSetting.PreviousMoveTarget = movementInterrupted ? moveDistance : newSetting.MoveDistance;
                }
                else
                {
                    // Set duration to 0 if movement is continuous.
                    axis.MovementDuration = 0.0f;
                    newSetting.PreviousMoveTarget = float.MaxValue;
                }
            }
            else
            {
                // Set duration to 0 if movement is continuous or no movement speed.
                axis.MovementDuration = type == MovementType.Distance ? moveDuration : 0.0f;
                newSetting.PreviousMoveTarget = newSetting.MoveDistance;
            }

            // Set direction of movement speed and reset movement time.
            axis.MovementSpeed = newSetting.MoveDirection == positiveDirection ? newSetting.MoveSpeed : -newSetting.MoveSpeed;
            axis.MovementTime = 0.0f;
        }

        void UpdateRotation(MotionSetting setting, RotationType type)
        {
            if (type != RotationType.NoChange)
            {
                switch (setting.RotateDirection)
                {
                    case RotationDirection.PitchUp: // X-axis
                    case RotationDirection.PitchDown:
                    {
                        if (type == RotationType.Stop)
                        {
                            m_XAxis.RotationSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_XAxis.ActiveRotationSetting;
                            m_XAxis.ActiveRotationSetting = setting;
                            SetRotationAxis(m_XAxis, previousSetting, type, RotationDirection.PitchDown);
                        }
                        break;
                    }
                    case RotationDirection.TurnRight: // Y-axis
                    case RotationDirection.TurnLeft:
                    {
                        if (type == RotationType.Stop)
                        {
                            m_YAxis.RotationSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_YAxis.ActiveRotationSetting;
                            m_YAxis.ActiveRotationSetting = setting;
                            SetRotationAxis(m_YAxis, previousSetting, type, RotationDirection.TurnRight);
                        }
                        break;
                    }
                    case RotationDirection.RollRight: // Z-axis
                    case RotationDirection.RollLeft:
                    {
                        if (type == RotationType.Stop)
                        {
                            m_ZAxis.RotationSpeed = 0.0f;
                        }
                        else
                        {
                            var previousSetting = m_ZAxis.ActiveRotationSetting;
                            m_ZAxis.ActiveRotationSetting = setting;
                            SetRotationAxis(m_ZAxis, previousSetting, type, RotationDirection.RollLeft);
                        }
                        break;
                    }
                }
            }
        }

        static void SetRotationAxis(Axis axis, MotionSetting previousSetting, RotationType type, RotationDirection positiveDirection)
        {
            var newSetting = axis.ActiveRotationSetting;

            var rotateDuration = (float)newSetting.RotateAngle / newSetting.RotateSpeed;

            if (newSetting.RotateAlternate)
            {
                var hasMoved = newSetting.PreviousRotateTarget > 0.0f;

                if (newSetting != previousSetting && previousSetting != null)
                {
                    if (previousSetting.RotateAlternate)
                    {
                        previousSetting.RotateDirection = GetOppositeRotationDirection(previousSetting.RotateDirection);
                    }
                }
                else
                {
                    newSetting.RotateDirection = hasMoved ? GetOppositeRotationDirection(newSetting.RotateDirection) : newSetting.RotateDirection;
                }

                if (type == RotationType.Angle)
                {
                    // If RotationRemaining > 0, the rotation was interrupted before reaching goal.
                    var remainingAngle = hasMoved ? newSetting.PreviousRotateTarget - axis.RotationTime * newSetting.RotateSpeed : newSetting.RotateAngle;
                    var rotateAngle = newSetting.RotateAngle - remainingAngle;

                    var rotationInterrupted = rotateAngle > 0.0f && newSetting == previousSetting;
                    axis.RotationDuration = rotationInterrupted ? rotateAngle / newSetting.RotateSpeed : rotateDuration;
                    newSetting.PreviousRotateTarget = rotationInterrupted ? rotateAngle : newSetting.RotateAngle;
                }
                else
                {
                    // Set duration to 0 if movement is continuous.
                    axis.RotationDuration = 0.0f;
                    newSetting.PreviousRotateTarget = newSetting.RotateAngle;
                }
            }
            else
            {
                // Set duration to 0 if movement is continuous or no movement speed.
                axis.RotationDuration = type == RotationType.Angle ? rotateDuration : 0.0f;
                newSetting.PreviousRotateTarget = newSetting.RotateAngle;
            }

            // Set direction of rotation speed and reset rotation time.
            axis.RotationSpeed = newSetting.RotateDirection == positiveDirection ? newSetting.RotateSpeed : -newSetting.RotateSpeed;
            axis.RotationTime = 0.0f;
        }

        bool IsMovementColliding(Vector3 targetDirection)
        {
            if (IsColliding())
            {
                foreach (var activeColliderPair in m_ActiveColliderPairs)
                {
                    if (Physics.ComputePenetration(activeColliderPair.Item1, activeColliderPair.Item1.transform.position, activeColliderPair.Item1.transform.rotation,
                        activeColliderPair.Item2, activeColliderPair.Item2.transform.position, activeColliderPair.Item2.transform.rotation, out Vector3 direction, out _))
                    {
                        if (Vector3.Dot(direction, targetDirection) < -0.0001f)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool IsRotationColliding(float angle, Vector3 axis)
        {
            if (IsColliding())
            {
                foreach (var activeColliderPair in m_ActiveColliderPairs)
                {
                    if (Physics.ComputePenetration(activeColliderPair.Item1, activeColliderPair.Item1.transform.position, activeColliderPair.Item1.transform.rotation,
                        activeColliderPair.Item2, activeColliderPair.Item2.transform.position, activeColliderPair.Item2.transform.rotation, out Vector3 direction, out _))
                    {
                        // Attempt to find a point to represent the collision. This is an approximation.
                        Vector3 point;
                        var center = GetBrickCenter();
                        var colliderType = activeColliderPair.Item2.GetType();
                        if (colliderType == typeof(BoxCollider) || colliderType == typeof(SphereCollider) || colliderType == typeof(CapsuleCollider) || (colliderType == typeof(MeshCollider) && ((MeshCollider)activeColliderPair.Item2).convex))
                        {
                            point = activeColliderPair.Item2.ClosestPoint(center);
                        }
                        else
                        {
                            point = activeColliderPair.Item2.ClosestPointOnBounds(center);
                        }
                        point = activeColliderPair.Item1.ClosestPoint(point);

                        // Compute linear velocity of point as a result of the current rotation. This is again an approximation.
                        var rotatedPoint = Quaternion.AngleAxis(angle, axis) * (point - center) + center;
                        var velocity = rotatedPoint - point;
                        if (Vector3.Dot(direction, velocity) < -0.0001f)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        void UpdateGrounding(float rayExtension, GroundingState state)
        {
            var minDistance = float.MaxValue;

            foreach (var point in m_ActiveGroundingCheckPoints)
            {
                var numColliders = Physics.OverlapSphereNonAlloc(point, Mathf.Epsilon, m_GroundingColliders, m_LayerMask, QueryTriggerInteraction.Ignore);
                var ignorePoint = false;

                for (var i = 0; i < numColliders; ++i)
                {
                    if (!m_ScopedBricks.Contains(m_GroundingColliders[i].transform.GetComponentInParent<Brick>()))
                    {
                        ignorePoint = true;
                        break;
                    }
                }

                if (!ignorePoint)
                {
                    var direction = state == GroundingState.Jump ? Vector3.up : Vector3.down;
                    var numHits = Physics.RaycastNonAlloc(point, direction, m_GroundingRaycastHits, k_MaxBumpHeight + rayExtension, m_LayerMask, QueryTriggerInteraction.Ignore);

                    for (var i = 0; i < numHits; ++i)
                    {
                        var hit = m_GroundingRaycastHits[i];
                        if (!m_ScopedBricks.Contains(hit.transform.GetComponentInParent<Brick>()))
                        {
                            if (hit.distance < minDistance)
                            {
                                minDistance = hit.distance;

                                if (state != GroundingState.Jump)
                                {
                                    m_GroundedTransform = hit.collider.transform;
                                }
                            }
                        }
                    }
                }
            }

            if (minDistance < float.MaxValue)
            {
                var distance = k_MaxBumpHeight - minDistance;

                switch (state)
                {
                    case GroundingState.Bump:
                        if (Mathf.Abs(distance) < k_MaxBumpHeight)
                        {
                            // Bump.
                            m_Group.transform.position += Vector3.up * (distance + k_BumpEpsilon);
                        }
                        break;
                    case GroundingState.Jump:
                        m_RemainingFallDistance = Mathf.Abs(distance);
                        m_FallVelocity = Vector3.zero;
                        break;
                    case GroundingState.Falling:
                        m_RemainingFallDistance = Mathf.Abs(distance);
                        break;
                }
            }
            else
            {
                m_GroundedTransform = null;
            }
        }

        void UpdateGroundingCheckPoints(bool isJumping)
        {
            // Update points used to perform grounding checks.
            for (var i = 0; i < m_WorldGroundingCheckPoints.Count; i++)
            {
                m_WorldGroundingCheckPoints[i] = transform.TransformPoint(m_LocalGroundingCheckPoints[i]);
            }

            // Find points lower than max height.
            var samplePoint = isJumping ? m_WorldGroundingCheckPoints.Max(worldPoint => worldPoint.y) : m_WorldGroundingCheckPoints.Min(worldPoint => worldPoint.y);
            var pointThreshold = isJumping ? samplePoint - k_GroundingPointThreshold : samplePoint + k_GroundingPointThreshold;

            // Update active grounding check points list.
            m_ActiveGroundingCheckPoints.Clear();
            for (var i = 0; i < m_WorldGroundingCheckPoints.Count; i++)
            {
                if (isJumping ? m_WorldGroundingCheckPoints[i].y > pointThreshold : m_WorldGroundingCheckPoints[i].y < pointThreshold)
                {
                    var direction = isJumping ? Vector3.down : Vector3.up;
                    var activePoint = m_WorldGroundingCheckPoints[i] + direction * k_MaxBumpHeight;

                    m_ActiveGroundingCheckPoints.Add(activePoint);
                }
            }
        }

        void FindLocalGroundingCheckPoints()
        {
            var collidersComponents = m_Group.GetComponentsInChildren<Colliders>();

            var brickColliders = new List<Collider>();
            foreach (var collidersComponent in collidersComponents)
            {
                brickColliders.AddRange(collidersComponent.colliders);
            }

            // Get all corner points from brick colliders.
            m_LocalGroundingCheckPoints = ControlMovementUtilities.GetColliderCornerPoints(brickColliders, transform);

            // Remove duplicate points to only get outer points on the model.
            m_LocalGroundingCheckPoints = ControlMovementUtilities.RemoveInnerPoints(m_LocalGroundingCheckPoints);

            foreach (var localPoint in m_LocalGroundingCheckPoints)
            {
                var worldPoint = transform.TransformPoint(localPoint);
                m_WorldGroundingCheckPoints.Add(worldPoint);
            }
        }

        static MovementDirection GetOppositeMovementDirection(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Forward:
                    return MovementDirection.Back;
                case MovementDirection.Back:
                    return MovementDirection.Forward;
                case MovementDirection.Right:
                    return MovementDirection.Left;
                case MovementDirection.Left:
                    return MovementDirection.Right;
                case MovementDirection.Up:
                    return MovementDirection.Down;
                case MovementDirection.Down:
                    return MovementDirection.Up;
                default:
                    return direction;
            }
        }

        static RotationDirection GetOppositeRotationDirection(RotationDirection direction)
        {
            switch (direction)
            {
                case RotationDirection.TurnRight:
                    return RotationDirection.TurnLeft;
                case RotationDirection.TurnLeft:
                    return RotationDirection.TurnRight;
                case RotationDirection.PitchUp:
                    return RotationDirection.PitchDown;
                case RotationDirection.PitchDown:
                    return RotationDirection.PitchUp;
                case RotationDirection.RollRight:
                    return RotationDirection.RollLeft;
                case RotationDirection.RollLeft:
                    return RotationDirection.RollRight;
                default:
                    return direction;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_IsPlayer)
            {
                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                EventManager.Broadcast(evt);
            }
        }
    }
}
