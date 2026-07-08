using System;
using System.Collections;
using UnityEngine;

namespace PSS
{
    public enum BearPose { Idle, Aim, Ready, Swing, Impact, Victory, Cheer }

    /// Simple sprite-swap animator for the polar bear slugger.
    public class BearAnimator : MonoBehaviour
    {
        SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        static string SpriteFor(BearPose p) => p switch
        {
            BearPose.Idle    => "bear_idle",
            BearPose.Aim     => "bear_aim",
            BearPose.Ready   => "bear_ready",
            BearPose.Swing   => "bear_swing_hit",
            BearPose.Impact  => "bear_swing_impact",
            BearPose.Victory => "bear_victory",
            BearPose.Cheer   => "bear_cheer",
            _                => "bear_idle",
        };

        public void SetPose(BearPose p)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            var s = AssetDB.S(SpriteFor(p));
            if (s != null) sr.sprite = s;
        }

        /// Plays the swing, invokes onImpact at the contact frame, then settles.
        public void Swing(Action onImpact)
        {
            StopAllCoroutines();
            StartCoroutine(SwingRoutine(onImpact));
        }

        IEnumerator SwingRoutine(Action onImpact)
        {
            SetPose(BearPose.Ready);
            yield return new WaitForSeconds(0.05f);
            SetPose(BearPose.Swing);
            yield return new WaitForSeconds(0.04f);
            onImpact?.Invoke();
            SetPose(BearPose.Impact);
            yield return new WaitForSeconds(0.18f);
            SetPose(BearPose.Idle);
        }
    }
}
