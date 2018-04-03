using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to create the opening animation on the game menu
public class MonsterFlying : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	public Vector3 movement = new Vector3 (-20, 0, 0);
	public Vector3 rotation = new Vector3 (15, 30, 45);

	// Update is called once per frame
	void Update () {
		transform.Translate (movement * Time.deltaTime, Space.World); 
		//transform.Rotate (rotation * Time.deltaTime); //delta time helps make it smooth
	}
}
