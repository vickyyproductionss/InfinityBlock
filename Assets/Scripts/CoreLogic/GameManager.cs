using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using Unity.Mathematics;
using System.Linq;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public double NextBlockNumber;

    ///UI EVENT
    public delegate void UIUpdateEvent();
    public event UIUpdateEvent OnUIUpdate;

    ///GameOver EVENT
    public delegate void GameOverEvent();
    public event UIUpdateEvent OnGameOver;
    private bool _isGameOver;
    public bool IsGameOver
    {
        get { return _isGameOver; }
        set { _isGameOver = value; }
    }

    private static double _score;
    public static double Score
    {
        get { return _score; }
        set
        {
            _score = value;
            //OnUIUpdate?.Invoke();  // Trigger the event when score changes
        }
    }

    [Header("Pre-requisite")]
    public float GapSize;
    public float blockSize;
    public List<Color> colors;
    private Vector3 lastTouchPosition;
    public GameObject BlockPrefab;
    public GameObject BlockParent;
    public List<Vector3> blockPositions;
    private Dictionary<Vector3,int> removedElements;
    private Camera mainCamera;
    public static GameManager instance;

    [Header("Blocks Power")]
    public int startingPower;
    public int endingPower;

    [Header("BlocksInfo")]
    int blocksSpawned = 0;
    GameObject lastBlock;
    bool iterating;

    #region TestingStuff
    public int TestBlocksCount;
    IEnumerator DropAndFillTestBlocks()
    {
        yield return new WaitForEndOfFrame();
        blocksSpawned++;
        Vector2 screenRandom = new Vector2(UnityEngine.Random.Range(0, Screen.width), UnityEngine.Random.Range(0, Screen.width));
        lastTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenRandom.x, screenRandom.y, mainCamera.nearClipPlane));
        GameObject block = Instantiate(BlockPrefab, lastTouchPosition, Quaternion.identity);
        lastBlock = block;
        block.name = "Block_" + blocksSpawned;
        int pow = UnityEngine.Random.Range(1, 4);
        double blockNum = NextBlockNumber;
        block.GetComponent<SpriteRenderer>().color = colors[pow];
        block.GetComponent<Block>().BlockValue = blockNum;
        NextBlockNumber = Mathf.Pow(2, pow);
        block.transform.localScale = Vector3.one * blockSize;
        Vector3 newBlockPos = DeterminePartitionAndPosition(screenRandom.x);
        block.GetComponent<Block>().HoldingPosition = newBlockPos;
        if (newBlockPos == Vector3.one * -1)
        {
            OnGameOver.Invoke();
        }
        else
        {
            LeanTween.move(block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
            {
                block.transform.parent = BlockParent.transform;
                block.transform.localPosition = newBlockPos;
                TestBlocksCount++;
                if (TestBlocksCount < 10)
                {
                    StartCoroutine(DropAndFillTestBlocks());
                }

            });
        }
    }

    ///Testing purpose only

    public bool LogAllInfoOfThisBlock;
    public int blockIndex;
    public bool testingMode;
    #endregion

    #region UnityDefault
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        removedElements = new Dictionary<Vector3, int>();
        mainCamera = Camera.main;
        CalculateSquareScale();
        CalculateBlockPositions();
        NextBlockNumber = 2;
        if (testingMode)
        {
            StartCoroutine(DropAndFillTestBlocks());
        }
    }
    bool isMergeRunning;
    float timeBetweenDrops = 0;
    void Update()
    {
        DropNextBlock();
        if(blocksSpawned > 1)
        {
            if(!isMergeRunning)
            {
                StartCoroutine(moveAllSquares());
            }
            else
            {
                StartCoroutine(mergeAllPossibleBlocks());
            }
        }
        if (testingMode)
        {

            if (LogAllInfoOfThisBlock)
            {
                LogAllInfoOfThisBlock = false;
                List<GameObject> allsortedCreatedAt = GetSortedBlocksByCreatedAt();
                List<GameObject> AllBlocksNearby = GetNearbyBlocks(BlockParent.transform.GetChild(blockIndex).transform.position);
                List<GameObject> AllSimilarBlocks = GetSimilarBlocks(AllBlocksNearby, BlockParent.transform.GetChild(blockIndex).GetComponent<Block>().BlockValue);
                Debug.Log("Info is ABOUT: " + BlockParent.transform.GetChild(blockIndex).gameObject.name + "\n======================");
                foreach (GameObject block in allsortedCreatedAt)
                {
                    Debug.Log("SortedBlock: " + block.name + " made at: " + block.GetComponent<Block>().CreatedAt);
                }
                foreach (GameObject block in AllBlocksNearby)
                {
                    Debug.Log("Nearby Block: " + block.name);
                }
                foreach (GameObject block in AllSimilarBlocks)
                {
                    Debug.Log("Similar Block: " + block.name);
                }
            }
        }
    }
    void FixedUpdate()
    {
        timeBetweenDrops += Time.fixedDeltaTime;
    }
    #endregion

    #region  CoreLogic
    void DropNextBlock()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !testingMode && !IsGameOver && timeBetweenDrops > 0.3f)
        {
            timeBetweenDrops = 0;
            blocksSpawned++;
            Touch lastTouch = Input.GetTouch(Input.touchCount - 1);
            Vector3 lastTouchPositionScreen = lastTouch.position;
            lastTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(lastTouchPositionScreen.x, lastTouchPositionScreen.y, mainCamera.nearClipPlane));
            GameObject block = Instantiate(BlockPrefab, lastTouchPosition, Quaternion.identity);
            lastBlock = block;
            block.name = "Block_" + blocksSpawned;
            int pow = UnityEngine.Random.Range(startingPower, endingPower);
            double blockNum = NextBlockNumber;
            block.GetComponent<Block>().BlockValue = blockNum;
            NextBlockNumber = Mathf.Pow(2, pow);
            block.transform.localScale = Vector3.one * blockSize;
            Vector3 newBlockPos = DeterminePartitionAndPosition(lastTouchPositionScreen.x);
            RemovePosition(newBlockPos);
            block.GetComponent<Block>().HoldingPosition = newBlockPos;
            LeanTween.move(block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => { OnBlockAnimFinished(block, newBlockPos); });
            OnUIUpdate.Invoke();
            if (blocksSpawned == 2)
            {
                StartCoroutine(moveAllSquares());
            }
        }
    }

    void OnBlockAnimFinished(GameObject block, Vector3 finalPos)
    {
        block.transform.position = finalPos;
        block.transform.parent = BlockParent.transform;
    }

    //First Block Drops then it merges with surrounding and then it moves all the grid the new possible positions and then it checks for the further merges if then merges happen then again move the grid to further positions
    IEnumerator moveAllSquares()
    {
        yield return new WaitForSeconds(0.01f);
        for (int i = 0; i < BlockParent.transform.childCount; i++)
        {
            GameObject block = BlockParent.transform.GetChild(i).gameObject;
            float blockXPosition = block.transform.localPosition.x;
            var availablePositions = blockPositions.Where(p => p.x == blockXPosition).ToList();
            // var occupiedPositions = BlockParent.transform.Cast<Transform>().Where(t => t.localPosition.x == blockXPosition).Select(t => t.localPosition).ToList();
            // availablePositions.RemoveAll(pos => occupiedPositions.Contains(pos));

            Vector3 newBlockPos = new Vector3();
            if (availablePositions.Count > 0)
            {
                newBlockPos = availablePositions[0];
                if (newBlockPos.y > block.transform.localPosition.y)
                {
                    Vector3 posToAdd = block.transform.localPosition;
                    AddPosition(block.GetComponent<Block>().HoldingPosition);
                    block.GetComponent<Block>().HoldingPosition = newBlockPos;
                    RemovePosition(newBlockPos);
                    iterating = true;
                    LeanTween.move(block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
                    {
                        var nearbyBlocks = GetNearbyBlocks(block.transform.position);
                        var similarBlocks = GetSimilarBlocks(nearbyBlocks, block.GetComponent<Block>().BlockValue);
                        if (!similarBlocks.Contains(block))
                        {
                            similarBlocks.Add(block);
                        }
                        if (similarBlocks.Count >= 2)
                        {
                            DestroySimilarBlocks(similarBlocks, block);
                        }
                        else
                        {
                            similarBlocks.Clear();
                        }
                        iterating = false;
                    });

                }
            }
            ///Debug.Log("New Pos: " + newBlockPos + " actual pos: " + BlockParent.transform.GetChild(i).gameObject.transform.localPosition);

            else
            {
                iterating = false;
            }
            while (iterating)
            {
                yield return null;
            }
        }
        isMergeRunning = true;
    }
    IEnumerator mergeAllPossibleBlocks()
    {
        yield return new WaitForEndOfFrame();
        List<GameObject> allblocks = GetSortedBlocksByCreatedAt();
        for (int i = 0; i < allblocks.Count; i++)
        {
            if (allblocks[i])
            {
                GameObject block = allblocks[i];
                var nearbyBlocks = GetNearbyBlocks(block.transform.position);
                var similarBlocks = GetSimilarBlocks(nearbyBlocks, block.GetComponent<Block>().BlockValue);
                if (similarBlocks.Contains(lastBlock))
                {
                    block = lastBlock;
                    nearbyBlocks.Clear();
                    similarBlocks.Clear();
                    nearbyBlocks = GetNearbyBlocks(block.transform.position);
                    similarBlocks = GetSimilarBlocks(nearbyBlocks, block.GetComponent<Block>().BlockValue);
                }
                if (!similarBlocks.Contains(block))
                {
                    similarBlocks.Add(block);
                }
                if (similarBlocks.Count >= 2)
                {
                    DestroySimilarBlocks(similarBlocks, block);
                    yield return new WaitForSecondsRealtime(0.25f);

                }
                else
                {
                    similarBlocks.Clear();
                    iterating = false;
                }
            }
        }
        isMergeRunning = false;
    }
    void RemovePosition(Vector3 element)
    {
        int index = blockPositions.IndexOf(element);
        if (index != -1)
        {
            removedElements[element] = index;
            blockPositions.RemoveAt(index);

            Debug.Log($"Removed element {element} from index {index}");
        }
        else
        {
            Debug.LogError("Element not found in the list");
        }
    }

    void AddPosition(Vector3 element)
    {
        if (removedElements.TryGetValue(element, out int index))
        {
            // Ensure the index is within bounds
            if (index >= 0 && index <= blockPositions.Count)
            {
                blockPositions.Insert(index, element);
                Debug.Log($"Added element {element} back at index {index}");
                
                // Remove from the dictionary after adding back
                removedElements.Remove(element);
            }
            else
            {
                Debug.LogError("Index out of range");
            }
        }
        else
        {
            Debug.LogError($"Element not found in removed elements dictionary {element}");
        }
    }
    List<GameObject> GetSortedBlocksByCreatedAt()
    {
        // Get all children of BlockParent
        List<GameObject> blockList = new List<GameObject>();
        foreach (Transform child in BlockParent.transform)
        {
            blockList.Add(child.gameObject);
        }

        // Sort the list based on the CreatedAt value
        blockList = blockList.OrderByDescending(block => block.GetComponent<Block>().CreatedAt).ToList();

        return blockList;
    }

    List<GameObject> GetNearbyBlocks(Vector3 position)
    {
        return BlockParent.transform.Cast<Transform>()
            .Where(t => Vector3.Distance(t.position, position) <= blockSize + blockSize / 5)
            .Select(t => t.gameObject)
            .OrderByDescending(block => block.GetComponent<Block>().CreatedAt)
            .ToList();
    }

    List<GameObject> GetSimilarBlocks(List<GameObject> nearbyBlocks, double value)
    {
        return nearbyBlocks
            .Where(b => b.GetComponent<Block>().BlockValue == value)
            .OrderByDescending(block => block.GetComponent<Block>().CreatedAt)
            .ToList();
    }

    void DestroySimilarBlocks(List<GameObject> similarBlocks, GameObject _block)
    {
        //var newestBlock = similarBlocks.OrderByDescending(b => b.GetComponent<Block>().CreatedAt).First();
        if (similarBlocks.Contains(lastBlock))
        {
            similarBlocks.Remove(lastBlock);
            _block = lastBlock;
        }
        else
        {
            similarBlocks.Remove(_block);
        }
        double myValue = _block.GetComponent<Block>().BlockValue;
        double myNewValue = IncreaseScore(myValue, similarBlocks.Count + 1);
        _block.GetComponent<Block>().BlockValue = myNewValue;
        foreach (var b in similarBlocks)
        {
            AddPosition(b.GetComponent<Block>().HoldingPosition);
            DestroyImmediate(b);
        }
        float blockXPosition = _block.transform.localPosition.x;
        var availablePositions = blockPositions.Where(p => p.x == blockXPosition).ToList();
        Vector3 newBlockPos = new Vector3();
        if (availablePositions.Count > 0)
        {
            newBlockPos = availablePositions[0];
            
        }
        ///Debug.Log("New Pos: " + newBlockPos + " actual pos: " + BlockParent.transform.GetChild(i).gameObject.transform.localPosition);
        if (newBlockPos.y > _block.transform.localPosition.y)
        {
            RemovePosition(newBlockPos);
            AddPosition(_block.GetComponent<Block>().HoldingPosition);
            _block.GetComponent<Block>().HoldingPosition = newBlockPos;
            iterating = true;
            LeanTween.move(_block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => { iterating = false; });
        }
        similarBlocks.Clear();
    }

    public Vector3 DeterminePartitionAndPosition(float touchXScreenPos)
    {
        Rect safeArea = Screen.safeArea;
        Vector3 safeAreaBottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, mainCamera.nearClipPlane));
        float startX = safeAreaBottomLeft.x + blockSize / GapSize + blockSize / 2f;
        float partitionWidth = blockSize + blockSize / GapSize;

        int partitionIndex = Mathf.FloorToInt((touchXScreenPos - safeArea.xMin) / (safeArea.width / 5));
        float blockXPosition = startX + partitionIndex * partitionWidth;

        var availablePositions = blockPositions.Where(p => p.x == blockXPosition).ToList();
        return availablePositions.Count > 0 ? availablePositions[0] : new Vector3(-1, -1, -1);
    }


    void CalculateBlockPositions()
    {
        blockPositions = new List<Vector3>();

        Rect safeArea = Screen.safeArea;
        Vector3 safeAreaTopLeft = mainCamera.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMax, mainCamera.nearClipPlane));

        float startX = safeAreaTopLeft.x + blockSize / GapSize + blockSize / 2f;
        float startY = safeAreaTopLeft.y - blockSize / GapSize - blockSize / 2f;
        float partitionWidth = blockSize + blockSize / GapSize;
        float partitionHeight = blockSize + blockSize / GapSize;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                float posX = startX + i * partitionWidth;
                float posY = startY - j * partitionHeight;
                blockPositions.Add(new Vector3(posX, posY, 0));
            }
        }
    }

    public double IncreaseScore(double numOnBlock, int numOfBlocks)
    {
        double increasedScore = 0;
        if (numOfBlocks == 2)
        {
            increasedScore = numOnBlock * 2;
        }
        else if (numOfBlocks == 3)
        {
            increasedScore = numOnBlock * 4;
        }
        else if (numOfBlocks == 4)
        {
            increasedScore = numOnBlock * 8;
        }

        Score += increasedScore;
        return increasedScore;
    }

    public void CalculateSquareScale()
    {
        Rect safeArea = Screen.safeArea;
        Vector3 safeAreaBottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, mainCamera.nearClipPlane));
        Vector3 safeAreaBottomRight = mainCamera.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMin, mainCamera.nearClipPlane));
        float safeAreaWidth = Vector3.Distance(safeAreaBottomLeft, safeAreaBottomRight);

        // Calculate the total width required for 5 squares and the spaces between them
        // Let S be the side length of each square
        // 5 * S (width of 5 squares) + 6 * (S / 20) (6 gaps: 5 between squares + 1 on each side) = safeAreaWidth
        // => 5S + 6 * (S / 20) = safeAreaWidth
        // => 5S + (6S / 20) = safeAreaWidth
        // => 5S + 0.3S = safeAreaWidth
        // => 5.3S = safeAreaWidth
        // => S = safeAreaWidth / 5.3
        //5s + 6s/gapsize = safearea
        //gapsize*5s +6s = safeare*gapsize
        //s(5*gapsize +6) = safeare*gapsize
        // s = safeare* gapsize/(5*gapsize + 6)

        float numerator = safeAreaWidth * GapSize;
        float denominator = 5 * GapSize + 6;


        blockSize = numerator / denominator;


    }
    #endregion

    #region Gameover Stuff

    public void CheckGameOver()
    {
        if (BlockParent.transform.childCount > 35)
        {
            OnGameOver.Invoke();
            IsGameOver = true;
        }
    }

    #endregion
}
