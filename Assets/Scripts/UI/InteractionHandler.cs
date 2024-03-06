using Interactables;
using UnityEngine;

namespace UI
{
    public class InteractionHandler : ButtonElement
    {
        protected override void OnButtonClicked()
        {
            base.OnButtonClicked();
            
            if (Camera.main == null)
            {
                return;
            }

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    Mathf.Infinity))
            {
                return;
            }

            if (hit.collider.TryGetComponent(out AbstractInteractable interactable))
            {
                interactable.OnInteracted(hit);
            }
        }
    }
}