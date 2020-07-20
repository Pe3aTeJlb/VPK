using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public GameObject leftWheelVisuals;

    private bool leftGrounded = false;
    private float travelL = 0f;
    private float leftAckermanCorrectionAngle = 0;

    public WheelCollider rightWheel;
    public GameObject rightWheelVisuals;
    private bool rightGrounded = false;
    private float travelR = 0f;
    private float rightAckermanCorrectionAngle = 0;

    public bool motor;
    public bool steering;

    public float Antiroll = 10000;
    private float AntrollForce = 0;

    public float ackermanSteering = 1f;
    public void ApplyLocalPositionToVisuals()
    {
        //left wheel
        if (leftWheelVisuals == null)
        {
            return;
        }
        Vector3 position;
        Quaternion rotation;
        leftWheel.GetWorldPose(out position, out rotation);

        leftWheelVisuals.transform.position = position;
        leftWheelVisuals.transform.rotation = rotation;

        //right wheel
        if (rightWheelVisuals == null)
        {
            return;
        }
        rightWheel.GetWorldPose(out position, out rotation);

        rightWheelVisuals.transform.position = position;
        rightWheelVisuals.transform.rotation = rotation;
    }
    public void CalculateAndApplyAntiRollForce(Rigidbody theBody)
    {
        WheelHit hit;

        leftGrounded = leftWheel.GetGroundHit(out hit);
        if (leftGrounded)
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        else
            travelL = 1f;

        rightGrounded = rightWheel.GetGroundHit(out hit);
        if (rightGrounded)
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        else
            travelR = 1f;

        AntrollForce = (travelL - travelR) * Antiroll;

        if (leftGrounded)
            theBody.AddForceAtPosition(leftWheel.transform.up * -AntrollForce, leftWheel.transform.position);
        if (rightGrounded)
            theBody.AddForceAtPosition(rightWheel.transform.up * AntrollForce, rightWheel.transform.position);

    }
    public void CalculateAndApplySteering(float input, float maxSteerAngle, List<AxleInfo> allAxles)
    {
        //first find farest axle, we got to apply default values
        AxleInfo farestAxle = allAxles[0];
        //calculate start point for checking
        float farestAxleDistantion = ((allAxles[0].leftWheel.transform.localPosition - allAxles[0].rightWheel.transform.localPosition) / 2f).z;
        for (int a = 0; a < allAxles.Count; a++)
        {
            float theDistance = ((allAxles[a].leftWheel.transform.localPosition - allAxles[a].rightWheel.transform.localPosition) / 2f).z;
            // if we found axle that farer - save it
            if (theDistance < farestAxleDistantion)
            {
                farestAxleDistantion = theDistance;
                farestAxle = allAxles[a];
            }
        }
        float wheelBaseWidth = (Mathf.Abs(leftWheel.transform.localPosition.x) + Mathf.Abs(rightWheel.transform.localPosition.x)) / 2;
        float wheelBaseLength = Mathf.Abs(((farestAxle.leftWheel.transform.localPosition + farestAxle.rightWheel.transform.localPosition) / 2f).z) +
            Mathf.Abs(((leftWheel.transform.localPosition + rightWheel.transform.localPosition) / 2f).z);

        float angle = maxSteerAngle * input;
        //ackerman implementation
        float turnRadius = Mathf.Abs(wheelBaseLength * Mathf.Tan(Mathf.Deg2Rad * (90 - Mathf.Abs(angle))));
        if (input != 0)
        {
            //right wheel
            if (angle > 0)
            {//turn right

                rightAckermanCorrectionAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBaseLength / (turnRadius - wheelBaseWidth / 2f));
                rightAckermanCorrectionAngle = (rightAckermanCorrectionAngle - Mathf.Abs(angle)) * ackermanSteering + (Mathf.Abs(angle));
                rightAckermanCorrectionAngle = Mathf.Sign(angle) * rightAckermanCorrectionAngle;
            }
            else
            {//turn left

                rightAckermanCorrectionAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBaseLength / (turnRadius + wheelBaseWidth / 2f));
                rightAckermanCorrectionAngle = (rightAckermanCorrectionAngle - Mathf.Abs(angle)) * ackermanSteering + (Mathf.Abs(angle));
                rightAckermanCorrectionAngle = Mathf.Sign(angle) * rightAckermanCorrectionAngle;
            }
            //left wheel
            if (angle > 0)
            {//turn right
                leftAckermanCorrectionAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBaseLength / (turnRadius + wheelBaseWidth / 2f));
                leftAckermanCorrectionAngle = (leftAckermanCorrectionAngle - Mathf.Abs(angle)) * ackermanSteering + (Mathf.Abs(angle));
                leftAckermanCorrectionAngle = Mathf.Sign(angle) * leftAckermanCorrectionAngle;
            }
            else
            {//turn left
                leftAckermanCorrectionAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBaseLength / (turnRadius - wheelBaseWidth / 2f));
                leftAckermanCorrectionAngle = (leftAckermanCorrectionAngle - Mathf.Abs(angle)) * ackermanSteering + (Mathf.Abs(angle));
                leftAckermanCorrectionAngle = Mathf.Sign(angle) * leftAckermanCorrectionAngle;
            }
        }
        else
        {
            rightAckermanCorrectionAngle = 0f;
            leftAckermanCorrectionAngle = 0f;
        }
        leftWheel.steerAngle = leftAckermanCorrectionAngle;
        rightWheel.steerAngle = rightAckermanCorrectionAngle;
        Debug.Log(leftAckermanCorrectionAngle + " " + rightAckermanCorrectionAngle);
    }
}
[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    private Rigidbody body;
    public UIButtonInfo up, down;
    private void Start()
    {
        up = GameObject.FindGameObjectWithTag("Up").GetComponent<UIButtonInfo>();
        down = GameObject.FindGameObjectWithTag("Down").GetComponent<UIButtonInfo>();
        body = GetComponent<Rigidbody>();
    }
    public void FixedUpdate()
    {

        //float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float motor = 0;
        if (up.isDown)
        {
            motor = maxMotorTorque * 1;
        }
        else if (down.isDown)
        {
            motor = maxMotorTorque * -1;
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.CalculateAndApplySteering(Input.GetAxis("Horizontal"), maxSteeringAngle, axleInfos);
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            axleInfo.ApplyLocalPositionToVisuals();
            axleInfo.CalculateAndApplyAntiRollForce(body);
        }
    }
}