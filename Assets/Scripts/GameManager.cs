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
    public PlayerScript playerScript;
    public PlayerScript dealerScript;

    // public Text to access and update - hud
    public Text scoreText;
    public Text dealerScoreText;
    public Text potText;
    public Text cashText;
    public Text mainText;
    public Text standBtnText;

    // Card hiding dealer's 2nd card
    public GameObject hideCard;

    private int money = 0;
    int pot = 0;

    private int[] settings;
    private bool countMode;
    private int decks;
    private int players;
    private bool deviations;

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
        hitBtn.gameObject.SetActive(false);
        standBtn.gameObject.SetActive(false);
        doubleBtn.gameObject.SetActive(false);
        splitBtn.gameObject.SetActive(false);
        insuranceBtn.gameObject.SetActive(false);
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
        hitBtn.gameObject.SetActive(true);
        standBtn.gameObject.SetActive(true);
        doubleBtn.gameObject.SetActive(true);
        splitBtn.gameObject.SetActive(true);
        insuranceBtn.gameObject.SetActive(true);
        standBtnText.text = "Stand";

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
                money -= (pot / 2);
                cashText.text = "Money: $" + money.ToString();
                pot *= 2;
                potText.text = "Pot: $" + pot.ToString();
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
        // Check that there is still room on the table
        if (playerScript.cardIndex <= 10)
        {
            playerScript.GetCard();
            scoreText.text = "Hand: " + playerScript.handValue.ToString();
            if (playerScript.handValue > 20) RoundOver();
        }
    }

    private void StandClicked()
    {
        // Next player acts
        HitDealer();
    }

    private void HitDealer()
    {
        while (dealerScript.handValue < 16 && dealerScript.cardIndex < 10)
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
        if (playerBust)
        {
            mainText.text = "Dealer wins!";
        }
        // if dealer busts, player didnt, or player has more points, player wins
        else if (dealerBust || playerScript.handValue > dealerScript.handValue)
        {
            mainText.text = "You win!";
            money += pot;
        }
        //Check for tie, return bets
        else if (playerScript.handValue == dealerScript.handValue)
        {
            mainText.text = "Push: Bets returned";
            money += (pot / 2);
        }
        else
        {
            roundOver = false;
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
            mainText.gameObject.SetActive(true);
            dealerScoreText.gameObject.SetActive(true);
            hideCard.GetComponent<Renderer>().enabled = false;
            cashText.text = "$" + money.ToString();
        }
    }

    // Add money to pot if bet clicked
    void BetClicked()
    {
        Text newBet = betBtn.GetComponentInChildren(typeof(Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString().Remove(0, 1));
        money -= intBet;
        cashText.text = "$" + money.ToString();
        pot += (intBet * 2);
        potText.text = "Pot: $" + pot.ToString();
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
}
