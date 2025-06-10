using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeDetector : MonoBehaviour
{
    public Tube currentTube;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect left mouse button click
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("ClickDetection")); // Raycast only on the "Detection" layer

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                GameManager.Instance.OnClickTube(currentTube);
            }
        }
    }
}
