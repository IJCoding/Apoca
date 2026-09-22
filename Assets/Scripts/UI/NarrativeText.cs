using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class NarrativeText : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    [Tooltip("Displays the narrative text.")]
    private TMP_Text text;

    public void SetText(
        string content)
    {
        if (text == null)
        {
            Debug.LogWarning(
                $"NarrativeText '{name}' has no TMP_Text assigned.",
                this);

            return;
        }

        text.text = content;
    }
}