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
using SteeringCarFromAbove;

namespace SteeringCarFromAboveWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 39, angleTolerance: 19.0d,
                positionStep: 40.0, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            planner.NewSuccessorFound += planner_NewSuccessorFound;

            Line l = new Line();
            l.Stroke = Brushes.LightSteelBlue;
            l.X1 = 0d;
            l.X2 = 500d;
            l.Y1 = 0d;
            l.Y2 = 500d;
            l.StrokeThickness = 2;

            Canvas_trackPlanner.Children.Add(l);


            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0, _y: 900.0, _angle: 90.0d);
            map.obstacles.Add(new System.Drawing.Rectangle(350, 500, 300, 50));

            List<PositionAndOrientation> track = planner.PlanTrack(map);
        }

        private int counter = 0;
        void planner_NewSuccessorFound(object sender, PositionAndOrientation e)
        {
            counter++;

            Line elem = new Line();

            const double LENGTH = 15.0d;
            elem.Stroke = Brushes.LightSteelBlue;
            elem.StrokeThickness = 2;
            elem.X1 = e.x;
            elem.X2 = e.x + Math.Cos(e.angle / 180.0d * Math.PI) * LENGTH;
            elem.Y1 = e.y;
            elem.Y2 = e.y + Math.Sin(e.angle / 180.0d * Math.PI) * LENGTH;
            elem.Width = 10;

            Canvas_trackPlanner.Children.Add(elem);
        }

    }
}
