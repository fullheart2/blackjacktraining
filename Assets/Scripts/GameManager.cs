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
	public Button surrenderBtn;
	public Button betBtn;


	//Game Objects
	public DeckScript deckScript;

	//Dealer script
	public PlayerScript dealerScript;

	//Active player list
	public PlayerHelper playerList;

	// public Text to access and update - hud
	public Text scoreText;
	public Text dealerScoreText;
	public Text cashText;
	public Text betText;
	public Text standBtnText;
	public Text arrow;
	public Text alertText;

	float duration = 3f;
	float endTime;


	// Card hiding dealer's 2nd card
	public GameObject hideCard;
	public GameObject playerStartCard;
	public GameObject dealerStartCard;
	private int bank = 0;
	int round_base_bet = 20;
	private int offset = 0;
	private bool surr = false;

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
		surrenderBtn.onClick.AddListener(() => PlayerAction(4));
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
		surrenderBtn.gameObject.SetActive(false);
		arrow.gameObject.SetActive(false);
		hideCard.GetComponent<Renderer>().sortingOrder = 999;
		deckScript.SetDecks(decks);
		deckScript.GetCardValues();
		playerList = new PlayerHelper(new PlayerScript(deckScript, playerStartCard));
		dealerScript = new PlayerScript(deckScript, dealerStartCard, 0);
	}

	void Update()
	{
		if (alertText.gameObject.activeSelf && (Time.time >= endTime))
		{
			alertText.gameObject.SetActive(false);
		}
	}

	private void DealClicked()
	{
		// Reset round, hide text, prep for new hand
		while (true) {
			if (playerList.next == null)
			{
				break;
			} playerList = playerList.next;
		}
		while (true) {
			playerList.next = null;

			if (playerList.previous == null) {
				break;
			}
			else
			{
				playerList.data.ResetHand();
				offset--;
				playerList = playerList.previous;
			}
		}
		playerList.data.ResetHand();
		dealerScript.ResetHand();
		// Hide deal hand score at start of deal
		// dealerScoreText.gameObject.SetActive(false);
		GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
		dealerScript.StartHand();
		playerList.data.StartHand();
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
		if (SplitCheck(playerList.data)) splitBtn.gameObject.SetActive(true);
		surrenderBtn.gameObject.SetActive(true);
		arrow.gameObject.SetActive(true);
		surr = false;

		//sam added this line
		playerList.data.bet = round_base_bet;

		if (dealerScript.handValue == 21)
		{
			RoundOver();
		}
	}

	private void PlayerAction(int choice)
	{
		int correct = GetCorrectMove(playerList.data, dealerScript.hand[1].GetComponent<CardScript>().GetValueOfCard());
		if (correct != choice) HandleMistake(choice, correct);
		switch (choice)
		{
			case 0: // Hit
				HitClicked(false);
				break;
			case 1: // Stand
				StandClicked();
				break;
			case 2: // Double
				playerList.data.bet = playerList.data.bet * 2;
				HitClicked(true);
				StandClicked();
				break;
			case 3: // Split
				SplitClicked();
				break;
			case 4: // Surrender
				surr = true;
				StandClicked();
				break;
		}
	}

	private void HitClicked(bool doub)
	{
		doubleBtn.gameObject.SetActive(false);
		splitBtn.gameObject.SetActive(false);
		surrenderBtn.gameObject.SetActive(false);
		playerList.data.GetCard();
		scoreText.text = "Hand: " + playerList.data.handValue.ToString();
		if (playerList.data.handValue > 21 && !doub) StandClicked();
	}

	private void StandClicked()
	{
		// Next player acts
		if (playerList.next == null) {
			HitDealer();
		}
		else {
			playerList = playerList.next;
			doubleBtn.gameObject.SetActive(true);
			if (SplitCheck(playerList.data)) splitBtn.gameObject.SetActive(true);
			arrow.transform.Translate(2, 0, 0);
		}
	}

	private void SplitClicked()//need to add checking if number of cards in hand is two and if cards are the same
	{
		Debug.Log("Split clicked");

		PlayerScript temp = new PlayerScript(deckScript, playerStartCard);
		temp.bet = round_base_bet;
		temp.offset = ++offset;
		temp.GetLastSplitCard(playerList.data, offset - playerList.data.offset);
		playerList.data.ResetCard();
		temp.GetCard();
		playerList.addPlayer(temp);
		playerList.data.GetCard();
		scoreText.text = "Hand: " + playerList.data.handValue.ToString();
		if (!SplitCheck(playerList.data)) splitBtn.gameObject.SetActive(false);
		surrenderBtn.gameObject.SetActive(false);
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


		if (surr)
		{
			bank -= playerList.data.bet / 2;
		}
		else {
			while (true) {
				// Booleans (true/false) for bust and blackjack/21
				bool playerBust = playerList.data.handValue > 21;
				bool player21 = playerList.data.handValue == 21;
				// if player busts, dealer wins
				if (playerBust || (dealerScript.handValue > playerList.data.handValue && !dealerBust))
				{
					bank -= playerList.data.bet;
				}
				// if dealer busts, player didnt, or player has more points, player wins
				else if (dealerBust || playerList.data.handValue > dealerScript.handValue)
				{
					if (player21 && playerList.data.hand[2] == null) bank += playerList.data.bet / 2;
					bank += playerList.data.bet;
				}
				//Check for tie, return bets
				else if (playerList.data.handValue == dealerScript.handValue)
				{
					if (player21 && playerList.data.hand[2] == null)
					{
						bank += playerList.data.bet / 2;
					}
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
				if (playerList.previous == null) {
					break;
				}
				else {
					playerList = playerList.previous;
				}
			}
		}
		// Set ui up for next move / hand / turn
		if (roundOver)
		{
			hitBtn.gameObject.SetActive(false);
			standBtn.gameObject.SetActive(false);
			doubleBtn.gameObject.SetActive(false);
			splitBtn.gameObject.SetActive(false);
			surrenderBtn.gameObject.SetActive(false);
			dealBtn.gameObject.SetActive(true);
			betBtn.gameObject.SetActive(true);
			// dealerScoreText.gameObject.SetActive(true);
			hideCard.GetComponent<Renderer>().enabled = false;
			arrow.transform.Translate(-2 * offset, 0, 0);
			arrow.gameObject.SetActive(false);

			round_base_bet = 20;
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

	private bool SplitCheck(PlayerScript player)
	{
		if (player.hand[0].GetComponent<CardScript>().GetValueOfCard() == player.hand[1].GetComponent<CardScript>().GetValueOfCard())
			return true;
		if (player.hand[0].GetComponent<CardScript>().GetValueOfCard() == 1 && player.hand[1].GetComponent<CardScript>().GetValueOfCard() == 11)
			return true;
		if (player.hand[1].GetComponent<CardScript>().GetValueOfCard() == 1 && player.hand[0].GetComponent<CardScript>().GetValueOfCard() == 11)
			return true;
		return false;
	}

	private void ShowText(Text text) 
	{
		text.gameObject.SetActive(true);
		endTime = Time.time + duration;
	}

    private int GetCorrectMove(PlayerScript player, int dealer) 
    {
		if (dealer == 1) dealer = 11;
		bool legalsur = surrenderBtn.gameObject.activeSelf;
		bool legaldouble = doubleBtn.gameObject.activeSelf;
		if (splitBtn.gameObject.activeSelf)
		{
			if (player.handValue == 16 && dealer == 11 && hit17 && legalsur) return 4;
			if (player.handValue == 12 || player.handValue == 16) return 3;
			if (player.handValue == 18) { if (dealer != 7 || dealer > 9) return 3; else return 1; }
			if (player.handValue == 14) { if (dealer > 7) return 0; else return 3; }
			if (player.handValue == 12) { if (dealer > 6) return 0; else return 3; }
			if (player.handValue == 4) { if (dealer > 7) return 0; else return 3; }
			if (player.handValue == 6) { if (dealer > 7) return 0; else return 3; }
			if (player.handValue == 8) { if (dealer > 6 || dealer < 5) return 0; else return 3; }
		}
		if (!player.softCount) 
		{
			if (player.handValue > 17) return 1;
			if (player.handValue == 17) if (dealer == 11 && hit17 && legalsur) return 4; else return 1;
			if (player.handValue == 16) { if (dealer > 8 && legalsur) return 4; if (dealer > 6) return 0; else return 1; }
			if (player.handValue == 15) { if (dealer == 10 && legalsur) return 4; if (dealer == 11 && hit17 && legalsur) return 4; if (dealer > 6) return 0; else return 1; }
			if (player.handValue == 14 || player.handValue == 13) { if (dealer > 6) return 0; else return 1; }
			if (player.handValue == 12) { if (dealer > 6 || dealer < 4) return 0; else return 1; }
			if (player.handValue == 11) { if (dealer > 10 && !hit17 || !legaldouble) return 0; else return 2; }
			if (player.handValue == 10) { if (dealer > 9 || !legaldouble) return 0; else return 2; }
			if (player.handValue == 9) { if (dealer > 6 || dealer < 3 || !legaldouble) return 0; else return 2; }
			if (player.handValue < 9) return 0;
		}
		if (player.softCount)
		{
			if (player.handValue > 19) return 1;
			if (player.handValue == 19) { if (dealer == 6 && hit17 && legaldouble) return 2; else return 1; }
				if (player.handValue == 18) { if (dealer > 8) return 0; if (dealer > 6 || (dealer == 2 && !hit17) || !legaldouble) return 1; else return 2; }
				if (player.handValue == 17) { if (dealer > 6 || dealer < 3 || !legaldouble) return 0; else return 2; }
				if (player.handValue == 16) { if (dealer > 6 || dealer < 4 || !legaldouble) return 0; else return 2; }
				if (player.handValue == 15) { if (dealer > 6 || dealer < 4 || !legaldouble) return 0; else return 2; }
				if (player.handValue == 14) { if (dealer > 6 || dealer < 5 || !legaldouble) return 0; else return 2; }
				if (player.handValue == 13) { if (dealer > 6 || dealer < 5 || !legaldouble) return 0; else return 2; }
		}
		return -1;
    }

    private void HandleMistake(int choice, int correct) 
    {
        string[] choices = {"Hit", "Stand", "Double", "Split", "Surrender"};
        //if (!deviations)
        //{
            Debug.Log("Basic Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // Maybe use on-screen text instead of debug log
			alertText.text = "Basic Strategy Mistake: \nYou chose: " + choices[choice] + ", \nbut the correct move was: " + choices[correct];
			ShowText(alertText);
			Debug.Log("Player had: " + playerList.data.handValue + " Dealer had: " + dealerScript.hand[1].GetComponent<CardScript>().GetValueOfCard() + " Soft Count: " + playerList.data.softCount) ;
		//} else {  
            //Debug.Log("Strategy Mistake: You chose: " + choices[choice] + ", but the correct move was: " + choices[correct]); // NEEDS TO CHANGE, OK FOR NOW
        //}
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

		public void addPlayer(PlayerScript player) {
			PlayerHelper curr = this;
			while (curr.next != null) {
				curr = curr.next;
			}
			curr.next = new PlayerHelper(player, curr);
		}
	}

	
	
	
	public class PlayerScript
	{
		// --- This script is for BOTH player and dealer

		// Get other scripts
		public DeckScript deckScript;

		// Total value of player/dealer's hand
		public int handValue = 0;
		public bool softCount = false;
		
		
		public int bet = 0;
		public int offset = 0;
		
		//is player a split instance
		public bool split = false;
		
		// Array of card objects on table
		public GameObject[] hand = new GameObject[10];
		public GameObject startCard;
		public int dir;

		// Index of next card to be turned over
		public int cardIndex = 0;
		// Tracking aces for 1 to 11 conversions
		List<CardScript> aceList = new List<CardScript>();
		
		public PlayerScript(DeckScript deck, GameObject c, int d = 1){
			this.deckScript = deck;
			this.startCard = c;
			this.dir = d;
		}
		
		
		public void StartHand()
		{
			GetCard();
			GetCard();
		}

		// Add a hand to the player/dealer's hand
		public int GetCard()
		{
			// Get a card, use deal card to assign sprite and value to card on table
			// GameObject temp = Instantiate(startCard, new Vector3(cardIndex * 20f, cardIndex * 20f, 0), Quaternion.identity);
			int cardValue;
			GameObject temp = Instantiate(startCard);
			temp.transform.Translate(new Vector3(offset*2f + cardIndex * 0.5f, cardIndex * dir * 0.5f, 0));
			hand[cardIndex] = temp;
			cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());
			
			// Show card on game screen
			hand[cardIndex].GetComponent<Renderer>().enabled = true;
			// Add card value to running total of the hand
			handValue += cardValue;
			// If value is 1, it is an ace
			if(cardValue == 1)
			{
				aceList.Add(hand[cardIndex].GetComponent<CardScript>());
			}
			// Check if we should use an 11 instead of a 1
			AceCheck();
			cardIndex++;
			return handValue;
		}

		public int GetLastSplitCard(PlayerScript splitter, int localoffset)
		{
			// Get a card, use deal card to assign sprite and value to card on table
			// GameObject temp = Instantiate(startCard, new Vector3(cardIndex * 20f, cardIndex * 20f, 0), Quaternion.identity);
			int cardValue;
			GameObject temp = splitter.hand[1];
			temp.transform.Translate(new Vector3(localoffset * 2f - 0.5f, -1 * dir * 0.5f, 0));
			hand[cardIndex] = temp;
			cardValue = temp.GetComponent<CardScript>().GetValueOfCard();

			// Show card on game screen
			hand[cardIndex].GetComponent<Renderer>().enabled = true;

			// Add card value to running total of the hand
			handValue += cardValue;
			// If value is 1, it is an ace
			if (cardValue == 1)
			{
				aceList.Add(hand[cardIndex].GetComponent<CardScript>());
			}
			// Check if we should use an 11 instead of a 1
			AceCheck();
			cardIndex++;
			return handValue;
		}

		// Search for needed ace conversions, 1 to 11 or vice versa
		public void AceCheck()
		{
			// for each ace in the list check
			foreach (CardScript ace in aceList)
			{
				if(handValue + 10 < 22 && ace.GetValueOfCard() == 1)
				{
					// if converting, adjust card object value and hand
					ace.SetValue(11);
					handValue += 10;
					softCount = true;
				} else if (handValue > 21 && ace.GetValueOfCard() == 11)
				{
					// if converting, adjust gameobject value and hand value
					ace.SetValue(1);
					handValue -= 10;
					softCount = false;
				}
			}
		}

		// Remove 2nd card for a split
		public void ResetCard() 
		{
			handValue -= (hand[1].GetComponent<CardScript>()).GetValueOfCard();
			if (hand[1] != null)
			{
				hand[1] = null;
			}
			cardIndex = 1;
		}

		// Hides all cards, resets the needed variables
		public void ResetHand()
		{
			for(int i = 0; i < hand.Length; i++)
			{
				if (hand[i] != null) 
				{
					hand[i].GetComponent<CardScript>().ResetCard();
					hand[i].GetComponent<Renderer>().enabled = false;
					hand[i] = null;
				}
			}
			cardIndex = 0;
			handValue = 0;
			aceList = new List<CardScript>();
			softCount = false;
		}
	}
}