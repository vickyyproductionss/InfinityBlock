using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageBackground : MonoBehaviour
{
    public float GapSize;
    private float blockSize;
    public GameObject TopLine;
    public GameObject BottomLine;
    public GameObject[] spriteStrips;

    public Vector3 PlayScreenTopPoint;
    public Vector3 PlayScreenBottomPoint;

    public List<Vector3> blockPositions;
    float gapSize;
    void Start()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 safeAreaTopPoint = new Vector2(safeArea.center.x, safeArea.yMax);

        PlayScreenTopPoint = Camera.main.ScreenToWorldPoint(new Vector3(safeAreaTopPoint.x, safeAreaTopPoint.y, Camera.main.nearClipPlane));
        Vector3 safeAreaBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, Camera.main.nearClipPlane));
        Vector3 safeAreaBottomRight = Camera.main.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMin, Camera.main.nearClipPlane));
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
        PlayScreenBottomPoint = new Vector3(PlayScreenTopPoint.x, PlayScreenTopPoint.y - (7 * blockSize + 8 * blockSize / GapSize), PlayScreenTopPoint.z);
        PlayScreenTopPoint.y -= (blockSize/GapSize)/2;
        PlayScreenBottomPoint.y -= GameManager.instance.PaddingFromTop;
        PlayScreenTopPoint.y -= GameManager.instance.PaddingFromTop;
        TopLine.transform.position = PlayScreenTopPoint;
        BottomLine.transform.position = PlayScreenBottomPoint;
        CalculateBlockPositions();
    }
    void CalculateBlockPositions()
    {
        blockPositions = new List<Vector3>();

        Rect safeArea = Screen.safeArea;
        Vector3 safeAreaTopLeft = Camera.main.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMax, Camera.main.nearClipPlane));

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
        SetStrips();
    }


    void SetStrips()
    {

        // Ensure there are exactly 5 sprite strips
        if (spriteStrips.Length != 5)
        {
            Debug.LogError("There must be exactly 5 sprite strips.");
            return;
        }

        // Calculate the top and bottom points of the screen in world coordinates
        Camera mainCamera = Camera.main;
        Vector3 screenTopPoint = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, mainCamera.nearClipPlane));
        Vector3 screenBottomPoint = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, mainCamera.nearClipPlane));

        // Calculate the height of the screen in world units
        float screenHeight = screenTopPoint.y - screenBottomPoint.y;

        // Calculate the left and right points of the screen in world coordinates
        Vector3 screenLeftPoint = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, mainCamera.nearClipPlane));
        Vector3 screenRightPoint = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, mainCamera.nearClipPlane));

        // Calculate the total width available for the sprites
        float totalWidth = screenRightPoint.x - screenLeftPoint.x - 2 * gapSize;

        // Calculate the width of each sprite
        float spriteWidth = (totalWidth - 6 * gapSize) / 5;
        float spriteHeight = (PlayScreenTopPoint.y - PlayScreenBottomPoint.y);
        float YPos = (PlayScreenBottomPoint.y + PlayScreenTopPoint.y) / 2;

        // Set the position and scale of each sprite
        for (int i = 0; i < spriteStrips.Length; i++)
        {
            // Calculate the position of the sprite
            float posX = screenLeftPoint.x + gapSize + (spriteWidth / 2) + i * (spriteWidth + gapSize);
            Vector3 spritePosition = new Vector3(blockPositions[i*7].x, YPos, 0);

            // Set the position and scale
            spriteStrips[i].transform.position = spritePosition;
            spriteStrips[i].transform.localScale = new Vector3(spriteWidth, spriteHeight, 1);
        }

    }
}
