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

            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0, _y: 900, _angle: 90.0d);
            map.obstacles.Add(new System.Drawing.Rectangle(350, 500, 300, 50));

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Canvas_trackPlanner.UpdateLayout();

            DrawMap(map);
            DrawTrack(track);
        }

        private void DrawTrack(List<PositionAndOrientation> track)
        {
            foreach (PositionAndOrientation item in track)
            {
                Line l = new Line();

                const double LENGTH = 40.0d;
                l.Stroke = Brushes.OrangeRed;
                l.StrokeThickness = 1;
                l.X1 = item.x;
                l.X2 = item.x + Math.Cos(item.angle / 180.0d * Math.PI) * LENGTH;
                l.Y1 = item.y;
                l.Y2 = item.y + Math.Sin(item.angle / 180.0d * Math.PI) * LENGTH;

                Canvas_trackPlanner.Children.Add(l);
            }
        }

        private void DrawMap(Map map)
        {
            DrawBorder(map);
            DrawCar(map);
            DrawParking(map);
        }

        private void DrawParking(Map map)
        {
            const double parkingSizeX = 15;
            const double parkingSizeY = 30;

            System.Windows.Shapes.Rectangle parking = new System.Windows.Shapes.Rectangle();
            parking.Stroke = new SolidColorBrush(Colors.Red);
            parking.Width = parkingSizeX;
            parking.Height = parkingSizeY;
            Canvas.SetLeft(parking, map.parking.x - parkingSizeX / 2);
            Canvas.SetTop(parking, map.parking.y - parkingSizeY / 2);
            parking.StrokeThickness = 7;

            Canvas_trackPlanner.Children.Add(parking);
        }

        private void DrawCar(Map map)
        {
            const double carSizeX = 10;
            const double carSizeY = 24;

            System.Windows.Shapes.Rectangle car = new System.Windows.Shapes.Rectangle();
            car.Stroke = new SolidColorBrush(Colors.Red);
            car.Width = carSizeX;
            car.Height = carSizeY;
            Canvas.SetLeft(car, map.car.x - carSizeX / 2);
            Canvas.SetTop(car, map.car.y - carSizeY / 2);
            car.StrokeThickness = 7;

            Canvas_trackPlanner.Children.Add(car);
        }

        private void DrawBorder(Map map)
        {
            System.Windows.Shapes.Rectangle border = new System.Windows.Shapes.Rectangle();
            border.Stroke = new SolidColorBrush(Colors.Black);
            border.Width = map.mapSizeX;
            border.Height = map.mapSizeY;
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            border.StrokeThickness = 5;

            Canvas_trackPlanner.Children.Add(border);
        }

        private int counter = 0;
        void planner_NewSuccessorFound(object sender, PositionAndOrientation e)
        {
            Line l = new Line();

            const double LENGTH = 15.0d;
            l.Stroke = Brushes.LightSteelBlue;
            l.StrokeThickness = 1;
            l.X1 = e.x;
            l.X2 = e.x + Math.Cos(e.angle / 180.0d * Math.PI) * LENGTH;
            l.Y1 = e.y;
            l.Y2 = e.y + Math.Sin(e.angle / 180.0d * Math.PI) * LENGTH;

            Canvas_trackPlanner.Children.Add(l);
        }

    }
}
