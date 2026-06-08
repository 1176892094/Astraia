using Astraia.Core;

namespace Astraia
{
    internal sealed class UIQueue
    {
        private UIPanel current;
        private UIPanel reverse;

        public void Push(UIPanel panel)
        {
            if (current == panel)
            {
                return;
            }

            if (current != null && current.owner)
            {
                UIGroup.SetActive(current, false);
                reverse = current;
            }

            current = panel;
            UIGroup.SetActive(current, true);
        }

        public void Back(UIPanel panel)
        {
            if (reverse == null)
            {
                return;
            }

            if (current != panel)
            {
                return;
            }

            var forward = current;
            if (current != null && current.owner)
            {
                UIGroup.SetActive(current, false);
            }

            current = reverse;
            reverse = forward;
            UIGroup.SetActive(current, true);
        }

        public void Clear()
        {
            if (current != null && current.owner)
            {
                UIGroup.SetActive(current, false);
            }

            reverse = null;
            current = null;
        }
    }
}