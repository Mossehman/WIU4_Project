using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thanks to S�bastien Rombauts for simplex noise algorithm, ported from C++ to C#
/// <br>(I understand half of whatever this is :skull:)</br>
/// <br></br>
/// <br></br>
/// https://github.com/SRombauts/SimplexNoise/blob/master/src/SimplexNoise.cpp
/// </summary>

[CreateAssetMenu(fileName = "Simplex Noise", menuName = "Noise/Simplex Noise")]
public class SimplexNoise : BaseNoise
{
    private int Hash(int value)
    {
        value = (value << 13) ^ value ^ seed;
        return (value * (value * value * 15731 + 789221) + 1376312589) & 0x7fffffff;
    }

    private float Gradient(int hash, float x)
    {
        int h = hash & 0x0F;
        float grad = 1.0f + (h & 7);

        if ((h & 8) != 0) grad = -grad;
        return grad * x;
    }

    private float Gradient(int hash, float x, float y)
    {
        int h = hash & 0x3F;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;

        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }

    private float Gradient(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
    }


    public override float GenerateNoise(float x)
    {
        float n0, n1;
        int i0 = Mathf.FloorToInt(x);
        int i1 = i0 + 1;

        float x0 = x - i0;
        float x1 = x0 - 1.0f;

        float t0 = 1.0f - x0 * x0;
        t0 *= t0;

        n0 = t0 * t0 * Gradient(Hash(i0), x0);

        float t1 = 1.0f - x1 * x1;

        t1 *= t1;
        n1 = t1 * t1 * Gradient(Hash(i1), x1);

        return (0.395f * (n0 + n1) + 1.0f) * 0.5f;
    }

    public override float GenerateNoise(float x, float y)
    {
        float n0, n1, n2;   // Noise contributions from the three corners

        // Skewing/Unskewing factors for 2D
        const float F2 = 0.366025403f;  // F2 = (sqrt(3) - 1) / 2
        const float G2 = 0.211324865f;  // G2 = (3 - sqrt(3)) / 6   = F2 / (1 + 2 * K)

        // Skew the input space to determine which simplex cell we're in
        float s = (x + y) * F2;  // Hairy factor for 2D
        float xs = x + s;
        float ys = y + s;
        int i = Mathf.FloorToInt(xs);
        int j = Mathf.FloorToInt(ys);

        // Unskew the cell origin back to (x,y) space
        float t = (float)(i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;  // The x,y distances from the cell origin
        float y0 = y - Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        int i1, j1;  // Offsets for second (middle) corner of simplex in (i,j) coords
        if (x0 > y0)
        {   // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            i1 = 1;
            j1 = 0;
        }
        else
        {   // upper triangle, YX order: (0,0)->(0,1)->(1,1)
            i1 = 0;
            j1 = 1;
        }

        // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
        // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
        // c = (3-sqrt(3))/6

        float x1 = x0 - i1 + G2;            // Offsets for middle corner in (x,y) unskewed coords
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2;   // Offsets for last corner in (x,y) unskewed coords
        float y2 = y0 - 1.0f + 2.0f * G2;

        // Work out the hashed gradient indices of the three simplex corners
        int gi0 = Hash(i + Hash(j));
        int gi1 = Hash(i + i1 + Hash(j + j1));
        int gi2 = Hash(i + 1 + Hash(j + 1));

        // Calculate the contribution from the first corner
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0.0f)
        {
            n0 = 0.0f;
        }
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Gradient(gi0, x0, y0);
        }

        // Calculate the contribution from the second corner
        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0.0f)
        {
            n1 = 0.0f;
        }
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Gradient(gi1, x1, y1);
        }

        // Calculate the contribution from the third corner
        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0.0f)
        {
            n2 = 0.0f;
        }
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Gradient(gi2, x2, y2);
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [-1,1].
        return (45.23065f * (n0 + n1 + n2) + 1.0f) * 0.5f;
    }

    public override float GenerateNoise(float x, float y, float z)
    {
        float n0, n1, n2, n3; // Noise contributions from the four corners

        // Skewing/Unskewing factors for 3D
        const float F3 = 1.0f / 3.0f;
        const float G3 = 1.0f / 6.0f;

        // Skew the input space to determine which simplex cell we're in
        float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
        int i = Mathf.FloorToInt(x + s);
        int j = Mathf.FloorToInt(y + s);
        int k = Mathf.FloorToInt(z + s);
        float t = (i + j + k) * G3;
        float X0 = i - t; // Unskew the cell origin back to (x,y,z) space
        float Y0 = j - t;
        float Z0 = k - t;
        float x0 = x - X0; // The x,y,z distances from the cell origin
        float y0 = y - Y0;
        float z0 = z - Z0;

        // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
        // Determine which simplex we are in.
        int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
        int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
        if (x0 >= y0)
        {
            if (y0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; // X Y Z order
            }
            else if (x0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; // X Z Y order
            }
            else
            {
                i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; // Z X Y order
            }
        }
        else
        { // x0<y0
            if (y0 < z0)
            {
                i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; // Z Y X order
            }
            else if (x0 < z0)
            {
                i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; // Y Z X order
            }
            else
            {
                i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; // Y X Z order
            }
        }

        // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
        // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
        // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
        // c = 1/6.
        float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;
        float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
        float y2 = y0 - j2 + 2.0f * G3;
        float z2 = z0 - k2 + 2.0f * G3;
        float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
        float y3 = y0 - 1.0f + 3.0f * G3;
        float z3 = z0 - 1.0f + 3.0f * G3;

        // Work out the hashed gradient indices of the four simplex corners
        int gi0 = Hash(i + Hash(j + Hash(k)));
        int gi1 = Hash(i + i1 + Hash(j + j1 + Hash(k + k1)));
        int gi2 = Hash(i + i2 + Hash(j + j2 + Hash(k + k2)));
        int gi3 = Hash(i + 1 + Hash(j + 1 + Hash(k + 1)));

        // Calculate the contribution from the four corners
        float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
        if (t0 < 0)
        {
            n0 = 0.0f;
        }
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Gradient(gi0, x0, y0, z0);
        }
        float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
        if (t1 < 0)
        {
            n1 = 0.0f;
        }
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Gradient(gi1, x1, y1, z1);
        }
        float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
        if (t2 < 0)
        {
            n2 = 0.0f;
        }
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Gradient(gi2, x2, y2, z2);
        }
        float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
        if (t3 < 0)
        {
            n3 = 0.0f;
        }
        else
        {
            t3 *= t3;
            n3 = t3 * t3 * Gradient(gi3, x3, y3, z3);
        }
        // Add contributions from each corner to get the final noise value.
        // The result is scaled to stay just inside [0, 1]
        return (32.0f * (n0 + n1 + n2 + n3) + 1.0f) * 0.5f;

    }
}
