using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SM08
{
    public partial class UserControl1: UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            this.Width = 800;
            this.Height = 600;

            Button generateButton = new Button
            {
                Text = "Generate Polygon",
                Location = new Point(10, 10)
            };
            generateButton.Click += GenerateButton_Click;
            this.Controls.Add(generateButton);

            this.Paint += DrawingPanel_Paint;
        }

        private List<PointF> points = new List<PointF>();
        private List<Triangle> triangles = new List<Triangle>();
        private const int PointRadius = 3;


        private void GenerateButton_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            points.Clear();
            triangles.Clear();
            int n = 10;

            for (int i = 0; i < n; i++)
            {
                points.Add(new PointF(rand.Next(50, 700), rand.Next(50, 450)));
            }

            points = points.OrderBy(p => Math.Atan2(p.Y - points.Average(pt => pt.Y), p.X - points.Average(pt => pt.X))).ToList();

            TriangulatePolygon();
            this.Invalidate();
        }
    ////////////////////////////////////////////
        private void TriangulatePolygon()
        {
            List<PointF> polygon = new List<PointF>(points);

            while (polygon.Count > 3)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    PointF p1 = polygon[i];
                    PointF p0 = polygon[(i + polygon.Count - 1) % polygon.Count];
                    PointF p2 = polygon[(i + 1) % polygon.Count];

                    if (!IsEar(p0, p1, p2, polygon))
                        continue;

                    triangles.Add(new Triangle(p0, p1, p2));
                    polygon.RemoveAt(i);
                    break;
                }
            }

            triangles.Add(new Triangle(polygon[0], polygon[1], polygon[2]));
        }

        private bool IsEar(PointF p0, PointF p1, PointF p2, List<PointF> polygon)
        {
            if (Orientation(p0, p1, p2) != 1)
                return false;

            foreach (var point in polygon)
            {
                if (point.Equals(p0) || point.Equals(p1) || point.Equals(p2))
                    continue;

                if (IsPointInsideTriangle(point, p0, p1, p2))
                    return false;
            }

            return true;
        }

        private int Orientation(PointF p0, PointF p1, PointF p2)
        {
            float val = (p1.Y - p0.Y) * (p2.X - p1.X) - (p1.X - p0.X) * (p2.Y - p1.Y);

            if (Math.Abs(val) < 0.000001)
                return 0;
            return (val > 0) ? 2 : 1;
        }

        private bool IsPointInsideTriangle(PointF point, PointF p0, PointF p1, PointF p2)
        {
            int o1 = Orientation(point, p0, p1);
            int o2 = Orientation(point, p1, p2);
            int o3 = Orientation(point, p2, p0);

            return (o1 == o2) && (o2 == o3);
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen polygonPen = new Pen(Color.Red, 2);
            Pen trianglePen = new Pen(Color.Blue, 2);
            Brush pointBrush = Brushes.Blue;

            foreach (var point in points)
            {
                g.FillEllipse(pointBrush, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
            }

            if (points.Count > 1)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    PointF p1 = points[i];
                    PointF p2 = points[(i + 1) % points.Count];
                    g.DrawLine(polygonPen, p1, p2);
                }
            }

            Color[] colorPalette = { Color.Red, Color.Green, Color.Blue };

            Dictionary<Triangle, Color> triangleColors = new Dictionary<Triangle, Color>();


            foreach (var triangle in triangles)
            {
                List<Triangle> adjacentTriangles = triangles.Where(t =>
                    t != triangle &&
                    (t.P1 == triangle.P1 || t.P1 == triangle.P2 || t.P1 == triangle.P3 ||
                    t.P2 == triangle.P1 || t.P2 == triangle.P2 || t.P2 == triangle.P3 ||
                    t.P3 == triangle.P1 || t.P3 == triangle.P2 || t.P3 == triangle.P3)).ToList();

                HashSet<Color> neighborColors = new HashSet<Color>();
                foreach (var adjacentTriangle in adjacentTriangles)
                {
                    if (triangleColors.ContainsKey(adjacentTriangle))
                        neighborColors.Add(triangleColors[adjacentTriangle]);
                }

                foreach (var color in colorPalette)
                {
                    if (!neighborColors.Contains(color))
                    {
                        triangleColors.Add(triangle, color);
                        break;
                    }
                }
            }

            foreach (var triangle in triangles)
            {
                if (triangleColors.ContainsKey(triangle))
                {
                    Pen pen = new Pen(triangleColors[triangle], 2);
                    g.DrawLine(pen, triangle.P1, triangle.P2);
                    g.DrawLine(pen, triangle.P2, triangle.P3);
                    g.DrawLine(pen, triangle.P3, triangle.P1);
                }
            }
        }
    }

    public class Triangle
    {
        public PointF P1 { get; }
        public PointF P2 { get; }
        public PointF P3 { get; }

        public Triangle(PointF p1, PointF p2, PointF p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }
    }
   /////////////////////////////////////////////////////////////
}
