using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.UI
{
    public class UtilityButton : MonoBehaviour, IUtilityButton
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI utilityText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (utilityText == null) utilityText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();

            ClearUtility();
        }

        public void SetUtilityText(string text)
        {
            utilityText.text = text;
            buttonImage.color = activeColor;
            SetInteractable(true);
        }

        public void ClearUtility()
        {
            utilityText.text = string.Empty;
            buttonImage.color = inactiveColor;
            SetInteractable(false);
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }

        public void AddClickListener(UnityEngine.Events.UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void RemoveClickListener(UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveListener(action);
        }
    }
} 