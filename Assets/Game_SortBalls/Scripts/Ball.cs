using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public BallType CurrentBallType;
    public Sprite[] Balls;
    public SpriteRenderer BallSprite;

    public void InitBall(BallType type)
    {
        if (type == BallType.Blue)
        {
            CurrentBallType = BallType.Blue;
            BallSprite.sprite = Balls[0];
        }
        else if (type == BallType.Purple)
        {
            CurrentBallType = BallType.Purple;
            BallSprite.sprite = Balls[1];
        }
        else if (type == BallType.Green)
        {
            CurrentBallType = BallType.Green;
            BallSprite.sprite = Balls[2];
        }
        else if (type == BallType.Yellow)
        {
            CurrentBallType = BallType.Yellow;
            BallSprite.sprite = Balls[3];
        }
        else if (type == BallType.Orange)
        {
            CurrentBallType = BallType.Orange;
            BallSprite.sprite = Balls[4];
        }
        else if (type == BallType.Pink)
        {
            CurrentBallType = BallType.Pink;
            BallSprite.sprite = Balls[5];
        }
    }
}
public enum BallType
{
    Blue, Purple, Green, Yellow, Orange, Pink
}
