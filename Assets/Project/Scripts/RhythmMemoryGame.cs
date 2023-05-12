using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class RhythmMemoryGame : MonoBehaviour
{
    public int numberOfCards = 20;
    public int cardsToReveal = 4;
    public float beatInterval = 1f;
    public GameObject cardPrefab;
    public Transform cardContainer;
    public Text statusText;
    public Button playButton;

    private List<Button> cards;
    private List<int> targetIndices;
    private List<int> playerIndices;
    private int currentIndex;
    private int lives;
    private bool playing;
    private bool revealing;
    private bool preRoll;
    public Sprite bgImage; // Define this in your script to assign default image
    private List<Text> cardNumbers;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        cards = new List<Button>();
        targetIndices = new List<int>();
        playerIndices = new List<int>();
        currentIndex = 0;
        lives = 3;
        playing = false;
        revealing = false;
        preRoll = false;
        cardNumbers = new List<Text>();
    for (int i = 0; i < numberOfCards; i++)
    {
        GameObject card = Instantiate(cardPrefab, cardContainer);
        Button cardButton = card.GetComponent<Button>();
        int index = i;
        cardButton.onClick.AddListener(() => OnCardClick(index));
        cards.Add(cardButton);
        cards[i].image.sprite = bgImage;

        Text cardNumber = card.GetComponentInChildren<Text>(true); // Get the Text component
        cardNumber.gameObject.SetActive(false); // Hide the number initially
        cardNumbers.Add(cardNumber); // Store the Text component
    }

        for (int i = 0; i < cardsToReveal; i++)
        {
            targetIndices.Add(Random.Range(0, numberOfCards));
        }

        statusText.text = $"Find these tiles: {string.Join(", ", targetIndices)}";
        playButton.onClick.AddListener(Play);
    }

    private void Play()
    {
        if (!playing)
        {
            playing = true;
            StartCoroutine(PreRoll());
        }
    }

  public void OnCardClick(int index)
    {
        Debug.Log($"Card clicked: {index}");
        if (!playing && !revealing && index < cards.Count)
        {
            playerIndices.Add(index);
            if (targetIndices.Contains(index))
            {
                cards[index].image.color = Color.green;
                int targetIndex = targetIndices.IndexOf(index);
                cardNumbers[index].text = (targetIndex + 1).ToString(); // Set the order
                cardNumbers[index].gameObject.SetActive(true); // Show the number
            }
            else
            {
                cards[index].image.color = Color.red;
            }
            StartCoroutine(RevealCard(index));
        }
    }



    private IEnumerator RevealCard(int index)
    {
        revealing = true;

        yield return new WaitForSeconds(0.5f);

        cards[index].image.color = Color.white;
        cardNumbers[index].gameObject.SetActive(false); // Hide the number

        revealing = false;
    }


    private void CheckMatch()
    {
        if (currentIndex < cardsToReveal)
        {
            if (playerIndices[currentIndex] == targetIndices[currentIndex])
            {
                cards[playerIndices[currentIndex]].image.color = Color.green;
            }
            else
            {
                cards[playerIndices[currentIndex]].image.color = Color.red;
                LoseLife();
            }

            currentIndex++;
        }

        if (currentIndex == cardsToReveal)
        {
            if (lives > 0)
            {
                NextLevel();
            }
            else
            {
                // You lose the game
                statusText.text = "Game Over";
            }
        }
    }


   private IEnumerator PreRoll()
    {
        preRoll = true;
        statusText.text = "3";

        yield return new WaitForSeconds(1f);

        statusText.text = "2";

        yield return new WaitForSeconds(1f);

        statusText.text = "1";

        yield return new WaitForSeconds(1f);

        statusText.text = "Go!";
        preRoll = false;

        while (currentIndex < cardsToReveal)
        {
            yield return new WaitForSeconds(beatInterval);
            CheckMatch();
        }
    }


    private void NextLevel()
    {
        currentIndex = 0;
        targetIndices.Clear();
        playerIndices.Clear();
        playing = false;
        for (int i = 0; i < cardsToReveal; i++)
        {
            targetIndices.Add(Random.Range(0, numberOfCards));
        }

        // Reset card numbers and their visibility
        for (int i = 0; i < numberOfCards; i++)
        {
            cardNumbers[i].text = "";
            cardNumbers[i].gameObject.SetActive(false);
        }

        statusText.text = $"Find these tiles: {string.Join(", ", targetIndices.Select(x => x + 1))}";
    }


    private void LoseLife()
    {
        lives--;
        statusText.text = $"Lives left: {lives}";
    }

    private void Shuffle(Sprite[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Sprite temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

}