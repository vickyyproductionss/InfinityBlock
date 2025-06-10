using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupOpenAnimation : MonoBehaviour
{
    void OnEnable()
    {
        // Start the animation when the popup is enabled
        StartCoroutine(OpenAnimation());
    }
    private IEnumerator OpenAnimation()
    {
        // Set the initial scale to zero
        transform.localScale = Vector3.zero;

        // Define the target scale and duration
        Vector3 targetScale = Vector3.one;
        float duration = 0.5f;
        float elapsedTime = 0f;

        // Animate the scale from zero to one
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the final scale is set to the target scale
        transform.localScale = targetScale;
    }
    void OnDisable()
    {
        // Reset the scale when the popup is disabled
        transform.localScale = Vector3.zero;
    }
}
