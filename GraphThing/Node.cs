using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphThing
{
    public class Node : INode
    {
        public Color Color;

        #region Interface Implementation

        // public Point Position { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public INode Parent { get; set; }
        public IEnumerable<INode> Neighbours { get; set; }

        public bool Checked { get; set; }
        public bool Closed { get; set; }
        public int ID { get; set; }

        public float H { get; set; }
        public float G { get; set; }

        public float F
        {
            get { return H + G; }
        }

        #endregion
    }
}
