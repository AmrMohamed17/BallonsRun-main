using UnityEngine;
namespace cowsins2D
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        public string interactText;


        /// <summary>
        /// Override this on your custom Interactables
        /// </summary>
        /// <param name="source">Who interacted with this interactable. ( The Player )</param>
        public virtual void Interact(InteractionManager source)
        {
            print(source.name);
        }
    }

}