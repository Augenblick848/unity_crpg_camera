using UnityEngine;

namespace Interactables
{
    public abstract class AbstractInteractable : MonoBehaviour
    {
        public abstract void OnInteracted(RaycastHit hit);
    }
}