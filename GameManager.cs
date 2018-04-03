using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * GameManager for BaconMaster
 * Created by Mitch and Sierra
 */
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour {
	
	//Game flow
	private float startDelay = 3f;         
	private float endDelay = 6f;  
	private int countdownSeconds = 3;
	private WaitForSeconds startWait;     
	private WaitForSeconds endWait;   
	private int currRoundNum = 0;    
	private PlayerManager roundWinner = null;
	private PlayerManager gameWinner = null; 

	//Audio
	AudioSource audioSource;
	public AudioClip countdownMid;
	public AudioClip countdownEnd; //play these as the timer is counting down
	public AudioClip levelPlayMusic;
	public AudioClip gameEndMusic;

	//Game text
	public GameObject canvasGameObject; 
	public Text screenMessageText;  
	public Text countdownText;

	//Pickups	
	public GameObject allPickUpsPrefab;
	private GameObject pickupsInstance;
	public Transform pickUpsSpawnPoint; 
	private const int numTotalPickUps = 68; //change this if you add/take away pickup items!! this count includes baconbits and big bacons
	private int pickupsCounter = numTotalPickUps; //this starts at max and decrements every time a player hits a pickup 

	//Players	
	private int p1CurrScore = 0;
	private int p2CurrScore = 1; //use these to check if updating ScoreText is necessary	
	public GameObject player1;   //make one playerPrefab for each player - give each one its own texture  
	public GameObject player2; 
	public PlayerManager playerMgr1;
	public PlayerManager playerMgr2;

	//Pigs	
	public GameObject pigL;
	public GameObject pigR;
	public GameObject pigMidH;
	public GameObject pigMidV;
	public GameObject pigMidBox;
	[HideInInspector] public PigMovement pigLMvt;
	[HideInInspector] public PigMovement pigRMvt;
	[HideInInspector] public PigMovement pigMidVMvt;
	[HideInInspector] public PigMovement pigMidHMvt;
	[HideInInspector] public PigMovement pigMidBoxMvt;

	//Called only once, before roundStarting()
	private void Start()
	{
		startWait = new WaitForSeconds(startDelay);
		endWait = new WaitForSeconds(endDelay);

		audioSource = GetComponent<AudioSource>(); 

		ConnectPigScripts ();
		pickupsInstance = Instantiate (allPickUpsPrefab, pickUpsSpawnPoint.position, pickUpsSpawnPoint.rotation) as GameObject;
		SpawnPlayers();

		StartCoroutine(GameLoop());
	}

	//Connects pigMovement scripts to control their activation states from here in GM
	//If you add another pig you need to add to this function!
	private void ConnectPigScripts() {
		pigLMvt = pigL.GetComponent<PigMovement> ();
		pigRMvt = pigR.GetComponent<PigMovement> ();
		pigMidVMvt = pigMidV.GetComponent<PigMovement> ();
		pigMidHMvt = pigMidH.GetComponent<PigMovement> ();
		pigMidBoxMvt = pigMidBox.GetComponent<PigMovement> ();
	}

	//this should reset the pickups
	private void ResetLevel() {
		print (">>GM: ResetLevel(): starting");

		//spawn pickups
		ActivateAllPickups (pickupsInstance);
		pickupsCounter = numTotalPickUps; //reset counter
	}

	private void SpawnPlayers()
	{
		playerMgr1.playerInstance = Instantiate(player1, playerMgr1.spawnPoint.position, playerMgr1.spawnPoint.rotation) as GameObject;
		playerMgr1.playerNumber = 1;
		playerMgr1.Setup ();

		playerMgr2.playerInstance = Instantiate(player2, playerMgr2.spawnPoint.position, playerMgr2.spawnPoint.rotation) as GameObject;
		playerMgr2.playerNumber = 2;
		playerMgr2.Setup ();
	}


	private IEnumerator GameLoop()
	{
		yield return StartCoroutine(RoundStarting());
		yield return StartCoroutine(RoundPlaying());
		yield return StartCoroutine(RoundEnding());

		if (gameWinner != null) //shouldn't reload level because game is over
		{
			screenMessageText.text = "GAME OVER";
			audioSource.PlayOneShot (gameEndMusic, 0.4F);
			//Game ends here!
		}
		else
		{
			ResetLevel ();
			StartCoroutine(GameLoop()); //restart game loop
		}
	}

	private IEnumerator RoundStarting()
	{
		print (">GM: RoundStarting(): starting");
	
		ResetLevel ();
		ResetPlayers ();
		DisablePlayerControl (); 

		currRoundNum++; 
		canvasGameObject.SetActive (true);
		screenMessageText.text = "ROUND #" + currRoundNum;

		StartCoroutine (countdownToRoundStart());

		yield return startWait;
	}

	private IEnumerator countdownToRoundStart(){

		int time = countdownSeconds;
		while (time > 0) {
			countdownText.text = time.ToString (); 
			time--;

			//Play countdown sound effects
			if (time == 0) {
				//play countdown end 
				audioSource.PlayOneShot(countdownEnd, 0.4F); 
			} else {
				//play countdown mid
				audioSource.PlayOneShot(countdownMid, 0.4F); 
			}

			yield return new WaitForSeconds(1);
		}
	}

	private IEnumerator RoundPlaying()
	{
		print (">GM: RoundPlaying(): starting");

		//start bg music
		audioSource.PlayOneShot (levelPlayMusic, 0.2F);
		//clear text on screen
		screenMessageText.text = string.Empty;
		countdownText.text = string.Empty;

		EnablePigs ();
		EnablePlayerControl ();


		UpdatePlayerScoreText ();

		while (!AllPickupsGone () && BothPlayersAlive ()) {  //keep looping until all pickups are gone OR until a player hits an enemy

			//Check if it's necessary to update either scoreText
			if (playerMgr1.controller.GetScore () != p1CurrScore) {
				p1CurrScore = playerMgr1.controller.GetScore ();
				UpdatePlayerScoreText ();
			}
			if (playerMgr2.controller.GetScore () != p2CurrScore) {
				p2CurrScore = playerMgr2.controller.GetScore ();
				UpdatePlayerScoreText ();
			}
				
			yield return null;
		}

		print("*****************\n >GM: RoundPlaying(): gameloop ended!");
	}


	private IEnumerator RoundEnding()
	{
		print (">GM: RoundEnding(): starting");

		DisablePlayerControl ();
		DisablePigs ();
		audioSource.Stop ();

		roundWinner = GetRoundWinner ();

		if (roundWinner != null) {
			roundWinner.incNumRoundWins ();
		} 
		//if they end up with the same score, neither player gets to count it at as a round win

		//check if they have enough roundWins to win the whole game
		gameWinner = GetGameWinner();

		string message = CreateEndMessage ();
		screenMessageText.text = message;

		yield return endWait;
	}

	//Checks both players' IsPlayerAlive() to see whether anyone has hit a pig 
	private bool BothPlayersAlive() {
		return (playerMgr1.controller.IsPlayerAlive () && playerMgr2.controller.IsPlayerAlive ());
	}

	//Checks whether there are still bacon bits left on the board
	//every time a player gets a pickup, decrement the pickup counter
	//when pickupcounter <= 0, then all pickups are gone
	private bool AllPickupsGone()
	{
		//update number of pickups left by checking if either player just got a pickup in the last frame
		if (playerMgr1.controller.GotPickup ()){
			pickupsCounter--;
			playerMgr1.controller.ResetGotPickup ();
		}
		if (playerMgr2.controller.GotPickup ()){
			pickupsCounter--;
			playerMgr2.controller.ResetGotPickup ();
		}

		if (pickupsCounter <= 0) {
			return true;
		}

		return false; 
	}

	//Checks which who had the most points when all pickups are gone
	//or if one player dies from an enemy the winner is the remaining player
	private PlayerManager GetRoundWinner()
	{
		int p1score = playerMgr1.controller.GetScore ();
		int p2score = playerMgr2.controller.GetScore ();

		//calculate who has highest score
		if (p1score > p2score) {
			//player1 won the round
			return playerMgr1;
		}
		else if (p1score < p2score) {
			//player2 won the round
			return playerMgr2;
		}
		else {
			//both their scores are the same
			print (">GM: GetRoundWinner(): Scores are the same - P1= " + p1score + ", P2= " + p2score);
		}

		return null;
	}

	// the first player to win 3 rounds wins the whole game
	private PlayerManager GetGameWinner()
	{
		int p1wins = playerMgr1.GetRoundWins ();
		int p2wins = playerMgr2.GetRoundWins ();

		//check: is either player at 3+ wins? if so, they win the whole game
		if (p1wins >= 3) {
			//player1 won the whole game
			return playerMgr1;
		}
		else if (p2wins >= 3) {
			//player2 won the whole game	
			return playerMgr2;
		}

		print (">GM: GetGameWinner(): no gamewinner yet - #wins: P1= " + p1wins + ", P2= " + p2wins);
		return null;
	}

	//Prints out the number of wins each player had
	//TODO: maybe take away some of their bacons as a penalty 
	private string CreateEndMessage()
	{
		string message = "";

		if (roundWinner == null) {
			message = "DRAW!\n";  //this will display if no roundwinner was picked
		}
 		else if (roundWinner != null) {
			message = roundWinner.playerText + "  WINS THE ROUND!\n";
		}

		if (!playerMgr1.controller.IsPlayerAlive ()) { 
			//player1 hit the pig
			message += playerMgr1.playerText + " HIT A PIG\n";
		}
		else if (!playerMgr2.controller.IsPlayerAlive ()){
			//player2 hit the pig
			message += playerMgr2.playerText + " HIT A PIG\n";
		}

		message += "\n\n";
		message += playerMgr1.playerText + ": " + playerMgr1.controller.GetScore () + " BACONS, " + playerMgr1.numRoundWins + "/3 WINS\n";
		message += playerMgr2.playerText + ": " + playerMgr2.controller.GetScore () + " BACONS, " + playerMgr2.numRoundWins + "/3 WINS\n";


		if (gameWinner != null) {
			message = gameWinner.playerText + "  WINS THE GAME!";
		}
	
		return message;
	}

	private void UpdatePlayerScoreText() {
		playerMgr1.scoreText.text = "P1 Score: " + playerMgr1.controller.GetScore ();
		playerMgr2.scoreText.text = "P2 Score: " + playerMgr2.controller.GetScore ();
	}

	//Activates all child objects attached to the parent object
	private void ActivateAllPickups(GameObject parent) {

		foreach (Transform child in parent.transform) 
		{
			if (child != null) {

				//The ind baconbits are all child objects of a child object baconBits! We need to do recursively or something ******
				child.gameObject.SetActive (true);
				ActivateAllPickups (child.gameObject); //Recurse in case this object also has child objects
			}
		}
	}

	//Deactivates all child objects attached to the parent object
	private void DeactivateAllPickups(GameObject parent){

		foreach (Transform child in parent.transform)
		{
			if (child != null) {

				child.gameObject.SetActive (false); 
				DeactivateAllPickups (child.gameObject); //Recurse in case this object also has child objects
			}
		}
	}
		
	private void ResetPlayers()
	{
		playerMgr1.Reset ();
		playerMgr2.Reset ();
	}

	private void EnablePigs() {
		pigLMvt.SetPigActive ();
		pigRMvt.SetPigActive ();
		pigMidHMvt.SetPigActive ();
		pigMidVMvt.SetPigActive ();
		pigMidBoxMvt.SetPigActive ();
	}

	private void DisablePigs() {
		pigLMvt.SetPigInactive ();
		pigRMvt.SetPigInactive ();
		pigMidVMvt.SetPigInactive ();
		pigMidHMvt.SetPigInactive ();
		pigMidBoxMvt.SetPigInactive ();
	}

	private void EnablePlayerControl()
	{
		playerMgr1.EnableControl ();
		playerMgr2.EnableControl ();
	}

	private void DisablePlayerControl()
	{
		playerMgr1.DisableControl ();
		playerMgr2.DisableControl ();
	}
}
