using UnityEngine;
using UnityEngine.AI;

namespace Interactables
{
    public class PathInteractable : AbstractInteractable
    {
        [SerializeField] private NavMeshAgent characterAgent;
        
        public override void OnInteracted(RaycastHit hit)
        {
            characterAgent.SetDestination(hit.point);
        }
    }
}