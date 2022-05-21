using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roller : MonoBehaviour
{
    [Header("Modifiable Variables")]
    // Speed at which the roller will roll. Pixels/second
    // The number comes from the height of each cell/sprite, multiplied by the 3 cells visible and the number of complete rotations we want,
    // in this case, it will make eight complete rotations in a second.
    [SerializeField] private float rollSpeed = 5088;

    // Highlight image used to show winning pattern 
    public RectTransform highlight;

    // Array of cells we will use to show the fruits and simulate the roll
    private Cell[] cells;

    // Sequence of IDs each roller will use to show the specific order of fruits given by the test
    private int[] fruitSequence;

    // Number used to know which number in the sequence we need to assign next
    private int nextSequenceNumber = 0;

    // Number we will use to know each cell position. This number will be used to check solutions
    [HideInInspector] public int highestCell = 0;

    private Coroutine currentRoll = null;

    // Using Awake instead of Start because we will not depend on other scripts getting the array and we will avoid
    // scripting execution order issues between the RollerManager Start and the Roller's Start
    private void Awake()
    {
        // Getting and assigning the images we will use to assign the sprites into the cells
        Image[] cellImages = GetComponentsInChildren<Image>();

        cells = new Cell[cellImages.Length];

        for (int i = 0; i < cells.Length; ++i)
        {
            cells[i] = new Cell();
            cells[i].image = cellImages[i];
            cells[i].rectTransform = cells[i].image.rectTransform;
        }
    }

    public void AssignFruitSequence(int[] sequence)
    {
        fruitSequence = sequence;

        for (int i = 0; i < cells.Length; ++i)
        {
            // Assigning each cell their start fruit and saving fruit id
            cells[i].AssignFruit(fruitSequence[i]);

            // Increasing the next sequence number each time we assign a number in the sequence to a cell
            ++nextSequenceNumber;

            // Checking if the sequence has been completed and we need to cycle and start the sequence again
            if (nextSequenceNumber >= fruitSequence.Length) nextSequenceNumber = 0;
        }
    }

    public void BeginRoll(float spinningTime)
    {
        if (currentRoll != null) StopCoroutine(currentRoll);
        currentRoll = StartCoroutine(Roll(spinningTime));
    }

    IEnumerator Roll(float spinningTime)
    {
        float time = 0;

        while (time <= spinningTime)
        {
            time += Time.deltaTime;

            // Iterating between the cells and moving them to simulate the roll
            for (int i = 0; i < cells.Length; ++i)
            {
                Vector2 cellPos = cells[i].rectTransform.anchoredPosition;
                cellPos.y += rollSpeed * Time.deltaTime;
                cells[i].rectTransform.anchoredPosition = cellPos;

                // 424 is the coordinate where the cells are no longer visible.
                if (cells[i].rectTransform.anchoredPosition.y >= 424f)
                {
                    // Moving the highest cell in the roller, no longer visible, down, to simulate the roll. 
                    // -424 is the coordinate where the cell is still completelly invisible
                    cellPos.y = -424f;
                    cells[i].rectTransform.anchoredPosition = cellPos;

                    // Assigning new fruit sprite to moved cell
                    cells[i].AssignFruit(fruitSequence[nextSequenceNumber]);

                    // Increasing the next sequence number each time we assign a number in the sequence to a cell
                    ++nextSequenceNumber;

                    // Checking if the sequence has been completed and we need to cycle and start the sequence again
                    if (nextSequenceNumber >= fruitSequence.Length) nextSequenceNumber = 0;
                }
            }

            // 212 is the last coordinate where the cells are completely visible.
            if (cells[highestCell].rectTransform.anchoredPosition.y > 212f)
            {
                // Increasing the highestCell by 1 because the previous highest cell has passed the maximum correct height
                // and it's no loger completely visible.
                // In case the highestCell number is higher than the max cell available, it will cycle to the first one
                highestCell = highestCell + 1 >= cells.Length ? 0 : highestCell + 1;
            }

            yield return null;
        }

        // Ending the roll exactly when the cells fall in their correct positions
        // 212 is the coordinate of the highest cell, because it's also the sprite's height
        while (cells[highestCell].rectTransform.anchoredPosition.y != 212f)
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                Vector2 cellPos = cells[i].rectTransform.anchoredPosition;
                cellPos.y += rollSpeed * Time.deltaTime;
                cells[i].rectTransform.anchoredPosition = cellPos;

                // 424 is the coordinate where the cells are no longer visible.
                if (cells[i].rectTransform.anchoredPosition.y >= 424f)
                {
                    // Moving the highest cell in the roller, no longer visible, down, to simulate the roll. 
                    // -424 is the coordinate where the cell is still completelly invisible
                    cellPos.y = -424f;
                    cells[i].rectTransform.anchoredPosition = cellPos;
                }
            }

            // Checking if the highest cell we will have has passed its max correct height 
            if (cells[highestCell].rectTransform.anchoredPosition.y > 212f)
            {
                for (int i = 0; i < cells.Length; ++i)
                {
                    int cell = highestCell + i;

                    if (cell >= cells.Length) cell -= cells.Length;

                    Vector2 cellPos = cells[cell].rectTransform.anchoredPosition;
                    cellPos.y = 212 - 212 * i;
                    cells[cell].rectTransform.anchoredPosition = cellPos;
                }
            }

            yield return null;
        }

        currentRoll = null;
    }

    // Returns the asked cell by position, being the top position the highest Cell
    public Cell GetCellAtPosition(int position)
    {
        int cell = highestCell + position;

        if (cell >= cells.Length) cell -= cells.Length;

        return cells[cell];
    }
}

// Small class used to keep together the images used for showing the fruit and
// the ID of the fruit they are showing
public class Cell
{
    public RectTransform rectTransform = null;
    public Image image = null;
    public int fruitID = -1;

    public void AssignFruit(int sprite)
    {
        image.sprite = RollerManager.instance.fruits[sprite];
        fruitID = sprite;
    }
}