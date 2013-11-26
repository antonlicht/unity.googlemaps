
using System;
using UnityEngine;

public class S2
{
    const int kFaceBits = 3;
    const int kNumFaces = 6;
    const int kMaxCellLevel = 30;
    const int kMaxLevel = kMaxCellLevel;
    const int kMaxSize = 1 << kMaxLevel;
    const int kPosBits = 2 * kMaxLevel + 1;

    const int kSwapMask = 0x01;
    const int kInvertMask = 0x02;
    const int kLookupBits = 4;

    static ushort[] lookup_pos = new ushort[1 << (2 * kLookupBits + 2)];
    static ushort[] lookup_ij = new ushort[1 << (2 * kLookupBits + 2)];

    private static bool bInit;
    private delegate void DelGetBits(int k);

    private static readonly int[,] kPosToIJ = new int[4, 4] {
        // 0  1  2  3
        {  0, 1, 3, 2 },    // canonical order:    (0,0), (0,1), (1,1), (1,0)
        {  0, 2, 3, 1 },    // axes swapped:       (0,0), (1,0), (1,1), (0,1)
        {  3, 2, 0, 1 },    // bits inverted:      (1,1), (1,0), (0,0), (0,1)
        {  3, 1, 0, 2 },    // swapped & inverted: (1,1), (0,1), (0,0), (1,0)
    };

    private static readonly int[] kPosToOrientation = { kSwapMask, 0, 0, kInvertMask + kSwapMask };

    static void InitLookupCell(int level, int i, int j, int orig_orientation, int pos, int orientation)
    {
        if (level == kLookupBits)
        {
            int ij = (i << kLookupBits) + j;
            lookup_pos[(ij << 2) + orig_orientation] = (ushort)((pos << 2) + orientation);
            lookup_ij[(pos << 2) + orig_orientation] = (ushort)((ij << 2) + orientation);
        }
        else
        {
            level++;
            i <<= 1;
            j <<= 1;
            pos <<= 2;
            InitLookupCell(level, i + (kPosToIJ[orientation, 0] >> 1), j + (kPosToIJ[orientation, 0] & 1), orig_orientation,
                           pos, orientation ^ kPosToOrientation[0]);
            InitLookupCell(level, i + (kPosToIJ[orientation, 1] >> 1), j + (kPosToIJ[orientation, 1] & 1), orig_orientation,
                           pos + 1, orientation ^ kPosToOrientation[1]);
            InitLookupCell(level, i + (kPosToIJ[orientation, 2] >> 1), j + (kPosToIJ[orientation, 2] & 1), orig_orientation,
                           pos + 2, orientation ^ kPosToOrientation[2]);
            InitLookupCell(level, i + (kPosToIJ[orientation, 3] >> 1), j + (kPosToIJ[orientation, 3] & 1), orig_orientation,
                           pos + 3, orientation ^ kPosToOrientation[3]);
        }

    }

    static void Init()
    {
        InitLookupCell(0, 0, 0, 0, 0, 0);
        InitLookupCell(0, 0, 0, kSwapMask, 0, kSwapMask);
        InitLookupCell(0, 0, 0, kInvertMask, 0, kInvertMask);
        InitLookupCell(0, 0, 0, kSwapMask | kInvertMask, 0, kSwapMask | kInvertMask);
    }

    static void MaybeInit()
    {

        if (!bInit) { Init(); bInit = true; };
    }

    static ulong FromFaceIJ(int face, int i, int j)
    {
        MaybeInit();
        uint[] n = { 0, (uint)(face << (kPosBits - 33)) };

        // Alternating faces have opposite Hilbert curve orientations; this
        // is necessary in order for all faces to have a right-handed
        // coordinate system.
        int bits = (face & kSwapMask);

        // Each iteration maps 4 bits of "i" and "j" into 8 bits of the Hilbert
        // curve position.  The lookup table transforms a 10-bit key of the form
        // "iiiijjjjoo" to a 10-bit value of the form "ppppppppoo", where the
        // letters [ijpo] denote bits of "i", "j", Hilbert curve position, and
        // Hilbert curve orientation respectively.

        DelGetBits getBits = k =>
            {

                int mask = (1 << kLookupBits) - 1;
                bits += ((i >> (k * kLookupBits)) & mask) << (kLookupBits + 2);
                bits += ((j >> (k * kLookupBits)) & mask) << 2;
                bits = lookup_pos[bits];
                n[k >> 2] |= (uint)((bits >> 2) << ((k & 3) * 2 * kLookupBits));
                bits &= (kSwapMask | kInvertMask);

            };


        getBits(7);
        getBits(6);
        getBits(5);
        getBits(4);
        getBits(3);
        getBits(2);
        getBits(1);
        getBits(0);

        return ((((ulong)(n[1]) << 32) + n[0]) * 2 + 1);
    }

    static double UVtoST(double u)
    {
        if (u >= 0)
        {
            return Math.Sqrt(1 + 3 * u) - 1;
        }

        return 1 - Math.Sqrt(1 - 3 * u);
        //return 0.5 * (u + 1);
    }

    static int STtoIJ(double s)
    {
        int m = kMaxSize / 2; // scaling multiplier
        return Math.Max(0, Math.Min((int)(2 * m - 1), (int)((m * s + (m - 0.5)) + 0.5)));
        //return Max(0, Min(kMaxSize - 1, (int)(kMaxSize * s - 0.5)));
    }

    public static double Deg2Rad(double deg)
    {
        return (deg * Math.PI) / 180;
    }

    public static double Rad2Deg(double deg)
    {
        return (deg * 180) / Math.PI;
    }

    public static ulong PosToCellID(Vector2 p_xPos)
    {
        double theta = Deg2Rad(p_xPos.x);
        double phi = Deg2Rad(p_xPos.y);
        double cosphi = Math.Cos(phi);

        //topoint
        double dX = Math.Cos(theta) * cosphi;
        double dY = Math.Sin(theta) * cosphi;
        double dZ = Math.Sin(phi);

        int face = 0;
        if (Math.Abs(dY) > Math.Abs(dX) && Math.Abs(dY) > Math.Abs(dZ)) { face = 1; };
        if (Math.Abs(dZ) > Math.Abs(dX) && Math.Abs(dZ) > Math.Abs(dY)) { face = 2; };
        if (face == 0 && dX < 0) { face += 3; };
        if (face == 1 && dY < 0) { face += 3; };
        if (face == 2 && dZ < 0) { face += 3; };

        double pu;
        double pv;
        switch (face)
        {
            case 0: pu = dY / dX; pv = dZ / dX; break;
            case 1: pu = -dX / dY; pv = dZ / dY; break;
            case 2: pu = -dX / dZ; pv = -dY / dZ; break;
            case 3: pu = dZ / dX; pv = dY / dX; break;
            case 4: pu = dZ / dY; pv = -dX / dY; break;
            default: pu = -dY / dZ; pv = -dX / dZ; break;
        }

        int i = STtoIJ(UVtoST(pu));
        int j = STtoIJ(UVtoST(pv));

        return FromFaceIJ(face, i, j);
    }

    static ulong CellIDLsb(ulong id) { return id & (ulong)(-(long)id); }
    static int CellIDFace(ulong id) { return (int)(id >> kPosBits); }
    static ulong lsbForLevel(int level) { return (ulong)1 << (2 * (kMaxLevel - level)); }

    static double stToUV(double s)
    {
        if (s >= 0.5)
        {
            return (1.0 / 3.0) * (4 * s * s - 1);
        }
        return (1.0 / 3.0) * (1 - 4 * (1 - s) * (1 - s));
    }

    static Vector3 faceUVToXYZ(int face, float u, float v)
    {
        switch (face)
        {
            case 0:
                return new Vector3(1, u, v);
            case 1:
                return new Vector3(-u, 1, v);
            case 2:
                return new Vector3(-u, -v, 1);
            case 3:
                return new Vector3(-1, -v, -u);
            case 4:
                return new Vector3(v, -1, -u);
            case 5:
                return new Vector3(v, u, -1);
        }
        return new Vector3(0, 0, 0);
    }

    public static Vector2 CellIDToPos(ulong id)
    {
        MaybeInit();
        int face = CellIDFace(id);
        int bits = face & kSwapMask;
        int nbits = kMaxLevel - 7 * kLookupBits; // first iteration
        int i = 0;
        int j = 0;


        for (int k = 7; k >= 0; k--)
        {
            bits += ((int)(id >> (k * 2 * kLookupBits + 1)) & ((1 << (2 * nbits)) - 1)) << 2;
            bits = lookup_ij[bits];
            i += (bits >> (kLookupBits + 2)) << (k * kLookupBits);
            j += ((bits >> 2) & ((1 << kLookupBits) - 1)) << (k * kLookupBits);
            bits &= (kSwapMask | kInvertMask);
            nbits = kLookupBits; // following iterations
        }

        if ((CellIDLsb(id) & 0x1111111111111110) != 0)
        {
            bits ^= kSwapMask;
        };


        int delta = 0;
        if (CellIDIsLeaf(id))
        {
            delta = 1;
        }
        else
        {
            if (((i ^ ((int)(id) >> 2)) & 1) != 0)
            {
                delta = 2;
            }
        };


        int si = 2 * i + delta;
        int ti = 2 * j + delta;


        double pu = stToUV((0.5 / kMaxSize) * si);
        double pv = stToUV((0.5 / kMaxSize) * ti);
        Vector3 p = faceUVToXYZ(face, (float)pu, (float)pv);

        double dLat = Rad2Deg(Math.Atan2(p.z, Math.Sqrt(p.x * p.x + p.y * p.y)));
        double dLon = Rad2Deg(Math.Atan2(p.y, p.x));
        return new Vector2((float)dLon, (float)dLat);
        //return faceUVToXYZ(face, stToUV((0.5/maxSize)*float64(si)), stToUV((0.5/maxSize)*float64(ti)))
    }

    public static string PosToE6(Vector2 p_xPos)
    {
        string sLat = Convert.ToString((int)(p_xPos.y * 1E6), 16);
        string sLon = Convert.ToString((int)(p_xPos.x * 1E6), 16);
        while (sLat.Length < 8) { sLat = "0" + sLat; };
        while (sLon.Length < 8) { sLon = "0" + sLon; };
        return sLat + "," + sLon;
    }

    public static float PosDistance(Vector2 pa, Vector2 pb)
    {
        float lat1 = (float)Deg2Rad(pa.y);
        float lon1 = (float)Deg2Rad(pa.x);
        float lat2 = (float)Deg2Rad(pb.y);
        float lon2 = (float)Deg2Rad(pb.x);

        float R = 6371;
        float dLat = lat2 - lat1;
        float dLon = lon2 - lon1;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
              Mathf.Cos(lat1) * Mathf.Cos(lat2) *
              Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1.0f - a));
        float d = R * c;
        return d * 1000;
    }

    public static bool CellIDIsLeaf(ulong id) { return ((id) & 1) != 0; }
    public static int CellIDLevel(ulong id)
    {
        // Fast path for leaf cells.
        if (CellIDIsLeaf(id)) { return kMaxCellLevel; }
        uint x = (uint)id;
        int level = -1;
        if (x != 0)
        {
            level += 16;
        }
        else
        {
            x = (uint)(id >> 32);
        }
        // Only need to look at even-numbered bits for valid cell IDs.
        x &= (uint)(-(int)x); // remove all but the LSB.
        if ((x & 0x00005555) != 0)
        {
            level += 8;
        }
        if ((x & 0x00550055) != 0)
        {
            level += 4;
        }
        if ((x & 0x05050505) != 0)
        {
            level += 2;
        }
        if ((x & 0x11111111) != 0)
        {
            level += 1;
        }
        return level;
    }

    public static ulong CellIDParent(ulong id, int level)
    {
        ulong lsb = lsbForLevel(level);
        return (ulong)((id & (ulong)(-(long)lsb)) | lsb);
    }

}
