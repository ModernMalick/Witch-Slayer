using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.Player.Arsenal.Guns;
using UnityEngine;

namespace ModernMalick.Player.Arsenal
{
    [RequireComponent(typeof(Animator))]
    public class PlayerIK : MonoBehaviourExtended
    {
        [Component] private Animator _animator;

        private Transform _rightGrip;
        private Transform _leftGrip;

        public void SetWeapon(Gun gun)
        {
            _rightGrip = gun.RightGrip;
            _leftGrip = gun.LeftGrip;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_rightGrip) return;

            SetHandIK(AvatarIKGoal.RightHand, _rightGrip);

            if (_leftGrip)
                SetHandIK(AvatarIKGoal.LeftHand, _leftGrip);
            else
                ClearHandIK(AvatarIKGoal.LeftHand);
        }

        private void SetHandIK(AvatarIKGoal goal, Transform target)
        {
            _animator.SetIKPositionWeight(goal, 1f);
            _animator.SetIKRotationWeight(goal, 1f);

            _animator.SetIKPosition(goal, target.position);
            _animator.SetIKRotation(goal, target.rotation);
        }

        private void ClearHandIK(AvatarIKGoal goal)
        {
            _animator.SetIKPositionWeight(goal, 0f);
            _animator.SetIKRotationWeight(goal, 0f);
        }
    }
}