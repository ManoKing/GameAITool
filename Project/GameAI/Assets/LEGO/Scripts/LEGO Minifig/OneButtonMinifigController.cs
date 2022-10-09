using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LEGO.Minifig
{
    public class OneButtonMinifigController : MinifigController
    {
        const float LEGOHorizontalModule = 0.8f;
        const float ColliderOffsetEpsilon = 0.1f;

        public enum StartMovementType
        {
            No,
            Continuously,
            Distance
        }

        [SerializeField, Tooltip("The movement type to activate on start.")]
        StartMovementType startMovementType = StartMovementType.No;

        public enum StartRotationType
        {
            No,
            Continuously,
            Angle
        }

        [SerializeField, Tooltip("The rotation type to activate on start.")]
        StartRotationType startRotationType = StartRotationType.No;

        public enum MovementType
        {
            NoChange,
            Continuously,
            Distance,
            Stop
        }

        [SerializeField, Tooltip("The movement type to activate when a button is pressed.")]
        MovementType pressedMovementType = MovementType.NoChange;

        [SerializeField, Tooltip("The movement type to activate when a button is released.")]
        MovementType releasedMovementType = MovementType.NoChange;

        public enum RotationType
        {
            NoChange,
            Continuously,
            Angle,
            Stop
        }

        [SerializeField, Tooltip("The rotation type to activate when a button is pressed.")]
        RotationType pressedRotationType = RotationType.NoChange;

        [SerializeField, Tooltip("The rotation type to activate when a button is released.")]
        RotationType releasedRotationType = RotationType.NoChange;

        public enum MovementDirection
        {
            Forward,
            Back,
            Right,
            Left
        }

        public enum RotationDirection
        {
            TurnRight,
            TurnLeft
        }

        [Serializable]
        public class MotionSetting
        {
            [Tooltip("The direction of movement.")]
            public MovementDirection moveDirection = MovementDirection.Forward;
            [Tooltip("The direction to rotate.")]
            public RotationDirection rotateDirection = RotationDirection.TurnRight;

            [Range(0.0f, 50f), Tooltip("The speed in units per second.")]
            public uint moveMaxSpeed = 10;
            [Tooltip("The distance in LEGO modules.")]
            public uint moveDistance = 10;
            [Range(1.0f, 60.0f)]
            public uint moveAcceleration = 20;
            [Range(0.0f, 720.0f), Tooltip("The speed in degrees per second.")]
            public uint rotateSpeed = 180;
            [Tooltip("The angle in degrees.")]
            public uint rotateAngle = 90;

            public bool jump = false;
            [Tooltip("Alternate to opposite movement direction.")]
            public bool moveAlternate = false;
            [Tooltip("Alternate to opposite rotation direction.")]
            public bool rotateAlternate = false;

            public float previousMoveTarget;
            public float previousRotateTarget;
        }

        [SerializeField]
        MotionSetting startSetting = new MotionSetting(), pressedSetting = new MotionSetting(), releasedSetting = new MotionSetting();

        class Axis
        {
            public MotionSetting activeMovementSetting;
            public MovementType movementType;

            public float movementTraveled;
            public float movementTargetSpeed;
            public float movementCurrentSpeed;
            public float movementTargetDistance;
        }

        Axis xAxis = new Axis(), zAxis = new Axis();

        BoxCollider mainCollider;
        HashSet<Collider> activeCollisions = new HashSet<Collider>();

        MotionSetting activeRotationSetting;

        float rotationDuration;
        float rotationTime;
        float rotationSpeed;
        bool inputHeld;
        bool performJump;

        void Start()
        {
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;

            // Add trigger collider surrounding minifig to be used for collision detection.
            mainCollider = gameObject.AddComponent<BoxCollider>();
            mainCollider.size = new Vector3(controller.radius * 2.5f, controller.height - controller.stepOffset, controller.radius * 2.5f);
            mainCollider.center = controller.center + Vector3.up * (controller.stepOffset / 2.0f - ColliderOffsetEpsilon);
            mainCollider.isTrigger = true;

            var movementType = MovementType.NoChange;
            if (startMovementType != StartMovementType.No)
            {
                movementType = startMovementType == StartMovementType.Continuously ? MovementType.Continuously : MovementType.Distance;
            }

            var rotationType = RotationType.NoChange;
            if (startRotationType != StartRotationType.No)
            {
                rotationType = startRotationType == StartRotationType.Continuously ? RotationType.Continuously : RotationType.Angle;
            }

            // Update movement and rotation to its start settings.
            UpdateMovement(startSetting, movementType);
            UpdateRotation(startSetting, rotationType);
        }

        protected override void Update()
        {
            if (exploded)
            {
                return;
            }

            // Handle input.
            if (inputEnabled)
            {
                HandleInput();

                var targetVelocity = Movement(xAxis, transform.right);
                targetVelocity += Movement(zAxis, transform.forward);
                directSpeed = targetVelocity;

                speed = directSpeed.magnitude;

                // Set rotation speed.
                rotateSpeed = Rotation();

                // Calculate move delta.
                moveDelta = new Vector3(directSpeed.x, moveDelta.y, directSpeed.z);

                // Check if player is grounded.
                if (!airborne)
                {
                    jumpsInAir = maxJumpsInAir;
                }

                // Check if player is jumping.
                if (performJump)
                {
                    performJump = false;

                    if (!airborne || jumpsInAir > 0)
                    {
                        if (airborne)
                        {
                            jumpsInAir--;

                            if (doubleJumpAudioClip)
                            {
                                audioSource.PlayOneShot(doubleJumpAudioClip);
                            }
                        }
                        else
                        {
                            if (jumpAudioClip)
                            {
                                audioSource.PlayOneShot(jumpAudioClip);
                            }
                        }

                        moveDelta.y = jumpSpeed;
                        animator.SetTrigger(jumpHash);

                        airborne = true;
                        airborneTime = coyoteDelay;
                    }
                }

                // Cancel special.
                cancelSpecial = Mathf.Approximately(xAxis.movementCurrentSpeed, 0.0f) && Mathf.Approximately(zAxis.movementCurrentSpeed, 0.0f);
            }
            else
            {
                HandleAutomaticAnimation();
            }

            HandleMotion();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger && !other.gameObject.CompareTag("Projectile") && !other.gameObject.CompareTag("Player"))
            {
                activeCollisions.Add(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.isTrigger && !other.gameObject.CompareTag("Projectile") && !other.gameObject.CompareTag("Player"))
            {
                activeCollisions.Remove(other);
            }
        }

        Vector3 Movement(Axis axis, Vector3 direction)
        {
            if (axis.activeMovementSetting != null)
            {
                // Distance required to stop current speed.
                var absoluteSpeed = Mathf.Abs(axis.movementCurrentSpeed);
                var breakDistance = (absoluteSpeed * absoluteSpeed) / (2.0f * axis.activeMovementSetting.moveAcceleration);

                // If breaking before target, only set target speed if still possible to break.
                var targetSpeed = 0.0f;
                var remainingDistance = axis.movementTargetDistance - axis.movementTraveled;
                if (remainingDistance > breakDistance || axis.movementType == MovementType.Continuously)
                {
                    targetSpeed = axis.movementTargetSpeed;
                }

                // Adjust speed based on target speed.
                if (targetSpeed > axis.movementCurrentSpeed)
                {
                    axis.movementCurrentSpeed = Mathf.Min(targetSpeed, axis.movementCurrentSpeed + axis.activeMovementSetting.moveAcceleration * Time.deltaTime);
                }
                else if (targetSpeed < axis.movementCurrentSpeed)
                {
                    axis.movementCurrentSpeed = Mathf.Max(targetSpeed, axis.movementCurrentSpeed - axis.activeMovementSetting.moveAcceleration * Time.deltaTime);
                }

                // Calculate move delta - prevent overshoot by limiting speed.
                if (remainingDistance > 0.0f && axis.movementType == MovementType.Distance)
                {
                    axis.movementCurrentSpeed = Mathf.Sign(axis.movementCurrentSpeed) * Mathf.Min(Mathf.Abs(axis.movementCurrentSpeed), remainingDistance / Time.deltaTime);
                }

                var velocity = direction * axis.movementCurrentSpeed;

                if (!IsColliding(velocity))
                {
                    absoluteSpeed = Mathf.Abs(axis.movementCurrentSpeed);
                    if (absoluteSpeed > 0.0f)
                    {
                        axis.movementTraveled += absoluteSpeed * Time.deltaTime;
                    }

                    if (axis.activeMovementSetting.previousMoveTarget > 0.0f && axis.movementTraveled >= axis.movementTargetDistance)
                    {
                        axis.activeMovementSetting.previousMoveTarget = 0.0f;
                        axis.activeMovementSetting.moveDirection = GetOppositeMovementDirection(axis.activeMovementSetting.moveDirection);
                    }

                    return velocity;
                }
            }

            axis.movementCurrentSpeed = 0.0f;
            return Vector3.zero;
        }

        float Rotation()
        {
            var angleDelta = rotationSpeed;
            var absoluteSpeed = Mathf.Abs(rotationSpeed);

            if (rotationDuration > 0.0f && absoluteSpeed > 0.0f) // Continuous if rotate duration is 0.
            {
                var targetAngle = absoluteSpeed * rotationDuration;

                if (absoluteSpeed * (rotationTime + Time.deltaTime) > targetAngle)
                {
                    var angle = targetAngle - rotationTime * absoluteSpeed;
                    angleDelta = Mathf.Sign(rotationSpeed) * (angle / Time.deltaTime);

                    rotationSpeed = 0.0f;
                    activeRotationSetting.previousRotateTarget = 0.0f;

                    if (activeRotationSetting.rotateAlternate)
                    {
                        activeRotationSetting.rotateDirection = GetOppositeRotationDirection(activeRotationSetting.rotateDirection);
                    }
                }

                rotationTime += Time.deltaTime;
            }

            return angleDelta;
        }

        void HandleInput()
        {
            if (Input.anyKeyDown)
            {
                UpdateMovement(pressedSetting, pressedMovementType);
                UpdateRotation(pressedSetting, pressedRotationType);

                if (pressedSetting.jump)
                {
                    performJump = true;
                }
            }
            else if (!Input.anyKey && inputHeld)
            {
                UpdateMovement(releasedSetting, releasedMovementType);
                UpdateRotation(releasedSetting, releasedRotationType);

                if (releasedSetting.jump)
                {
                    performJump = true;
                }
            }

            // Check if input key is held down.
            inputHeld = Input.anyKey;
        }

        void UpdateMovement(MotionSetting setting, MovementType type)
        {
            if (type != MovementType.NoChange)
            {
                switch (setting.moveDirection)
                {
                    case MovementDirection.Right: // X-axis
                    case MovementDirection.Left:
                        {
                            if (type == MovementType.Stop)
                            {
                                xAxis.movementTargetSpeed = 0.0f;
                            }
                            else
                            {
                                var previousSetting = xAxis.activeMovementSetting;
                                xAxis.activeMovementSetting = setting;
                                SetMovementAxis(xAxis, previousSetting, type, MovementDirection.Right);
                            }
                            break;
                        }
                    case MovementDirection.Forward: // Z-axis
                    case MovementDirection.Back:
                        {
                            if (type == MovementType.Stop)
                            {
                                zAxis.movementTargetSpeed = 0.0f;
                            }
                            else
                            {
                                var previousSetting = zAxis.activeMovementSetting;
                                zAxis.activeMovementSetting = setting;
                                SetMovementAxis(zAxis, previousSetting, type, MovementDirection.Forward);
                            }
                            break;
                        }
                }
            }
        }

        static void SetMovementAxis(Axis axis, MotionSetting previousSetting, MovementType type, MovementDirection positiveDirection)
        {
            var newSetting = axis.activeMovementSetting;
            axis.movementType = type;

            var moveDistanceInLEGOModules = newSetting.moveDistance * LEGOHorizontalModule;

            if (newSetting.moveAlternate)
            {
                var previouslyMoved = newSetting.previousMoveTarget > 0.0f;

                if (newSetting != previousSetting && previousSetting != null)
                {
                    if (previousSetting.moveAlternate)
                    {
                        previousSetting.moveDirection = GetOppositeMovementDirection(previousSetting.moveDirection);
                    }
                }
                else
                {
                    newSetting.moveDirection = previouslyMoved ? GetOppositeMovementDirection(newSetting.moveDirection) : newSetting.moveDirection;
                }

                if (type == MovementType.Distance)
                {
                    // If MovementRemaining > 0, the movement was interrupted before reaching goal.
                    var remainingDistance = previouslyMoved ? newSetting.previousMoveTarget - axis.movementTraveled : moveDistanceInLEGOModules;
                    var moveDistance = moveDistanceInLEGOModules - remainingDistance;

                    var movementInterrupted = moveDistance > 0.0f && newSetting == previousSetting;
                    axis.movementTargetDistance = movementInterrupted ? moveDistance : moveDistanceInLEGOModules;
                    newSetting.previousMoveTarget = axis.movementTargetDistance;
                    axis.movementCurrentSpeed = 0.0f;
                }
                else
                {
                    newSetting.previousMoveTarget = float.MaxValue;
                }
            }
            else
            {
                axis.movementTargetDistance = moveDistanceInLEGOModules;
            }

            // Set direction of movement speed and reset movement traveled.
            axis.movementTargetSpeed = newSetting.moveDirection == positiveDirection ? newSetting.moveMaxSpeed : -newSetting.moveMaxSpeed;
            axis.movementTraveled = 0.0f;
        }

        void UpdateRotation(MotionSetting setting, RotationType type)
        {
            if (type != RotationType.NoChange)
            {
                if (type == RotationType.Stop)
                {
                    rotationSpeed = 0.0f;
                }
                else
                {
                    var previousSetting = activeRotationSetting;
                    activeRotationSetting = setting;

                    var rotateDuration = (float)setting.rotateAngle / setting.rotateSpeed;

                    if (setting.rotateAlternate)
                    {
                        var hasMoved = setting.previousRotateTarget > 0.0f;

                        if (setting != previousSetting && previousSetting != null)
                        {
                            if (previousSetting.rotateAlternate)
                            {
                                previousSetting.rotateDirection = GetOppositeRotationDirection(previousSetting.rotateDirection);
                            }
                        }
                        else
                        {
                            setting.rotateDirection = hasMoved ? GetOppositeRotationDirection(setting.rotateDirection) : setting.rotateDirection;
                        }

                        if (type == RotationType.Angle)
                        {
                            // If RotationRemaining > 0, the rotation was interrupted before reaching goal.
                            var remainingAngle = hasMoved ? setting.previousRotateTarget - rotationTime * setting.rotateSpeed : setting.rotateAngle;
                            var rotateAngle = setting.rotateAngle - remainingAngle;

                            var rotationInterrupted = rotateAngle > 0.0f;
                            rotationDuration = rotationInterrupted ? rotateAngle / setting.rotateSpeed : rotateDuration;
                            setting.previousRotateTarget = rotationInterrupted ? rotateAngle : setting.rotateAngle;
                        }
                        else
                        {
                            // Set duration to 0 if movement is continuous.
                            rotationDuration = 0.0f;
                            setting.previousRotateTarget = setting.rotateAngle;
                        }
                    }
                    else
                    {
                        // Set duration to 0 if movement is continuous or no movement speed.
                        rotationDuration = type == RotationType.Angle ? rotateDuration : 0.0f;
                        setting.previousRotateTarget = setting.rotateAngle;
                    }

                    // Set direction of rotation speed and reset rotation time.
                    rotationSpeed = setting.rotateDirection == RotationDirection.TurnRight ? setting.rotateSpeed : -setting.rotateSpeed;
                    rotationTime = 0.0f;
                }
            }
        }

        bool IsColliding(Vector3 targetDirection)
        {
            foreach (var activeCollider in activeCollisions)
            {
                if (Physics.ComputePenetration(mainCollider, mainCollider.transform.position, mainCollider.transform.rotation,
                    activeCollider, activeCollider.transform.position, activeCollider.transform.rotation, out Vector3 direction, out _))
                {
                    var dot = Vector3.Dot(direction, targetDirection);

                    if (dot < -0.0001f)
                    {
                        return true;
                    }
                }
            }

            return false;
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
                default:
                    return direction;
            }
        }
    }
}
