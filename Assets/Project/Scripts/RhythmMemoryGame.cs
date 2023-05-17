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
    public AudioClip[] tileSounds;

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
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InitializeGame();
    }

    private void InitializeGame()
    {

    // Destroy old cards
    if (cards != null)
    {
        foreach (var card in cards)
        {
            Destroy(card.gameObject);
        }
    }

    cards = new List<Button>();
    HashSet<int> uniqueIndices = new HashSet<int>();
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

    while (uniqueIndices.Count < cardsToReveal)
    {
        uniqueIndices.Add(Random.Range(0, numberOfCards));
    }

    targetIndices = uniqueIndices.ToList();

    statusText.text = $"Find these tiles: {string.Join(", ", targetIndices)}";
    playButton.onClick.AddListener(Play);
    }

    public void Play()
    {
        if (!playing)
        {
            playing = true;
            currentIndex = 0; // Reset currentIndex to 0 when starting the game
            statusText.text = "Now, repeat the sequence!";
        }
    }


    public void OnCardClick(int index)
    {
        Debug.Log($"Card clicked: {index}");
        if (!revealing && index < cards.Count)
        {
            playerIndices.Add(index);
            if (targetIndices.Contains(index))
            {
                int targetIndex = targetIndices.IndexOf(index);

                // Play the corresponding sound
                if(targetIndex < tileSounds.Length)
                {
                    audioSource.PlayOneShot(tileSounds[targetIndex]);
                }

                if (!playing) // Preroll phase
                {
                    cards[index].image.color = Color.green;
                    cardNumbers[index].text = (targetIndex + 1).ToString(); // Set the order
                    cardNumbers[index].gameObject.SetActive(true); // Show the number
                    StartCoroutine(RevealCard(index));
                }
                else if (playing && targetIndex == currentIndex) // Play phase and correct sequence
                {
                    cards[index].image.color = Color.green;
                    cardNumbers[index].text = (targetIndex + 1).ToString(); // Set the order
                    cardNumbers[index].gameObject.SetActive(true); // Show the number
                    StartCoroutine(DelayedRevealCard(index));
                    CheckMatch();
                }
                else if (playing) // Play phase and wrong order
                {
                    cards[index].image.color = Color.red;
                    StartCoroutine(DelayedRevealCard(index));
                    LoseLife();
                    ResetPlayPhase();
                }
            }
            else
            {
                // An incorrect tile has been clicked during the play phase, handle it here
                cards[index].image.color = Color.red;
                StartCoroutine(DelayedRevealCard(index)); // Also reveal card when clicked on a wrong tile in play phase
                if (playing) // Play phase and wrong tile
                {
                    LoseLife();
                    StartCoroutine(ResetPlayPhaseWithDelay()); // Added a delayed reset
                }
            }
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
        int playerIndex = playerIndices.Last(); // Get the last clicked card index

        if (playerIndex != targetIndices[currentIndex])
        {
            cards[playerIndex].image.color = Color.red;
            LoseLife();
            ResetPlayPhase();
            return;
        }

        currentIndex++;
        if (currentIndex == cardsToReveal)
        {
            // You win the game
            statusText.text = "You Win!";
            // To proceed to the next level uncomment the line below
            // NextLevel();
        }
    }

    private void ResetPlayPhase()
    {
        playerIndices.Clear();
        currentIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].image.color = Color.white;
            cardNumbers[i].gameObject.SetActive(false); // Hide the number
        }
        statusText.text = "Try again!";
    }

    private IEnumerator DelayedRevealCard(int index)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(RevealCard(index));
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
            int targetIndex = targetIndices[currentIndex];
            cards[targetIndex].image.color = Color.green;
            cardNumbers[targetIndex].text = (currentIndex + 1).ToString(); // Set the order
            cardNumbers[targetIndex].gameObject.SetActive(true); // Show the number
            StartCoroutine(RevealCard(targetIndex));
            currentIndex++;
        }

        currentIndex = 0;
        statusText.text = "Now, repeat the sequence!";

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
        if (lives <= 0)
        {
            // Game Over - Restart game or load game over scene
            Debug.Log("Game Over");
            InitializeGame(); // Resets the game
            StartCoroutine(PreRoll()); // Initiate preroll phase after Game Over
        }
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
        
    private IEnumerator ResetPlayPhaseWithDelay()
    {
        yield return new WaitForSeconds(1f);
        ResetPlayPhase();
    }
}