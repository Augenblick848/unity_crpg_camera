using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimationHandler : MonoBehaviour
{
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent navigationAgent;
        [SerializeField] private float velocityThreshold = 0.5f;
    
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private void Update()
        {
                animator.SetBool(IsMoving, navigationAgent.velocity.magnitude > velocityThreshold);
        }
}