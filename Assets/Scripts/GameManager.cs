using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button dealBtn;
    public Button hitBtn;
    public Button standBtn;
    public Button doubleBtn;
    public Button splitBtn;
	public Button insuranceBtn;
    public Button betBtn;

    private int standClicks = 0;

    // Access the player and dealer's script
    public PlayerScript playerScript; //OG playerscript, grabbed from game
    public PlayerScript dealerScript; //same as above but for the dealer
	
	//Active player list
	public PlayerHelper playerList;

    // public Text to access and update - hud
    public Text scoreText;
    public Text dealerScoreText;
    public Text cashText;
    public Text mainText;
    public Text standBtnText;

    // Card hiding dealer's 2nd card
    public GameObject hideCard;

    private int bank = 0;
	int round_base_bet = 0;

    private int[] settings;
    private bool countMode;
    private int decks;
    private int players;
    private bool deviations;
    private bool hit17;

    void Start()
    {
        // Add on click listeners to the buttons
        dealBtn.onClick.AddListener(() => DealClicked());
        hitBtn.onClick.AddListener(() => PlayerAction(0));
        standBtn.onClick.AddListener(() => PlayerAction(1));
        doubleBtn.onClick.AddListener(() => PlayerAction(2));
        splitBtn.onClick.AddListener(() => PlayerAction(3));
        insuranceBtn.onClick.AddListener(() => PlayerAction(4));
        betBtn.onClick.AddListener(() => BetClicked());
        settings = SettingsManager.GetSettings();
        if (settings[0] == 0) countMode = false; else countMode = true;
        decks = settings[1];
        players = settings[2];
        if (settings[3] == 0) deviations = false; else deviations = true;
        if (settings[3] == 0) hit17 = false; else hit17 = true;
        hitBtn.gameObject.SetActive(false);
        standBtn.gameObject.SetActive(false);
        doubleBtn.gameObject.SetActive(false);
        splitBtn.gameObject.SetActive(false);
        insuranceBtn.gameObject.SetActive(false);
        hideCard.GetComponent<Renderer>().sortingOrder = 999;
        playerScript.deckScript.SetDecks(decks);
        playerScript.deckScript.GetCardValues();
		playerList = new PlayerHelper(playerScript);
    }

    private void DealClicked()
    {
        // Reset round, hide text, prep for new hand
        playerScript.ResetHand();
        dealerScript.ResetHand();
        // Hide deal hand score at start of deal
        dealerScoreText.gameObject.SetActive(false);
        mainText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
        playerScript.StartHand();
        dealerScript.StartHand();
        // Update the scores displayed
        scoreText.text = "Hand: " + playerScript.handValue.ToString();
        dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
        // Place card back on dealer card, hide card
        hideCard.GetComponent<Renderer>().enabled = true;
        // Adjust buttons visibility
        dealBtn.gameObject.SetActive(false);
		betBtn.gameObject.SetActive(false);
        hitBtn.gameObject.SetActive(true);
        standBtn.gameObject.SetActive(true);
        doubleBtn.gameObject.SetActive(true);
        splitBtn.gameObject.SetActive(true);
        insuranceBtn.gameObject.SetActive(true);
		
		//sam added this line
		playerScript.bet = round_base_bet;
		
        if (dealerScript.handValue == 21) 
        {
            RoundOver();
        }
    }

    private void PlayerAction(int choice) 
    {
        int correct = GetCorrectMove();
        if (correct != choice) HandleMistake(choice, correct);
        switch(choice) 
        {
            case 0: // Hit
                HitClicked();
                break;
            case 1: // Stand
                StandClicked();
                break;
            case 2: // Double
                bank -= round_base_bet;
                cashText.text = "Bank: $" + bank.ToString(); 
                playerScript.bet = playerScript.bet *2;
                HitClicked();
                StandClicked();
                break;
            case 3: // Split

                break;
            case 4: // Insurance

                break;
        }
    } 

    private void HitClicked()
    {
        playerScript.GetCard();
        scoreText.text = "Hand: " + playerScript.handValue.ToString();
        if (playerScript.handValue > 20) HitDealer();
    }

    private void StandClicked()
    {
        // Next player acts
        HitDealer();
    }
	
	private void SplitClicked()
	{
		
	}

    private void HitDealer()
    {
        while (dealerScript.handValue < 17 || (dealerScript.handValue == 17 && dealerScript.softCount && hit17))
        {
            dealerScript.GetCard();
            dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
        }
        RoundOver();
    }

    // Check for winnner and loser, hand is over
    void RoundOver()
    {
        // Booleans (true/false) for bust and blackjack/21
        bool playerBust = playerScript.handValue > 21;
        bool dealerBust = dealerScript.handValue > 21;
        bool player21 = playerScript.handValue == 21;
        bool dealer21 = dealerScript.handValue == 21;
        bool roundOver = true;
        // if player busts, dealer wins
        if (playerBust || (dealerScript.handValue > playerScript.handValue && !dealerBust))
        {
            mainText.text = "Dealer wins!";
        }
        // if dealer busts, player didnt, or player has more points, player wins
        else if (dealerBust || playerScript.handValue > dealerScript.handValue)
        {
            mainText.text = "You win!";
            bank += playerScript.bet*2;
        }
        //Check for tie, return bets
        else if (playerScript.handValue == dealerScript.handValue)
        {
            mainText.text = "Push: Bets returned";
            bank += playerScript.bet;
        }
        else
        {
			//error occured
			Debug.Log(dealerBust);
			Debug.Log(playerBust);
			Debug.Log(playerScript.handValue);
			Debug.Log(dealerScript.handValue);
			System.Environment.Exit(1);
        }
        // Set ui up for next move / hand / turn
        if (roundOver)
        {
            hitBtn.gameObject.SetActive(false);
            standBtn.gameObject.SetActive(false);
            doubleBtn.gameObject.SetActive(false);
            splitBtn.gameObject.SetActive(false);
            insuranceBtn.gameObject.SetActive(false);
            dealBtn.gameObject.SetActive(true);
			betBtn.gameObject.SetActive(true);
            mainText.gameObject.SetActive(true);
            dealerScoreText.gameObject.SetActive(true);
            hideCard.GetComponent<Renderer>().enabled = false;
			round_base_bet = 0;
			playerScript.bet = 0;
            cashText.text = "Bank: $" + bank.ToString();
        }
    }

    // Add bank to pot if bet clicked
    void BetClicked()
    {
        Text newBet = betBtn.GetComponentInChildren(typeof(Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString().Remove(0, 1));
        bank -= intBet;
		round_base_bet += intBet;
        cashText.text = "Bank: $" + bank.ToString();
    }

    private int GetCorrectMove() 
    {
        return 0;
    }

    private void HandleMistake(int choice, int correct) 
    {
        string[] choices = {"Hit", "Stand", "Double", "Split", "Insurance"};
        if (!deviations)
        {
            Debug.Log("Basic Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // Maybe use on-screen text instead of debug log
        } else {  
            Debug.Log("Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // NEEDS TO CHANGE, OK FOR NOW
        }
    }
	
	public class PlayerHelper
	{
		public PlayerHelper previous;
		public PlayerScript data;
		public PlayerHelper next;
		public PlayerHelper(PlayerScript data){
			this.previous = null;
			this.data = data;
		}
		public PlayerHelper(PlayerScript data, PlayerHelper prev){
			this.previous = prev;
			this.data = data;
		}
		
		public void addPlayer(PlayerScript player){
			this.next = new PlayerHelper(player, this);
		}
		
		public PlayerHelper getNext(){
			return this.next;
		}
		
		public PlayerHelper getPrev(){
			return this.previous;
		}
		
		public PlayerScript getData(){
			return this.data;
		}
	}
}