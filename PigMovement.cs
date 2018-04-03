using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PigMovement : MonoBehaviour {

	public float walkDistance; 
	public bool horizontal; //if this is false, pig walks vertical 
	private float min = 2f;
	private float max = 3f;
	private bool pigActive = false; //GM uses this to start/stop the pig's movement

	// Use this for initialization
	void Start () {
		if (horizontal){
			min=transform.position.z;
			max=transform.position.z + walkDistance;
		} else {
			min=transform.position.x;
			max=transform.position.x + walkDistance;
		}
	}
		
	// Update is called once per frame
	void Update () {
		if (pigActive) {

			if (horizontal) {
				transform.position = new Vector3 (transform.position.x, transform.position.y, Mathf.PingPong (Time.time * 3, max - min) + min);

				if (transform.position.z < max + .1 && transform.position.z > max - .1) {
					transform.rotation = Quaternion.Euler (0, 270, 0);
				}

				if (transform.position.z < min + .1 && transform.position.z > min - .1) {
					transform.rotation = Quaternion.Euler (0, 90, 0);
				}

			} else {
				//do vertical movement
				transform.position = new Vector3 (Mathf.PingPong (Time.time * 3, max - min) + min, transform.position.y, transform.position.z);

				if (transform.position.x < max + .1 && transform.position.x > max - .1) {
					transform.rotation = Quaternion.Euler (0, 0, 0);
				}

				if (transform.position.x < min + .1 && transform.position.x > min - .1) {
					transform.rotation = Quaternion.Euler (0, 180, 0);
				}
					
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