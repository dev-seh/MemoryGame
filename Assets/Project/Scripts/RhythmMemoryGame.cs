using System.Collections;
using System.Collections.Generic;
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
    public Sprite bgImage;
    public Sprite greenCardImage;
    public Sprite redCardImage;


    private List<SpriteRenderer> cards;
    private List<int> targetIndices;
    private List<int> playerIndices;
    private int currentIndex;
    private int lives;
    private bool playing;
    private bool revealing;
    private bool preRoll;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        cards = new List<SpriteRenderer>();
        targetIndices = new List<int>();
        playerIndices = new List<int>();
        currentIndex = 0;
        lives = 3;
        playing = false;
        revealing = false;
        preRoll = false;

        for (int i = 0; i < numberOfCards; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            card.GetComponent<Button>().onClick.AddListener(() => OnCardClick(i));
            cards.Add(card.GetComponent<SpriteRenderer>());
            cards[i].sprite = bgImage;
        }

        for (int i = 0; i < cardsToReveal; i++)
        {
            targetIndices.Add(Random.Range(0, numberOfCards));
        }

        foreach (int i in targetIndices)
        {
            cards[i].sprite = greenCardImage;
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
            StartCoroutine(RevealCard(index));
        }
    }

    private IEnumerator RevealCard(int index)
    {
        revealing = true;

        if (targetIndices.Contains(index))
        {
            cards[index].sprite = greenCardImage;
        }
        else
        {
            cards[index].sprite = redCardImage;
        }

        yield return new WaitForSeconds(1f);

        cards[index].sprite = bgImage;
        revealing = false;
    }

   private void CheckMatch()
    {
        if (currentIndex < cardsToReveal)
        {
            if (playerIndices[currentIndex] == targetIndices[currentIndex])
            {
                cards[playerIndices[currentIndex]].sprite = greenCardImage;
            }
            else
            {
                cards[playerIndices[currentIndex]].sprite = redCardImage;
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

        statusText.text = $"Find these tiles: {string.Join(", ", targetIndices)}";
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