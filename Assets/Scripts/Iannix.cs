using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iannix : MonoBehaviour {

	// the IP of Iannix (127.0.0.1 if testing locally)
	public string RemoteIP = "127.0.0.1";

	// the port IanniX is listening on
	public int RemotePort = 1234;

	// the port that Unity is listening on
	public int LocalPort = 57120;

	// drag the game objects onto the list, and set the first cursor id as it appears inside IanniX
	public int firstCursorID = 0;
	public GameObject[] cursors;

	// drag the game objects onto the list, and set the first trigger id as it appears inside IanniX
	public int firstTriggerID = 2000;
	public GameObject[] triggers;


	// mapping iannix ids to coordinates and trigger values
	private Vector3[] cursorCoord;
	private Vector3[] triggerCoord;
	private float[] triggerValue;

	// the OSC object
	private Osc osc;
	private Udp udp;

	// set this to true to print out the OSC messages
	public bool debug = false;

	// Use this for initialization
	void Start () {
			cursorCoord = new Vector3[cursors.Length];
			triggerCoord = new Vector3[triggers.Length];
			triggerValue = new float[triggers.Length];

			//Initializes on start up to listen for messages
			//make sure this game object has both UDPPackIO and OSC script attached

			Debug.Log("Connecting to IanniX via UDP.");

			udp = (Udp) GetComponent("Udp");
			udp.init(RemoteIP, RemotePort, LocalPort);

			osc = (Osc) GetComponent("Osc");
			osc.init(udp);
			osc.SetAllMessageHandler(AllMessageHandler);
	}

	// Update is called once per frame
	void Update () {
			// Debug.Log("Update.");

			// update position and rotation of all cursors
			for (int i = 0; i < cursors.Length; i++) {
					// look ahead
					cursors[i].transform.LookAt(cursorCoord[i]);
					// update position
					cursors[i].transform.position = cursorCoord[i];
			}

			// update position and scaling of all triggers
			for (int i = 0; i < triggers.Length; i++) {
				// update position
				triggers[i].transform.position = triggerCoord[i];
				// linear fall-off to zero
				float t = triggerValue[i];
				float tm = (float) (t - 0.01);
				triggerValue[i] = Mathf.Max(tm, 0);
				// scale the object to match the trigger value
				triggers[i].transform.localScale = new Vector3(t, t, t);
			}
	}

	void OnDisable() {
		    // close UDP socket of the listener
		    Debug.Log("Closing UDP socket");
		    osc.Cancel();
		    osc = null;
	}

	// custom logger.
	// Turn this on or off using the debug toggle.
	public void log(string msg) {
			if (debug == true) {
				Debug.Log(msg);
			}
	}

	public void AllMessageHandler(OscMessage msg) {

			// log the OSC message
			log(Osc.OscMessageToString(msg));

			// message parameters
			var address = msg.Address;
			var values = msg.Values;

			// variables to hold the dataa
			int id;
			string group_id;

			float x;
			float y;
			float z;

			float x_world;
			float y_world;
			float z_world;

			// index of our world objects
			int i;

			// different actions, based on the address pattern
			switch (address) {

					// FORMAT:  /cursor id group_id x y z x_world y_world z_world
					case "/cursor":
							// extract the data
							id = Convert.ToInt32(values[0]);
							group_id = (string) values[1];

							x = (float) values[2];
							y = (float) values[3];
							z = (float) values[4];

							x_world = (float) values[5];
							y_world = (float) values[6];
							z_world = (float) values[7];

							// log the data
							log(
								  "CURSOR id: " + id + "\t\t\t\t"
								+ "GROUP: " +  group_id + "\n"
								+ "COORDS: (" + x + ", " + y + ", " + z + ")" + "\t"
								+ "WORLD: (" + x_world + ", " + y_world + ", " + z_world + ")"
							);

							// index of the cursor object in our list of unity world objects
							i = id - firstCursorID;

							// update coordinates if there is a game object corresponding to the index
							if (i >= 0 && i < cursorCoord.Length) {
								cursorCoord[i] = new Vector3(x_world, y_world, z_world);
							}

							break;

					// FORMAT: /trigger id group_id x y z x_world y_world z_world
					case "/trigger":
							// extract the data
							id = Convert.ToInt32(values[0]);
							group_id = (string) values[1];

							x = (float) values[2];
							y = (float) values[3];
							z = (float) values[4];

							x_world = (float) values[5];
							y_world = (float) values[6];
							z_world = (float) values[7];

							// log the data
							log(
								  "CURSOR id: " + id + "\t\t\t\t"
								+ "GROUP: " +  group_id + "\n"
								+ "COORDS: (" + x + ", " + y + ", " + z + ")" + "\t"
								+ "WORLD: (" + x_world + ", " + y_world + ", " + z_world + ")"
							);

							// index of the trigger object in our list of unity world objects
							i = (int) id - firstTriggerID;

							// update coordinates if there is a game object corresponding to the index
							if (i >= 0 && i < triggerCoord.Length) {
								triggerCoord[i] = new Vector3(x_world, y_world, z_world);
								triggerValue[i] = 0.25f;
							}

							break;

					case "/curve":
							break;

					case "/transport":
							break;
			}

		}

}
