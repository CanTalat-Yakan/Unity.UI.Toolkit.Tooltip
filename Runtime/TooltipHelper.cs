using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEssentials
{
    [RequireComponent(typeof(UIDocument))]
    public class TooltipHelper : MonoBehaviour
    {
        private struct TooltipInfo
        {
            public string Text;
            public VisualElement Element;
        }

        private Vector2 _tooltipOffset = new Vector2(25, 5);
        private string _tooltipClass = "tooltip-label";

        private VisualElement _root;
        private Label _tooltipLabel;
        private Vector2 _lastMousePosition = Vector2.negativeInfinity;
        private VisualElement _lastTooltipElement;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AddTooltipHelpersToUIDocuments()
        {
            return;
            var documents = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var document in documents)
                if (document.GetComponent<TooltipHelper>() == null)
                    document.gameObject.AddComponent<TooltipHelper>();

        }

        private void Awake()
        {
            return;
            var document = GetComponent<UIDocument>();
            if (document == null)
            {
                enabled = false;
                return;
            }

            var tooltipUSS = ResourceLoader.LoadResource<StyleSheet>("UnityEssentials_USS_Tooltip");
            document.AddStyleSheet(tooltipUSS);

            _root = document.rootVisualElement;
            if (_root == null)
            {
                enabled = false;
                return;
            }

            _tooltipLabel = new Label();
            _tooltipLabel.AddToClassList(_tooltipClass);
            _tooltipLabel.style.position = Position.Absolute;
            _tooltipLabel.style.visibility = Visibility.Hidden;
            _root.Add(_tooltipLabel);
        }

        private void Update()
        {
            return;
            _root ??= GetComponent<UIDocument>().rootVisualElement;
            if (_root?.panel == null)
                return;

            Vector2 mousePosition = Input.mousePosition;
            if (mousePosition == _lastMousePosition && _tooltipLabel.style.visibility == Visibility.Visible)
                return; // No need to update if mouse hasn't moved

            _lastMousePosition = mousePosition;

            var tooltipInfo = GetTooltipInfoUnderPointer(_root.panel, mousePosition);
            _tooltipLabel.pickingMode = PickingMode.Ignore;
            _lastTooltipElement = tooltipInfo.Element;

            // Only show tooltip if the element is a descendant of this root
            if (_lastTooltipElement == null || !IsDescendantOfRoot(_lastTooltipElement))
            {
                _tooltipLabel.style.visibility = Visibility.Hidden;
                return;
            }

            if (tooltipInfo.Text != _tooltipLabel.text)
                _tooltipLabel.text = tooltipInfo.Text;

            if (string.IsNullOrEmpty(tooltipInfo.Text))
            {
                _tooltipLabel.style.visibility = Visibility.Hidden;
                return;
            }

            _tooltipLabel.style.visibility = Visibility.Visible;
            if (_lastTooltipElement != null)
                PositionTooltipAtElement(_lastTooltipElement);
        }

        private bool IsDescendantOfRoot(VisualElement element)
        {
            while (element != null)
            {
                if (element == _root)
                    return true;
                element = element.parent;
            }
            return false;
        }

        private void PositionTooltipAtElement(VisualElement element)
        {
            // Get the element's layout in panel space
            var layout = element.worldBound;

            // Convert to panel space if needed (worldBound is in panel space)
            float tooltipX = layout.xMin + _tooltipOffset.x;
            float tooltipY = layout.yMax + _tooltipOffset.y;

            // Clamp to panel bounds
            float maxX = _root.panel.visualTree.layout.width - _tooltipLabel.resolvedStyle.width - 5;
            float maxY = _root.panel.visualTree.layout.height - _tooltipLabel.resolvedStyle.height - 5;
            tooltipX = Mathf.Clamp(tooltipX, 0, maxX);
            tooltipY = Mathf.Clamp(tooltipY, 0, maxY);

            _tooltipLabel.style.left = tooltipX;
            _tooltipLabel.style.top = tooltipY;
        }

        private TooltipInfo GetTooltipInfoUnderPointer(IPanel panel, Vector2 screenPosition)
        {
            // Adjust y-coordinate for UI Toolkit's coordinate system
            screenPosition.y = Screen.height - screenPosition.y;

            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            VisualElement elementUnderPointer = panel.Pick(panelPosition);

            while (elementUnderPointer != null)
            {
                if (!string.IsNullOrEmpty(elementUnderPointer.tooltip))
                    return new TooltipInfo { Text = elementUnderPointer.tooltip, Element = elementUnderPointer };
                elementUnderPointer = elementUnderPointer.parent;
            }
            return new TooltipInfo { Text = string.Empty, Element = null };
        }
    }
}