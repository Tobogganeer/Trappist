using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Functions
{
    public static Vector3 Lissajous(float time, float amplitude, float period)
    {
        return new Vector3(Mathf.Sin(time), amplitude * Mathf.Sin(period * time * Mathf.PI));
    }

    public class DampedVector
    {
        // https://www.youtube.com/watch?v=KPoeNZZ6H4s

        private Vector3 xp;
        private Vector3 y, yd;
        //private float w, z, d, k1, k2, k3;
        private float k1, k2, k3;

        public Vector3 XP => xp;

        public DampedVector(Vector3 startPos, float fSpeed = 0.5f, float zDamping = 0.15f, float rAcceleration = 2f)
        {
            /*
            w = 2f * Mathf.PI * f;
            this.z = z;
            d = w * Mathf.Sqrt(Mathf.Abs(z * z - 1));
            k1 = z / (Mathf.PI * f);
            k2 = 1f / (w * w);
            k3 = r * z / w;
            */
            k1 = zDamping / (Mathf.PI * fSpeed);
            k2 = 1f / ((2 * Mathf.PI * fSpeed) * (2 * Mathf.PI * fSpeed));
            k3 = rAcceleration * zDamping / (2 * Mathf.PI * fSpeed);

            xp = startPos;
            y = startPos;
            yd = Vector3.zero;
        }

        public Vector3 Update(float dt, Vector3 x, Vector3? xd = null)
        {
            if (xd == null)
            {
                xd = (x - xp) / dt;
                xp = x;
            }

            #region Old
            /*
            float k1Stable, k2Stable;
            if (w * dt < z)
            {
                k1Stable = k1;
                k2Stable = Mathf.Max(k2, dt * dt / 2f, dt * k1);
            }
            else
            {
                float t1 = Mathf.Exp(-z * w * dt);
                float alpha = 2 * t1 * (z <= 1f ? Mathf.Cos(dt * d) : Mathf.Acos(dt * d));
                float beta = t1 * t1;
                float t2 = dt / (1f + beta - alpha);
                k1Stable = (1f - beta) * t2;
                k2Stable = dt * t2;
            }
            */
            #endregion

            float k2Stable = Mathf.Max(k2, dt * dt / 2f, dt * k1 / 2f, dt * k1);

            y = y + dt * yd;
            //yd = yd + dt * (x + k3 * xd.Value - y - k1Stable * yd) / k2Stable;
            yd = yd + dt * (x + k3 * xd.Value - y - k1 * yd) / k2Stable;
            return y;
        }
    }
}
