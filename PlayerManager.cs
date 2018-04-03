using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerManager {

	public Color playerColor;  //this is just used to set the text color           
	public Transform spawnPoint;  //you set these two in unity, not here in the code

	[HideInInspector] public int playerNumber;             
	[HideInInspector] public string playerText; //colored to match the player
	[HideInInspector] public GameObject playerInstance;          
	[HideInInspector] public int numRoundWins;  

	[HideInInspector] public PlayerController controller;
	public Text scoreText;

	// Use this for initialization
	public void Setup() {
		controller = playerInstance.GetComponent<PlayerController> ();
		controller.playerNum = this.playerNumber;
		playerText = "<color=#" + ColorUtility.ToHtmlStringRGB (playerColor) + ">PLAYER " + playerNumber + "</color>";
	}

	public void DisableControl()
	{
		controller.enabled = false;
	}

	public void EnableControl()
	{
		controller.enabled = true;
	}

	public void Reset()
	{
		playerInstance.transform.position = spawnPoint.position;
		playerInstance.transform.rotation = spawnPoint.rotation;

		playerInstance.SetActive(false);
		playerInstance.SetActive(true);

		controller.ResetPlayerAlive ();
		controller.ResetScore ();
	}

	public void incNumRoundWins(){
		numRoundWins++;
	}

	public int GetRoundWins(){
		return numRoundWins;
	}

}
