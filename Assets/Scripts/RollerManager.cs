using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollerManager : MonoBehaviour
{
    static public RollerManager instance;

    [Header("Modifiable Variables")]
    // Time each roller takes to start since the previous one starts
    [SerializeField] private float intervalTimeBetweenRollers = 0.2f;

    // Fruit sprites assigned in the inspector
    public Sprite[] fruits;

    [Header("External Components")]
    [SerializeField] private Button spinButton;
    [SerializeField] private CanvasGroup winningPanel;
    [SerializeField] private RectTransform winningPanelRT;
    [SerializeField] private Text pointsText;
    [SerializeField] private Text totalPointText;
    [SerializeField] private Image[] winningPanelImages;

    // The array of rollers we have in the Slot Machine
    private Roller[] rollers;

    // Dictionary storing all the winning combinations and the value prize of each one
    private Dictionary<int[], int> winCombinations = new Dictionary<int[], int>();

    // List storing the non-linear winning patterns
    private List<int[]> winningPatterns = new List<int[]>();

    // The summ of all the points earned between all rolls
    private int totalPoints = 0;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Getting the array of rollers in order in the scene from left to right
        rollers = transform.GetComponentsInChildren<Roller>();

        // Assigning each roller the sequence of fruit they have to follow. The Numbers ID go according to the sprites naming given by the test, minus 1, as following:
        // 0 -> Bell
        // 1 -> Watermelon
        // 2 -> Grapes
        // 3 -> Blueberries
        // 4 -> Orange
        // 5 -> Lemon
        // 6 -> Cherries
        rollers[0].AssignFruitSequence(new int[] {4, 0, 1, 6, 3, 5, 2, 3, 0, 0, 4, 2});
        rollers[1].AssignFruitSequence(new int[] {1, 6, 0, 3, 6, 2, 4, 5, 5, 5, 6, 5, 3, 5, 6});
        rollers[2].AssignFruitSequence(new int[] {2, 1, 3, 2, 0, 5, 6, 0, 0, 0, 4, 4, 2});
        rollers[3].AssignFruitSequence(new int[] {5, 3, 3, 5, 2, 4, 1, 1, 0, 6, 6, 5, 4, 3, 5});
        rollers[4].AssignFruitSequence(new int[] {2, 6, 0, 1, 4, 4, 3, 3, 4, 4, 2, 0, 1, 6});

        // Saving each winning combination using fruit IDs and the score they should give
        winCombinations.Add(new int[] { 0, 0, 0, 0 }, 100);
        winCombinations.Add(new int[] { 0, 0, 0 }, 75);
        winCombinations.Add(new int[] { 0, 0 }, 50);
        winCombinations.Add(new int[] { 1, 1, 1, 1 }, 60);
        winCombinations.Add(new int[] { 1, 1, 1 }, 30);
        winCombinations.Add(new int[] { 1, 1 }, 20);
        winCombinations.Add(new int[] { 2, 2, 2, 2 }, 50);
        winCombinations.Add(new int[] { 2, 2, 2 }, 20);
        winCombinations.Add(new int[] { 2, 2 }, 10);
        winCombinations.Add(new int[] { 3, 3, 3, 3 }, 40);
        winCombinations.Add(new int[] { 3, 3, 3 }, 20);
        winCombinations.Add(new int[] { 3, 3 }, 10);
        winCombinations.Add(new int[] { 4, 4, 4, 4 }, 30);
        winCombinations.Add(new int[] { 4, 4, 4 }, 15);
        winCombinations.Add(new int[] { 4, 4 }, 10);
        winCombinations.Add(new int[] { 5, 5, 5, 5 }, 20);
        winCombinations.Add(new int[] { 5, 5, 5 }, 10);
        winCombinations.Add(new int[] { 5, 5 }, 5);
        winCombinations.Add(new int[] { 6, 6, 6, 6 }, 10);
        winCombinations.Add(new int[] { 6, 6, 6 }, 5);
        winCombinations.Add(new int[] { 6, 6 }, 2);

        // Adding the linear winning patterns
        winningPatterns.Add(new int[] { 0, 0, 0, 0, 0 });
        winningPatterns.Add(new int[] { 1, 1, 1, 1, 1 });
        winningPatterns.Add(new int[] { 2, 2, 2, 2, 2 });

        // Adding the nonLinear winning patterns
        winningPatterns.Add(new int[] { 0, 2, 0, 2, 0});
        winningPatterns.Add(new int[] { 0, 1, 2, 1, 0});
    }

    public void SpinRollers()
    {
        // Making Spin button not interactable until the roll is over
        spinButton.interactable = false;

        StartCoroutine(StartSpin());
    }

    IEnumerator StartSpin()
    {
        // Getting the random time during which the rollers will be spinning
        float spinningTime = Random.Range(2f, 4f);

        // Iterating between the rollers and starting the roll of each one separated by a small space of time
        for (int i = 0; i < rollers.Length; ++i)
        {
            rollers[i].BeginRoll(spinningTime);
            yield return new WaitForSeconds(intervalTimeBetweenRollers);
        }

        // Waiting exactly the time that the rollers will roll to know the rolls have ended
        yield return new WaitForSeconds(spinningTime);

        CheckForWinningCombinations();
    }

    private void CheckForWinningCombinations()
    {
        // We will save the higher score combination in case more than one combination is achieved, it rewards the higher one
        KeyValuePair<int[], int> higherScoreCombination = new KeyValuePair<int[], int>(null, -1);
        int[] winningPattern = null;

        // Checking winning patterns
        for (int i = 0; i < winningPatterns.Count; ++i)
        {
            CheckWinningPatterns(ref higherScoreCombination, winningPatterns[i], ref winningPattern);
        }

        // If the combination is not null, it indicates that there is a winning combination
        if (higherScoreCombination.Key != null)
        {            
            StartCoroutine(WinningPanelAnimation(higherScoreCombination.Key[0], higherScoreCombination.Key.Length, higherScoreCombination.Value, winningPattern));
        }
        else
        {
            // Allowing the user to roll again
            spinButton.interactable = true;
        }
    }

    // Function used to check if there is a winning combination inside the passed pattern
    // In the case there is, the pattern passed is saved through a reference argument
    private void CheckWinningPatterns(ref KeyValuePair<int[], int> winningCombination, int[] pattern, ref int[] tryingPattern)
    {
        /// For the porpouse of the test, as indicated by instructions given through email,
        /// winning conditions only will be tested from the first roller to the right
        /// For Example: Lemon - Bell - Bell - Bell - Lemon; will NOT be a winning combination,
        /// because even if there are 3 consecutive fruits in line, the left-most one is not part of it

        // Number used to know the next roller we have to check
        int rollerToCheck = 1;

        // Recursively checking if the fruit of the first roller is the same as the next roller we have to check
        // We can do it this way because of the requirement that states that all winning combinations have to start at the
        // left-most roller
        while (rollers[0].GetCellAtPosition(pattern[0]).fruitID == rollers[rollerToCheck].GetCellAtPosition(pattern[rollerToCheck]).fruitID && rollerToCheck < rollers.Length)
        {
            // Increasing the number to check the next roller, because we already know the previous ones share the same fruit
            ++rollerToCheck;
        }

        // if it return true, it means that the combination is a winning combination and it's the current one that
        // gives more points
        if (FillUpWinningCombination(ref winningCombination, rollerToCheck, pattern))
        {
            // In this case, we will keep saved the winning pattern used
            tryingPattern = pattern;
        }
    }

    // Returns the value of the given combination if its a winning combination
    private int GetWinningCombinationPoints(int[] combination)
    {
        int value = -1;

        foreach (KeyValuePair<int[], int> pair in winCombinations)
        {
            // In C# there is no direct comparator to compare if two int arrays are equal, at least that I am aware of, so the first check I do is look if the
            // winning combination and the combination we are looking in the dictionary are the same, if not, we can discard this combination.
            // If true, we look at the first number in each combination, if it's the same, the arrays must be equal because all the numbers
            // inside the array are the same
            if (pair.Key.Length == combination.Length && pair.Key[0] == combination[0])
            {
                value = pair.Value;

                // If we find the correct combination, we can break the loop because we are not interested in the rest
                break;
            }
        }

        return value;
    }

    private bool FillUpWinningCombination(ref KeyValuePair<int[], int> winningCombination, int combinationLength, int[] pattern)
    {
        // Checking if there was any winning combination, because there isn't any combination with just 1 fruit
        if (combinationLength <= 1) return false;

        // Creating the combination array
        int[] combination = new int[combinationLength];

        // Filling Up the array with the fruit ids
        for (int i = 0; i < combination.Length; ++i)
        {
            combination[i] = rollers[0].GetCellAtPosition(pattern[0]).fruitID;
        }

        // Checking if the combination we found has any prize associated with it
        int value = GetWinningCombinationPoints(combination);

        // If not, we don't need to follow the function
        if (value == -1 || (winningCombination.Key != null && winningCombination.Value >= value)) return false;

        winningCombination = new KeyValuePair<int[], int>(combination, value);
        return true;
    }

    IEnumerator WinningPanelAnimation(int fruitID, int combinationLength, int value, int[] pattern)
    {
        // Correctly sizing the winning panel depending on the lenght of the winning combination
        Vector2 size = winningPanelRT.sizeDelta;
        size.x = 490 + 130 * (combinationLength - 2);
        winningPanelRT.sizeDelta = size;

        // Showing the value given by the winning combination
        pointsText.text = value.ToString();

        // Positioning the highlighters in the winning pattern
        for (int i = 0; i < combinationLength; ++i)
        {
            Vector2 highlightPos = rollers[i].highlight.anchoredPosition;

            highlightPos.y = pattern[i] * -212 + 212;

            rollers[i].highlight.anchoredPosition = highlightPos;
        }

        // Assigning the fruit sprites and activating or deactivating the necessary ones to show the winning combination
        for (int i = 0; i < 4; ++i)
        {
            if (i < combinationLength)
            {
                if (!winningPanelImages[i].gameObject.activeSelf) winningPanelImages[i].gameObject.SetActive(true);
                winningPanelImages[i].sprite = fruits[fruitID];
            }
            else winningPanelImages[i].gameObject.SetActive(false);
        }

        // Animation for showing the winning panel, changing between visible and not visible every 1 or 0.5 seconds
        for (int i = 0; i < 6; ++i)
        {
            winningPanel.alpha = winningPanel.alpha == 1 ? 0 : 1;

            for (int j = 0; j < combinationLength; ++j)
            {
                rollers[j].highlight.gameObject.SetActive(winningPanel.alpha == 1);
            }

            yield return new WaitForSeconds(winningPanel.alpha == 1 ? 1f : 0.5f);
        }

        // Adding received points to total points and showing them in the scene's UI
        totalPoints += value;
        totalPointText.text = totalPoints.ToString();

        // Allowing the user to roll again
        spinButton.interactable = true;
    }
}
