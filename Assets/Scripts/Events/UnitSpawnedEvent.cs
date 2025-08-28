using SFIT.RTS.EventBus;
using SFIT.RTS.Units;

namespace SFIT.RTS.Events {
    public struct UnitSpawnedEvent : IEvent {
        public AbstractUnit Unit { get; private set; }

        public UnitSpawnedEvent(AbstractUnit unit) {
            Unit = unit;
        }
    }
}
