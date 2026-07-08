using UnityEngine;

namespace PSS
{
    public enum HitQuality { Bad, Good, Perfect }

    /// Oscillating power meter + tap-to-hit launch. HUD reads Power/Aiming to draw the bar.
    public class LaunchController : MonoBehaviour
    {
        public bool  Aiming { get; private set; }
        public float Power  { get; private set; }   // 0..1 current meter value

        float _t;

        public void BeginAiming()
        {
            Aiming = true;
            _t = 0f;
            Power = 0f;
        }

        void Update()
        {
            if (!Aiming) return;

            _t += Time.deltaTime * GameConfig.PowerBarSpeed;
            Power = Mathf.PingPong(_t, 1f);

            if (GotTap())
                Hit();
        }

        static bool GotTap()
        {
            if (Input.GetMouseButtonDown(0)) return true;
            if (Input.GetKeyDown(KeyCode.Space)) return true;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
            return false;
        }

        void Hit()
        {
            Aiming = false;
            float p = Power;

            HitQuality q;
            if (p >= 0.80f && p <= 0.98f) q = HitQuality.Perfect;
            else if (p >= 0.52f)          q = HitQuality.Good;
            else                          q = HitQuality.Bad;

            float qMult = q switch
            {
                HitQuality.Perfect => GameConfig.PerfectMult,
                HitQuality.Good    => GameConfig.NormalMult,
                _                  => GameConfig.BadMult,
            };

            var gm = GameManager.Instance;
            float batMult = GameConfig.BatMultiplier(gm.UpgradeLevel(GameConfig.Upgrade.BatPower));
            float speed = GameConfig.BaseLaunchSpeed * (0.55f + 0.55f * p) * qMult * batMult;
            speed = Mathf.Min(speed, GameConfig.MaxSpeed);

            // Stronger hits launch on a flatter, longer arc.
            float angle = Mathf.Lerp(GameConfig.MaxAngle, GameConfig.MinAngle + 2f, p);

            gm.ConfirmLaunch(speed, angle, q);
        }
    }
}
