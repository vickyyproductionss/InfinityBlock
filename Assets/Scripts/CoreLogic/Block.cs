using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Souns/AudioSources")]
    public AudioSource OnDropSound;
    public AudioSource OnMergeSound;

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
        get
        {
            return _holdingPosition;
        }
        set
        {
            row = GameManager.instance.RowColumnDictionary[value].row;
            column = GameManager.instance.RowColumnDictionary[value].column;
            _holdingPosition = value;
        }
    }

    public int row;
    public int column;

    private bool _isMoving;
    public bool IsMoving
    {
        get { return _isMoving; }
        set { _isMoving = value; }
    }

    private bool _isMerging;
    public bool IsMerging
    {
        get { return _isMerging; }
        set { _isMerging = value; }
    }

    void Start()
    {
        CreatedAt = DateTime.Now;
    }

    void OnMouseDown()
    {
        if (GameManager.instance.isUsingPowerup && GameManager.instance.BlocksInPower.Count < 2 && GameManager.instance.activePowerIndex == 1)
        {
            if (!GameManager.instance.BlocksInPower.Contains(gameObject))
            {
                GameManager.instance.BlocksInPower.Add(gameObject);
                gameObject.transform.GetChild(1).gameObject.SetActive(true);
                CreatedAt = DateTime.Now;//Modified so that others merge into this assuming it as latest
            }
            if (GameManager.instance.BlocksInPower.Count == 2)
            {
                GameObject block1 = GameManager.instance.BlocksInPower[0];
                GameObject block2 = GameManager.instance.BlocksInPower[1];
                GameManager.instance.SwapTheseBlocks(block1, block2);
            }
        }
        else if (GameManager.instance.isUsingPowerup && GameManager.instance.BlocksInPower.Count < 1 && GameManager.instance.activePowerIndex == 2)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
            CreatedAt = DateTime.Now;//Modified so that others merge into this assuming it as latest
            GameManager.instance.SmashTheBlock(gameObject);
        }
    }


    public void OnBlockValueChanged(double num)
    {
        int suffixIndex = 0;
        int powerCount = (int)MathF.Log((float)num, 2);
        int colorIndex = powerCount % 10;
        GetComponent<SpriteRenderer>().color = colors[colorIndex];
        SetTrailColor(colors[colorIndex]);
        transform.GetChild(0).GetChild(0).GetComponent<TextColorChanger>().SetTextColorBasedOnBackground();

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

    void SetTrailColor(Color trailColor)
    {
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();
        // Create a new Gradient
        Gradient gradient = new Gradient();

        // Define the color keys for the gradient
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = trailColor; // Fully opaque white
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = trailColor; // Fully transparent white
        colorKeys[1].time = 1.0f;

        // Define the alpha keys for the gradient
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f; // Fully opaque
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 0.0f; // Fully transparent
        alphaKeys[1].time = 1.0f;

        // Assign the color and alpha keys to the gradient
        gradient.SetKeys(colorKeys, alphaKeys);

        // Assign the gradient to the trail renderer
        trailRenderer.colorGradient = gradient;
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0.0f, transform.localScale.x); // Width at the start of the trail
        widthCurve.AddKey(1.0f, transform.localScale.x); // Width at the end of the trail
        // Assign the width curve to the trail renderer
        //trailRenderer.widthCurve = widthCurve;

        // Optionally, set the overall width multiplier for the trail
        //trailRenderer.widthMultiplier = transform.localScale.x;
    }

}
