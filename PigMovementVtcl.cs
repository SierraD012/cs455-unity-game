using UnityEngine;
using UnityEngine.AI;
using System.Collections;


//DEPRECATED
public class PigMovementVtcl : MonoBehaviour {

	public float distance;
	private float min = 2f;
	private float max = 3f;
	private bool pigActive = false; //GM uses this to start/stop the pig's movement

	// Use this for initialization
	void Start () {
		min=transform.position.x;
		max=transform.position.x + distance;
	}

	//private float up = .05f;

	// Update is called once per frame
	void Update () {
		if (pigActive) {
			
			transform.position = new Vector3(Mathf.PingPong(Time.time*3,max-min)+min, transform.position.y, transform.position.z); //transform.position.z

			//transform.rotation = Quaternion.LookRotation (movement);

			if (transform.position.x < max + .1 && transform.position.x > max - .1) {
				transform.rotation = Quaternion.Euler(0, 0, 0);
			}

			if (transform.position.x < min + .1 && transform.position.x > min - .1) {
				transform.rotation = Quaternion.Euler(0, 180, 0);
			}
		}
			
	}


	public bool IsPigActive(){
		return pigActive;
	}

	public void SetPigActive() {
		pigActive = true;
	}

	public void SetPigInactive() {
		pigActive = false;
	}

}