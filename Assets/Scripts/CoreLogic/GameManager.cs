using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using Unity.Mathematics;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;

public class GameManager : MonoBehaviour
{
    public double NextBlockNumber;
    public double SuperNextBlockNumber;
    public float PaddingFromTop;

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

    private double _score;
    public double Score
    {
        get { return _score; }
        set
        {
            int oldHighscore = PlayerPrefs.GetInt("x2BlocksHighscore");
            if (value > oldHighscore)
            {
                oldHighscore = (int)value;
                PlayerPrefs.SetInt("x2BlocksHighscore", oldHighscore);
            }
            _score = value;
            OnUIUpdate?.Invoke();  // Trigger the event when score changes
        }
    }

    [Header("Pre-requisite")]
    public float GapSize;
    public float blockSize;
    public List<Color> colors;
    private Vector3 lastTouchPosition;
    public GameObject BlockPrefab;
    public GameObject BlockParent;
    public GameObject PopupsParent;
    public Vector3[] blockPositions;
    public Dictionary<Vector3, int> removedElements;
    private Camera mainCamera;
    public static GameManager instance;

    [Header("Blocks Power")]
    public int startingPower;
    public int endingPower;

    [Header("BlocksInfo")]
    int blocksSpawned = 0;
    GameObject lastBlock;
    GameObject lastBlockInSelectedRow;
    bool iterating;

    [Header("UI Blocks and BGs")]
    public UIBlock UINextBlock;
    public UIBlock UISuperNextBlock;
    public ManageBackground manageBackground;

    bool isProgressSaved = true;

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
        int pow1 = UnityEngine.Random.Range(startingPower, endingPower);
        int pow2 = UnityEngine.Random.Range(startingPower, endingPower);
        NextBlockNumber = Mathf.Pow(2, pow1);
        SuperNextBlockNumber = Mathf.Pow(2, pow2);
        UINextBlock.BlockValue = NextBlockNumber;
        UISuperNextBlock.BlockValue = SuperNextBlockNumber;
        RetrieveProgress();
    }

    void RetrieveProgress()
    {
        string json = PlayerPrefs.GetString("SavedBlocks");
        if (!string.IsNullOrEmpty(json))
        {
            SaveBlocks saveBlocks = JsonConvert.DeserializeObject<SaveBlocks>(json);
            Score = saveBlocks.Score;
            foreach (var block in saveBlocks.Blocks)
            {
                GameObject retrievedBlock = Instantiate(BlockPrefab, block.BlockPosition, Quaternion.identity);
                retrievedBlock.transform.localScale = Vector3.one * blockSize;
                retrievedBlock.GetComponent<Block>().BlockValue = block.BlockValue;
                retrievedBlock.transform.parent = BlockParent.transform;
                retrievedBlock.GetComponent<Block>().HoldingPosition = block.BlockPosition;
                RemovePosition(block.BlockPosition);
                lastBlock = retrievedBlock;
                blocksSpawned++;
            }
        }
    }
    bool isMergeRunning;
    float timeBetweenDrops = 0;
    void Update()
    {
        DropNextBlock();
        if (blocksSpawned > 1)
        {
            if (!isMergeRunning)
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
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !testingMode && !IsGameOver && timeBetweenDrops > 0.5f && !IsAnythingMoving() && !IsAnyPopupActive())
        {
            timeBetweenDrops = 0;
            blocksSpawned++;
            Touch lastTouch = Input.GetTouch(Input.touchCount - 1);
            Vector3 lastTouchPositionScreen = lastTouch.position;
            Vector3 newBlockPos = DeterminePartitionAndPosition(lastTouchPositionScreen.x);
            lastTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(lastTouchPositionScreen.x, lastTouchPositionScreen.y, mainCamera.nearClipPlane));
            if (newBlockPos != new Vector3(-100, -100, -100) && lastTouchPosition.y > manageBackground.BottomLine.transform.position.y)
            {
                lastTouchPosition = new Vector3(newBlockPos.x, manageBackground.BottomLine.transform.position.y, newBlockPos.z);
                GameObject block = Instantiate(BlockPrefab, lastTouchPosition, Quaternion.identity);
                lastBlock = block;
                block.name = "Block_" + blocksSpawned;
                int pow = UnityEngine.Random.Range(startingPower, endingPower);
                double blockNum = NextBlockNumber;
                NextBlockNumber = SuperNextBlockNumber;
                UINextBlock.BlockValue = SuperNextBlockNumber;
                block.transform.localScale = Vector3.one * blockSize;
                block.GetComponent<Block>().BlockValue = blockNum;
                SuperNextBlockNumber = Mathf.Pow(2, pow);
                UISuperNextBlock.BlockValue = SuperNextBlockNumber;
                RemovePosition(newBlockPos);
                block.GetComponent<Block>().HoldingPosition = newBlockPos;
                block.GetComponent<Block>().IsMoving = true;
                LeanTween.move(block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => { OnBlockAnimFinished(block, newBlockPos); });
            }
            else
            {
                if (BlockParent.transform.childCount == 35)
                {
                    OnGameOver.Invoke();
                }
                Debug.Log("No Position Found in this lane");
            }
        }
        else if (!IsAnythingMoving() && BlockParent.transform.childCount == 35)
        {
            OnGameOver.Invoke();
        }
        if (!IsAnythingMoving() && !isProgressSaved)
        {
            isProgressSaved = true;
            SaveProgress();
        }
    }
    void SaveProgress()
    {
        SaveBlocks saveBlocks = new SaveBlocks
        {
            Blocks = new List<BlockInfo>()
        };
        saveBlocks.Score = Score;
        foreach (Transform t in BlockParent.transform)
        {
            double BlockValue = t.GetComponent<Block>().BlockValue;
            Vector3 BlockPos = t.position;
            BlockInfo info = new BlockInfo();
            info.BlockValue = BlockValue;
            info.BlockPosition = BlockPos;
            info.createdAt = t.GetComponent<Block>().CreatedAt;
            saveBlocks.Blocks.Add(info);
        }
        string json = JsonConvert.SerializeObject(saveBlocks);
        PlayerPrefs.SetString("SavedBlocks", json);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(SerializableVector3 sv3)
        {
            return new Vector3(sv3.x, sv3.y, sv3.z);
        }

        public static implicit operator SerializableVector3(Vector3 v3)
        {
            return new SerializableVector3(v3.x, v3.y, v3.z);
        }
    }


    public class SaveBlocks
    {
        public double Score;
        public List<BlockInfo> Blocks;
    }
    public class BlockInfo
    {
        public double BlockValue;
        public SerializableVector3 BlockPosition;
        public DateTime createdAt;
    }

    bool IsAnythingMoving()
    {
        foreach (Transform block in BlockParent.transform)
        {
            if (block.GetComponent<Block>().IsMoving)
            {
                return true;
            }
        }
        return false;
    }

    bool IsAnyPopupActive()
    {
        foreach (Transform block in PopupsParent.transform)
        {
            if (block.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    void OnBlockAnimFinished(GameObject block, Vector3 finalPos)
    {
        block.GetComponent<Block>().OnDropSound.Play();
        block.GetComponent<Block>().IsMoving = false;
        block.transform.position = finalPos;
        block.transform.parent = BlockParent.transform;
        isProgressSaved = false;
    }

    //First Block Drops then it merges with surrounding and then it moves all the grid the new possible positions and then it checks for the further merges if then merges happen then again move the grid to further positions
    IEnumerator moveAllSquares()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < BlockParent.transform.childCount; i++)
        {
            GameObject block = BlockParent.transform.GetChild(i).gameObject;
            float blockXPosition = block.transform.localPosition.x;
            var availablePositions = blockPositions.Where(p => p.x == blockXPosition).ToList();

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
                    block.GetComponent<Block>().IsMoving = true;
                    LeanTween.move(block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
                    {
                        block.GetComponent<Block>().IsMoving = false;
                        var nearbyBlocks = GetNearbyBlocks(block.transform.position);
                        var similarBlocks = GetSimilarBlocks(nearbyBlocks, block.GetComponent<Block>().BlockValue);
                        if (!similarBlocks.Contains(block))
                        {
                            similarBlocks.Add(block);
                        }
                        if (similarBlocks.Count >= 2)
                        {
                            StartCoroutine(DestroySimilarBlocks(similarBlocks, block));
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
                if (!block.GetComponent<Block>().IsMoving)
                {
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
                        StartCoroutine(DestroySimilarBlocks(similarBlocks, block));
                    }
                }
            }
        }
        isMergeRunning = false;
    }
    void RemovePosition(Vector3 element)
    {
        int index = System.Array.IndexOf(blockPositions, element);
        if (index != -1)
        {
            removedElements[element] = index;
            blockPositions[index] = Vector3.zero;
        }
        else
        {
            Debug.LogError("Element not found in the array");
        }
    }

    void AddPosition(Vector3 element)
    {
        if (removedElements.TryGetValue(element, out int index))
        {
            // Ensure the index is within bounds
            if (index >= 0 && index < blockPositions.Length)
            {
                blockPositions[index] = element;
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
            Debug.LogError($"Element not found in removed elements dictionary: {element}");
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
            .Where(t => Vector3.Distance(t.position, position) <= blockSize + blockSize / 5 && !t.gameObject.GetComponent<Block>().IsMoving)
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

    IEnumerator DestroySimilarBlocks(List<GameObject> similarBlocks, GameObject _block)
    {
        //var newestBlock = similarBlocks.OrderByDescending(b => b.GetComponent<Block>().CreatedAt).First();
        if (similarBlocks.Contains(lastBlock))
        {
            _block = lastBlock;
        }
        similarBlocks.Remove(_block);

        double myValue = _block.GetComponent<Block>().BlockValue;
        double myNewValue = IncreaseScore(myValue, similarBlocks.Count + 1);
        _block.GetComponent<Block>().BlockValue = myNewValue;
        double highestBlock = 0;
        double.TryParse(PlayerPrefs.GetString("x2HighestBlock", "2"), out highestBlock);
        if (myNewValue > highestBlock)
        {
            PlayerPrefs.SetString("x2HighestBlock", myNewValue.ToString());
        }
        _block.GetComponent<SpriteRenderer>().sortingOrder = 5;
        _block.GetComponent<Block>().OnMergeSound.Play();
        foreach (var b in similarBlocks)
        {
            AddPosition(b.GetComponent<Block>().HoldingPosition);
            b.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 1;
            b.transform.parent = null;
            LeanTween.move(b, _block.transform.position, 0.1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
            {
                DestroyImmediate(b);
            });
        }
        float blockXPosition = _block.transform.localPosition.x;
        var availablePositions = blockPositions.Where(p => p.x == blockXPosition).ToList();
        Vector3 newBlockPos = new Vector3();
        if (availablePositions.Count > 0)
        {
            newBlockPos = availablePositions[0];
        }
        ///Debug.Log("New Pos: " + newBlockPos + " actual pos: " + BlockParent.transform.GetChild(i).gameObject.transform.localPosition);
        if (newBlockPos != Vector3.zero && newBlockPos.y > _block.transform.localPosition.y)
        {
            RemovePosition(newBlockPos);
            AddPosition(_block.GetComponent<Block>().HoldingPosition);
            _block.GetComponent<Block>().HoldingPosition = newBlockPos;
            iterating = true;
            _block.GetComponent<Block>().IsMoving = true;
            yield return new WaitForSecondsRealtime(0.1f);
            LeanTween.move(_block, newBlockPos, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
            {
                iterating = false;
                _block.GetComponent<Block>().IsMoving = false;
                _block.GetComponent<SpriteRenderer>().sortingOrder = 1;
                isProgressSaved = false;
            });
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.1f);
            _block.GetComponent<SpriteRenderer>().sortingOrder = 1;
            isProgressSaved = false;
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
        float YPosOfLastBlock = 100;
        if (availablePositions.Count == 0)
        {
            foreach (Transform t in BlockParent.transform)
            {
                if (t.position.x == blockXPosition && t.position.y < YPosOfLastBlock)
                {
                    YPosOfLastBlock = t.position.y;
                    lastBlockInSelectedRow = t.gameObject;
                }
            }
        }
        return availablePositions.Count > 0 ? availablePositions[0] : new Vector3(-100, -100, -100);
    }


    void CalculateBlockPositions()
    {
        blockPositions = new Vector3[35];

        Rect safeArea = Screen.safeArea;
        Vector3 safeAreaTopLeft = mainCamera.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMax, mainCamera.nearClipPlane));

        float startX = safeAreaTopLeft.x + blockSize / GapSize + blockSize / 2f;
        float startY = safeAreaTopLeft.y - blockSize / GapSize - blockSize / 2f;
        startY -= PaddingFromTop;
        float partitionWidth = blockSize + blockSize / GapSize;
        float partitionHeight = blockSize + blockSize / GapSize;
        int count = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                float posX = startX + i * partitionWidth;
                float posY = startY - j * partitionHeight;
                blockPositions[count] = new Vector3(posX, posY, 0);
                count++;
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

    #region  Button_OnClicks
    public void OnClickHome()
    {
        SceneManager.LoadScene(0);
    }
    public void OnClickPlayAgain()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
