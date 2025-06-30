using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEssentials
{
    [RequireComponent(typeof(UIDocument))]
    public class TooltipHelper : MonoBehaviour
    {
        [Header("Tooltip Settings")]
        [SerializeField] private Vector2 tooltipOffset = new Vector2(15, -15);
        [SerializeField] private string tooltipClass = "tooltip-label";

        private VisualElement _root;
        private Label _tooltipLabel;
        private string _lastTooltipText = "";
        private Vector2 _lastMousePosition = Vector2.negativeInfinity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AddTooltipHelpersToUIDocuments()
        {
            var documents = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var document in documents)
                if (document.GetComponent<TooltipHelper>() == null)
                    document.gameObject.AddComponent<TooltipHelper>();

        }

        private void Awake()
        {
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
            _tooltipLabel.AddToClassList(tooltipClass);
            _tooltipLabel.style.position = Position.Absolute;
            _tooltipLabel.style.visibility = Visibility.Hidden;
            _root.Add(_tooltipLabel);
        }

        private void Update()
        {
            if (_root?.panel == null)
                return;

            Vector2 mousePosition = Input.mousePosition;
            if (mousePosition == _lastMousePosition && _tooltipLabel.style.visibility == Visibility.Visible)
                return; // No need to update if mouse hasn't moved

            _lastMousePosition = mousePosition;

            string tooltipText = GetTooltipUnderPointer(_root.panel, mousePosition);
            if (tooltipText != _lastTooltipText)
            {
                _lastTooltipText = tooltipText;
                _tooltipLabel.text = tooltipText;
            }

            if (string.IsNullOrEmpty(tooltipText))
            {
                _tooltipLabel.style.visibility = Visibility.Hidden;
                return;
            }

            _tooltipLabel.style.visibility = Visibility.Visible;
            PositionTooltip(mousePosition);
        }

        private void PositionTooltip(Vector2 screenPosition)
        {
            // Convert screen position to panel position
            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(_root.panel, screenPosition);

            float tooltipX = panelPosition.x + tooltipOffset.x;
            float tooltipY = Screen.height - panelPosition.y + tooltipOffset.y;

            // Clamp to panel bounds
            float maxX = _root.panel.visualTree.layout.width - _tooltipLabel.resolvedStyle.width - 5;
            float maxY = _root.panel.visualTree.layout.height - _tooltipLabel.resolvedStyle.height - 5;
            tooltipX = Mathf.Clamp(tooltipX, 0, maxX);
            tooltipY = Mathf.Clamp(tooltipY, 0, maxY);

            _tooltipLabel.style.left = tooltipX;
            _tooltipLabel.style.top = tooltipY;
        }

        private string GetTooltipUnderPointer(IPanel panel, Vector2 screenPosition)
        {
            // Adjust y-coordinate for UI Toolkit's coordinate system
            screenPosition.y = Screen.height - screenPosition.y;

            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            VisualElement elementUnderPointer = panel.Pick(panelPosition);

            while (elementUnderPointer != null)
            {
                if (!string.IsNullOrEmpty(elementUnderPointer.tooltip))
                    return elementUnderPointer.tooltip;
                elementUnderPointer = elementUnderPointer.parent;
            }
            return string.Empty;
        }
    }
}