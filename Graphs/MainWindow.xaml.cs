using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Graphs
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // The data.

        private Brush[] DataBrushes = { Brushes.Red, Brushes.Green, Brushes.Blue };
        private bool isDragging = false;
        Polyline polyline1 = new Polyline();//for MLS method
        Polyline polyline2 = new Polyline();//for Lagrange method
        double[] Y = new double[5];
        double[] X = new double[5];
        Lagrange lagrange = new Lagrange();
        private Matrix WtoDMatrix, DtoWMatrix;
        private Ellipse[] DataEllipse = new Ellipse[7];//points

        private void PrepareTransformations(
            double wxmin, double wxmax, double wymin, double wymax,
            double dxmin, double dxmax, double dymin, double dymax)
        {
            // Make WtoD.
            WtoDMatrix = Matrix.Identity;
            WtoDMatrix.Translate(-wxmin, -wymin);

            double xscale = (dxmax - dxmin) / (wxmax - wxmin);
            double yscale = (dymax - dymin) / (wymax - wymin);
            WtoDMatrix.Scale(xscale, yscale);

            WtoDMatrix.Translate(dxmin, dymin);

            // Make DtoW.
            DtoWMatrix = WtoDMatrix;
            DtoWMatrix.Invert();
        }

        // Transform a point from world to device coordinates.
        private Point WtoD(Point point)
        {
            return WtoDMatrix.Transform(point);
        }

        // Transform a point from device to world coordinates.
        private Point DtoW(Point point)
        {
            return DtoWMatrix.Transform(point);
        }

        // Position a label at the indicated point.
        private void DrawText(Canvas can, string text,
            Point location, double angle, double font_size,
            HorizontalAlignment halign, VerticalAlignment valign)
        {
            // Make the label.
            Label label = new Label();
            label.Content = text;
            label.FontSize = font_size;
            can.Children.Add(label);

            // Rotate if desired.
            if (angle != 0) label.LayoutTransform = new RotateTransform(angle);

            // Position the label.
            label.Measure(new Size(double.MaxValue, double.MaxValue));

            double x = location.X;
            if (halign == HorizontalAlignment.Center)
                x -= label.DesiredSize.Width / 2;
            else if (halign == HorizontalAlignment.Right)
                x -= label.DesiredSize.Width;
            Canvas.SetLeft(label, x);

            double y = location.Y;
            if (valign == VerticalAlignment.Center)
                y -= label.DesiredSize.Height / 2;
            else if (valign == VerticalAlignment.Bottom)
                y -= label.DesiredSize.Height;
            Canvas.SetTop(label, y);
        }


      

        // Change the mouse cursor appropriately.
        private void canGraph_MouseMove(object sender, MouseEventArgs e)
        {
            // Find the data point at the mouse's location.
            try
            {
                Point mouse_location = e.GetPosition(canGraph);
                int index = Selected_Ellipse(mouse_location);
                if (index != 8) 
                if (isDragging)
                {
                    Canvas.SetTop(DataEllipse[index], mouse_location.Y - 2);
                }
                if (index == 8)
                {
                    isDragging = false;
                    mouse_location = DtoW(e.GetPosition(canGraph));
                    if ((mouse_location.X > 0.9 && mouse_location.X < 1.1) || (mouse_location.X > 1.9 && mouse_location.X < 2.1) ||
                        (mouse_location.X > 2.9 && mouse_location.X < 3.1) || (mouse_location.X > 3.9 && mouse_location.X < 4.1) ||
                        (mouse_location.X > 4.9 && mouse_location.X < 5.1))
                        canGraph.Cursor = Cursors.UpArrow;
                    else
                    {
                        canGraph.Cursor = null;
                    }
                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
                
        }
        private int Selected_Ellipse(Point mouse_location)
        {
            for(int i =0; i< DataEllipse.Length;i++)
            {
                try
                {
                    double getleft = Canvas.GetLeft(DataEllipse[i]);
                    double getTop = Canvas.GetTop(DataEllipse[i]);
                    if (mouse_location.X <= getleft + 5 && mouse_location.X >= getleft &&
                        mouse_location.Y <= getTop + 5 && mouse_location.Y >= getTop )
                    {
                        canGraph.Cursor = Cursors.Hand;
                        return i;
                    }

                }
                catch (ArgumentNullException exception)
                {
                    canGraph.Cursor = null;
                }
            }
            return 8;//if there haven't matches, then return 8
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            double wxmin = -2;
            double wxmax = 10;
            double wymin = -7;
            double wymax = 11;
            const double xstep = 1;
            const double ystep = 1;

            const double dmargin = 1;
            double dxmin = dmargin;
            double dxmax = canGraph.Width - dmargin;
            double dymin = dmargin;
            double dymax = canGraph.Height - dmargin;

            // Prepare the transformation matrices.
            PrepareTransformations(
                wxmin, wxmax, wymin, wymax,
                dxmin, dxmax, dymax, dymin);

            // Get the tic mark lengths.
            Point p0 = DtoW(new Point(0, 0));
            Point p1 = DtoW(new Point(5, 5));
            double xtic = p1.X - p0.X;
            double ytic = p1.Y - p0.Y;

            // Make the X axis.
            GeometryGroup xaxis_geom = new GeometryGroup();
            p0 = new Point(wxmin, 0);
            p1 = new Point(wxmax, 0);
            xaxis_geom.Children.Add(new LineGeometry(WtoD(p0), WtoD(p1)));

           for (double x = wxmin; x <= wxmax - xstep; x += xstep)
            {
                // Add the tic mark.
                Point tic0 = WtoD(new Point(x, -ytic));
                Point tic1 = WtoD(new Point(x, ytic));
                xaxis_geom.Children.Add(new LineGeometry(tic0, tic1));

                // Label the tic mark's X coordinate.
                DrawText(canGraph, x.ToString(),
                    new Point(tic0.X, tic0.Y + 5), 0, 12,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top);
            }
          
            Path xaxis_path = new Path();
            xaxis_path.StrokeThickness = 1;
            xaxis_path.Stroke = Brushes.Black;
            xaxis_path.Data = xaxis_geom;

            canGraph.Children.Add(xaxis_path);

            // Make the Y axis.
            GeometryGroup yaxis_geom = new GeometryGroup();
            p0 = new Point(0, wymin);
            p1 = new Point(0, wymax);
            xaxis_geom.Children.Add(new LineGeometry(WtoD(p0), WtoD(p1)));

            for (double y = wymin; y <= wymax - ystep; y += ystep)
            {
                // Add the tic mark.
                Point tic0 = WtoD(new Point(-xtic, y));
                Point tic1 = WtoD(new Point(xtic, y));
                xaxis_geom.Children.Add(new LineGeometry(tic0, tic1));

                // Label the tic mark's Y coordinate.
                DrawText(canGraph, y.ToString(),
                    new Point(tic0.X - 10, tic0.Y), -90, 12,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center);
            }

            Path yaxis_path = new Path();
            yaxis_path.StrokeThickness = 1;
            yaxis_path.Stroke = Brushes.Black;
            yaxis_path.Data = yaxis_geom;

            canGraph.Children.Add(yaxis_path);

          
            // Make a title
            Point title_location = WtoD(new Point(50, 10));
            DrawText(canGraph, "Aproximator",
                title_location, 0, 20,
                HorizontalAlignment.Center,
                VerticalAlignment.Top);
        }

        private void CanGraph_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canGraph.Cursor == Cursors.Hand)
            {
                isDragging = true;
            }
        }

        private void CanGraph_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if (canGraph.Cursor == Cursors.UpArrow)
            {
                Point mouse_location = DtoW(e.GetPosition(canGraph));
                mouse_location.X = Math.Round(mouse_location.X);
                bool check = true;
                foreach (Ellipse ellipse in DataEllipse)
                {
                    try
                    {
                        if (Math.Round(DtoW(new Point(Canvas.GetLeft(ellipse) + 2, 0)).X) == mouse_location.X)
                        {
                            MessageBox.Show("You have point with this X");
                            check = false;
                            break;
                        }
                    }
                    catch (ArgumentNullException exception)
                    {

                    }
                }
                if (check)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Fill = Brushes.DarkViolet;
                    ellipse.StrokeThickness = 1;
                    ellipse.Width = 5;
                    ellipse.Height = 5;
                    canGraph.Children.Add(ellipse);
                    ellipse.Stroke = Brushes.DarkViolet;
                    
                    DataEllipse[(int)mouse_location.X] = ellipse;
                    Canvas.SetLeft(ellipse, WtoD(mouse_location).X - 2);
                    Canvas.SetTop(ellipse, WtoD(mouse_location).Y - 2);
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PointCollection MLSPoints = new PointCollection();
            PointCollection LagrangePoints = new PointCollection();
            double[,] matrix = new double[2, 5];
            try
            {
                for (int i = 1; i < 6; i++)
                {
                    Point point = new Point(Canvas.GetLeft(DataEllipse[i]), Canvas.GetTop(DataEllipse[i]));
                    point = DtoW(point);
                    matrix[0, i-1] = point.X;
                    matrix[1,i- 1] = point.Y;
                    Y[i - 1] = point.Y;
                    X[i - 1] = point.X;
                }
                CalculateService calculate = new CalculateService();
                int basis = 3+1;
                double[,] gaussMatrix = calculate.MakeSystem(matrix, basis);
                double[] result = calculate.Gauss(gaussMatrix, basis, basis + 1);
                if (result == null)
                    throw new Exception("NO");
                for (double x = 0; x < 10; x += 0.1)
                {
                    double y1 = result[0] + result[1] * x + result[2] * Math.Pow(x, 2) + result[3] * Math.Pow(x, 3);
                    Point point1 = new Point(x, y1);                        
                    MLSPoints.Add(WtoD(point1));

                    double y2 = lagrange.GetValue(X, Y, x);
                    Point point2 = new Point(x, y2);
                    LagrangePoints.Add(WtoD(point2));
                }
             
                polyline1.StrokeThickness = 1;
                polyline1.Stroke = Brushes.Red;
                polyline1.Points = MLSPoints;
                if(!canGraph.Children.Contains(polyline1))
                canGraph.Children.Add(polyline1);

                polyline2.StrokeThickness = 1;
                polyline2.Stroke = Brushes.Yellow;
                polyline2.Points = LagrangePoints;
                if (!canGraph.Children.Contains(polyline2))
                    canGraph.Children.Add(polyline2);
            }
            catch(ArgumentNullException exception)
            {
                MessageBox.Show("There aren't 5 points");
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

    }
}
