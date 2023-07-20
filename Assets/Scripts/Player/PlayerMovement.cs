using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerInputs), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    private void Awake()
    {
        instance = this;
    }

    public float moveSpeed = 3.0f;
    public float maxAirSpeedFactor = 1.1f; // For limiting bhopping
    public float groundAcceleration = 20f;
    public float airAcceleration = 2f;
    public AnimationCurve accelDotFactor = new AnimationCurve(new Keyframe(-1, 2), new Keyframe(0, 1), new Keyframe(0, 1));
    public float accelLerpSpeed = 10f;
    public float groundMaxAccelerationForce = 10f;
    public float airMaxAccelerationForce = 3f;
    public AnimationCurve accelForceDotFactor = new AnimationCurve(new Keyframe(-1, 2), new Keyframe(0, 1), new Keyframe(0, 1));
    //public float friction = 0.3f;
    public float maxJumpHeight = 1f;
    public float jumpChargeTime = 0.5f;

    public LayerMask groundLayerMask;

    [Space]
    public bool enableBhopping = true;
    [Space]
    public bool debugGraphicsInterpolator;

    private bool grounded;
    private bool wasGrounded;

    [HideInInspector]
    public Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    Vector3 desiredVelocity;
    Vector3 moveVelocity;
    Vector3 groundNormal;
    Vector2 inputFromUpdate;

    #region Constants

    //Crouch
    //const float CrouchRaySize = 0.4f;
    //const float CrouchRayLength = 1f;
    //const float StandingHeight = 2f;
    //const float CrouchingHeight = 1f;
    //const float CrouchHeightDif = StandingHeight - CrouchingHeight;

    //Grounded
    const float GroundedSphereRadiusPadding = 0.025f;
    const float GroundedSphereDist = 0.15f;

    //const float AccelerationMultiplier = 500f;
    const float FrictionThreshold = 0.01f;

    #endregion

    float currentSpeed;
    float currentAcceleration;
    float currentMaxAcceleration;

    float airtime;
    bool holdingJump;
    float jumpHeldFor;
    float timeSinceJump;
    const float TimeSinceJumpThreshold = 0.3f;

    public static event Action<float> OnLand;
    public static event Action<float> OnJump;

    public static float JumpChargeFactor { get; private set; }
    public static bool Grounded { get; private set; }
    public static Vector3 Velocity { get; private set; }
    public static Vector3 LocalVelocity { get; private set; }
    public static Vector2 Input { get; private set; }

    Vector3 goalVelocity;
    public RBInterpolator graphicsInterpolator;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        graphicsInterpolator = new RBInterpolator(rb, transform.GetChild(0));
    }

    private void FixedUpdate()
    {
        graphicsInterpolator.OnFixedUpdate();

        UpdateSpeed();
        UpdateAcceleration();

        TryJump();
        Move();

        UpdateGrounded();

        //SetProperties();
    }

    private void Update()
    {
        graphicsInterpolator.OnUpdate();

        UpdateValues();

        SetProperties();
    }

    private void LateUpdate()
    {
        graphicsInterpolator.OnLateUpdate();
    }

    private void UpdateValues()
    {
        inputFromUpdate = PlayerInputs.Movement;
        holdingJump = PlayerInputs.Jump.IsPressed();
        timeSinceJump += Time.deltaTime;

        if (!enableBhopping && rb.velocity.y <= 0 && timeSinceJump < TimeSinceJumpThreshold)
            timeSinceJump = TimeSinceJumpThreshold;
        // Uncomment this to prevent bhopping

        if (holdingJump && grounded)
            jumpHeldFor += Time.deltaTime;
    }

    private void SetProperties()
    {
        JumpChargeFactor = Remap.Float(Mathf.Min(jumpHeldFor, jumpChargeTime), 0, jumpChargeTime, 0, 1f);
        Grounded = grounded;
        Velocity = rb.velocity;
        LocalVelocity = GetLocalVelocity(Velocity);
        Input = inputFromUpdate;
    }


    //float old;

    
    private void Move()
    {
        // https://www.youtube.com/watch?v=qdskE8PJy6Q

        Vector3 unitDesiredVelocity = transform.forward * inputFromUpdate.y + transform.right * inputFromUpdate.x;
        Vector3 currentGoalVelocity = unitDesiredVelocity * currentSpeed;

        float velDot = Vector3.Dot(unitDesiredVelocity, goalVelocity.normalized);
        float accel = currentAcceleration * accelDotFactor.Evaluate(velDot);

        goalVelocity = Vector3.MoveTowards(goalVelocity, currentGoalVelocity, accel * Time.deltaTime);

        Vector3 neededAccel = (goalVelocity - rb.velocity.Flat()) / Time.deltaTime;

        float maxAccel = currentMaxAcceleration * accelForceDotFactor.Evaluate(velDot);

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        rb.AddForce(neededAccel);
    }

    /*
    private void Move()
    {
        Vector2 input = inputFromUpdate;
        //if (flag)
        //    input = PlayerInputs.Movement;
        Vector3 localVelocity = GetLocalVelocity(rb.velocity.Flat());

        CounterMove(input, localVelocity);

        if (grounded)
        {
            TryJump();

            if (!wasGrounded)
            {
                // Just landed
                OnLand?.Invoke(airtime);
                airtime = 0;
            }
        }
        else
            airtime += Time.deltaTime;

        // Some sort of downforce for stairs here


        // Cancel input to avoid going over speed
        if (input.x > 0 && localVelocity.x > moveSpeed) input.x = 0;
        if (input.x < 0 && localVelocity.x < -moveSpeed) input.x = 0;
        if (input.y > 0 && localVelocity.z > moveSpeed) input.y = 0;
        if (input.y < 0 && localVelocity.z < -moveSpeed) input.y = 0;
        // TODO: Fix diagonal inputs going over speed limit

        //if (Mathf.Abs(input.x) > 0 && Mathf.Abs(localVelocity.x) > moveSpeed
        //    && Mathf.Sign(input.x) == Mathf.Sign(localVelocity.x)) input.x = 0;
        //if (Mathf.Abs(input.y) > 0 && Mathf.Abs(localVelocity.z) > moveSpeed
        //    && Mathf.Sign(input.y) == Mathf.Sign(localVelocity.z)) input.x = 0;
        // Figure the top way is more readable

        rb.AddForce(transform.forward * (input.y * currentAcceleration * AccelerationMultiplier * Time.deltaTime), ForceMode.Acceleration);
        rb.AddForce(transform.right * (input.x * currentAcceleration * AccelerationMultiplier * Time.deltaTime), ForceMode.Acceleration);

        float maxSpeedSqr = currentSpeed * currentSpeed;
        float maxSpeedSqrAir = maxSpeedSqr * maxAirSpeedFactor * maxAirSpeedFactor;
        float curSpeedSqr = rb.velocity.Flat().sqrMagnitude;
        if (grounded && curSpeedSqr > maxSpeedSqr)
        {
            if (timeSinceJump > TimeSinceJumpThreshold)
            {
                float y = rb.velocity.y;
                rb.velocity = (rb.velocity.Flat().normalized * currentSpeed).Y(y);
            }
            else if (curSpeedSqr > maxSpeedSqrAir)
            {
                // Allows bhopping
                float y = rb.velocity.y;
                rb.velocity = (rb.velocity.Flat().normalized * currentSpeed * maxAirSpeedFactor).Y(y);
            }
        }

        ///*
        float mag = rb.velocity.Flat().magnitude;
        float diff = old - mag;
        float threshold = 0.25f;
        if (Mathf.Abs(diff) > threshold)
        {
            Debug.Log($"Vel diff > {threshold}: Old - {old} Cur - {mag}");
        }

        old = mag;
        //*
    }
    */

    private void TryJump()
    {
        if (grounded)
        {
            if (!holdingJump && jumpHeldFor > 0f)
            {
                float factor = Remap.Float(Mathf.Min(jumpHeldFor, jumpChargeTime), 0, jumpChargeTime, 0, 1f);
                jumpHeldFor = 0;
                if (factor < 0.1f) return;

                timeSinceJump = 0f;
                OnJump?.Invoke(factor);

                //rb.AddForce(Vector3.up * Mathf.Sqrt(-2f * Physics.gravity.y * maxJumpHeight * factor), ForceMode.VelocityChange);
                rb.velocity = rb.velocity.WithY(Mathf.Sqrt(-2f * Physics.gravity.y * maxJumpHeight * factor));
            }

            if (!wasGrounded)
            {
                // Just landed
                OnLand?.Invoke(airtime);
                airtime = 0;
            }
        }
        else
            airtime += Time.deltaTime;
    }

    /*
    private void CounterMove(Vector2 desired, Vector3 localVelocity)
    {
        if (!grounded || timeSinceJump < TimeSinceJumpThreshold) return;

        if (ShouldApplyFriction(desired.x, localVelocity.x))
            rb.AddForce(transform.right * (currentAcceleration * AccelerationMultiplier * -localVelocity.x * friction * Time.deltaTime), ForceMode.Acceleration);

        if (ShouldApplyFriction(desired.y, localVelocity.z))
            rb.AddForce(transform.forward * (currentAcceleration * AccelerationMultiplier * -localVelocity.z * friction * Time.deltaTime), ForceMode.Acceleration);
    }
    

    bool ShouldApplyFriction(float desired, float current)
    {
        bool stationary = Mathf.Abs(current) > FrictionThreshold && Mathf.Abs(desired) < 0.05f;
        bool forward = current < -FrictionThreshold && desired > 0;
        bool backward = current > FrictionThreshold && desired < 0;

        // If you want to stop or go the opposite direction
        return stationary || forward || backward;
    }
    */

    private void UpdateSpeed()
    {
        currentSpeed = moveSpeed * (grounded ? 1f : maxAirSpeedFactor);
    }

    private void UpdateAcceleration()
    {
        float target = grounded ? groundAcceleration : airAcceleration;
        float targetMax = grounded ? groundMaxAccelerationForce : airMaxAccelerationForce;

        currentAcceleration = Mathf.Lerp(currentAcceleration, target, Time.deltaTime * accelLerpSpeed);
        currentMaxAcceleration = Mathf.Lerp(currentMaxAcceleration, targetMax, Time.deltaTime * accelLerpSpeed);
    }

    private void UpdateGrounded()
    {
        wasGrounded = grounded;
        RaycastHit hit;
        float distance = capsuleCollider.height / 2f - capsuleCollider.radius + GroundedSphereDist;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), capsuleCollider.radius - GroundedSphereRadiusPadding, out hit, distance, groundLayerMask);

        if (grounded)
            groundNormal = hit.normal;
        else
            groundNormal = Vector3.up;
    }

    Vector3 GetLocalVelocity(Vector3 worldVel)
    {
        return Quaternion.AngleAxis(-transform.eulerAngles.y, Vector3.up) * worldVel;
    }

    private void OnDrawGizmosSelected()
    {
        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();

        Gizmos.color = Color.red;
        float height = capsuleCollider.height / 2f - capsuleCollider.radius + GroundedSphereDist;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * height, capsuleCollider.radius - GroundedSphereRadiusPadding);
    }

    private void OnDrawGizmos()
    {
        if (debugGraphicsInterpolator)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rb.position, new Vector3(1, 2, 1));
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(graphicsInterpolator.renderNode.position, new Vector3(1, 2, 1));
        }
    }


    public static void AddForce(Vector3 force, ForceMode mode)
    {
        instance.rb.AddForce(force, mode);
        instance.timeSinceJump = 0;
    }
}
