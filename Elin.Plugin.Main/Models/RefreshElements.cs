using System.Collections.Generic;

namespace Elin.Plugin.Main.Models
{
    public readonly record struct RefreshElement
    {
        public RefreshElement(Card target, Color color)
        {
            Target = target;
            Color = color;
        }

        #region property

        public Card Target { get; }
        public Color Color { get; }

        #endregion
    }

    public class RefreshElements
    {
        public RefreshElements(Marker marker)
        {
            Marker = marker;
            EditableElements = new List<RefreshElement>();
        }

        public RefreshElements(Marker marker, List<RefreshElement> elements)
        {
            Marker = marker;
            EditableElements = elements;
        }


        #region property

        public Marker Marker { get; }

        private List<RefreshElement> EditableElements { get; }
        public IReadOnlyCollection<RefreshElement> Elements => EditableElements;

        #endregion

        #region function

        public void Add(RefreshElement element)
        {
            EditableElements.Add(element);
        }

        #endregion
    }
}
