using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextColorChanger : MonoBehaviour
{
    public TMP_Text textComponent; // Assign the Text component in the Inspector
    public SpriteRenderer backgroundImage; // Assign the background Image component in the Inspector
    public Image backgroundImage2; // Assign the background Image component in the Inspector
    public void SetTextColorBasedOnBackground2()
    {
        // Get the background color
        Color bgColor = backgroundImage2.color;

        // Calculate the luminance
        float luminance = (0.2126f * bgColor.r + 0.7152f * bgColor.g + 0.0722f * bgColor.b);

        // Set the text color based on luminance
        if (luminance > 0.5f)
        {
            // Light background -> Set text color to black
            textComponent.color = Color.black;
        }
        else
        {
            // Dark background -> Set text color to white
            textComponent.color = Color.white;
        }
    }

    public void SetTextColorBasedOnBackground()
    {
        // Get the background color
        Color bgColor = backgroundImage.color;

        // Calculate the luminance
        float luminance = (0.2126f * bgColor.r + 0.7152f * bgColor.g + 0.0722f * bgColor.b);

        // Set the text color based on luminance
        if (luminance > 0.5f)
        {
            // Light background -> Set text color to black
            textComponent.color = Color.black;
        }
        else
        {
            // Dark background -> Set text color to white
            textComponent.color = Color.white;
        }
    }
}
