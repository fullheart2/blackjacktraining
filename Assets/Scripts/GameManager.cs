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

    // Access the player and dealer's script
    public PlayerScript playerScript; //OG playerscript, grabbed from game
    public PlayerScript dealerScript; //same as above but for the dealer
	
	//Active player list
	public PlayerHelper playerList;

    // public Text to access and update - hud
    public Text scoreText;
    public Text dealerScoreText;
    public Text cashText;
	public Text betText;
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
        playerList.data.ResetHand();
        dealerScript.ResetHand();
        // Hide deal hand score at start of deal
        dealerScoreText.gameObject.SetActive(false);
        mainText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
        playerList.data.StartHand();
        dealerScript.StartHand();
        // Update the scores displayed
        scoreText.text = "Hand: " + playerList.data.handValue.ToString();
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
		playerList.data.bet = round_base_bet;
		
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
                playerList.data.bet = playerList.data.bet *2;
                HitClicked();
                StandClicked();
                break;
            case 3: // Split
				SplitClicked();
                break;
            case 4: // Insurance

                break;
        }
    } 

    private void HitClicked()
    {
        playerList.data.GetCard();
        scoreText.text = "Hand: " + playerList.data.handValue.ToString();
        if (playerList.data.handValue > 20) StandClicked();
    }

    private void StandClicked()
    {
        // Next player acts
		if(playerList.next == null){
			HitDealer();
		}
		else{
			playerList = playerList.next;
		}
    }
	
	private void SplitClicked()//need to add checking if number of cards in hand is two and if cards are the same
	{
		/*PlayerScript temp = new PlayerScript();
		temp.bet = round_base_bet;
		temp.offset = playerList.data.offset +1;
		playerList.addPlayer(temp);*/
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
		bool roundOver = true;
		// Booleans (true/false) for bust and blackjack/21
		bool dealerBust = dealerScript.handValue > 21;
		bool dealer21 = dealerScript.handValue == 21;
		
		
		while(true){
			// Booleans (true/false) for bust and blackjack/21
			bool playerBust = playerList.data.handValue > 21;
			bool player21 = playerList.data.handValue == 21;
			// if player busts, dealer wins
			if (playerBust || (dealerScript.handValue > playerList.data.handValue && !dealerBust))
			{
				mainText.text = "Dealer wins!";
				bank -= playerList.data.bet;
			}
			// if dealer busts, player didnt, or player has more points, player wins
			else if (dealerBust || playerList.data.handValue > dealerScript.handValue)
			{
				mainText.text = "You win!";
				bank += playerList.data.bet;
			}
			//Check for tie, return bets
			else if (playerList.data.handValue == dealerScript.handValue)
			{
				mainText.text = "Push: Bets returned";
			}
			else
			{
				//error occured
				Debug.Log(dealerBust);
				Debug.Log(playerBust);
				Debug.Log(playerList.data.handValue);
				Debug.Log(dealerScript.handValue);
				System.Environment.Exit(1);
			}
			if(playerList.previous == null){
				break;
			}
			else{
				playerList = playerList.previous;
				playerList.next = null;
			}
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
			betText.text = "Base Bet: $" + round_base_bet.ToString();
            cashText.text = "Bank: $" + bank.ToString();
			
			playerList.data.bet = 0;
        }
    }

    // Add bank to pot if bet clicked
    void BetClicked()
    {
        Text newBet = betBtn.GetComponentInChildren(typeof(Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString().Remove(0, 1));
		round_base_bet += intBet;
		betText.text = "Base Bet: $" + round_base_bet.ToString();
    }

    private int GetCorrectMove() 
    {
        return 0;
    }

    private void HandleMistake(int choice, int correct) 
    {
        /*string[] choices = {"Hit", "Stand", "Double", "Split", "Insurance"};
        if (!deviations)
        {
            Debug.Log("Basic Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // Maybe use on-screen text instead of debug log
        } else {  
            Debug.Log("Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // NEEDS TO CHANGE, OK FOR NOW
        }*/
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
	}
}