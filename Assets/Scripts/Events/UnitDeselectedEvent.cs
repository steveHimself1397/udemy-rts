using SFIT.RTS.EventBus;
using SFIT.RTS.Units;

namespace SFIT.RTS.Events {
    public struct UnitDeselectedEvent : IEvent {
        public ISelectable Unit { get; private set; }

        public UnitDeselectedEvent(ISelectable unit) {
            Unit = unit;
        }
    }
}
