using UnityEngine;

namespace MonsterTruckGame.Vehicle;

[DisallowMultipleComponent]
public sealed class MonsterTruckController2D : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D vehicleBody = null!;
    [SerializeField] private WheelJoint2D rearWheelJoint = null!;
    [SerializeField] private WheelJoint2D frontWheelJoint = null!;

    [Header("Driving")]
    [SerializeField, Min(0f)] private float maximumMotorSpeed = 1_100f;
    [SerializeField, Min(0f)] private float motorTorque = 1_800f;
    [SerializeField, Min(0f)] private float brakeTorque = 2_400f;
    [SerializeField, Min(0f)] private float airControlTorque = 28f;
    [SerializeField, Min(0f)] private float maximumAngularVelocity = 260f;

    private float throttle;
    private float airControl;
    private bool braking;

    public void SetThrottle(float value)
    {
        throttle = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetBrake(bool value)
    {
        braking = value;
    }

    public void SetAirControl(float value)
    {
        airControl = Mathf.Clamp(value, -1f, 1f);
    }

    public void ReleaseAllControls()
    {
        throttle = 0f;
        airControl = 0f;
        braking = false;
    }

    private void FixedUpdate()
    {
        ApplyWheelMotor(rearWheelJoint);
        ApplyWheelMotor(frontWheelJoint);

        vehicleBody.AddTorque(
            -airControl * airControlTorque,
            ForceMode2D.Force
        );

        vehicleBody.angularVelocity = Mathf.Clamp(
            vehicleBody.angularVelocity,
            -maximumAngularVelocity,
            maximumAngularVelocity
        );
    }

    private void OnDisable()
    {
        ReleaseAllControls();
        DisableMotor(rearWheelJoint);
        DisableMotor(frontWheelJoint);
    }

    private void ApplyWheelMotor(WheelJoint2D joint)
    {
        JointMotor2D motor = joint.motor;

        if (braking)
        {
            motor.motorSpeed = 0f;
            motor.maxMotorTorque = brakeTorque;
            joint.motor = motor;
            joint.useMotor = true;
            return;
        }

        if (Mathf.Abs(throttle) < 0.01f)
        {
            DisableMotor(joint);
            return;
        }

        motor.motorSpeed = -throttle * maximumMotorSpeed;
        motor.maxMotorTorque = motorTorque;
        joint.motor = motor;
        joint.useMotor = true;
    }

    private static void DisableMotor(WheelJoint2D joint)
    {
        joint.useMotor = false;
    }
}
