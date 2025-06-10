using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public List<Transform> BallSpawnTransforms;
    public GameObject BallPrefab;
    public SpriteRenderer TubeSprite;

    public bool isMatched;

    public List<Ball> Balls = new List<Ball>();

    public void HighlightTube()
    {
        TubeSprite.color = Color.green;
    }
    public void UnhighlightTube()
    {
        TubeSprite.color = Color.white;
    }

    public void InitTube(List<BallType> balls)
    {
        for (int i = 0; i < balls.Count; i++)
        {
            GameObject ball = Instantiate(BallPrefab, BallSpawnTransforms[i].position, Quaternion.identity);
            ball.transform.SetParent(transform);
            ball.GetComponent<Ball>().InitBall(balls[i]);
            Balls.Add(ball.GetComponent<Ball>());
        }
    }

    public List<Ball> GetTopSimilarBalls()
    {
        Balls = Balls.OrderByDescending(ball => ball.transform.position.y).ToList();
        List<Ball> returnBalls = new List<Ball>();

        if (Balls.Count != 0)
        {
            returnBalls.Add(Balls[0]);

            for (int i = 1; i < Balls.Count; i++)
            {
                if (Balls[i].CurrentBallType == Balls[0].CurrentBallType)
                {
                    returnBalls.Add(Balls[i]);
                }
                else
                {
                    break;
                }
            }
        }
        return returnBalls;
    }
}
