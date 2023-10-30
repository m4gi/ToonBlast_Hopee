using deVoid.Utils;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerSignal : ASignal { }
public class GameManagerGetClickedBrickSignal : ASignal<Transform> { }
public class GameManagerGetBombedBrickSignal : ASignal<Transform> { }
public class GameManagerDeleteFromGridSignal : ASignal<int, int> { }
public class GameManager : MonoBehaviour
{
    private const float BrickHeight = 0.5f;
    private const float BrickWidth = 0.5f;
    private const int Undefined = -1;

    private int _move;

    [Header("Game Setting")]
    [SerializeField]
    [Range(3, 20)]
    private int Width = 9;

    [SerializeField]
    [Range(3, 20)]
    private int Height = 9;

    [SerializeField]
    [Range(3, 20)]
    private int CreateBomb = 5;
    [SerializeField]
    private int ScoreNeed = 3000;
    public GameObject[] blocks;
    public GameObject bomb;

    private Transform[][] Grid;
    private IEnumerator _fallElementsDownCoroutine;

    private int _score;
    private int _blockCounter;

    public bool gameOver;
    public bool waitNewMatch;
    private bool _crRunning;

    [Header("Time Count Down Setting")]
    public float timeRemaining = 120;
    public bool timerIsRunning = true;

    [Header("UI Setting")]
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text timeText;

    void Start()
    {
        Signals.Get<GameManagerGetClickedBrickSignal>().AddListener(GetClickedBrick);
        Signals.Get<GameManagerDeleteFromGridSignal>().AddListener(DeleteFromGrid);
        Signals.Get<GameManagerGetBombedBrickSignal>().AddListener(GetBombedBrick);
        StartGame();
    }

    private void StartGame()
    {
        Grid = new Transform[Width][];
        for (int i = 0; i < Grid.Length; i++)
        {
            Grid[i] = new Transform[Height];
        }

        FillContainer();
        _score = 0;
    }

    /**
     * Fills container with random generated bricks.
     */
    private void FillContainer()
    {
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var position = transform.position + new Vector3(i * BrickWidth, j * BrickHeight, 0);
                int indexBlock = Random.Range(0, blocks.Length);
                var newBlock = Instantiate(blocks[indexBlock], position,
                    Quaternion.identity, transform);
                newBlock.GetComponent<BlockObject>().SetTypeBlock(blocks[indexBlock].name, _blockCounter++);
                Grid[i][j] = newBlock.transform;
            }
        }
    }

    private bool _elementsAreCreated;
    private bool _changeHappen;

    private void Update()
    {
        var bricksToBeAdded = new Dictionary<int, int>();
        var gridFull = true;
        if (!_crRunning)
        {
            gridFull = CheckGridNotNeedNewBrick(bricksToBeAdded);
        }

        if (!gridFull && !_crRunning)
        {
            _elementsAreCreated = false;
            _visitedBomb = new List<Point>();
            _crRunning = true;
            _fallElementsDownCoroutine = FallElementsDown(bricksToBeAdded);
            StartCoroutine(_fallElementsDownCoroutine);
            _move++;
            _changeHappen = true;
        }

        StartCountDownGame();

        if (gameOver)
        {
            EndGame();
        }
    }

    /**
     * Checks grids are do not need new bricks
     * Adds the missing bricks column information
     */
    private bool CheckGridNotNeedNewBrick(Dictionary<int, int> bricksToBeAdded)
    {
        int counter = 0;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i][j], null))
                {
                    counter++;
                    if (bricksToBeAdded.ContainsKey(i))
                    {
                        bricksToBeAdded[i] += 1;
                    }
                    else
                    {
                        bricksToBeAdded.Add(i, 1);
                    }
                }
            }
        }

        //Bomb creating also triggers so...
        return counter < 2;
    }

    private IEnumerator FallElementsDown(Dictionary<int, int> dictionary)
    {
        while (true)
        {
            bool stillFalling = true;
            bool allFilled = true;
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary.ElementAt(i).Value != 0)
                {
                    allFilled = false;
                    break;
                }
            }

            if (allFilled)
            {
                _elementsAreCreated = true;
                break;
            }

            while (stillFalling)
            {
                yield return new WaitForSeconds(0.01f);
                stillFalling = false;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (!ReferenceEquals(Grid[x][y], null)) continue;
                        for (int indexY = y + 1; indexY < Height; indexY++)
                        {
                            if (!ReferenceEquals(Grid[x][indexY], null))
                            {
                                stillFalling = true;
                                Grid[x][indexY - 1] = Grid[x][indexY];
                                Vector2 vector = Grid[x][indexY - 1].transform.position;
                                vector.y -= BrickHeight;
                                Grid[x][indexY - 1].transform.position = vector;
                                Grid[x][indexY] = null;
                            }
                        }
                    }
                }
            }

            BringNewBricks(dictionary);
        }


        _crRunning = false;
    }


    public void GetClickedBrick(Transform clickedTransform)
    {
        if (!_crRunning && !gameOver)
        {
            Signals.Get<AudioMangerSignal>().Dispatch(Constants.ClickBrick);
            FindAndDeleteElements(clickedTransform);
        }
    }

    private void FindAndDeleteElements(Transform clickedObject)
    {
        int clickedBlockX = Undefined, clickedBlockY = Undefined;

        GetClickedGrid(ref clickedBlockX, ref clickedBlockY, clickedObject);

        if (clickedBlockX != Undefined)
        {
            var clickedColor = Grid[clickedBlockX][clickedBlockY].GetComponent<BlockObject>().TypeBlock;
            List<Point> elementsToBeTraversed = new List<Point>();
            List<Point> elementsToBeDeleted = new List<Point>();
            elementsToBeTraversed.Add(new Point(clickedBlockX, clickedBlockY));
            elementsToBeDeleted.Add(new Point(clickedBlockX, clickedBlockY));

            Dictionary<int, int> missingBricksAtColumns = new Dictionary<int, int>();
            missingBricksAtColumns.Add(clickedBlockX, 1);

            TraverseNew(elementsToBeTraversed, clickedColor, elementsToBeDeleted, missingBricksAtColumns);

            if (elementsToBeDeleted.Count < 2)
            {
                clickedObject.DOShakePosition(0.5f, 0.1f);
                return;
            }


            AddScore(elementsToBeDeleted.Count);

            DeleteElements(elementsToBeDeleted, ShouldBoomBeCreated(elementsToBeDeleted), missingBricksAtColumns);
        }
    }

    private bool ShouldBoomBeCreated(List<Point> elementsToBeDeleted)
    {
        return elementsToBeDeleted.Count > CreateBomb;
    }

    private void BringNewBricks(Dictionary<int, int> dictionary)
    {
        var list = new List<int>();
        for (var i = 0; i < dictionary.Count; i++)
        {
            var item = dictionary.ElementAt(i);
            if (item.Value == 0) continue;
            list.Add(item.Key);
            dictionary[item.Key] -= 1;
        }

        CreateColumns(list);
    }

    private void CreateColumns(List<int> mc)
    {
        foreach (var t in mc)
        {
            int indexBlock = Random.Range(0, blocks.Length);
            UnityEngine.Object newBlock = Instantiate(blocks[indexBlock],
                new Vector3(transform.position.x + t * BrickWidth, transform.position.y + (Height - 1) * BrickHeight,
                    0), Quaternion.identity, transform);
            GameObject gameObjectBlock = (GameObject)newBlock;
            gameObjectBlock.GetComponent<BlockObject>().SetTypeBlock(blocks[indexBlock].name, _blockCounter++);
            Grid[t][Height - 1] = gameObjectBlock.transform;
        }
    }

    private void DeleteElements(List<Point> elementsToBeDeleted, bool createBomb, Dictionary<int, int> dictionary)
    {
        foreach (Point point in elementsToBeDeleted)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            if (createBomb)
            {
                createBomb = false;
                pos = Grid[point.GetX()][point.GetY()].gameObject.transform.position;
                Destroy(Grid[point.GetX()][point.GetY()].gameObject);
                var bombObject = Instantiate(bomb, pos, Quaternion.identity, transform);
                bombObject.GetComponent<BlockElement>().SetTypeBlock(bombObject.name, _blockCounter++);
                Grid[point.GetX()][point.GetY()] = bombObject.transform;
                dictionary[point.GetX()] -= 1;
            }
            else
            {
                var brickId = Grid[point.GetX()][point.GetY()].gameObject.GetComponent<BlockElement>();
                brickId.Trigger(point.GetX(), point.GetY());
            }
        }
    }

    public void DeleteFromGrid(int x, int y)
    {
        if (x != -1)
            Grid[x][y] = null;
    }

    private void GetClickedGrid(ref int x, ref int y, Transform clickedObject)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i][j], null))
                {
                    continue;
                }

                if (Grid[i][j].Equals(clickedObject))
                {
                    x = i;
                    y = j;
                    break;
                }
            }
        }
    }

    /**
     * Adds new score and updates text
     */
    private void AddScore(int count, bool isBonus = false)
    {
        var scoreToAdd = count * 50;
        if (isBonus && count >= 5) { scoreToAdd += 200; }
        _score += scoreToAdd;
        print($"Score: {_score}");
        scoreText.text = $"Score: {_score}";
        if (_score >= ScoreNeed)
        {
            gameOver = true;
        }
    }

    private void TraverseNew(List<Point> elementsToBeTraversed, BlockType color, List<Point> elementsToBeDeleted,
        Dictionary<int, int> dictionary)
    {
        while (elementsToBeTraversed.Count > 0)
        {
            int curX = elementsToBeTraversed[0].GetX();
            int curY = elementsToBeTraversed[0].GetY();
            CheckElement(curX - 1, curY);
            CheckElement(curX + 1, curY);
            CheckElement(curX, curY + 1);
            CheckElement(curX, curY - 1);

            elementsToBeTraversed.Remove(elementsToBeTraversed[0]);
        }

        void CheckElement(int x, int y)
        {
            if (x > -1 && x < Width && y > -1 && y < Height)
            {
                if (!ReferenceEquals(Grid[x][y], null) && Grid[x][y].GetComponent<BlockElement>().TypeBlock.Equals(color))
                {
                    Point newCur = new Point(x, y);
                    if (!elementsToBeDeleted.Contains(newCur) && !elementsToBeTraversed.Contains(newCur))
                    {
                        if (dictionary.ContainsKey(newCur.GetX()))
                        {
                            dictionary[newCur.GetX()] += 1;
                        }
                        else
                        {
                            dictionary.Add(newCur.GetX(), 1);
                        }

                        elementsToBeDeleted.Add(newCur);
                        elementsToBeTraversed.Add(newCur);
                    }
                }
            }
        }
    }

    public void GetBombedBrick(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
            BombIt(gameObjectTransform);
    }

    public void BombIt(Transform gameObjectTransform)
    {
        Signals.Get<AudioMangerSignal>().Dispatch(Constants.ClickBomb);
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();
        var dictionary = new Dictionary<int, int>();
        var listBomb = new List<Point>();
        listBomb.Add(new Point(x, y));

        FindBombedElements(listBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary);
        AddScore(elementsToDelete.Count);
    }

    private List<Point> _visitedBomb = new List<Point>();

    private void FindBombedElements(List<Point> listBomb, Dictionary<int, int> dictionary, List<Point> elementsToDelete)
    {
        _visitedBomb.Add(listBomb[0]);
        while (listBomb.Count > 0)
        {
            for (int i = listBomb[0].GetX() - 1; i <= listBomb[0].GetX() + 1; i++)
            {
                for (int j = listBomb[0].GetY() - 1; j <= listBomb[0].GetY() + 1; j++)
                {
                    if (i < 0 || i >= Width || j < 0 || j >= Height)
                        continue;
                    addElement(i, j, elementsToDelete, dictionary);
                }
            }

            if (listBomb.Count > 0)
            {
                listBomb.RemoveAt(0);
            }
        }
    }

    void addElement(int x, int y, List<Point> deleteList, Dictionary<int, int> dictionary)
    {
        if (x > -1 && x < Width && y > -1 && y < Height)
        {
            Point toAdd = new Point(x, y);
            if (!ReferenceEquals(Grid[x][y], null) && !deleteList.Contains(toAdd))
            {
                deleteList.Add(toAdd);
                if (dictionary.ContainsKey(x))
                {
                    dictionary[x] += 1;
                }
                else
                {
                    dictionary.Add(x, 1);
                }
            }
        }
    }
    private void StartCountDownGame()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                gameOver = true;
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }
    public void EndGame()
    {
        if (!waitNewMatch)
        {
            waitNewMatch = true;
            print("End Game");
        }
        
    }

}

struct Point
{
    int x;
    int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }
}