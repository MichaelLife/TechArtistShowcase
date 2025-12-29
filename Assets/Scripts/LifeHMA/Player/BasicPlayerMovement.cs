using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using LifeHMA.Utilities;
using LifeHMA.Interaction;

namespace LifeHMA.Player
{
    [RequireComponent(typeof(GroundChecker), typeof(Rigidbody), typeof(Interactor))]
    public class BasicPlayerMovement : MonoBehaviour
    {
        #region VARIABLES
        //INPUTS
        [Header("INPUT")]
        [SerializeField] InputReader input;

        //Components
        [Header("COMPONENTS")]
        public Rigidbody rb;
        public Interactor playerInteractor;
        private GroundChecker groundChecker;

        //Movement
        [Header("MOVEMENT")]
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float acceleration = 200f; 
        [SerializeField] private AnimationCurve accelerationFactor; //Curve that determines how the player accelerates
        [SerializeField] private float maxAccelerationForce = 150f; //Force that is used to calculate the final force to be applied to the rb
        [SerializeField] private AnimationCurve maxAccelerationForceFactor; //Curve that determines how the force accelerates

        private bool isMoving;
        private bool immobilized;
        private Vector3 movement; //Current movement input

        private Vector3 mTargetVel; //Ideal velocity to achieve

        //Gravity
        [Header("GRAVITY")]
        [SerializeField] private float gravityForce;
        [SerializeField] private float gravityMultiplier = 3f; //How the player accelerates when falling

        //Jump
        [Header("JUMP")]
        [SerializeField] private float jumpForce = 10f; //Initial jump force
        [SerializeField] private float jumpDuration = 0.5f;
        [SerializeField] private float jumpCooldown = 0f; //Cooldown to jump again
        [SerializeField] private float jumpMaxHeight = 2f;
        [SerializeField] private AnimationCurve jumpFactor; //How the jump velocity decelerates when in air

        private float jumpVelocity;
        private bool pressingJump; //Used for capacitive jump

        //Spring
        [Header("SPRING EFFECT")]
        [SerializeField] private float springStrengh;
        [SerializeField] private float springHeight;
        [SerializeField] private float springHeightOrigin;
        [SerializeField] private float springDamp;
        [SerializeField] private bool showSpringGizmo;

        //Rotation
        [Header("ROTATION")]
        [SerializeField] private float rotationFactorPerFrame = 10.0f;

        [SerializeField] private float uprightJointSpringStrenght = 3f;
        [SerializeField] private float uprightJointSpringDamp = 3f;
        private Quaternion uprightJointTargetRot;

        //Timers
        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        #endregion

        private void Awake()
        {
            //Start Timers
            #region TIMERS
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            timers = new List<Timer>(2) { jumpTimer, jumpCooldownTimer };

            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();
            #endregion

            //Get GroundChecker
            groundChecker = GetComponent<GroundChecker>();

            playerInteractor = GetComponent<Interactor>();

            //Set ideal upright rotation
            uprightJointTargetRot = transform.rotation;
        }

        private void Update()
        {
            HandleTimer();
        }

        private void FixedUpdate()
        {
            Movement();
            HandleGravity();
            SpringEffect();
            HandleRotation();
            UpdateUprightForce();

            playerInteractor.CheckForInteractable();
        }

        private void HandleTimer()
        {
            //Goes through all the timers and applys tick rate based on normal Update delta Time
            foreach (var timer in timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }

        private void Movement()
        {
            //Get movement axis from input
            Vector2 _movementInputVector = input.Direction;
            if (_movementInputVector.magnitude > 0 && !immobilized) { isMoving = true; } else if (isMoving) { isMoving = false; }

            _movementInputVector *= (immobilized ? 0f : 1f);

            movement.Normalize();
            movement = new Vector3(_movementInputVector.x, 0, _movementInputVector.y);

            //Get ideal speed from movement direction
            Vector3 dir = movement;
            Vector3 _currentTargetDir = movement * maxSpeed;

            //Calculate the change in direction 
            float _velDot = Vector3.Dot(dir, mTargetVel.normalized);

            //Calculate acceleration so that if you are changing direction abruptly you take longer to accelerate
            float _accel = acceleration * accelerationFactor.Evaluate(_velDot);

            //Create a target velocity and make it accelerate to catch up to the idial speed
            mTargetVel = Vector3.MoveTowards(mTargetVel, _currentTargetDir, _accel * Time.fixedDeltaTime);

            //Calculate needed acceleration to get to the target velocity in one frame
            Vector3 _neededAccel = (mTargetVel - rb.linearVelocity) / Time.fixedDeltaTime;

            float _maxAcceleration = maxAccelerationForce * maxAccelerationForceFactor.Evaluate(_velDot);

            //Clamp acceleration so that character doesn't go flying away too fast XD
            _neededAccel = Vector3.ClampMagnitude(_neededAccel, _maxAcceleration);

            //Apply force
            rb.AddForce(_neededAccel);
        }

        private void OnJump(bool start)
        {
            if (start) //Context.started
            {
                //If not jumping, jump cooldown finished and is in the ground start jump timer and calulate start jump velocity
                if (!jumpTimer.isRunning && !jumpCooldownTimer.isRunning && groundChecker.IsGrounded)
                {
                    pressingJump = true;
                    jumpTimer.Start();
                    jumpVelocity = Mathf.Sqrt(2 * jumpMaxHeight * Mathf.Abs(gravityForce));
                }
            }
            if (!start) //Context.canceled
            {
                //If button released pressingJump = false, used for capacitive jump
                if (jumpTimer.isRunning)
                {
                    pressingJump = false;
                }
            }
        }

        #region GRAVITY
        private void HandleGravity()
        {
            //Checks if player is jumping or falling and, if not it makes sure that the jump velocity is 0
            if (!jumpTimer.isRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = 0f;
                jumpTimer.Stop();
                return;
            }

            if (jumpTimer.isRunning)
            {
                float launchPoint = 0.9f;
                float minStopPoint = 0.7f;

                //Check if player has released the jump button early to make the jump shorter, uses a minimum height for jump so player doesnt jump only 1 mm
                if (jumpTimer.progress < minStopPoint && !pressingJump)
                {
                    jumpTimer.Stop();
                }

                //Applies max force at start of jump, then reduce jump velocity following the jumpFactor curve
                if (jumpTimer.progress > launchPoint)
                {
                    jumpVelocity = Mathf.Sqrt(2 * jumpMaxHeight * Mathf.Abs(gravityForce));
                }
                else
                {
                    jumpVelocity += jumpFactor.Evaluate(1 - jumpTimer.progress) * jumpForce * Time.deltaTime;
                }

                if(groundChecker.HeadChecker())
                {
                    jumpTimer.Stop();
                    jumpVelocity *= -0.5f;
                }
            }
            else
            {
                //If not jumping player is falling --> apply gravity

                jumpVelocity += -gravityForce * gravityMultiplier * Time.fixedDeltaTime;
            }

            //Apply jumpVelocity to rigidbody
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }

        private void SpringEffect()
        {
            //Sends a raycast to check if player is close to the ground so it can apply the springEffect
            RaycastHit _hit;
            Vector3 _rayDir = transform.TransformDirection(Vector3.down);
            if(showSpringGizmo) Debug.DrawRay(transform.position + _rayDir * springHeightOrigin, _rayDir * springHeight, Color.green);

            if (Physics.Raycast(transform.position + _rayDir * springHeightOrigin, _rayDir, out _hit, springHeight))
            {
                //Gets rigidbody velocity
                Vector3 _vel = rb.linearVelocity;
                Vector3 _otherVel = Vector3.zero;

                //Gets rigidbody of object under player in case the player has landed on something
                Rigidbody _hitRB = _hit.rigidbody;
                if (_hitRB != null)
                    _otherVel = _hitRB.linearVelocity;

                //Calculates force based on distance from ideal spring height and forces involved to get the player to that ideal height
                float _rayDirVel = Vector3.Dot(_rayDir, _vel);
                float _otherDirVel = Vector3.Dot(_rayDir, _otherVel);

                float _relVel = _rayDirVel - _otherDirVel;
                float _x = _hit.distance - springHeight;

                float springForce = (_x * springStrengh) - (_relVel * springDamp);

                if (showSpringGizmo) Debug.DrawRay(transform.position, transform.position + (_rayDir * springForce), Color.yellow);

                //Applies force to player
                rb.AddForce(_rayDir * springForce);

                //If there is an object with a rigidbody below the player, it applies the same force on the oposite direction
                if(_hitRB != null)
                {
                    _hitRB.AddForceAtPosition(_rayDir * -springForce, _hit.point);
                }
            }
        }
        #endregion

        #region ROTATION
        private void UpdateUprightForce()
        {
            //Gets the character rotation and calculates the shortest path to rotate to the ideal rotation (upright)
            Quaternion _characterCurrent = transform.rotation;
            Quaternion _rotGoal = HMA_Math.ShortestRotation(uprightJointTargetRot, _characterCurrent);

            Vector3 _rotAxis;
            float _rotDegres;

            _rotGoal.ToAngleAxis(out _rotDegres, out _rotAxis);
            _rotAxis.Normalize();

            float _rotRadians = _rotDegres * Mathf.Deg2Rad;

            //Adds the force to the player
            rb.AddTorque((_rotAxis * _rotRadians * uprightJointSpringStrenght) - (rb.angularVelocity * uprightJointSpringDamp));
        }

        private void HandleRotation()
        {
            //Gets the target rotation
            Vector3 _posToLookAt;

            _posToLookAt.x = movement.x;
            _posToLookAt.y = 0f;
            _posToLookAt.z = movement.z;

            Quaternion _currentRotation = transform.rotation;

            //If player is moving applies slowly the rotation
            if (isMoving)
            {
                Quaternion _targetRotation = Quaternion.LookRotation(_posToLookAt);
                uprightJointTargetRot = _targetRotation;
                transform.rotation = Quaternion.Slerp(_currentRotation, _targetRotation, rotationFactorPerFrame * Time.fixedDeltaTime);
            }
        }
        #endregion

        private void OnEnable()
        {
            input.Jump += OnJump;
        }

        private void OnDisable()
        {
            input.Jump -= OnJump;
        }
    }
}
