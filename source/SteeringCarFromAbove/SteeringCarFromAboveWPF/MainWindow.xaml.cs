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
        const int POSITION_STEP = 30;
        private TrackPlanner planner_ = null;

        public MainWindow()
        {
            InitializeComponent();

            planner_ = new TrackPlanner(
                locationTolerance: POSITION_STEP - 1, angleTolerance: 9.0d,
                positionStep: (int)POSITION_STEP, angleStep: 10.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            planner_.NewSuccessorFound += planner_NewSuccessorFound;

            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 1500.0, _y: 900, _angle: 90.0d);
            map.obstacles.Add(new System.Drawing.Rectangle(350, 500, 300, 50));

            //List<PositionAndOrientation> track = planner.PlanTrack(map);
            planner_.PrepareTracks(map);
            Canvas_trackPlanner.UpdateLayout();

            DrawMap(map);
            //DrawTrack(track);
        }

        private void DrawTrack(List<PositionAndOrientation> track)
        {
            foreach (PositionAndOrientation item in track)
            {
                Line l = new Line();

                const double LENGTH = POSITION_STEP;
                l.Stroke = Brushes.OrangeRed;
                l.StrokeThickness = 1;
                l.X1 = item.x;
                l.X2 = item.x - Math.Cos(item.angle / 180.0d * Math.PI) * LENGTH;
                l.Y1 = item.y;
                l.Y2 = item.y - Math.Sin(item.angle / 180.0d * Math.PI) * LENGTH;

                Canvas_trackPlanner.Children.Add(l);
            }
        }

        private void DrawMap(Map map)
        {
            DrawBorder(map);
            DrawCar(map);
            DrawParking(map);
            DrawObstacles(map);
        }

        private void DrawObstacles(Map map)
        {
            foreach (var obstacle in map.obstacles)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Red);
                rect.Width = obstacle.Width;
                rect.Height = obstacle.Height;
                Canvas.SetLeft(rect, obstacle.X);
                Canvas.SetTop(rect, obstacle.Y);
                rect.StrokeThickness = 3;

                Canvas_trackPlanner.Children.Add(rect);
            }
        }

        private void DrawParking(Map map)
        {
            const double parkingSizeX = 38;
            const double parkingSizeY = 70;

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
            const double carSizeX = 25;
            const double carSizeY = 55;

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

            const double LENGTH = POSITION_STEP;
            l.Stroke = Brushes.LightSteelBlue;
            l.StrokeThickness = 1;
            l.X1 = e.x;
            l.X2 = e.x - Math.Cos(e.angle / 180.0d * Math.PI) * LENGTH;
            l.Y1 = e.y;
            l.Y2 = e.y - Math.Sin(e.angle / 180.0d * Math.PI) * LENGTH;

            Canvas_trackPlanner.Children.Add(l);
        }


        private Point lastMouseDown_;
        private void Canvas_trackPlanner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMouseDown_ = e.GetPosition(Canvas_trackPlanner);
        }

        private void Canvas_trackPlanner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mouseUp = e.GetPosition(Canvas_trackPlanner);

            double deltaY = mouseUp.Y - lastMouseDown_.Y;
            double deltaX = mouseUp.X - lastMouseDown_.X;

            double angleInDegrees = Math.Atan2(deltaY, deltaX) * 180 / Math.PI;

            List<PositionAndOrientation> track = planner_.GetTrackFromPreparedPlanner(
                new PositionAndOrientation(lastMouseDown_.X, lastMouseDown_.Y, angleInDegrees));

            DrawTrack(track);
        }

    }
}
