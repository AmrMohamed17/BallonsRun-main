using UnityEngine;

namespace cowsins2D
{
    public interface ITriggerable  
    { 
        void Trigger(GameObject target);
        void StayTrigger(GameObject target);
        void ExitTrigger(GameObject target); 
    }
}
