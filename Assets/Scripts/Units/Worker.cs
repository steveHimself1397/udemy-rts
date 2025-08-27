using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace SFIT.RTS.Units {

    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable {
        [SerializeField] private Transform target;
        [SerializeField] private DecalProjector selectionIndicator;
        private NavMeshAgent agent;


        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update() {
            if (target != null) {
                agent.SetDestination(target.position);
            }
        }
        public void Deselect() {
            selectionIndicator.gameObject.SetActive(false);
        }

        public void Select() {
            selectionIndicator.gameObject.SetActive(true);
        }
    }
}
