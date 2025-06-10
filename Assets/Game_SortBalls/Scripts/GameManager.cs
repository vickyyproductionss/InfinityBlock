using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Starters
    public GameObject TubePrefab;
    public List<BallType> AllBalls = new List<BallType>();
    public List<GameObject> Tubes = new List<GameObject>();

    public List<LevelData> levels;
    public int currentLevelIndex;
    void Start()
    {
        if (PlayerPrefs.HasKey("STBHighestLevel"))
        {
            currentLevelIndex = int.Parse(PlayerPrefs.GetString("STBHighestLevel"));
        }
        else
        {
            currentLevelIndex = 0;
        }
        LevelData currentLevel = levels[currentLevelIndex];

        AllBalls = currentLevel.BallTypes.OrderBy(item => Random.value).ToList();

        //total tubes = 10/5 = 2 + 1
        int tubeCount = Mathf.CeilToInt((float)AllBalls.Count / 5) + currentLevel.MaxEmptyTubeCount;
        List<Vector3> tubePositions = CalculateTubePositions(tubeCount);

        for (int i = 0; i < tubeCount; i++)
        {
            GameObject tube = Instantiate(TubePrefab, tubePositions[i], Quaternion.identity);
            Tubes.Add(tube);

            if (i < tubeCount - currentLevel.MaxEmptyTubeCount)
            {
                List<BallType> balls = AllBalls.Take(5).ToList();
                AllBalls.RemoveRange(0, balls.Count);
                tube.GetComponent<Tube>().InitTube(balls);
            }
        }
    }
    public List<Vector3> CalculateTubePositions(int tubeCount)
    {
        List<Vector3> positions = new List<Vector3>();

        int rowCount = Mathf.CeilToInt((float)tubeCount / 5);

        for (int row = 0; row < rowCount; row++)
        {
            int tubesInRow = Mathf.Min(5, tubeCount - (row * 5));// 5, 6
            float yPosition = row == 0 && tubeCount <= 5 ? 0 : (row == 0 ? 1.75f : -1.75f);

            float startX = -(tubesInRow / 2) + (tubesInRow % 2 == 0 ? 0.5f : 0);

            for (int i = 0; i < tubesInRow; i++)
            {
                positions.Add(new Vector3(startX + i, yPosition, 0));
            }
        }

        return positions;
    }
    #endregion

    #region Tube Detection
    [HideInInspector] public Tube InitialTube = null;
    [HideInInspector] public Tube FinalTube = null;

    public float BallMoveSpeed = 5;
    public GameObject LevelCompletePanel;
    public bool isMovingABall;
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnClickTube(Tube tube)
    {
        if (tube.isMatched || isMovingABall)
        {
            if (isMovingABall)
            {
                Debug.Log("Moving Ball");
            }
            return;
        }
        // Case 1: No tube selected yet
        if (InitialTube == null)
        {
            InitialTube = tube;
            tube.HighlightTube();
            return;
        }

        // Case 2: Same tube clicked again
        if (InitialTube == tube)
        {
            InitialTube.UnhighlightTube();
            InitialTube = null; // Reset selection
            return;
        }

        // Case 3: Different tube clicked
        if (InitialTube != tube)
        {
            FinalTube = tube;
            InitialTube.UnhighlightTube(); // Unhighlight the initial tube
            MoveTheBallsIfPossible(); // Handle ball movement logic
        }
    }

    void MoveTheBallsIfPossible()
    {
        isMovingABall = true;
        if (FinalTube.Balls.Count >= 5 || InitialTube.GetTopSimilarBalls().Count == 0 || (FinalTube.Balls.Count > 0 && FinalTube.GetTopSimilarBalls()[0].CurrentBallType != InitialTube.GetTopSimilarBalls()[0].CurrentBallType))
        {
            Debug.Log("Movement not possible");
            InitialTube = null;
            isMovingABall = false;
            return;
        }

        int ballsToMoveCount = Mathf.Min(5 - FinalTube.Balls.Count, InitialTube.GetTopSimilarBalls().Count);
        StartCoroutine(MoveBallsCoroutine(InitialTube.GetTopSimilarBalls().Take(ballsToMoveCount).ToList()));
    }

    IEnumerator MoveBallsCoroutine(List<Ball> ballsToMove)
    {
        foreach (Ball ball in ballsToMove)
        {
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            CircleCollider2D cd = ball.GetComponent<CircleCollider2D>();
            SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();

            if (rb != null && cd != null && sr != null)
            {
                sr.sortingOrder = 99;
                rb.isKinematic = true;
                cd.isTrigger = true;
            }

            yield return MoveToPosition(ball.transform, InitialTube.transform.position + new Vector3(0, 2f, 0));

            yield return MoveToPosition(ball.transform, FinalTube.transform.position + new Vector3(0, 2, 0));

            if (rb != null && cd != null && sr != null)
            {
                rb.isKinematic = false;
                cd.isTrigger = false;
                sr.sortingOrder = 1;
            }

            InitialTube.Balls.Remove(ball);
            FinalTube.Balls.Add(ball);
        }
        InitialTube = null;
        if (FinalTube.GetTopSimilarBalls().Count == 5)
        {
            FinalTube.isMatched = true;
        }
        Tube[] allTubes = FindObjectsByType<Tube>(FindObjectsSortMode.None);

        bool finished = true;
        foreach (var t in allTubes)
        {
            if (t.GetTopSimilarBalls().Count == 5 || t.Balls.Count == 0)
            {

            }
            else
            {
                finished = false;
            }
        }
        isMovingABall = false;
        if (finished)
        {
            yield return new WaitForSeconds(2);
            LevelCompletePanel.SetActive(true);
            Debug.Log("Game is completed");
            currentLevelIndex += 1;
            if(currentLevelIndex >= levels.Count)
            {
                currentLevelIndex = 0; // Reset to first level if all levels are completed
            }
            PlayerPrefs.SetString("STBHighestLevel", currentLevelIndex.ToString());
        }
    }

    IEnumerator MoveToPosition(Transform ballTransform, Vector3 targetPosition)
    {
        while (Vector3.Distance(ballTransform.position, targetPosition) > 0.01f)
        {
            ballTransform.position = Vector3.MoveTowards(ballTransform.position, targetPosition, BallMoveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
