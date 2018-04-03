using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (25, 0, 0) * Time.deltaTime); //delta time helps make it smooth
	}
}
