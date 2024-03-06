using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ButtonElement : MonoBehaviour
    {
        [SerializeField] private Button button;
        
        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        protected virtual void OnButtonClicked() { }
    }
}