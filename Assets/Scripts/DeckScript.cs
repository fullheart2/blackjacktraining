using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;
    int[] cardMapping;
    int[] cardValues;
    int currentIndex = 0;

    void Start()
    {
        // GetCardValues();
    }

    public void GetCardValues()
    {
        int num = 0;
        // Loop to assign values to the cards
        for (int i = 0; i < cardValues.Length; i++)
        {
            num = i;
            // Count up to the amout of cards, 52
            num %= 13;
            // if there is a remainder after x/13, then remainder
            // is used as the value, unless over 10, the use 10
            if(num > 10 || num == 0)
            {
                num = 10;
            }
            cardValues[i] = num++;
            cardMapping[i] = i;
        }
    }

    public void Shuffle()
    {
        // Standard array data swapping technique
        for(int i = cardMapping.Length -1; i > 0; i--)
        {
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * (cardMapping.Length - 1)) + 1;
            int value = cardMapping[i];
            cardMapping[i] = cardMapping[j];
            cardMapping[j] = value;

            value = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = value;
        }
        currentIndex = 1;
    }

    public int DealCard(CardScript cardScript)
    {
        cardScript.SetSprite(cardSprites[(cardMapping[currentIndex]-1)%52+1], currentIndex);
        cardScript.SetValue(cardValues[currentIndex]);
        currentIndex++;
        return cardScript.GetValueOfCard();
    }

    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }
    public void SetDecks(int decks) 
    {
        cardValues = new int[1 + 52 * decks];
        cardMapping = new int[1 + 52 * decks];
    }
}
