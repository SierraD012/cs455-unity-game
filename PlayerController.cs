using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {

	public const string pickUpTag = "Pick Up";
	public const string powerUpTag = "Power Up";
	public const string playerTag = "Player";
	public const string enemyTag = "Enemy";

	public int playerNum = 1;         
	public float moveSpeed = 12f;            
	public float turnSpeed = 300; 

	private string forwardMvtInputName;  //Used to tell GetAxis the name of the axis to look for   
	private string sidewaysMvtInputName; 
	private float verticalInput;    //Used to store the result of calling GetAxis
	private float horizontalInput;  
	private Vector3 movement = new Vector3 ();
	private Vector3 currentDirection;
	private Vector3 nextDirection;
	private Rigidbody rigidBody;  

	private int score = 0;
	private bool gotPickup = false; //set this to true each time thru onTriggerEnter, then back to false every time GotPickup() getter is called by GM 
	private bool playerAlive = true; //set this to false if they hit a pig

	public AudioClip eatBaconBit;
	public AudioClip eatBaconPowerUp;
	public AudioClip bumpPlayer;
	public AudioClip pigNoise;
	AudioSource audioSource;

	void Awake () {
		audioSource = GetComponent<AudioSource>();
	}

	// Use this function for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody>();

		forwardMvtInputName = "Vertical" + playerNum;
		sidewaysMvtInputName = "Horizontal" + playerNum;
	}
		
	//Called after Awake() and before any updates happen
	//I honestly don't know if it's necessary or not
	private void OnEnable ()
	{
		verticalInput = 0.0f;
		horizontalInput = 0.0f;
	}
	private void OnDisable ()
	{
		//rigidBody.isKinematic = true;
	}


	// Update is called once per frame
	void Update () {

		verticalInput = Input.GetAxisRaw(forwardMvtInputName);
		horizontalInput = Input.GetAxisRaw (sidewaysMvtInputName); 
	}

	// Called after Update() - applies calculated movement
	private void FixedUpdate()
	{
		//Constrain movement to 1 axis at a time
		if(verticalInput != 0)
		{
			horizontalInput = 0; 
		} 

		movement.Set (horizontalInput, 0.0f, verticalInput);
		if (movement != Vector3.zero) { //without this if stmt it immediately flips back to original rotation 
			transform.rotation = Quaternion.LookRotation (movement);

			movement = transform.forward * moveSpeed * Time.deltaTime;
			rigidBody.MovePosition (rigidBody.position + movement);
		}
	}

	//Called when player object touches a trigger collider
	//Determines what they hit
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag(pickUpTag)){
			audioSource.PlayOneShot(eatBaconBit, 0.4F);
			other.gameObject.SetActive (false);
			score++;
			gotPickup = true;
		}

		else if (other.gameObject.CompareTag (powerUpTag)) {
			audioSource.PlayOneShot(eatBaconPowerUp, 0.2F); 
			other.gameObject.SetActive (false);
			score += 5;
			gotPickup = true;
		}


		else if (other.gameObject.CompareTag (enemyTag)) { 
			print (">PM: OnTriggerEnter(): hit a pig ");
			audioSource.PlayOneShot (pigNoise, 0.3F);

			playerAlive = false; //tells GM to end the level 
			score = 0; //TOO FREAKING BAD
		}

	}

	//Checks for collisions with moving objects (other players or enemies)
	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag (playerTag)) {
			audioSource.PlayOneShot (bumpPlayer, 0.3F);
		}
	}
		
	//so the GameManager can update the player text
	public int GetScore(){
		return score;
	}

	//called by GM at the start of each round
	public void ResetScore() {
		score = 0;
	}

	//so the GameManager can keep track of how many pickups are left
	public bool GotPickup() {
		return gotPickup;
	}

	//called by GM after it calls GotPickup so the gotPickup bool can reset to false for the next frame
	public void ResetGotPickup() {
		gotPickup = false;
	}

	//call this in GM to check if the level should continue
	public bool IsPlayerAlive() {
		return playerAlive;
	}

	//call this in PM.Reset()
	public void ResetPlayerAlive() {
		playerAlive = true;
	}
}

