using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveVisualDefinition", menuName = "Game/Interactive Visuals/Interactive Visual Definition")]
public class InteractiveVisualDefinition : ScriptableObject
{
    [Serializable]
    public class Hotspot
    {
        [SerializeField] private string displayName;
        [SerializeField] private LocationDefinition location;
        [SerializeField] private List<Vector2> points = new List<Vector2>();

        public string DisplayName => !string.IsNullOrWhiteSpace(displayName) ? displayName : location != null ? location.DisplayName : "Unnamed Hotspot";
        public LocationDefinition Location => location;
        public IReadOnlyList<Vector2> Points => points;

#if UNITY_EDITOR
        public string EditorDisplayName => displayName;
        public List<Vector2> EditorPoints => points;
        public void EditorSetLocation(LocationDefinition value) => location = value;
        public void EditorSetDisplayName(string value) => displayName = value;
#endif
    }

    [SerializeField, Tooltip("The image displayed by this interactive visual.")]
    private Sprite visual;
    [SerializeField] private List<Hotspot> hotspots = new List<Hotspot>();

    public Sprite Visual => visual;
    public IReadOnlyList<Hotspot> Hotspots => hotspots;

#if UNITY_EDITOR
    public List<Hotspot> EditorHotspots => hotspots;
    public void EditorSetVisual(Sprite value) => visual = value;
#endif
}
