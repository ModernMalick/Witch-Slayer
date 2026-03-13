using UnityEngine;

namespace ModernMalick.Player
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Pickup : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var pickedUp = TryPickup(other.gameObject);
            if(pickedUp) OnPickedUp(other.gameObject);
        }

        protected abstract bool TryPickup(GameObject other);

        protected virtual void OnPickedUp(GameObject other)
        {
            Destroy(gameObject);
        }
    }
}