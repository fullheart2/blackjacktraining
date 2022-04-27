using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // --- This script is for BOTH player and dealer

    // Get other scripts
    public CardScript cardScript;   
    public DeckScript deckScript;

    // Total value of player/dealer's hand
    public int handValue = 0;
    public bool softCount = false;
	public int bet = 0;
	
    // Array of card objects on table
    private GameObject[] hand = new GameObject[10];
    public GameObject startCard;
    public int dir;

    // Index of next card to be turned over
    public int cardIndex = 0;
    // Tracking aces for 1 to 11 conversions
    List<CardScript> aceList = new List<CardScript>();

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
        if (cardIndex == 0)
        {
            cardValue = deckScript.DealCard(startCard.GetComponent<CardScript>());
            hand[0] = startCard;
        }
        else
        {
            GameObject temp = Instantiate(startCard);
            temp.transform.Translate(new Vector3(cardIndex * 0.5f, cardIndex * dir * 0.5f-1.05f, 0));
            hand[cardIndex] = temp;
            cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());
        }
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

    // Hides all cards, resets the needed variables
    public void ResetHand()
    {
        for(int i = 0; i < hand.Length; i++)
        {
            if (hand[i] != null) 
            {
                hand[i].GetComponent<CardScript>().ResetCard();
                hand[i].GetComponent<Renderer>().enabled = false;
            }
        }
        cardIndex = 0;
        handValue = 0;
        aceList = new List<CardScript>();
    }
}
