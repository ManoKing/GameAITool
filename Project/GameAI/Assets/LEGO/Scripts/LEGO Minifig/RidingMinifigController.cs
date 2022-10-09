using System;
using UnityEngine;

namespace Unity.LEGO.Minifig
{
    public class RidingMinifigController : MinifigController
    {
        // Constants.
        const float slowDownDuration = 1.0f;

        public new enum SpecialAnimation
        {
            Bow,
            Dance,
            Fart,
            Gallop,
            Idle,
            Jump,
            Look,
            Maybe,
            No,
            Trot,
            Yes
        }

        float time;
        Animator[] animators;

        protected override void OnValidate()
        {
            maxForwardSpeed = Mathf.Clamp(maxForwardSpeed, 5, 30);

            if (!leftArmTip || !rightArmTip || !leftLegTip || !rightLegTip || !head)
            {
                var minifig = GetComponentInChildren<Minifig>();
                if (minifig)
                {
                    var rigTransform = minifig.transform.GetChild(1);
                    if (rigTransform)
                    {
                        FindJointReferences(rigTransform);
                    }
                    else
                    {
                        Debug.LogError("Failed to find Minifigure rig.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to find a Minifigure.");
                }
            }
        }

        protected override void Awake()
        {
            minifig = GetComponentInChildren<Minifig>();
            controller = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            animators = GetComponentsInChildren<Animator>();
            animator = GetComponent<Animator>();

            // Initialise animation.
            foreach (var animator in animators)
            {
                animator.SetBool(groundedHash, true);
            }

            // Make sure the Character Controller is grounded if starting on the ground.
            if (controller.enabled)
            {
                controller.Move(Vector3.down * 0.01f); 
            }
        }

        protected override void Update()
        {
            // Handle input.
            if (!exploded)
            {
                if (inputEnabled)
                {
                    switch (inputType)
                    {
                        case InputType.Tank:
                            {
                                // Calculate speed.
                                var targetSpeed = Input.GetAxisRaw("Vertical");
                                targetSpeed *= targetSpeed > 0 ? maxForwardSpeed : maxBackwardSpeed;
                                if (targetSpeed > speed)
                                {
                                    speed = Mathf.Min(targetSpeed, speed + acceleration * Time.deltaTime);
                                }
                                else if (targetSpeed < speed)
                                {
                                    speed = Mathf.Max(targetSpeed, speed - acceleration * Time.deltaTime);
                                }

                                // Calculate rotation speed.
                                var targetRotateSpeed = Input.GetAxisRaw("Horizontal");
                                targetRotateSpeed *= maxRotateSpeed;
                                if (targetRotateSpeed > rotateSpeed)
                                {
                                    rotateSpeed = Mathf.Min(targetRotateSpeed, rotateSpeed + rotateAcceleration * Time.deltaTime);
                                }
                                else if (targetRotateSpeed < rotateSpeed)
                                {
                                    rotateSpeed = Mathf.Max(targetRotateSpeed, rotateSpeed - rotateAcceleration * Time.deltaTime);
                                }

                                // Calculate move delta.
                                moveDelta = new Vector3(0, moveDelta.y, speed);
                                moveDelta = transform.TransformDirection(moveDelta);
                                break;
                            }
                        case InputType.Direct:
                            {
                                // Calculate direct speed and speed.
                                var right = Vector3.right;
                                var forward = Vector3.forward;
                                if (Camera.main)
                                {
                                    right = Camera.main.transform.right;
                                    right.y = 0.0f;
                                    right.Normalize();
                                    forward = Camera.main.transform.forward;
                                    forward.y = 0.0f;
                                    forward.Normalize();
                                }

                                var targetSpeed = right * Input.GetAxisRaw("Horizontal");
                                targetSpeed += forward * Input.GetAxisRaw("Vertical");
                                if (targetSpeed.sqrMagnitude > 0.0f)
                                {
                                    targetSpeed.Normalize();
                                }
                                targetSpeed *= maxForwardSpeed;

                                var speedDiff = targetSpeed - directSpeed;
                                if (speedDiff.sqrMagnitude < acceleration * acceleration * Time.deltaTime * Time.deltaTime)
                                {
                                    directSpeed = targetSpeed;
                                }
                                else if (speedDiff.sqrMagnitude > 0.0f)
                                {
                                    speedDiff.Normalize();

                                    directSpeed += speedDiff * acceleration * Time.deltaTime;
                                }
                                speed = directSpeed.magnitude;

                                // Calculate rotation speed - ignore rotate acceleration.
                                rotateSpeed = 0.0f;
                                if (targetSpeed.sqrMagnitude > 0.0f)
                                {
                                    var localTargetSpeed = transform.InverseTransformDirection(targetSpeed);
                                    var angleDiff = Vector3.SignedAngle(Vector3.forward, localTargetSpeed.normalized, Vector3.up);

                                    if (angleDiff > 0.0f)
                                    {
                                        rotateSpeed = maxRotateSpeed;
                                    }
                                    else if (angleDiff < 0.0f)
                                    {
                                        rotateSpeed = -maxRotateSpeed;
                                    }

                                    // Assumes that x > NaN is false - otherwise we need to guard against Time.deltaTime being zero.
                                    if (Mathf.Abs(rotateSpeed) > Mathf.Abs(angleDiff) / Time.deltaTime)
                                    {
                                        rotateSpeed = angleDiff / Time.deltaTime;
                                    }
                                }

                                // Calculate move delta.
                                moveDelta = new Vector3(directSpeed.x, moveDelta.y, directSpeed.z);
                                break;
                            }
                    }

                    // Check if player is grounded.
                    if (!airborne)
                    {
                        jumpsInAir = maxJumpsInAir;
                    }

                    // Check if player is jumping.
                    if (Input.GetButtonDown("Jump"))
                    {
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

                            foreach (var animator in animators)
                            {
                                animator.SetTrigger(jumpHash);
                            }

                            airborne = true;
                            airborneTime = coyoteDelay;
                        }
                    }

                    // Cancel special.
                    cancelSpecial = !Mathf.Approximately(Input.GetAxis("Vertical"), 0) || !Mathf.Approximately(Input.GetAxis("Horizontal"), 0) || Input.GetButtonDown("Jump");

                }
                else
                {
                    HandleAutomaticAnimation();
                }
            }
            else
            {
                // Slow down speed and movement to 0 over time.
                time += Time.deltaTime;
                speed = Mathf.Lerp(speed, 0f, time / slowDownDuration);
                rotateSpeed = Mathf.Lerp(rotateSpeed, 0f, time / slowDownDuration);
                moveDelta.x = Mathf.Lerp(moveDelta.x, 0f, time / slowDownDuration);
                moveDelta.z = Mathf.Lerp(moveDelta.z, 0f, time / slowDownDuration);
            }

            HandleMotion();
        }

        protected override void UpdateMotionAnimations()
        {
            // Update animation - delay airborne animation slightly to avoid flailing arms when falling a short distance.
            foreach (var animator in animators)
            {
                animator.SetBool(cancelSpecialHash, cancelSpecial);
                animator.SetFloat(speedHash, speed);
                animator.SetFloat(rotateSpeedHash, rotateSpeed);
                animator.SetBool(groundedHash, !airborne);
            }
        }

        public void PlaySpecialAnimation(SpecialAnimation animation, AudioClip specialAudioClip = null, Action<bool> onSpecialComplete = null)
        {
            foreach (var animator in animators)
            {
                animator.SetBool(playSpecialHash, true);
                animator.SetInteger(specialIdHash, (int)animation);
            }

            if (specialAudioClip)
            {
                audioSource.PlayOneShot(specialAudioClip);
            }

            this.onSpecialComplete = onSpecialComplete;
        }

        public override void Explode()
        {
            if (!exploded)
            {
                exploded = true;

                // Move Minifigure to root hierarchy.
                minifig.transform.parent = null;
                
                // Find and disable Minifigure animator.
                foreach (var animator in animators)
                {
                    if (animator.gameObject == minifig.gameObject)
                    {
                        animator.enabled = false;
                    }
                }

                HandleExplode();
            }
        }
    }
}
