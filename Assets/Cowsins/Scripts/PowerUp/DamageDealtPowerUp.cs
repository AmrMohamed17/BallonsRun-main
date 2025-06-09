using UnityEngine;

namespace cowsins2D
{
    public class DamageDealtPowerUp : PowerUp
    {
        [SerializeField] private float valueAdded;

        // this method gets called when the power up is triggered
        public override void TriggerAction(GameObject target)
        {
            PlayerMultipliers.Instance.damageModifier += valueAdded;
            base.TriggerAction(target);
        }
    }

}