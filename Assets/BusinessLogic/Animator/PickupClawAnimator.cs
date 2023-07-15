using Assets.Enums;
using Assets.Utils;
using UnityEngine;

namespace Assets.Animators
{
    class PickupClawAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        public void StartMovingForward()
        {
            _animator.SetBool(PickupClawAnimation.IS_SHOOTING.GetDescription(), true);
        }

        public void StartGrabbing()
        {
            _animator.SetBool(PickupClawAnimation.IS_GRABBING.GetDescription(), true);
            _animator.SetBool(PickupClawAnimation.IS_SHOOTING.GetDescription(), false);
        }

        public void StartMovingBackward()
        {
            _animator.SetBool(PickupClawAnimation.IS_NOT_GRABBING.GetDescription(), true);
            _animator.SetBool(PickupClawAnimation.IS_SHOOTING.GetDescription(), false);
            _animator.SetBool(PickupClawAnimation.IS_GRABBING.GetDescription(), false);
        }

        public void ReturningToHalalit()
        {
            _animator.SetBool(PickupClawAnimation.IS_NOT_GRABBING.GetDescription(), false);
        }
    }
}
