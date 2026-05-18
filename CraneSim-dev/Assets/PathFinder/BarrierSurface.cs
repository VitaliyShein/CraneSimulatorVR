using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Surfaces
{
    //методы расширений для float
    public static class FloatMethods
    {
        public static int _toDiscretCoord(this float val, float step)
        {
            return (int)(val / step);
        }
        //для улучшения читабельности, лучше переделать
        public static float _offset(this float val, float offset)
        {
            return val + offset;
        }
    } 

    public abstract class Surface
    {
        protected float[][] surface;

        public virtual float Length { get; set; }
        public virtual float Width { get; set; }
        public virtual float Height { get; set; }

        //for discret coord
        public abstract float this[int x, int y] { get; set; }

        //for global coord
        public abstract float this[float x, float y] { get; }

        public virtual float DiscretStep { get; set; }

        //private readonly int discretLength;

        public abstract int GetDiscretLength();

        //private readonly int discretWidth;

        public abstract int GetDiscretWidth();

        //private readonly int discretHeight;

        public abstract int GetDiscretHeight();

        public abstract void Show(Action<object> print);
        public abstract int[][] ToIntArray();
        public abstract Vector3Int ToDiscretCoord(Vector3 point);
        public abstract Vector3 ToGlobalCoord(Vector3Int point);
        public abstract void Clear();

    }

    public class BarrierSurface : Surface
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3 Position
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
        }

        public override int GetDiscretLength()
        { return Length._toDiscretCoord(DiscretStep); }

        public override int GetDiscretWidth()
        { return Width._toDiscretCoord(DiscretStep); }

        public override int GetDiscretHeight()
        { return Height._toDiscretCoord(DiscretStep); }
        public List<Barrier> bars = new List<Barrier>();

        public override float this[int x, int y]
        {
            get { return surface[x][y]; }
            set { surface[x][y] = value; }
        }

        public override float this[float x, float y]
        {
            get
            {
                var node = ToDiscretCoord(new Vector3(x, 0, y));
                return surface[node.x][node.z];
            }
        }


        public BarrierSurface(float length, float width, float step)
        {
            DiscretStep = step;
            Length = length;
            Width = width;

            var dlen = GetDiscretLength();
            var dwid = GetDiscretWidth();

            surface = new float[dlen][];
            for(int i = 0; i < dlen; i++)
            {
                surface[i] = new float[dwid];
            }

            Kernel.draws.Add(DebugList);
            
        }

        public void SetPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public List<KeyValuePair<Vector3, Vector3>> DebugList = new List<KeyValuePair<Vector3, Vector3>>();

        public void PutBarrier(Barrier barrier)
        {
            bars.Add(barrier);
            var _begin_x = Math.Max(barrier.X - barrier.Length / 2, X - Length / 2);
            var _begin_y = Math.Max(barrier.Z - barrier.Width / 2, Z - Width / 2);


            var _end_x = Math.Min(barrier.X + barrier.Length / 2, X + Length / 2);
            var _end_y = Math.Min(barrier.Z + barrier.Width / 2, Z + Width / 2);

            var _begin_x_surface_coord = _begin_x._offset(-X)._offset(Length / 2)._toDiscretCoord(DiscretStep);
            var _begin_y_surface_coord = _begin_y._offset(-Z)._offset(Width / 2)._toDiscretCoord(DiscretStep);

            var _end_x_surface_coord = _end_x._offset(-X)._offset(Length / 2)._toDiscretCoord(DiscretStep);
            var _end_y_surface_coord = _end_y._offset(-Z)._offset(Width / 2)._toDiscretCoord(DiscretStep);

            float height_xy;
            for (int i = _begin_x_surface_coord; i < _end_x_surface_coord; i++)
            {
                for(int j = _begin_y_surface_coord; j < _end_y_surface_coord; j++)
                {
                    height_xy = barrier.GetHeight(i * DiscretStep + X - Length / 2, j * DiscretStep + Z - Width / 2);
                    
                    if (surface[i][j] < height_xy)
                    {
                        surface[i][j] = height_xy;
                        if(height_xy > Height + Y)
                        {
                            Height = height_xy - Y;
                        }
                    }
                }
            }
        }

        public override void Show(Action<object> print)
        {
            string row = "";
            row = "";
            for (int i = 0; i < surface.Length; i++)
            {
                for (int j = 0; j < surface[i].Length; j++)
                {
                    row += string.Format("{0, 5}", surface[i][j]);//Math.Abs(surface[i, j] - Z) < 0.0001 ? '_' : '+';
                }
                row += '\n';
            }

            print(row);
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

        public void ReBarrier()
        {
            Barrier[] barriers = new Barrier[bars.Count];
            bars.CopyTo(barriers);
            bars.Clear();
            Clear();
            foreach(var b in barriers)
            {
                PutBarrier(b);
            }
        }

        public override int[][] ToIntArray()
        {
            var dlen = GetDiscretLength();
            var dwid = GetDiscretWidth();
            int[][] result = new int[dlen][];

            for(int i = 0; i < dlen; i++)
            {
                result[i] = new int[dwid];
                for(int j = 0; j < dwid; j++)
                {
                    result[i][j] = surface[i][j]._toDiscretCoord(DiscretStep);
                }
            }

            
            return result;
        }

        public override Vector3Int ToDiscretCoord(Vector3 point)
        {
            var x = (point.x + Length / 2 - X)._toDiscretCoord(DiscretStep);
            var y = (point.y + Height / 2 - Y)._toDiscretCoord(DiscretStep);
            var z = (point.z + Width / 2 - Z)._toDiscretCoord(DiscretStep);
            return new Vector3Int(x, y, z);
        }

        public override Vector3 ToGlobalCoord(Vector3Int point)
        {
            var x = point.x * DiscretStep - Length / 2 + X;
            var y = point.y * DiscretStep - Height / 2 + Y;
            var z = point.z * DiscretStep - Width / 2 + Z;
            return new Vector3(x, y, z);
        }
    }
}
