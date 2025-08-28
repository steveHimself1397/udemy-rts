using SFIT.RTS.EventBus;
using SFIT.RTS.Units;

namespace SFIT.RTS.Events {
    public struct UnitSelectedEvent : IEvent {
        public ISelectable Unit { get; private set; }

        public UnitSelectedEvent(ISelectable unit) {
            Unit = unit;
        }
    }
}
