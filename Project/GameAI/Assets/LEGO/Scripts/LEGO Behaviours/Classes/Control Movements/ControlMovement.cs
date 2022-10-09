using LEGOModelImporter;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.LEGO.Behaviours.Controls
{
    public abstract class ControlMovement : MonoBehaviour
    {
        protected const float k_RotationBounceDampingSpeed = 1.0f;

        protected ModelGroup m_Group;

        protected Camera m_MainCamera;

        protected Vector3 m_BrickPivotOffset;
        protected Vector3 m_Velocity;
        protected Vector3 m_CollisionVelocity;

        protected float m_RotationBounceAngle;
        protected float m_RotationBounceDamping;

        protected bool m_CameraRelativeMovement;
        protected bool m_CameraAlignedRotation;

        public virtual void Setup(ModelGroup group, HashSet<Brick> bricks, List<MeshRenderer> scopedPartRenderers, Vector3 brickPivotOffset, Bounds scopedBounds, bool cameraAlignedRotation, bool cameraRelativeMovement, float gravity)
        {
            m_Group = group;
            m_BrickPivotOffset = brickPivotOffset;
            m_CameraAlignedRotation = cameraAlignedRotation;
            m_CameraRelativeMovement = cameraRelativeMovement;

            m_MainCamera = Camera.main;
        }

        public abstract void Movement(Vector3 targetDirection, float minSpeed, float maxSpeed, float idleSpeed, float jumpSpeed, int maxJumpsInAir);
        public abstract void Rotation(Vector3 targetDirection, float rotationSpeed);
        public abstract void Collision(Vector3 direction);

        public Vector3 GetVelocity()
        {
            return m_Velocity + m_CollisionVelocity;
        }

        protected void RotationBounce(Vector3 pivot, Vector3 axis)
        {
            m_RotationBounceDamping = ControlMovementUtilities.Acceleration(0.0f, m_RotationBounceDamping, k_RotationBounceDampingSpeed);
            m_RotationBounceAngle *= m_RotationBounceDamping;

            m_Group.transform.RotateAround(pivot, axis, m_RotationBounceAngle * Time.deltaTime);
        }
    }
}
