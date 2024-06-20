using UnityEngine;

public class ScreenEdgeColliders : MonoBehaviour
{
    void Start()
    {
        CreateEdgeColliders();
    }

    void CreateEdgeColliders()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main camera not found. Please assign the MainCamera tag to your camera.");
            return;
        }

        // Get the safe area in pixels
        Rect safeArea = Screen.safeArea;

        // Convert safe area corners from screen space to world space
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMin, cam.nearClipPlane));
        Vector3 bottomRight = cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMin, cam.nearClipPlane));
        Vector3 topLeft = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, safeArea.yMax, cam.nearClipPlane));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, safeArea.yMax, cam.nearClipPlane));

        // Create an empty GameObject to hold the colliders
        GameObject edgeCollidersParent = new GameObject("EdgeColliders");
        edgeCollidersParent.transform.position = Vector3.zero;

        // Create the edge colliders
        CreateEdgeCollider(edgeCollidersParent, bottomLeft, bottomRight); // Bottom
        CreateEdgeCollider(edgeCollidersParent, bottomRight, topRight); // Right
        CreateEdgeCollider(edgeCollidersParent, topRight, topLeft); // Top
        CreateEdgeCollider(edgeCollidersParent, topLeft, bottomLeft); // Left
    }

    void CreateEdgeCollider(GameObject parent, Vector2 pointA, Vector2 pointB)
    {
        EdgeCollider2D edgeCollider = new GameObject("EdgeCollider").AddComponent<EdgeCollider2D>();
        edgeCollider.transform.parent = parent.transform;
        edgeCollider.points = new Vector2[] { pointA, pointB };
    }
}
