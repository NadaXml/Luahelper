using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerizerLogic
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;

    public float t;

    public Vector3 getRectV3()
    {
        Vector3 final = (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        return final;
    }
}


public class NoiseGen
{
    private readonly Vector2 N_1 = new Vector2(233.34f, 851.73f);
    private readonly Vector2 N_12 = new Vector2(23.45f, 23.45f);

    public float flash = 0;

    private float fract(float tmp)
    {
        return tmp - Mathf.Floor(tmp);
    }

    public float _radius = 0.1f;

    private Vector2 fract(Vector2 tmp)
    {
        return new Vector2(tmp.x - Mathf.Floor(tmp.x), tmp.y - Mathf.Floor(tmp.y));
    }

    private float N21(Vector2 v2)
    {
        v2 = fract(v2 * N_1);
        float temp = Vector2.Dot(v2, v2 + N_12);
        v2.x += temp;
        v2.y += temp;
        return fract(v2.x * v2.y);
    }

    private Vector2 N22(Vector2 v2)
    {
        float n = N21(v2);
        v2.x += n;
        v2.y += n;
        return new Vector2(n, N21(v2));
    }

    public Vector2 getPoint(Vector2 v2)
    {
        Vector2 n = N22(v2);
        float x = Mathf.Sin(flash * n.x);
        float y = Mathf.Cos(flash * n.y);
        //用来防止点超出方格
        return new Vector2(x, y) * (0.5f - _radius);
    }

    //噪波函数（b站搬运工）
    //fixed N21(fixed2 p)

    //    {
    //    p = frac(p * fixed2(233.34, 851.73));
    //    p += dot(p, p + 23.45);
    //    return frac(p.x * p.y);
    //}

    ////噪波函数（b站搬运工）
    //fixed2 N22(fixed2 p)
    //{
    //    fixed n = N21(p);
    //    return fixed2(n, N21(p + n));
    //}

    ////点的圆心
    //fixed2 pointPos(fixed2 id)
    //{
    //    fixed2 n = N22(id);
    //    fixed x = sin(_pointNoise * n.x);
    //    fixed y = cos(_pointNoise * n.y);
    //    //用来防止点超出方格
    //    return fixed2(x, y) * (0.5 - _radius);
    //}
}