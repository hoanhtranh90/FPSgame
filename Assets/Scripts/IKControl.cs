﻿using UnityEngine;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour {

    [SerializeField]
    private bool ikActive = false;
    [SerializeField]
    private Transform rightHandObj = null;
    [SerializeField]
    private Transform leftHandObj = null;
    [SerializeField]
    private Transform rightElbowObj = null;
    [SerializeField]
    private Transform leftElbowObj = null;
    [SerializeField]
    private Transform lookObj = null;

    private Animator animator;

    
    void Start() {
        animator = GetComponent<Animator>();
    }

   
    void OnAnimatorIK(int layerIndex) {
        
        if (ikActive) {
            // Set the look target position, if one has been assigned.
            if (lookObj != null) {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookObj.position);
            }
            // Set the right hand target position and rotation, if one has been assigned.
            if (rightHandObj != null) {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
            }
            // Set the left hand target position and rotation, if one has been assigned.
            if (leftHandObj != null) {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
            }
            // Set the right elbow target position and rotation, if one has been assigned.
            if (rightElbowObj != null) {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowObj.position);
            }
            // Set the left elbow target position and rotation, if one has been assigned.
            if (leftElbowObj != null) {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowObj.position);
            }
        } else {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            animator.SetLookAtWeight(0);
        }
    }

}
