using UnityEngine;

namespace PSS
{
    /// The launched penguin. Rigidbody2D arcade flight with drag, bounce and item effects.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PenguinController : MonoBehaviour
    {
        public Vector3 StartPos = new Vector3(-6.4f, -3.3f, 0);

        Rigidbody2D rb;
        SpriteRenderer sr;
        PhysicsMaterial2D mat;

        bool flying;
        float runTime;
        float stopTimer;
        float boostTimer;
        float boostSpeed;
        float spinTimer;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            mat = new PhysicsMaterial2D("penguin") { friction = 0.2f, bounciness = 0.6f };
            var col = GetComponent<Collider2D>();
            if (col != null) col.sharedMaterial = mat;
            rb.freezeRotation = true;
            ResetToStart();
        }

        public void ResetToStart()
        {
            flying = false;
            runTime = stopTimer = boostTimer = spinTimer = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = StartPos;
            transform.rotation = Quaternion.identity;
            if (sr != null) { sr.sprite = AssetDB.S("penguin_idle"); sr.flipX = false; }
        }

        public void Launch(float speed, float angleDeg)
        {
            var gm = GameManager.Instance;
            int glide  = gm.UpgradeLevel(GameConfig.Upgrade.Glide);
            int bounce = gm.UpgradeLevel(GameConfig.Upgrade.Bounce);

            mat.bounciness = GameConfig.BounceRestitution(bounce);
            var col = GetComponent<Collider2D>();
            if (col != null) col.sharedMaterial = mat;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = GameConfig.Gravity / 9.81f;
            rb.linearDamping = GameConfig.GlideDrag(glide);
            transform.position = StartPos;

            float a = angleDeg * Mathf.Deg2Rad;
            rb.linearVelocity = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed;

            flying = true;
            runTime = stopTimer = 0f;
            if (sr != null) sr.sprite = AssetDB.S("penguin_fly_fast");
            Fx.Spawn("fx_star_burst", transform.position, 0.9f);
        }

        void FixedUpdate()
        {
            if (!flying) return;

            // rocket boost keeps a minimum forward speed
            if (boostTimer > 0f)
            {
                boostTimer -= Time.fixedDeltaTime;
                if (rb.linearVelocity.x < boostSpeed)
                    rb.linearVelocity = new Vector2(boostSpeed, rb.linearVelocity.y);
            }

            // clamp max speed
            var v = rb.linearVelocity;
            if (v.magnitude > GameConfig.MaxSpeed)
                rb.linearVelocity = v.normalized * GameConfig.MaxSpeed;

            runTime += Time.fixedDeltaTime;

            float dist = (transform.position.x - StartPos.x) * GameConfig.MetersPerUnit;
            float h = Mathf.Max(0f, (transform.position.y - GameConfig.GroundY)) * GameConfig.MetersPerUnit;
            GameManager.Instance.ReportFlight(dist, h);

            // stop detection
            bool grounded = transform.position.y <= GameConfig.GroundY + 0.55f;
            if (grounded && rb.linearVelocity.magnitude < GameConfig.StopSpeed)
            {
                stopTimer += Time.fixedDeltaTime;
                if (stopTimer > 0.45f) EndFlight();
            }
            else stopTimer = 0f;

            if (runTime > 45f) EndFlight();
        }

        void Update()
        {
            if (!flying) return;
            // face velocity direction
            var v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.5f)
            {
                float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                ang = Mathf.Clamp(ang, -60f, 60f);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, ang), 12f * Time.deltaTime);
            }
            if (spinTimer > 0f)
            {
                spinTimer -= Time.deltaTime;
                if (sr != null && AssetDB.Has("penguin_spin")) sr.sprite = AssetDB.S("penguin_spin");
            }
            else if (sr != null)
            {
                sr.sprite = AssetDB.S("penguin_fly_fast");
            }
        }

        void EndFlight()
        {
            if (!flying) return;
            flying = false;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            transform.rotation = Quaternion.identity;
            if (sr != null) sr.sprite = AssetDB.S("penguin_splash");
            Fx.Spawn("fx_snow_puff", transform.position, 1.1f);
            GameManager.Instance.EndRun();
        }

        // ---- item effects (called by WorldItem) ----
        public bool IsFlying => flying;
        public Rigidbody2D Body => rb;

        public void CollectCoin(int value)
        {
            GameManager.Instance.AddCoins(value);
            Fx.Spawn("fx_coin_sparkle", transform.position, 0.6f);
        }

        public void BounceBoost(float upSpeed, float fwdBonus)
        {
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(v.x + fwdBonus, Mathf.Max(v.y, 0f) + upSpeed);
            spinTimer = 0.5f;
            GameManager.Instance.RegisterBoosterHit();
            Fx.Spawn("fx_impact_burst", transform.position, 0.9f);
        }

        public void RocketBoost(float addSpeed, float duration)
        {
            boostSpeed = Mathf.Min(rb.linearVelocity.x + addSpeed, GameConfig.MaxSpeed);
            boostTimer = duration;
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(boostSpeed, v.y + 1.5f);
            GameManager.Instance.RegisterBoosterHit();
            Fx.Spawn("fx_speed_lines", transform.position, 1.2f);
        }

        public void SlowDown(float factor)
        {
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(v.x * factor, v.y);
            GameManager.Instance.BreakCombo();
            Fx.Spawn("fx_ice_crystal", transform.position, 0.8f);
        }
    }
}
