using UnityEngine;
using UnityEngine.AI;

namespace Interactables
{
    public class PathInteractable : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent characterAgent;
        
        public void OnMouseUp()
        {
            if (Camera.main == null)
            {
                return;
            }
            
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    Mathf.Infinity))
            {
                return;
            }
            
            characterAgent.SetDestination(hit.point);
        }
    }
}