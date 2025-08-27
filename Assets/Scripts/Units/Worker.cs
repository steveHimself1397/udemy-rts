using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace SFIT.RTS.Units {

    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable, IMoveable {
        [SerializeField] private DecalProjector selectionIndicator;
        private NavMeshAgent agent;


        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Deselect() {
            selectionIndicator.gameObject.SetActive(false);
        }

        public void Select() {
            selectionIndicator.gameObject.SetActive(true);
        }

        public void MoveTo(Vector3 position) {
            agent.SetDestination(position);
        }
    }
}
