using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cargos
{
    public abstract class Cargo
    {
        protected MonoBehaviour cargoObject; //объект Unity груза
        protected GeometryBody body; //тело груза

        public Cargo(Vector3 pos, Vector3 size, Vector3 rot)
        {
            
        }

        public Vector3 Center { get { return body.Center; } }
        //public Vector3 Position { get { return body.Position; } }
        public Vector3 Size { get { return new Vector3(body.Length, body.Height, body.Width); } }
        public Vector3 Rotation { get { return body.rotation; } }

        public Vector3[] Nodes { get { return body.Nodes.ToArray(); } }

        public abstract Cargo GetCopy();
        public abstract void RotateOn(Vector3 rotation);
        //связывает объект Unity с объектом класса
        public abstract void Bind(MonoBehaviour cargoObject);
        public abstract void Update();
    }

    //коробка
    public class BoxCargo : Cargo
    {
        public BoxCargo(Vector3 pos, Vector3 size, Vector3 rot) : base(pos, size, rot)
        {
            var nodes = GeometryBody.CreateBoxNodes(pos, size);
            var edges = GeometryBody.CreateBoxEdges(nodes);

            body = new GeometryBody(nodes.ToArray(), edges, pos, rot);

        }

        public override Cargo GetCopy()
        {
            return new BoxCargo(Center, Size, Rotation);
        }

        public override void RotateOn(Vector3 rotation)
        {
            body.RotateOn(rotation);
        }

        public override void Update()
        {
            if(cargoObject != null)
            {
                var trans = cargoObject.GetComponent<Transform>();
                var nodes = GeometryBody.CreateBoxNodes(trans.position, trans.lossyScale);
                var edges = GeometryBody.CreateBoxEdges(nodes);

                body = new GeometryBody(nodes.ToArray(), edges, trans.position, trans.eulerAngles);
            }
        }

        public override void Bind(MonoBehaviour cargoObject)
        {
            this.cargoObject = cargoObject;
        }

    }


}

