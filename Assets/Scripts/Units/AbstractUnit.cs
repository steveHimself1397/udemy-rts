using SFIT.RTS.EventBus;
using SFIT.RTS.Events;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace SFIT.RTS.Units {
    [RequireComponent(typeof(NavMeshAgent))]
    abstract public class AbstractUnit : MonoBehaviour, ISelectable, IMoveable {
        [SerializeField] private DecalProjector selectionIndicator;
        private NavMeshAgent agent;

        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start() {
            Bus<UnitSpawnedEvent>.Publish(new UnitSpawnedEvent(this));
        }

        public void Deselect() {
            if (selectionIndicator != null) {
                selectionIndicator.gameObject.SetActive(false);
            }
            Bus<UnitDeselectedEvent>.Publish(new UnitDeselectedEvent(this));

        }

        public void Select() {
            if (selectionIndicator != null) {
                selectionIndicator.gameObject.SetActive(true);
            }
            Bus<UnitSelectedEvent>.Publish(new UnitSelectedEvent(this));
        }

        public void MoveTo(Vector3 position) {
            agent.SetDestination(position);
        }
    }
}
