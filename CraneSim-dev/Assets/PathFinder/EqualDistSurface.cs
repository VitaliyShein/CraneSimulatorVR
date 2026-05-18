using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cargos;

namespace Surfaces
{
    public class EqualDistSurface : Surface
    {
        public float X
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.X;
            }
        }
        public float Y
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.Y;
            }
        }
        public float Z
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.Z;
            }
        }
        
        public override float DiscretStep
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.DiscretStep;
            }
            set { }
        }

        public override int GetDiscretLength()
        {
            if (barriers == null)
                throw new NullReferenceException("Не задана поверхность препятствий");
            return barriers.Length._toDiscretCoord(barriers.DiscretStep);
        }

        public override int GetDiscretWidth()
        {
            if (barriers == null)
                throw new NullReferenceException("Не задана поверхность препятствий");
            return barriers.Width._toDiscretCoord(barriers.DiscretStep);
        }

        public override int GetDiscretHeight()
        {
            if (barriers == null)
                throw new NullReferenceException("Не задана поверхность препятствий");
            return (barriers.Height + R_vert)._toDiscretCoord(barriers.DiscretStep);
        }

        public override float Length
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.Length;
            }
            set { }
        }
        public override float Width
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.Width;
            }
            set { }
        }

        //с учетом отступов высота может измениться, поэтому не наследуется
        public override float Height
        {
            get
            {
                if (barriers == null)
                    throw new NullReferenceException("Не задана поверхность препятствий");
                return barriers.Width + R_vert;
            }
            set { }
        }
        
        public const float V_EXTRA = 3f;
        public const float H_EXTRA = 7f;
        
        //высота через дискретные относительные координаты 
        public override float this[int x, int y]
        {
            get
            {
                return surface[x][y];
            }
            set
            {
                surface[x][y] = value;
            }
        }
        
        //высота через глобальные координаты
        public override float this[float x, float y]
        {
            get
            {
                var node = ToDiscretCoord(new Vector3(x, 0, y));
                return surface[node.x][node.z];
            }
        }
        
        BarrierSurface barriers = null;

        //горизонтальный и вертикальный отступы
        float R_horz = 0;
        float R_vert = 0;

        public EqualDistSurface(BarrierSurface barriers, Vector3 cargoSize)
        {
            this.barriers = barriers;

            var dlen = GetDiscretLength();
            var dwid = GetDiscretWidth();

            surface = new float[dlen][];
            for (int i = 0; i < dlen; i++)
            {
                surface[i] = new float[dwid];
            }

            SetCargo(cargoSize, DiscretStep);

            var discretBarrierSurface = barriers.ToIntArray();
            var discretR_vert = R_vert._toDiscretCoord(DiscretStep);
            var discretR_horz = R_horz._toDiscretCoord(DiscretStep);


            float k_vert;
            float rp_2;
            // foreach cell
            for (int i = 0; i < dlen; i++)
            {
                for (int j = 0; j < dwid; j++)
                {
                    // foreach another cell nearby that cell
                    if (discretBarrierSurface[i][j] > 0)
                    {
                        for (int k = Math.Max(i - discretR_horz, 0); k < Math.Min(i + discretR_horz, dlen); k++)
                        {
                            for (int l = Math.Max(j - discretR_horz, 0); l < Math.Min(j + discretR_horz, dwid); l++)
                            {
                                rp_2 = (float)(Math.Pow(k - i, 2) + Math.Pow(l - j, 2)) * Mathf.Pow(DiscretStep, 2);
                                k_vert = R_vert * (float)Math.Sqrt(1 - rp_2/(R_horz*R_horz));// ;
                                if (discretBarrierSurface[i][j] * DiscretStep + k_vert > surface[k][l])
                                {
                                    surface[k][l] = (discretBarrierSurface[i][j]) * DiscretStep + k_vert;
                                }
                            }
                        }
                    }
                }
            }
            
        }

        //Задаем габариты груза для отступов
        private void SetCargo(Vector3 cargoSize, float step)
        {
            float dx = cargoSize.x;
            float dy = cargoSize.y;
            float dz = cargoSize.z;

            R_horz = (float)Math.Sqrt((dx * dx + dz * dz))/2 + H_EXTRA;
            R_vert = dy / 2 + V_EXTRA;
        }

        public override int[][] ToIntArray()
        {
            var dlen = GetDiscretLength();
            var dwid = GetDiscretWidth();
            int[][] result = new int[dlen][];

            for (int i = 0; i < dlen; i++)
            {
                result[i] = new int[dwid];
                for (int j = 0; j < dwid; j++)
                {
                    result[i][j] = (surface[i][j] - Y)._toDiscretCoord(DiscretStep);
                }
            }


            return result;
        }

        //Вывод в консоль (необязательно)
        public override void Show(Action<object> print)
        {
            string row = "";
            row = "";
            for (int i = 0; i < surface.Length; i++)
            {
                for (int j = 0; j < surface[i].Length; j++)
                {
                    row += surface[i][j] < 0.0001 ? "_____ " : string.Format("{0, 5} ", surface[i][j]);
                }
                row += '\n';
            }

            print(row);
        }

        //Перевод из глобальных координат в дискретные относительные
        public override Vector3Int ToDiscretCoord(Vector3 point)
        {
            var x = (point.x + Length / 2 - X)._toDiscretCoord(DiscretStep);
            var y = (point.y + Height / 2 - Y)._toDiscretCoord(DiscretStep);
            var z = (point.z + Width / 2 - Z)._toDiscretCoord(DiscretStep);
            return new Vector3Int(x, y, z);
        }

        //Перевод из дискретных относительные координат в глобальные
        public override Vector3 ToGlobalCoord(Vector3Int point)
        {
            var x = point.x * DiscretStep - Length / 2 + X;
            var y = point.y * DiscretStep - Height / 2 + Y;
            var z = point.z * DiscretStep - Width / 2 + Z;
            return new Vector3(x, y, z);
        }

        public override void Clear()
        {
            for (int i = 0; i < surface.Length; i++)
            {
                for (int j = 0; j < surface[i].Length; j++)
                {
                    surface[i][j] = 0;
                }
            }
        }
    }
}
