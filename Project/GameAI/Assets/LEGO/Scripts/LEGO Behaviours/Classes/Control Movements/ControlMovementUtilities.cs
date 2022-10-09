using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LEGOModelImporter;
using UnityEngine;

namespace Unity.LEGO.Behaviours.Controls
{
    public static class ControlMovementUtilities
    {
        public static void SetScopeToPlayer(HashSet<Brick> scopedBricks)
        {
            // Tag all the part colliders to make other LEGO Behaviours act as if this is the player.
            foreach (var brick in scopedBricks)
            {
                foreach (var part in brick.parts)
                {
                    foreach (var collider in part.colliders)
                    {
                        collider.gameObject.tag = "Player";
                        collider.gameObject.layer = LayerMask.NameToLayer("Player");
                    }
                }
            }
        }

        public static List<Vector3> GetColliderCornerPoints(List<Collider> colliders, Transform rootTransform)
        {
            var result = new List<Vector3>();

            foreach (var collider in colliders)
            {
                var colliderType = collider.GetType();
                if (colliderType == typeof(BoxCollider))
                {
                    var boxCollider = (BoxCollider)collider;
                    var colliderSize = new Vector3(boxCollider.size.x + 0.1f, boxCollider.size.y, boxCollider.size.z + 0.1f);
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(-0.5f, -0.5f, -0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(-0.5f, -0.5f, 0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(-0.5f, 0.5f, -0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(-0.5f, 0.5f, 0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(0.5f, -0.5f, -0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(0.5f, -0.5f, 0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(0.5f, 0.5f, -0.5f), colliderSize))));
                    result.Add(rootTransform.InverseTransformPoint(boxCollider.transform.TransformPoint(boxCollider.center + Vector3.Scale(new Vector3(0.5f, 0.5f, 0.5f), colliderSize))));
                }
                else if (colliderType == typeof(SphereCollider))
                {
                    var sphereCollider = (SphereCollider)collider;
                    sphereCollider.radius += 0.1f;
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(1.0f, 0.0f, 0.0f) * sphereCollider.radius));
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(-1.0f, 0.0f, 0.0f) * sphereCollider.radius));
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(0.0f, 1.0f, 0.0f) * sphereCollider.radius));
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(0.0f, -1.0f, 0.0f) * sphereCollider.radius));
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(0.0f, 0.0f, 1.0f) * sphereCollider.radius));
                    result.Add(rootTransform.InverseTransformPoint(sphereCollider.transform.TransformPoint(sphereCollider.center) + new Vector3(0.0f, 0.0f, -1.0f) * sphereCollider.radius));
                }
            }

            return result;
        }

        public static Vector3 Acceleration(Vector3 targetVelocity, Vector3 currentVelocity, float acceleration)
        {
            var speedDiff = targetVelocity - currentVelocity;
            if (speedDiff.sqrMagnitude < acceleration * acceleration * Time.deltaTime * Time.deltaTime)
            {
                currentVelocity = targetVelocity;
            }
            else if (speedDiff.sqrMagnitude > 0.0f)
            {
                speedDiff.Normalize();
                currentVelocity += speedDiff * acceleration * Time.deltaTime;
            }

            return currentVelocity;
        }

        public static float Acceleration(float targetSpeed, float currentSpeed, float acceleration)
        {
            var speedDiff = targetSpeed - currentSpeed;
            if (Mathf.Abs(speedDiff) < acceleration * acceleration * Time.deltaTime * Time.deltaTime)
            {
                currentSpeed = targetSpeed;
            }
            else if (Mathf.Abs(speedDiff) > 0.0f)
            {
                currentSpeed += Mathf.Sign(speedDiff) * acceleration * Time.deltaTime;
            }

            return currentSpeed;
        }

        public static List<Vector3> RemoveInnerPoints(List<Vector3> points, float threshold = 0.2f)
        {
            var pointsToBeRemoved = new HashSet<Vector3>();

            for (var i = 0; i < points.Count; i++)
            {
                for (var j = 0; j < points.Count; j++)
                {
                    if (i != j)
                    {
                        if (Mathf.Abs(points[i].x - points[j].x) < threshold &&
                            Mathf.Abs(points[i].y - points[j].y) < threshold &&
                            Mathf.Abs(points[i].z - points[j].z) < threshold)
                        {
                            pointsToBeRemoved.Add(points[i]);
                            pointsToBeRemoved.Add(points[j]);
                        }
                    }
                }
            }

            return points.Except(pointsToBeRemoved).ToList();
        }
    }
}
