using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[System.Serializable]
public class AxleInfo
{
    [Range(5, 15000)]
    [Tooltip("In this variable you can define the mass that the wheels will have. The script will leave all wheels with the same mass.")]
    public int wheelMass = 100;
    [Space(10)]
    [Tooltip("The front right wheel collider must be associated with this variable")]
    public WheelClassFree rightWheel;
    [Tooltip("The front left wheel collider must be associated with this variable")]
    public WheelClassFree leftWheel;
}


[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    [Tooltip("In this class you can configure the vehicle torque, number of gears and their respective torques.")]
    public TorqueAdjustmentClassFree _vehicleTorque;

    [Tooltip("In this class you can adjust various settings that allow changing the way the vehicle is controlled, as well as the initial state of some variables, such as the engine and the brake.")]
    public VehicleAdjustmentClassFree _vehicleSettings;

    public List<AxleInfo> axleInfos;

	[Tooltip("In this class, you can adjust all vehicle sounds, and the preferences of each.")]
	public VehicleSoundsClassFree _sounds;

	public UIButtonInfo up, down, brake;
    public SteeringWheel stWheelController;
	public int gearRatio;
	public int maxSteeringAngle;


	float verticalInput = 0;
    float horizontalInput = 0;

	bool changinGearsAuto;
	bool theEngineIsRunning;
	bool enableEngineSound;
	bool brakingAuto;
	bool colliding;

	int groundedWheels;
	float sumRPM;
	float mediumRPM;
	float angle1Ref;
	float angle2Volant;
	float leftDifferential;
	float rightDifferential;
	float timeAutoGear;
	float reverseForce;
	float engineInput;
	float angleRefVolant;
	float pitchAUD = 1;
	float speedLerpSound = 1;
	float engineSoundFactor;
	float vehicleScale;

	public float maxAngleVolant;

	float torqueM;
	float rpmTempTorque;
	float clampInputTorque;
	float adjustTorque;

	bool isGroundedExtraW;
	Vector3 axisFromRotate;
	Vector3 torqueForceAirRotation;

	Vector2 tireSlipTireSlips;
	Vector2 tireForceTireSlips;
	Vector2 localRigForceTireSlips;
	Vector2 localVelocityWheelTireSlips;
	Vector2 localSurfaceForceDTireSlips;
	Vector2 rawTireForceTireSlips;
	Vector2 tempLocalVelocityVector2;
	Vector3 tempWheelVelocityVector3;
	Vector3 velocityLocalWheelTemp;
	Vector2 surfaceLocalForce;
	Vector3 surfaceLocalForceTemp;
	Vector3 wheelSpeedLocalSurface;
	Vector3 downForceUPTemp;
	float normalTemp;
	float forceFactorTempLocalSurface;
	float downForceTireSlips;
	float estimatedSprungMass;
	float angularWheelVelocityTireSlips;
	float wheelMaxBrakeSlip;
	float minSlipYTireSlips;
	float maxFyTireSlips;


	float velxCurrentRPM;
	float nextPitchAUD;

	float additionalCurrentGravity;
	float currentBrakeValue;
	float forceEngineBrake;

	float currentDownForceVehicle;

	Rigidbody ms_Rigidbody;

	AudioSource engineSoundAUD;
	//AudioSource beatsSoundAUD;
	//AudioSource beatsOnWheelSoundAUD;
	AudioSource skiddingSoundAUD;

	WheelCollider[] wheelColliderList;
	Vector2 tireSL;
	Vector2 tireFO;

	Vector3 lateralForcePointTemp;
	Vector3 forwardForceTemp;
	Vector3 lateralForceTemp;
	float distanceXForceTemp;

	WheelHit tempWheelHit;

	float leftFrontForce;
	float rightFrontForce;
	float leftRearForce;
	float rightRearForce;
	float roolForce1;
	float roolForce2;

	float gravityValueFixedUpdate;
	float downForceValueFixedUpdate;
	float inclinationFactorForcesDown;
	float downForceUpdateRef;
	float downForceTempLerp;


	bool isBraking;
	float brakeVerticalInput;
	float handBrake_Input;
	float totalFootBrake;
	float totalHandBrake;
	float absBrakeInput;
	float absSpeedFactor;

    readonly bool wheelFDIsGrounded;
	readonly bool wheelFEIsGrounded;
	readonly bool wheelTDIsGrounded;
	readonly bool wheelTEIsGrounded;

	Vector3 vectorMeshPos1;
	Vector3 vectorMeshPos2;
	Quaternion quatMesh1;
	Quaternion quatMesh2;

	
	public float KMh;

	public int currentGear;
	[HideInInspector]
	public bool disableVehicle = false;
	[HideInInspector]
	public bool handBrakeTrue;
	[HideInInspector]
	public bool isInsideTheCar;

	public int gCount = 0;
	bool allGrounded;

	private void Start()
    {
        up = GameObject.FindGameObjectWithTag("Up").GetComponent<UIButtonInfo>();
        down = GameObject.FindGameObjectWithTag("Down").GetComponent<UIButtonInfo>();
        brake = GameObject.FindGameObjectWithTag("Brake").GetComponent<UIButtonInfo>();
		stWheelController = GetComponent<SteeringWheel>();

		stWheelController.maximumSteeringAngle = gearRatio * maxSteeringAngle;

		forceEngineBrake = 0.75f * _vehicleSettings.vehicleMass;
		vehicleScale = transform.lossyScale.y;

		wheelColliderList = new WheelCollider[(axleInfos.Count*2)];
		for (int i = 0; i < wheelColliderList.Length-1; i+=2) {
			wheelColliderList[i] = axleInfos[i / 2].leftWheel.wheelCollider;
			wheelColliderList[i+1] = axleInfos[i / 2].rightWheel.wheelCollider;
		}

		currentDownForceVehicle = _vehicleSettings.improveControl.downForce;

		handBrakeTrue = false;

		theEngineIsRunning = _vehicleSettings.startOn;

		if (theEngineIsRunning)
		{
			StartCoroutine("StartEngineCoroutine", true);
			StartCoroutine("TurnOffEngineTime");
		}

		ms_Rigidbody = GetComponent<Rigidbody>();
		ms_Rigidbody.useGravity = true;
		ms_Rigidbody.mass = _vehicleSettings.vehicleMass;
		ms_Rigidbody.drag = 0.0f;
		ms_Rigidbody.angularDrag = 0.05f;
		ms_Rigidbody.maxAngularVelocity = 14.0f;
		ms_Rigidbody.maxDepenetrationVelocity = 8.0f;
		additionalCurrentGravity = 4.0f * ms_Rigidbody.mass;
		ms_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		ms_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		WheelCollider WheelColliders = GetComponentInChildren<WheelCollider>();
		WheelColliders.ConfigureVehicleSubsteps(1000.0f, 20, 20);

		if (_vehicleSettings.centerOfMass)
		{
			ms_Rigidbody.centerOfMass = transform.InverseTransformPoint(_vehicleSettings.centerOfMass.position);
		}
		else
		{
			ms_Rigidbody.centerOfMass = Vector3.zero;
		}

		speedLerpSound = 5;
		enableEngineSound = false;
		if (_sounds.engineSound)
		{
			engineSoundAUD = GenerateAudioSource("Sound of engine", 10, 0, _sounds.engineSound, true, true, true);
		}
		if (_sounds.wheelImpactSound)
		{
			//beatsOnWheelSoundAUD = GenerateAudioSource("Sound of wheel beats", 10, 0.25f, _sounds.wheelImpactSound, false, false, false);
		}
		if (_sounds.skiddingSound.standardSound)
		{
			skiddingSoundAUD = GenerateAudioSource("Sound of skid", 10, 1, _sounds.skiddingSound.standardSound, false, false, false);
		}
		if (_sounds.collisionSounds.Length > 0)
		{
			if (_sounds.collisionSounds[0])
			{
				//beatsSoundAUD = GenerateAudioSource("Sound of beats", 10, _sounds.volumeCollisionSounds, _sounds.collisionSounds[UnityEngine.Random.Range(0, _sounds.collisionSounds.Length)], false, false, false);
			}
		}
		skiddingSoundAUD.clip = _sounds.skiddingSound.standardSound;

		/*
		lastRightForwardPositionY = _wheels.rightFrontWheel.wheelMesh.transform.localPosition.y;
		lastLeftForwardPositionY = _wheels.leftFrontWheel.wheelMesh.transform.localPosition.y;
		lastRightRearPositionY = _wheels.rightRearWheel.wheelMesh.transform.localPosition.y;
		lastLeftRearPositionY = _wheels.leftRearWheel.wheelMesh.transform.localPosition.y;

		sensImpactFR = 0.075f * (2.65f * _wheels.rightFrontWheel.wheelCollider.radius);
		sensImpactFL = 0.075f * (2.65f * _wheels.leftFrontWheel.wheelCollider.radius);
		sensImpactRR = 0.075f * (2.65f * _wheels.rightRearWheel.wheelCollider.radius);
		sensImpactRL = 0.075f * (2.65f * _wheels.leftRearWheel.wheelCollider.radius);
		*/

	}

    public void Update()
    {
		if (up.isDown)
		{
			verticalInput = 1;
		}
		else if (down.isDown)
		{
			verticalInput = -1;
		}
		else
		{
			verticalInput = Input.GetAxis("Vertical");
		}

		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
		{
			horizontalInput = Input.GetAxis("Horizontal");
		}
		else
		{
			horizontalInput = stWheelController.GetClampedValue();
		}


		KMh = ms_Rigidbody.velocity.magnitude * 3.6f;
		inclinationFactorForcesDown = Mathf.Clamp(Mathf.Abs(Vector3.Dot(Vector3.up, transform.up)), _vehicleSettings._aerodynamics.downForceAngleFactor, 1.0f);


		ms_Rigidbody.drag = Mathf.Clamp((KMh / _vehicleTorque.maxVelocityKMh) * 0.075f, 0.001f, 0.075f);
		//
		if (!changinGearsAuto)
		{
			engineInput = Mathf.Clamp01(verticalInput);
		}
		else
		{
			engineInput = 0;
		}

		if ((Input.GetKeyDown(KeyCode.Space) || brake.isDown) && Time.timeScale > 0.2f)
		{
			handBrakeTrue = !handBrakeTrue;
		}

		foreach (AxleInfo axle in axleInfos) {

			if (axle.rightWheel.wheelCollider.isGrounded || axle.leftWheel.wheelCollider.isGrounded)
			{
				downForceTempLerp = (ms_Rigidbody.mass * _vehicleSettings._aerodynamics.minDownForceValue + (_vehicleSettings._aerodynamics.verticalDownForce * Mathf.Abs(KMh * 3.0f) * (ms_Rigidbody.mass / 125.0f))) * inclinationFactorForcesDown;
				downForceUpdateRef = Mathf.Lerp(downForceUpdateRef, downForceTempLerp, Time.deltaTime * 2.5f);
				break;
			}
			else
			{
				downForceTempLerp = ms_Rigidbody.mass * _vehicleSettings._aerodynamics.minDownForceValue * inclinationFactorForcesDown;
				downForceUpdateRef = Mathf.Lerp(downForceUpdateRef, downForceTempLerp, Time.deltaTime * 2.5f);
			}

		}




		DiscoverAverageRpm();
		//TurnOnEngine();
		Sounds();
		UpdateWheelMeshes();
		AutomaticGears();

	}

    public void FixedUpdate()
    {
		gCount = 0;
		allGrounded = false;

        foreach (AxleInfo axleInfo in axleInfos)
        {
			ApplyTorque(axleInfo);
			Brakes(axleInfo);
			Volant(axleInfo);

			StabilizeWheelRPM(axleInfo);
			StabilizeVehicleRollForces(axleInfo);
			StabilizeAirRotation();
			StabilizeAngularRotation();

			SetWheelForces(axleInfo.rightWheel.wheelCollider);
			SetWheelForces(axleInfo.leftWheel.wheelCollider);

			if (axleInfo.rightWheel.wheelCollider.isGrounded) gCount++;
			if (axleInfo.leftWheel.wheelCollider.isGrounded) gCount++;
		}

		if (gCount == axleInfos.Count * 2) allGrounded = true;

		//extra gravity
		if (_vehicleSettings._aerodynamics.extraGravity)
		{
			gravityValueFixedUpdate = 0;
			if (wheelFDIsGrounded && wheelFEIsGrounded && wheelTDIsGrounded && wheelTEIsGrounded)
			{
				gravityValueFixedUpdate = 4.0f * ms_Rigidbody.mass * Mathf.Clamp((KMh / _vehicleTorque.maxVelocityKMh), 0.05f, 1.0f);
			}
			else
			{
				gravityValueFixedUpdate = 4.0f * ms_Rigidbody.mass * 3.0f;
			}
			additionalCurrentGravity = Mathf.Lerp(additionalCurrentGravity, gravityValueFixedUpdate, Time.deltaTime);
			ms_Rigidbody.AddForce(Vector3.down * additionalCurrentGravity);
		}

		//forcaparaBaixo
		downForceValueFixedUpdate = _vehicleSettings.improveControl.downForce * (((KMh / 10.0f) + 0.3f) / 2.5f);
		currentDownForceVehicle = Mathf.Clamp(Mathf.Lerp(currentDownForceVehicle, downForceValueFixedUpdate, Time.deltaTime * 2.0f), 0.1f, 4.0f);

		//forcaparaBaixo2
		ms_Rigidbody.AddForce(-transform.up * downForceUpdateRef);

		
		//brakes ABS
		if (allGrounded)
		{

			absSpeedFactor = Mathf.Clamp(KMh, 70, 150);
			if (currentGear > 0 && mediumRPM > 0)
			{
				absBrakeInput = Mathf.Abs(Mathf.Clamp(verticalInput, -1.0f, 0.0f));
			}
			else if (currentGear <= 0 && mediumRPM < 0)
			{
				absBrakeInput = Mathf.Abs(Mathf.Clamp(verticalInput, 0.0f, 1.0f)) * -1;
			}
			else
			{
				absBrakeInput = 0.0f;
			}
			if (isBraking && Mathf.Abs(KMh) > 1.2f)
			{
				ms_Rigidbody.AddForce(-transform.forward * absSpeedFactor * ms_Rigidbody.mass * 0.125f  * absBrakeInput);
			}
		}
		

	}

	void DiscoverAverageRpm()
	{
		groundedWheels = 0;
		sumRPM = 0;

		foreach (AxleInfo axle in axleInfos)
		{
			axle.rightWheel.wheelColliderRPM = axle.rightWheel.wheelCollider.rpm;
			if (axle.rightWheel.wheelCollider.isGrounded)
			{
				groundedWheels++;
				sumRPM += axle.rightWheel.wheelColliderRPM;
			}

			axle.leftWheel.wheelColliderRPM = axle.leftWheel.wheelCollider.rpm;
			if (axle.leftWheel.wheelCollider.isGrounded)
			{
				groundedWheels++;
				sumRPM += axle.leftWheel.wheelColliderRPM;
			}

		}

		mediumRPM = sumRPM / groundedWheels;
		
		if (Mathf.Abs(mediumRPM) < 0.01f)
		{
			mediumRPM = 0.0f;
		}

	}

	#region UpdateTorque
	void ApplyTorque(AxleInfo axle)
	{
		leftDifferential = 1 + Mathf.Abs((0.2f * Mathf.Abs(Mathf.Clamp(horizontalInput, 0, 1))) * (angleRefVolant / 60));
		rightDifferential = 1 + Mathf.Abs((0.2f * Mathf.Abs(Mathf.Clamp(horizontalInput, -1, 0))) * (angleRefVolant / 60));
		//torque do motor
		if (theEngineIsRunning)
		{
			if (axle.rightWheel.wheelDrive)
			{
				axle.rightWheel.wheelCollider.motorTorque = VehicleTorque(axle.rightWheel.wheelCollider) * rightDifferential;
			}
			if (axle.leftWheel.wheelDrive)
			{
				axle.leftWheel.wheelCollider.motorTorque = VehicleTorque(axle.leftWheel.wheelCollider) * leftDifferential;
			}
		}
		else
		{
			if (axle.rightWheel.wheelDrive)
			{
				axle.rightWheel.wheelCollider.motorTorque = 0;
			}
			if (axle.leftWheel.wheelDrive)
			{
				axle.leftWheel.wheelCollider.motorTorque = 0;
			}
		}
	}

	public float VehicleTorque(WheelCollider wheelCollider)
	{
		torqueM = 0;
		rpmTempTorque = Mathf.Abs(wheelCollider.rpm);

		if ((Mathf.Abs(verticalInput) < 0.5f) || KMh > _vehicleTorque.maxVelocityKMh)
		{
			return 0;
		}
		if ((rpmTempTorque * wheelCollider.radius) > (50.0f * _vehicleTorque.numberOfGears * _vehicleTorque.speedOfGear))
		{
			return 0;
		}
		if (KMh < 0.5f)
		{
			if (rpmTempTorque > (25.0f / wheelCollider.radius))
			{
				return 0;
			}
		}
		if (!theEngineIsRunning)
		{
			return 0;
		}
		if (handBrakeTrue)
		{
			return 0;
		}
		if (isBraking)
		{
			return 0;
		}
		if (currentBrakeValue > 0.1f)
		{
			return 0;
		}
		if (Input.GetKey(KeyCode.Space) || brake.isDown)
		{
			return 0;
		}
		if (currentGear < 0)
		{
			clampInputTorque = Mathf.Abs(Mathf.Clamp(verticalInput, -1f, 0f));
			torqueM = (500.0f * _vehicleTorque.engineTorque) * clampInputTorque * (_vehicleTorque.gears[0].Evaluate((KMh / _vehicleTorque.speedOfGear))) * -0.8f;
		}
		else if (currentGear == 0)
		{
			return 0;
		}
		else
		{
			torqueM = (500.0f * _vehicleTorque.engineTorque) * (Mathf.Clamp(engineInput, 0f, 1f)) * _vehicleTorque.gears[currentGear - 1].Evaluate((KMh / _vehicleTorque.speedOfGear));
		}
		//AJUSTE MANUAL DAS MARCHAS
		adjustTorque = 1;
		if (currentGear < _vehicleTorque.manualAdjustmentOfTorques.Length && currentGear > 0)
		{
			if (currentGear == -1)
			{
				adjustTorque = _vehicleTorque.manualAdjustmentOfTorques[0];
			}
			else if (currentGear == 0)
			{
				adjustTorque = 0;
			}
			else if (currentGear > 0)
			{
				adjustTorque = _vehicleTorque.manualAdjustmentOfTorques[currentGear - 1];
			}
		}
		else
		{
			adjustTorque = 1;
		}

		return torqueM * adjustTorque * vehicleScale;
	}

	#endregion

	#region tireForces
	void SetWheelForces(WheelCollider wheelCollider)
	{
		wheelCollider.GetGroundHit(out tempWheelHit);
		if (wheelCollider.isGrounded)
		{
			TireSlips(wheelCollider, tempWheelHit);
			distanceXForceTemp = ms_Rigidbody.centerOfMass.y - transform.InverseTransformPoint(wheelCollider.transform.position).y + wheelCollider.radius + (1.0f - wheelCollider.suspensionSpring.targetPosition) * wheelCollider.suspensionDistance;
			lateralForcePointTemp = tempWheelHit.point + wheelCollider.transform.up * _vehicleSettings.improveControl.helpToStraightenOut * distanceXForceTemp;
			forwardForceTemp = tempWheelHit.forwardDir * (tireFO.y) * 3.0f;
			lateralForceTemp = tempWheelHit.sidewaysDir * (tireFO.x);
			if (Mathf.Abs(horizontalInput) > 0.1f && wheelCollider.steerAngle != 0.0f && Mathf.Sign(wheelCollider.steerAngle) != Mathf.Sign(tireSL.x))
			{
				lateralForcePointTemp += tempWheelHit.forwardDir * _vehicleSettings.improveControl.helpToTurn;
			}
			ms_Rigidbody.AddForceAtPosition(forwardForceTemp, tempWheelHit.point);
			ms_Rigidbody.AddForceAtPosition(lateralForceTemp, lateralForcePointTemp);
		}
	}

	public Vector2 WheelLocalVelocity(WheelHit wheelHit)
	{
		tempLocalVelocityVector2 = new Vector2(0, 0);
		tempWheelVelocityVector3 = ms_Rigidbody.GetPointVelocity(wheelHit.point);
		velocityLocalWheelTemp = tempWheelVelocityVector3 - Vector3.Project(tempWheelVelocityVector3, wheelHit.normal);
		tempLocalVelocityVector2.y = Vector3.Dot(wheelHit.forwardDir, velocityLocalWheelTemp);
		tempLocalVelocityVector2.x = Vector3.Dot(wheelHit.sidewaysDir, velocityLocalWheelTemp);
		return tempLocalVelocityVector2;
	}
	public float AngularVelocity(Vector2 localVelocityVector, WheelCollider wheelCollider)
	{
		wheelCollider.GetGroundHit(out tempWheelHit);
		return (localVelocityVector.y + (tempWheelHit.sidewaysSlip * ((Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)) / 2.0f) * (-2.0f))) / wheelCollider.radius;
	}
	public Vector2 LocalSurfaceForce(WheelHit wheelHit)
	{
		wheelSpeedLocalSurface = ms_Rigidbody.GetPointVelocity(wheelHit.point);
		forceFactorTempLocalSurface = Mathf.InverseLerp(1.0f, 0.25f, (wheelSpeedLocalSurface - Vector3.Project(wheelSpeedLocalSurface, wheelHit.normal)).sqrMagnitude);
		if (forceFactorTempLocalSurface > 0.0f)
		{
			normalTemp = Vector3.Dot(Vector3.up, wheelHit.normal);
			if (normalTemp > 0.000001f)
			{
				downForceUPTemp = Vector3.up * wheelHit.force / normalTemp;
				surfaceLocalForceTemp = downForceUPTemp - Vector3.Project(downForceUPTemp, wheelHit.normal);
			}
			else
			{
				surfaceLocalForceTemp = Vector3.up * 1000000.0f;
			}
			surfaceLocalForce.y = Vector3.Dot(wheelHit.forwardDir, surfaceLocalForceTemp);
			surfaceLocalForce.x = Vector3.Dot(wheelHit.sidewaysDir, surfaceLocalForceTemp);
			surfaceLocalForce *= forceFactorTempLocalSurface;
		}
		else
		{
			surfaceLocalForce = Vector2.zero;
		}
		return surfaceLocalForce;
	}

	public void TireSlips(WheelCollider wheelCollider, WheelHit wheelHit)
	{
		localVelocityWheelTireSlips = WheelLocalVelocity(wheelHit);
		localSurfaceForceDTireSlips = LocalSurfaceForce(wheelHit);
		if (KMh > _vehicleTorque.maxVelocityKMh)
		{
			reverseForce = -5 * ms_Rigidbody.velocity.magnitude;
		}
		else
		{
			reverseForce = 0;
		}
		angularWheelVelocityTireSlips = AngularVelocity(localVelocityWheelTireSlips, wheelCollider);
		if (wheelCollider.isGrounded)
		{
			estimatedSprungMass = Mathf.Clamp(wheelHit.force / -Physics.gravity.y, 0.0f, wheelCollider.sprungMass) * 0.5f;
			localRigForceTireSlips = (-estimatedSprungMass * localVelocityWheelTireSlips / Time.deltaTime) + localSurfaceForceDTireSlips;
			tireSlipTireSlips.x = localVelocityWheelTireSlips.x;
			tireSlipTireSlips.y = localVelocityWheelTireSlips.y - angularWheelVelocityTireSlips * wheelCollider.radius;
			downForceTireSlips = (currentDownForceVehicle * _vehicleSettings.vehicleMass);
			if (wheelCollider.brakeTorque > 10)
			{
				wheelMaxBrakeSlip = Mathf.Max(Mathf.Abs(localVelocityWheelTireSlips.y * 0.2f), 0.3f);
				minSlipYTireSlips = Mathf.Clamp(Mathf.Abs(reverseForce * tireSlipTireSlips.x) / downForceTireSlips, 0.0f, wheelMaxBrakeSlip);
			}
			else
			{
				minSlipYTireSlips = Mathf.Min(Mathf.Abs(reverseForce * tireSlipTireSlips.x) / downForceTireSlips, Mathf.Clamp((verticalInput * 2.5f), -2.5f, 1.0f));
				if (reverseForce != 0.0f && minSlipYTireSlips < 0.1f) minSlipYTireSlips = 0.1f;
			}
			if (Mathf.Abs(tireSlipTireSlips.y) < minSlipYTireSlips) tireSlipTireSlips.y = minSlipYTireSlips * Mathf.Sign(tireSlipTireSlips.y);
			rawTireForceTireSlips = -downForceTireSlips * tireSlipTireSlips.normalized;
			rawTireForceTireSlips.x = Mathf.Abs(rawTireForceTireSlips.x);
			rawTireForceTireSlips.y = Mathf.Abs(rawTireForceTireSlips.y);
			tireForceTireSlips.x = Mathf.Clamp(localRigForceTireSlips.x, -rawTireForceTireSlips.x, +rawTireForceTireSlips.x);
			if (wheelCollider.brakeTorque > 10)
			{
				maxFyTireSlips = Mathf.Min(rawTireForceTireSlips.y, reverseForce);
				tireForceTireSlips.y = Mathf.Clamp(localRigForceTireSlips.y, -maxFyTireSlips, +maxFyTireSlips);
			}
			else
			{
				tireForceTireSlips.y = Mathf.Clamp(reverseForce, -rawTireForceTireSlips.y, +rawTireForceTireSlips.y);
			}
		}
		else
		{
			tireSlipTireSlips = Vector2.zero;
			tireForceTireSlips = Vector2.zero;
		}
		tireSL = tireSlipTireSlips * _vehicleSettings.improveControl.tireSlipsFactor;
		tireFO = tireForceTireSlips * _vehicleSettings.improveControl.tireSlipsFactor;
	}
	#endregion

	#region BrakesUpdate
	void Brakes(AxleInfo axle)
	{
		brakeVerticalInput = 0.0f;

		brakeVerticalInput = verticalInput;

		//Freio de pé
		if (currentGear > 0)
		{
			currentBrakeValue = Mathf.Abs(Mathf.Clamp(brakeVerticalInput, -1.0f, 0.0f)) * 1.5f;
		}
		else if (currentGear < 0)
		{
			currentBrakeValue = Mathf.Abs(Mathf.Clamp(brakeVerticalInput, 0.0f, 1.0f)) * 1.5f;
		}
		else if (currentGear == 0)
		{
			if (mediumRPM > 0)
			{
				currentBrakeValue = Mathf.Abs(Mathf.Clamp(brakeVerticalInput, -1.0f, 0.0f)) * 1.5f;
			}
			else
			{
				currentBrakeValue = Mathf.Abs(Mathf.Clamp(brakeVerticalInput, 0.0f, 1.0f)) * 1.5f;
			}
		}

		// FREIO DE MÃO
		handBrake_Input = 0.0f;
		if (handBrakeTrue)
		{
			if (Mathf.Abs(brakeVerticalInput) < 0.9f)
			{
				handBrake_Input = 2;
			}
			else
			{
				handBrake_Input = 0;
				handBrakeTrue = false;
			}
		}
		else
		{
			handBrake_Input = 0;
		}
		if (Input.GetKey(KeyCode.Space) || brake.isDown)
		{
			handBrake_Input = 2;
		}
		handBrake_Input = handBrake_Input * 1000;
		
		//FREIO TOTAL
		totalFootBrake = currentBrakeValue * 0.5f * _vehicleSettings.vehicleMass*100;
		totalHandBrake = handBrake_Input * 0.5f * _vehicleSettings.vehicleMass;

			if (Mathf.Abs(mediumRPM) < 15 && Mathf.Abs(brakeVerticalInput) < 0.05f && !handBrakeTrue && (totalFootBrake + totalHandBrake) < 100)
			{
				brakingAuto = true;
				totalFootBrake = 1.5f * _vehicleSettings.vehicleMass;
			}
			else
			{
				brakingAuto = false;
			}
		
		//freiar\/
		if (totalFootBrake > 10)
		{
			isBraking = true;
		}
		else
		{
			isBraking = false;
		}

		if (!brakingAuto)
		{
			if (isBraking && Mathf.Abs(KMh) > 1.2f)
			{
				totalFootBrake = 0;
			}
		}

		ApplyBrakeInWheels(axle.rightWheel.wheelCollider, axle.rightWheel.wheelHandBrake);
		ApplyBrakeInWheels(axle.leftWheel.wheelCollider, axle.leftWheel.wheelHandBrake);
	}

	void ApplyBrakeInWheels(WheelCollider wheelCollider, bool handBrake)
	{
		if (handBrake)
		{
			wheelCollider.brakeTorque = totalFootBrake + totalHandBrake;
		}
		else
		{
			wheelCollider.brakeTorque = totalFootBrake;
		}
		//evitar RPM, freio ou torques invalidos, EvitarRotacaoSemTorque
		if (!wheelCollider.isGrounded && Mathf.Abs(wheelCollider.rpm) > 0.5f && Mathf.Abs(verticalInput) < 0.05f && wheelCollider.motorTorque < 5.0f)
		{
			wheelCollider.brakeTorque += _vehicleSettings.vehicleMass * Time.deltaTime * 50;
		}
		if (KMh < 0.5f && Mathf.Abs(verticalInput) < 0.05f)
		{
			if (wheelCollider.rpm > (25 / wheelCollider.radius))
			{
				wheelCollider.brakeTorque += 0.5f * _vehicleSettings.vehicleMass * Mathf.Abs(wheelCollider.rpm) * Time.deltaTime;
			}
		}
	}
	#endregion

	#region Stabilizers
	void StabilizeAngularRotation()
	{
		if (Mathf.Abs(horizontalInput) < 0.9f)
		{
			ms_Rigidbody.angularVelocity = Vector3.Lerp(ms_Rigidbody.angularVelocity, new Vector3(ms_Rigidbody.angularVelocity.x, 0, ms_Rigidbody.angularVelocity.z), Time.deltaTime * 2);
		}
	}

	void StabilizeAirRotation()
	{
		if (!colliding)
		{
			isGroundedExtraW = false;
			if (!wheelFDIsGrounded && !wheelFEIsGrounded && !wheelTDIsGrounded && !wheelTEIsGrounded && !isGroundedExtraW)
			{
				axisFromRotate = Vector3.Cross(transform.up, Vector3.up);
				torqueForceAirRotation = axisFromRotate.normalized * axisFromRotate.magnitude * 2.0f;
				torqueForceAirRotation -= ms_Rigidbody.angularVelocity;
				ms_Rigidbody.AddTorque(torqueForceAirRotation * ms_Rigidbody.mass * 0.02f, ForceMode.Impulse);
				if (Mathf.Abs(horizontalInput) > 0.1f)
				{
					ms_Rigidbody.AddTorque(transform.forward * -horizontalInput * _vehicleSettings.vehicleMass * 0.6f);
				}
				if (Mathf.Abs(verticalInput) > 0.1f)
				{
					ms_Rigidbody.AddTorque(transform.right * verticalInput * _vehicleSettings.vehicleMass * 0.44f);
				}
			}
		}
	}

	void StabilizeWheelRPM(AxleInfo axle)
	{
		if (currentGear > 0)
		{
			if (KMh > (_vehicleTorque.maxVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear) && Mathf.Abs(verticalInput) < 0.5f)
			{
				if (axle.rightWheel.wheelDrive)
				{
					axle.rightWheel.wheelCollider.brakeTorque = forceEngineBrake;
				}
				if (axle.leftWheel.wheelDrive)
				{
					axle.leftWheel.wheelCollider.brakeTorque = forceEngineBrake;
				}
			}
		}
		else if (currentGear == -1)
		{
			if (KMh > (_vehicleTorque.maxVelocityGears[0] * _vehicleTorque.speedOfGear) && Mathf.Abs(verticalInput) < 0.5f)
			{
				if (axle.rightWheel.wheelDrive)
				{
					axle.rightWheel.wheelCollider.brakeTorque = forceEngineBrake / 5.0f;
				}
				if (axle.leftWheel.wheelDrive)
				{
					axle.leftWheel.wheelCollider.brakeTorque = forceEngineBrake / 5.0f;
				}
			}
		}
	}

	void StabilizeVehicleRollForces(AxleInfo axle)
	{
		leftFrontForce = 1.0f;
		rightFrontForce = 1.0f;
		leftRearForce = 1.0f;
		rightRearForce = 1.0f;

		//CHECAR COLISOES
		//rodasTraz
		//rodasFrente
		bool isGround1 = axle.leftWheel.wheelCollider.GetGroundHit(out tempWheelHit);
		if (isGround1)
		{
			leftFrontForce = (-axle.leftWheel.wheelCollider.transform.InverseTransformPoint(tempWheelHit.point).y - axle.leftWheel.wheelCollider.radius) / axle.leftWheel.wheelCollider.suspensionDistance;
		}
		bool isGround2 = axle.rightWheel.wheelCollider.GetGroundHit(out tempWheelHit);
		if (isGround2)
		{
			rightFrontForce = (-axle.rightWheel.wheelCollider.transform.InverseTransformPoint(tempWheelHit.point).y - axle.rightWheel.wheelCollider.radius) / axle.rightWheel.wheelCollider.suspensionDistance;
		}

		//APLICAR FORCAS DESCOBERTAS
		roolForce1 = (leftRearForce - rightRearForce) * _vehicleSettings._aerodynamics.feelingHeavy * _vehicleSettings.vehicleMass * inclinationFactorForcesDown;
		roolForce2 = (leftFrontForce - rightFrontForce) * _vehicleSettings._aerodynamics.feelingHeavy * _vehicleSettings.vehicleMass * inclinationFactorForcesDown;
		//rodasFrente
		if (isGround1)
		{
			ms_Rigidbody.AddForceAtPosition(axle.leftWheel.wheelCollider.transform.up * -roolForce2, axle.leftWheel.wheelCollider.transform.position);
		}
		if (isGround2)
		{
			ms_Rigidbody.AddForceAtPosition(axle.rightWheel.wheelCollider.transform.up * roolForce2, axle.rightWheel.wheelCollider.transform.position);
		}
	}
	#endregion

	#region GearsManager
	void AutomaticGears()
	{//aqui
		if (currentGear == 0)
		{//entre -5 e 5 RPM, se a marcha estver em 0
			if (mediumRPM < 5 && mediumRPM >= 0)
			{
				currentGear = 1;
			}
			if (mediumRPM > -5 && mediumRPM < 0)
			{
				currentGear = -1;
			}
		}
		if (mediumRPM < -0.1f && Mathf.Abs(verticalInput) < 0.1f)
		{
			currentGear = -1;
		}
		if (Mathf.Abs(verticalInput) < 0.1f && mediumRPM >= 0 && currentGear < 2)
		{
			currentGear = 1;
		}
		if ((Mathf.Abs(Mathf.Clamp(verticalInput, -1f, 0f))) > 0.8f)
		{
			if ((KMh < 5 && mediumRPM < 1) || mediumRPM < -2)
			{
				currentGear = -1;
			}
		}
		if ((Mathf.Abs(Mathf.Clamp(verticalInput, 0f, 1f))) > 0.8f)
		{
			if ((KMh < 5) || (mediumRPM > 2 && currentGear < 2))
			{
				currentGear = 1;
			}
		}


		//
		if (currentGear > 0)
		{
			if (KMh > (_vehicleTorque.idealVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear + 7 * _vehicleTorque.speedOfGear))
			{
				if (currentGear < _vehicleTorque.numberOfGears && !changinGearsAuto && currentGear != -1)
				{
					timeAutoGear = 1.5f;
					StartCoroutine("TimeAutoGears", currentGear + 1);
				}
			}
			else if (KMh < (_vehicleTorque.idealVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear - 15 * _vehicleTorque.speedOfGear))
			{
				if (currentGear > 1 && !changinGearsAuto)
				{
					timeAutoGear = 0;
					StartCoroutine("TimeAutoGears", currentGear - 1);
				}
			}
			if (verticalInput > 0.1f && KMh > (_vehicleTorque.idealVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear + 1 * _vehicleTorque.speedOfGear))
			{
				if (currentGear < _vehicleTorque.numberOfGears && currentGear != -1)
				{
					timeAutoGear = 0.0f;
					StartCoroutine("TimeAutoGears", currentGear + 1);
				}
			}
		}
	}
	IEnumerator TimeAutoGears(int gear)
	{
		changinGearsAuto = true;
		yield return new WaitForSeconds(0.4f);
		currentGear = gear;
		yield return new WaitForSeconds(timeAutoGear);
		changinGearsAuto = false;
	}
	#endregion

	#region UpdateWheelMesh
	void UpdateWheelMeshes()
	{

		foreach (AxleInfo axle in axleInfos)
		{
			axle.rightWheel.wheelCollider.GetWorldPose(out vectorMeshPos1, out quatMesh1);
			axle.rightWheel.wheelWorldPosition = axle.rightWheel.wheelMesh.position = vectorMeshPos1;
			axle.rightWheel.wheelMesh.rotation = quatMesh1;
			//
			axle.leftWheel.wheelCollider.GetWorldPose(out vectorMeshPos2, out quatMesh2);
			axle.leftWheel.wheelWorldPosition = axle.leftWheel.wheelMesh.position = vectorMeshPos2;
			axle.leftWheel.wheelMesh.rotation = quatMesh2;
		
		}

	}
	#endregion

	#region VolantManager

	void Volant(AxleInfo axle)
	{
		angle1Ref = Mathf.MoveTowards(angle1Ref, horizontalInput, 2 * Time.deltaTime);
		angle2Volant = Mathf.MoveTowards(angle2Volant, horizontalInput, 2 * Time.deltaTime);

		angleRefVolant = Mathf.Clamp(angle1Ref * maxAngleVolant, -maxAngleVolant, maxAngleVolant);

		//APLICAR ANGULO NAS RODAS--------------------------------------------------------------------------------------------------------------
		if (angle1Ref > 0.2f)
		{
			if (axle.rightWheel.wheelTurn)
			{
				axle.rightWheel.wheelCollider.steerAngle = angleRefVolant * 1.2f;
			}
			if (axle.leftWheel.wheelTurn)
			{
				axle.leftWheel.wheelCollider.steerAngle = angleRefVolant;
			}
		}
		else if (angle1Ref < -0.2f)
		{
			if (axle.rightWheel.wheelTurn)
			{
				axle.rightWheel.wheelCollider.steerAngle = angleRefVolant;
			}
			if (axle.leftWheel.wheelTurn)
			{
				axle.leftWheel.wheelCollider.steerAngle = angleRefVolant * 1.2f;
			}

		}
		else
		{
			if (axle.rightWheel.wheelTurn)
			{
				axle.rightWheel.wheelCollider.steerAngle = angleRefVolant;
			}
			if (axle.leftWheel.wheelTurn)
			{
				axle.leftWheel.wheelCollider.steerAngle = angleRefVolant;
			}

		}

	}
	#endregion

	#region CoroutineStartEndTurnOff
	void TurnOnEngine()
	{
		StartCoroutine("StartEngineCoroutine", false);
		//StartCoroutine("TurnOffEngineTime");
	}
	IEnumerator StartEngineTime()
	{
		yield return new WaitForSeconds(3);
	}
	IEnumerator TurnOffEngineTime()
	{
		yield return new WaitForSeconds(1);
	}
	IEnumerator StartEngineCoroutine(bool startOn)
	{
		if (startOn)
		{
			yield return new WaitForSeconds(1.5f);
			theEngineIsRunning = true;
		}
		else
		{
			enableEngineSound = false;
			theEngineIsRunning = false;
		}
	}
	#endregion

	#region SoundsManager
	public AudioSource GenerateAudioSource(string name, float minDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool playAwake)
	{
		GameObject audioSource = new GameObject(name);
		audioSource.transform.position = transform.position;
		audioSource.transform.parent = transform;
		AudioSource temp = audioSource.AddComponent<AudioSource>() as AudioSource;
		temp.minDistance = minDistance;
		temp.volume = volume;
		temp.clip = audioClip;
		temp.loop = loop;
		temp.playOnAwake = playAwake;
		temp.spatialBlend = 1.0f;
		temp.dopplerLevel = 0.0f;
		if (playNow)
		{
			temp.Play();
		}
		return temp;
	}

	void Sounds()
	{

		//SOM DO MOTOR
		if (changinGearsAuto)
		{
			engineSoundFactor = Mathf.Lerp(engineSoundFactor, 0.75f, Time.deltaTime * 0.6f);
		}
		else
		{
			engineSoundFactor = 1;
		}
		if (currentGear == -1 || currentGear == 0)
		{
			velxCurrentRPM = (Mathf.Clamp(KMh, (_vehicleTorque.minVelocityGears[0] * _vehicleTorque.speedOfGear), (_vehicleTorque.maxVelocityGears[0] * _vehicleTorque.speedOfGear)));
			pitchAUD = Mathf.Clamp(((velxCurrentRPM / (_vehicleTorque.maxVelocityGears[0] * _vehicleTorque.speedOfGear)) * _sounds.speedOfEngineSound * engineSoundFactor), 0.85f, _sounds.speedOfEngineSound);
		}
		else
		{
			velxCurrentRPM = (Mathf.Clamp(KMh, (_vehicleTorque.minVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear), (_vehicleTorque.maxVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear)));
			nextPitchAUD = ((velxCurrentRPM / (_vehicleTorque.maxVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear)) * _sounds.speedOfEngineSound * engineSoundFactor);
			if (KMh < (_vehicleTorque.minVelocityGears[currentGear - 1] * _vehicleTorque.speedOfGear))
			{
				nextPitchAUD = 0.85f;
				speedLerpSound = 0.5f;
			}
			else
			{
				if (speedLerpSound < 4.9f)
				{
					speedLerpSound = Mathf.Lerp(speedLerpSound, 5.0f, Time.deltaTime);
				}
			}
			pitchAUD = Mathf.Clamp(nextPitchAUD, 0.85f, _sounds.speedOfEngineSound);
		}
		if (_sounds.engineSound)
		{
			if (theEngineIsRunning)
			{
				engineSoundAUD.volume = Mathf.Lerp(engineSoundAUD.volume, Mathf.Clamp(Mathf.Abs(engineInput), 0.35f, 0.85f), Time.deltaTime * 5.0f);
				if (handBrakeTrue || currentGear == 0)
				{
					engineSoundAUD.pitch = Mathf.Lerp(engineSoundAUD.pitch, 0.85f + Mathf.Abs(verticalInput) * (_sounds.speedOfEngineSound * 0.7f - 0.85f), Time.deltaTime * 5.0f);
				}
				else
				{
					engineSoundAUD.pitch = Mathf.Lerp(engineSoundAUD.pitch, pitchAUD, Time.deltaTime * speedLerpSound);
				}
			}
			else
			{
				if (enableEngineSound)
				{
					engineSoundAUD.volume = 1;
					engineSoundAUD.pitch = Mathf.Lerp(engineSoundAUD.pitch, 0.7f, Time.deltaTime);
				}
				else
				{
					engineSoundAUD.volume = Mathf.Lerp(engineSoundAUD.volume, 0f, Time.deltaTime);
					engineSoundAUD.pitch = Mathf.Lerp(engineSoundAUD.pitch, 0f, Time.deltaTime);
				}
			}
		}

	}
	#endregion

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F) { angle += 360F; }
		if (angle > 360F) { angle -= 360F; }
		return Mathf.Clamp(angle, min, max);
	}

}
