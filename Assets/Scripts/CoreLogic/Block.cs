using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class Block : MonoBehaviour
{
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "P", "E", "Z", "Y" };
    public TMP_Text blockText;
    public List<Color> colors;

    /// <summary>
    /// Block Value
    /// </summary>
    private double _blockValue;
    public double BlockValue
    {
        get { return _blockValue; }
        set { _blockValue = value; OnBlockValueChanged(value); }
    }

    /// <summary>
    /// Created At
    /// </summary>
    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = value; }
    }

    /// <summary>
    /// Holding Position
    /// </summary>
    private Vector3 _holdingPosition;
    public Vector3 HoldingPosition
    {
        get { return _holdingPosition;}
        set { _holdingPosition = value;}
    }

    private bool _isMoving;
    public bool IsMoving
    {
        get { return _isMoving; }
        set { _isMoving = value; }
    }

    void Start()
    {
        CreatedAt = DateTime.Now;
    }


    public void OnBlockValueChanged(double num)
    {
        int suffixIndex = 0;
        int powerCount = (int)MathF.Log((float)num, 2);
        int colorIndex = powerCount%10;
        GetComponent<SpriteRenderer>().color = colors[colorIndex];

        // Continue to next suffix only if num is 10000 or greater
        while (num >= 10000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        // If number is extremely large, handle it using scientific notation
        if (num >= 10000 && suffixIndex == suffixes.Length - 1)
        {

        }
        string newtext = num.ToString("0.#") + suffixes[suffixIndex];
        blockText.text = newtext;
    }

}
