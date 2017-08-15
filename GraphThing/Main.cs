using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace GraphThing
{
    public partial class Main : Form
    {
        private int _numberOfPoints = 250;
        private int _minMargin = 10;
        private int _maxMargin = 20;
        private float _circleRadius = 5f;

        private Bitmap _canvas;
        private Random _random;
        private Pathfinding<Node> _pathfinding;

        private static Color _darkWhite = ColorFromHex("D8D8C0");
        private static Color _lightRed = ColorFromHex("FF6F59");
        private static Color _saturatedTurquise = ColorFromHex("70877F");
        private static Color _lightBlue = ColorFromHex("34403A");
        private static Color _darkGrey = ColorFromHex("172121");
        private static Color _darkBlack = ColorFromHex("172121");
        private static Color _lightBrown = ColorFromHex("664E4C");

        private Node[] _nodes;

        public Main()
        {
            InitializeComponent();

            _pathfinding = new Pathfinding<Node>();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            panel.Dock = DockStyle.Fill;

            Console.WriteLine(sizeof(byte));
            Console.WriteLine(sizeof(float));
            Console.WriteLine(sizeof(double));

            Generate();
        }

        private void KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Space:

                    var sw = new Stopwatch();
                    sw.Start();

                    Generate();
                    DrawPoints();

                    GabrielNeighbours(_nodes);
                    //while (_index != _nodes.Length) Step();

                    TestPathfinding();
                    sw.Stop();
                    Console.WriteLine("Elapsed: " + sw.Elapsed.Milliseconds);
                    break;
            }
        }

        private void TestPathfinding()
        {
            if (_random == null) return;

            // Reset all colors
            // _nodes.ToList<Node>().ForEach(x => x.Color = _lightRed);

            // Select two random nodes
            var sindex = _random.Next(0, _nodes.Length - 1);
            var eindex = _random.Next(0, _nodes.Length - 1);

            _nodes[sindex].Color = _saturatedTurquise;
            _nodes[eindex].Color = _lightBlue;

            DrawPoints();

            var startNode = _nodes[sindex];
            var endNode = _nodes[eindex];

            // Find all the neighbours
            // while (_index != _nodes.Length) Step();

            // Then calculate the path
            var path = _pathfinding.CalculatePath(startNode, endNode, _nodes);

            if (path != null)
                DrawPath(path);
            else
                Console.WriteLine("No path was found");

            Console.WriteLine("i think i found a path");
        }

        private void MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) Generate();
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //while(_index != _nodes.Length) Step();
                GabrielNeighbours(_nodes);

                DrawPoints();
            }
        }

        private static Color ColorFromHex(string hexRgb)
        {
            var hex = hexRgb.StartsWith("#") ? hexRgb.Replace("#", "FF") : "FF" + hexRgb;
            int argb = Int32.Parse(hex, NumberStyles.HexNumber);

            return Color.FromArgb(argb);
        }

        private double Length(double x1, double y1, double x2, double y2)
        {
            var len = Math.Sqrt(Math.Abs(
                Math.Pow(x1 - x2, 2) +
                Math.Pow(y1 - y2, 2)));

            return len;
        }

        private double Length(Point p1, Point p2)
        {
            return Length(p1.X, p1.Y, p2.X, p2.Y);
        }

        private Point GetRandomPoint(Random rnd, int xmin, int xmax, int ymin, int ymax)
        {
            var x = rnd.Next(xmin, xmax);
            var y = rnd.Next(ymin, ymax);

            return new Point(x, y);
        }

        private bool PointIntersectsCircle(Point a, Point b, Point c)
        {
            var len = Length(a, b);
            var circleRadi = len / 2;

            var dx = b.X - a.X;
            var dy = b.Y - a.Y;

            var midx = a.X + ((dx / len) * circleRadi);
            var midy = a.Y + ((dy / len) * circleRadi);

            var clen = Math.Abs(Length(midx, midy, c.X, c.Y));

            if (clen <= circleRadi)
                return true;

            return false;
        }

        private void DrawCircle(Graphics g, Brush b, int x, int y, float r)
        {
            g.FillEllipse(b, (float)x, (float)y, r * 2, r * 2);
        }

        private void DrawLine(Graphics g, Brush b, float x1, float y1, float x2, float y2)
        {
            g.DrawLine(new Pen(b, 2f),
                new Point((int)(x1 + _circleRadius), (int)(y1 + _circleRadius)),
                new Point((int)(x2 + _circleRadius), (int)(y2 + _circleRadius)));
        }

        private void DrawPoints()
        {
            this.Text = "Rendering";

            using (var graphics = Graphics.FromImage((Image)_canvas))
            {
                graphics.Clear(_darkBlack);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                foreach (var n in _nodes)
                    foreach(var neighbour in n.Neighbours)
                        DrawLine(graphics, new SolidBrush(_lightBlue), n.X, n.Y, neighbour.X, neighbour.Y);

                foreach (var n in _nodes) 
                    DrawCircle(graphics, new SolidBrush(_saturatedTurquise), (int)n.X, (int)n.Y, 5f);
            }

            panel.BackgroundImage = _canvas;
            panel.Refresh();
            this.Text = "Operation complete";
        }

        private void DrawPath(IEnumerable<Node> path)
        {
           path.ToList<Node>().ForEach(x => x.Color = ColorFromHex("DA2745"));

            using (var graphics = Graphics.FromImage((Image)_canvas))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var current = path.First();
                while (current.Parent != null)
                {
                    DrawLine(graphics, new SolidBrush(_lightBrown), 
                        (int)current.X, (int)current.Y,
                        (int)current.Parent.X, (int)current.Parent.Y);

                    current = (Node)current.Parent;
                }

                foreach(var node in path)
                    DrawCircle(graphics, new SolidBrush(node.Color), node.X, node.Y, 5f);
            }

            panel.BackgroundImage = _canvas;
            panel.Refresh();
        }

        private void Generate()
        {
            _random = new Random(Environment.TickCount);
            _canvas = new Bitmap(this.Size.Width, this.Size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _nodes = new Node[_numberOfPoints];
            for (int i = 0; i < _numberOfPoints; i++)
            {
                var p = GetRandomPoint(_random,
                    _minMargin, this.Size.Width - _maxMargin,
                    _minMargin, this.Size.Height - _maxMargin);

                _nodes[i] = new Node()
                {
                    X = p.X,
                    Y = p.Y,
                    Color = _saturatedTurquise,
                    G = 10f,
                    H = 0f,
                    ID = i,
                    Parent = null,
                    Neighbours = new List<Node>()
                };
            }

            DrawPoints();
        }

        private void GabrielNeighbours(Node[] nodes)
        {
            // Circle through all the nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var positiveNeighbours = new List<Node>();

                // Test the selected node against all the others
                for (int y = 0; y < nodes.Length; y++)
                {
                    // make sure we dont compare the new
                    // node to itself
                    if (y == i) continue;

                    var intersects = true;

                    // circle through all the nodes again
                    for (int j = 0; j < nodes.Length; j++)
                    {
                        // make sure this node isn't
                        // any of the two nodes we're checking
                        if (j == y || j == i) continue;

                        intersects = PointIntersectsCircle(
                            new Point(node.X, node.Y), 
                            new Point(nodes[y].X, nodes[y].Y), 
                            new Point(nodes[j].X, nodes[j].Y));
                        
                        // if the latter node is intersecting
                        // with the other two we break
                        if (intersects) 
                            break;
                    }

                    // if we have no intersects we add the
                    // node to the list of neighbours
                    if (!intersects)
                        positiveNeighbours.Add(nodes[y]);
                }

                // set the neigbour and color
                nodes[i].Neighbours = positiveNeighbours;
                //nodes[i].Color = _lightRed;
            }
        }

        /*private void Step()
        {
            var node = _nodes[_index];
            var positiveNeighbours = new List<Node>();

            for (int i = 0; i < _nodes.Length; i++)
            {
                if (i == _index) continue;
                var intersects = true;

                for (int y = 0; y < _nodes.Length; y++)
                {
                    if (y == i || y == _index) continue;

                    intersects = PointIntersectsCircle(new Point(node.X, node.Y), new Point(_nodes[i].X, _nodes[i].Y), new Point(_nodes[y].X, _nodes[y].Y));
                    if (intersects) break;
                }

                if (!intersects)
                    positiveNeighbours.Add(_nodes[i]);
            }

            _nodes[_index].Neighbours = positiveNeighbours;
            _nodes[_index].Color = _lightRed;
            _index++;

            //DrawPoints();
        }*/
    }
}
