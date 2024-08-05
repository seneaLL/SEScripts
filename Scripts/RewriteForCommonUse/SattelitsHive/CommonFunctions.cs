using System;
using System.Collections.Generic;
using VRageMath;

namespace SpaceEngineers.UWBlockPrograms.Transmission.Common
{
    public static class CommonFunctions {
            public static int GetTimeStampNow()
            {
                return (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            public static string IpToString(int[] ip)
            {
                string ipStr = "";
                int ipSectionsCount = ip.Length;

                for (int i = 0; i < ipSectionsCount; i++) {
                    if (i == ipSectionsCount - 1) {
                        ipStr += ip[i].ToString();
                        continue;
                    }

                    ipStr += $"{ip[i].ToString()}.";
                }

                return ipStr;
            }

            public static int[] IpStringToArray(string ipStr)
            {
                var sections = ipStr.Split('.');
                var ip = new int[sections.Length];

                for (int i = 0; i < sections.Length; i++) {
                    ip[i] = Convert.ToInt32(sections[i]);
                }

                return ip;
            }

            public static Vector3[,,] GetGridMatrix()
            {
                var positions = new Vector3[3, 3, 3];
                for (int x = -1; x < 3 - 1; x++)
                    for (int y = -1; y < 3 - 1; y++)
                        for (int z = -1; z < 3 - 1; z++)
                            positions[x + 1, y + 1, z + 1] = new Vector3(x, y, z);

                return positions;
            }

            public static Vector3[,,] GetScaledGridMatrix(float maxDistance)
            {
                var a = (float)(maxDistance / Math.Sqrt(3));
                var m = GetGridMatrix();

                for (int x = 0; x < 3; x++)
                    for (int y = 0; y < 3; y++)
                        for (int z = 0; z < 3; z++) {
                            var v = m[x, y, z];
                            m[x, y, z] = new Vector3(v.X * a, v.Y * a, v.Z * a);
                        }
                return m;
            }

            public static List<Vector3> GetShiftedGridMatrix(float maxDistance, Vector3 centerPoint)
            {
                var m = GetScaledGridMatrix(maxDistance);
                var mRes = new List<Vector3>();

                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 3; y++) {
                        for (int z = 0; z < 3; z++) {
                            if (x == 0 && y == 0 && z == 0)
                                continue;

                            var gridVector = m[x, y, z];
                            mRes.Add(new Vector3(gridVector.X + centerPoint.X, gridVector.Y + centerPoint.Y, gridVector.Z + centerPoint.Z));
                        }
                    }
                }

                return mRes;
            }
        }
}