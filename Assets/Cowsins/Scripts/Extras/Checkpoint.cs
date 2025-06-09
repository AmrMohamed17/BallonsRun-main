using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class Checkpoint : MonoBehaviour,ITriggerable
    {
        public UnityEvent OnTriggerCheckpoint;
        public void Trigger(GameObject target)
        {
            // Store a new checkpoint in the checkpoint manager
            OnTriggerCheckpoint?.Invoke();
            CheckPointManager.Checkpoint(this.transform); 
        }

        public void StayTrigger(GameObject target)
        {

        }
        public void ExitTrigger(GameObject target)
        {

        }
    }

}