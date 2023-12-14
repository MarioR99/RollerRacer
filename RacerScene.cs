//============================================================================
// RacerScene
//============================================================================
using Godot;
using System;
using System.Linq.Expressions;

public partial class RacerScene : Node3D
{
	private CamRig cam;
	float longitudeDeg;
	float latitudeDeg;
	float camDist;
	float camFOV;
	Vector3 camTg;       // coords of camera target
	Vector3 camSubject;  // coords that cam is lerping toward
	float lerpSpeed;     // factor for lerping the camera

	Cart cart;           // model of the cart
	double wheelRad;     // wheel radius
	double wheelRadS;    // wheel radius of steared wheel
	double wheelSep;     // distance separating the two wheels.
	double baseLen;      // base length, from rear wheel axle to steer axis.
	double casterLen;    // length of caster
	double cgDist;       // longit distance from wheel axle to CG
	float tireWd;        // tire width
	float wheelWd;      // wheel width
	float tireWdS;      // thickness of steered wheel
	float wheelWdS;     // width of steered wheel
	float frameThk;      // frame thickness
	double m;            // mass of racer
	double rGyr;         // radius of gyration

	double steerSig;     // steer signal from pilot

	RollerRacer racer;   // simulation of the Roller Racer
	double time;         // elapsed time

	UIPanelDisplay dataDisplay;

	//------------------------------------------------------------------------
	// _Ready: Called when the node enters the scene tree for the first time.
	//------------------------------------------------------------------------
	public override void _Ready()
	{
		// Set up the camera rig
		longitudeDeg = 30.0f;
		latitudeDeg = 20.0f;
		camDist = 4.0f;
		camFOV = 55.0f;

		lerpSpeed = 3.0f;
		camSubject = new Vector3(0.0f, 1.0f, 0.0f);
		camTg = new Vector3(0.0f, 1.0f, 0.0f);
		cam = GetNode<CamRig>("CamRig");
		cam.LongitudeDeg = longitudeDeg;
		cam.LatitudeDeg = latitudeDeg;
		cam.Distance = camDist;
		cam.Target = camTg;
		cam.FOVDeg = camFOV;

		// set up the data display
		dataDisplay = GetNode<UIPanelDisplay>("Control/MarginContainer/DataDisplay");
		dataDisplay.SetNDisplay(5);
		dataDisplay.SetLabel(0,"Roller Racer");
		dataDisplay.SetValue(0,"");
		dataDisplay.SetLabel(1,"Speed");
		dataDisplay.SetValue(1,"---");
		dataDisplay.SetLabel(2,"Kin. Energy");
		dataDisplay.SetValue(2,"---");
		dataDisplay.SetLabel(3,"Slip Rate F");
		dataDisplay.SetValue(3,"---");
		dataDisplay.SetLabel(4,"Slip Rate R");
		dataDisplay.SetValue(4,"---");

		// set up the cart model
		cart = GetNode<Cart>("Cart");
		wheelRad = 0.5 * 0.75;  cart.WheelRadius = (float)wheelRad;
		wheelRadS = 0.15;       cart.SteeredWheelRadius = (float)wheelRadS;
		wheelSep = 1.0;         cart.WheelSeparation = (float)wheelSep;
		baseLen = 1.3;			cart.BaseLength = (float)baseLen;
		casterLen = 0.3; 		cart.CasterLength = (float)casterLen;
		cgDist = 0.6;
		tireWd = 0.08f;         cart.TireWidth = tireWd;
		wheelWd = 0.5f * tireWd; 	cart.WheelWidth = wheelWd;
		tireWdS = 0.05f;		cart.SteeredTireWidth = tireWdS;
		wheelWdS = 0.7f * tireWdS;	cart.SteeredWheelWidth = wheelWdS;
		frameThk = 0.08f;		cart.FrameBarWidth = frameThk;
		cart.BuildModel();

		// set up the simulation
		m = 25.0;
		rGyr = 0.3;
		racer = new RollerRacer();    // simulation
		time = 0.0;
	}

	//------------------------------------------------------------------------
	// _Process
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//------------------------------------------------------------------------
	public override void _Process(double delta)
	{
		
		cart.SetLoc(0.0f, 0.0f, 0.0f, // need to update this with other states
			0.0f, 0.0f, 0.0f, 
			(float)racer.SteerAngle);
	}

    //------------------------------------------------------------------------
    // _PhysicsProcess
    //------------------------------------------------------------------------
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		ProcessPilotInput();
		racer.SteerAngleSignal = (-50.0 * steerSig)*Math.PI/180.0;

		racer.StepRK2(time,delta);  // You are going to use the RK4 integrator
		time += delta;

		camSubject.X = (float)racer.xG;
		camSubject.Z = (float)racer.zG;
		camTg = camTg.Lerp(camSubject, lerpSpeed*(float)delta);
		cam.Target = camTg;
    }

    //------------------------------------------------------------------------
    // ProcessPilotInput:
    //------------------------------------------------------------------------
    private void ProcessPilotInput()
	{

		steerSig = (double)Input.GetJoyAxis(0, JoyAxis.LeftX);
		if(Math.Abs(steerSig) < 0.01)
			steerSig = 0.0;

		if(Input.IsActionPressed("ui_left")){
			steerSig = -1.0;
		}
		if(Input.IsActionPressed("ui_right")){
			steerSig = 1.0;
		}

	}

}
