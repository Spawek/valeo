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
using AForge.Video.DirectShow;
using AForge.Video;
using System.Runtime.InteropServices;

namespace SteeringCarFromAboveWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int POSITION_STEP = 30;
        private TrackPlanner planner_ = null;
        GlyphRecognitionStudio.MainForm glyphRecogniser;
        IVideoSource videoSource = null;
        bool waitingForNextBaseImage = false;
        System.Drawing.Bitmap baseImage = null;
        Map map = null;
        MapBuilder mapBuilder = null;
        Image plannerBackGround = new Image();

        public MainWindow()
        {
            glyphRecogniser = new GlyphRecognitionStudio.MainForm();
            glyphRecogniser.frameProcessed += glyphRecogniser_frameProcessed;
            glyphRecogniser.Show();

            InitializeComponent();

            Canvas_trackPlanner.Children.Add(plannerBackGround); //REMOVE IT FROM HERE

            MarkerFinder markerFinder = new MarkerFinder();
            ObstaclesFinder obstaclesFinder = new ObstaclesFinder();
            ObjectsToTrace objectsToTrace = new ObjectsToTrace(new List<string>() { "s1", "s2" }, "car", "parking");

            mapBuilder = new MapBuilder(markerFinder, obstaclesFinder, objectsToTrace);

            planner_ = new TrackPlanner(
                locationTolerance: POSITION_STEP - 1, angleTolerance: 9.0d,
                positionStep: (int)POSITION_STEP, angleStep: 10.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            planner_.NewSuccessorFound += planner_NewSuccessorFound;

            //map = new Map(1000, 1000);

            //map.car = new PositionAndOrientation(_x: 500.0, _y: 100.0d, _angle: 90.0d);
            //map.parking = new PositionAndOrientation(_x: 1500.0, _y: 900, _angle: 90.0d);
            //map.obstacles.Add(new System.Drawing.Rectangle(350, 570, 300, 50));
            //map.obstacles.Add(new System.Drawing.Rectangle(350, 700, 300, 50));
            //map.obstacles.Add(new System.Drawing.Rectangle(150, 150, 50, 300));
            //map.obstacles.Add(new System.Drawing.Rectangle(150, 550, 50, 300));

            //planner_.PrepareTracks(map);

            //DrawMap(map);
        }

        // http://stackoverflow.com/questions/1118496/using-image-control-in-wpf-to-display-system-drawing-bitmap
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        void glyphRecogniser_frameProcessed(object sender, GlyphRecognitionStudio.MainForm.FrameData frameData)
        {
            if (waitingForNextBaseImage)
            {
                baseImage = frameData.getImage();
                this.Dispatcher.Invoke(new Action(() => image_baseImagePicker.Source = loadBitmap(baseImage)));

                map = mapBuilder.BuildMap(baseImage, frameData.getGlyphs());

                if (map != null)
                {
                    this.Dispatcher.Invoke(new Action(() => TextBlock_marksInfo.Text = map.ToString()));

                    this.Dispatcher.Invoke(new Action(() => DrawMap(map)));

                    this.Dispatcher.Invoke(new Action(() => plannerBackGround.Source = loadBitmap(baseImage)));
                    this.Dispatcher.Invoke(new Action(() => Canvas_trackPlanner.UpdateLayout()));
                }

                waitingForNextBaseImage = false;
                Console.WriteLine("New base frame acquired!");
            }
            else
            {
                if (map != null)
                {
                    mapBuilder.UpdateCarPosition(map, frameData.getGlyphs());

                    this.Dispatcher.Invoke(new Action(() => DrawMap(map)));

                    Console.WriteLine("Car position updated!");
                }
            }
            Console.WriteLine("Frame processed");
        }

        List<Line> lastTrack = new List<Line>();
        private void DrawTrack(List<PositionAndOrientation> track)
        {
            foreach (var item in lastTrack)
	        {
                Canvas_trackPlanner.Children.Remove(item);
	        }
            
            lastTrack.Clear();
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

                lastTrack.Add(l);
            }
        }

        private void DrawMap(Map map)
        {
            DrawBorder(map);
            DrawCar(map);
            DrawParking(map);
            DrawObstacles(map);
            Canvas_trackPlanner.UpdateLayout();
        }

        private List<System.Windows.Shapes.Rectangle> lastObstacles = new List<Rectangle>();
        private void DrawObstacles(Map map)
        {
            foreach (var obstacle in lastObstacles)
            {
                Canvas_trackPlanner.Children.Remove(obstacle);
            }
            lastObstacles.Clear();

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
                lastObstacles.Add(rect);
            }
        }

        private void DrawParking(Map map)
        {
            if (map.parking != null)
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
        }

        private System.Windows.Shapes.Polyline lastCar = null;
        private void DrawCar(Map map)
        {
            if (lastCar != null)
                Canvas_trackPlanner.Children.Remove(lastCar);

            const double CAR_WIDTH = 55;
            const double CAR_HEIGHT = 25;
            const double FRONT_SHOWER_LENTH = 10;

            System.Windows.Shapes.Polyline car = new Polyline();
            car.StrokeThickness = 7;
            car.Stroke = new SolidColorBrush(Colors.Red);
            car.Points = new PointCollection() { 
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2),
                new Point(map.car.x - CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2),
                new Point(map.car.x - CAR_WIDTH / 2, map.car.y - CAR_HEIGHT / 2),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y - CAR_HEIGHT / 2),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH + FRONT_SHOWER_LENTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y),
                new Point(map.car.x + CAR_WIDTH / 2, map.car.y + CAR_HEIGHT / 2)};

            RotateTransform rt = new RotateTransform(map.car.angle, map.car.x, map.car.y);

            car.Points = new PointCollection(car.Points.Select(x => rt.Transform(x)));
            Canvas_trackPlanner.Children.Add(car);

            lastCar = car;

            Canvas_trackPlanner.UpdateLayout();
        }

        private System.Windows.Shapes.Rectangle lastBorder = null;
        private void DrawBorder(Map map)
        {
            if (lastBorder != null)
                Canvas_trackPlanner.Children.Remove(lastBorder);

            System.Windows.Shapes.Rectangle border = new System.Windows.Shapes.Rectangle();
            border.Stroke = new SolidColorBrush(Colors.Black);
            border.Width = map.mapWidth;
            border.Height = map.mapHeight;
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            border.StrokeThickness = 5;

            Canvas_trackPlanner.Children.Add(border);
            lastBorder = border;
        }

        //http://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private int counter = 0;
        void planner_NewSuccessorFound(object sender, SteeringCarFromAbove.TrackPlanner.BFSNode e)
        {
            if (counter++ % 3 == 0)
            {
                int predecessorsCount = 0;
                SteeringCarFromAbove.TrackPlanner.BFSNode curr = e;
                while ((curr = curr.predecessor) != null) predecessorsCount++;

                Line l = new Line();

                const double LENGTH = POSITION_STEP;
                l.Stroke = new SolidColorBrush(ColorFromHSV((25.0d * predecessorsCount) % 360.0d, 0.3d, 1.0d)); //Brushes.LightSteelBlue;
                l.StrokeThickness = 1;
                l.X1 = e.position.x;
                l.X2 = e.position.x - Math.Cos(e.position.angle / 180.0d * Math.PI) * LENGTH;
                l.Y1 = e.position.y;
                l.Y2 = e.position.y - Math.Sin(e.position.angle / 180.0d * Math.PI) * LENGTH;

                Canvas_trackPlanner.Children.Add(l);
            }
        }


        private Point lastMouseDown_;
        private void Canvas_trackPlanner_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMouseDown_ = e.GetPosition(Canvas_trackPlanner);
            Console.WriteLine(String.Format("Click down: {0}, {1}", e.GetPosition(Canvas_trackPlanner).X, e.GetPosition(Canvas_trackPlanner).Y));
        }

        private void Canvas_trackPlanner_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (trackPlannerMode)
            {
                case TrackPlannerMode.SETTING_PARKING_PLACE:
                    {
                        Point mouseUp = e.GetPosition(Canvas_trackPlanner);
                        Console.WriteLine(String.Format("Click up: {0}, {1}", e.GetPosition(Canvas_trackPlanner).X, e.GetPosition(Canvas_trackPlanner).Y));

                        double deltaY = mouseUp.Y - lastMouseDown_.Y;
                        double deltaX = mouseUp.X - lastMouseDown_.X;

                        double angleInDegrees = Math.Atan2(deltaY, deltaX) * 180 / Math.PI;

                        List<PositionAndOrientation> track = planner_.GetTrackFromPreparedPlanner(
                            new PositionAndOrientation(lastMouseDown_.X, lastMouseDown_.Y, angleInDegrees));

                        DrawTrack(track);
                    }
                    break;
                case TrackPlannerMode.ADDING_OBSTACLES:
                    break;
                case TrackPlannerMode.REMOVING_OBSTACLES:
                    break;
                case TrackPlannerMode.NONE:
                    break;
                default:
                    break;
            }

        }

        private void button_ChangeVideoSource_Click(object sender, RoutedEventArgs e)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                videoSource = form.VideoDevice;
                glyphRecogniser.InjectVideoSource(videoSource);
            }
            else
            {
                Console.WriteLine("Couldnt open video source");
            }

        }

        private void button_GetNextImage_Click(object sender, RoutedEventArgs e)
        {
            waitingForNextBaseImage = true;
        }

        private bool trackPrepared = false;
        private void button_PrepareTrack_Click(object sender, RoutedEventArgs e)
        {
            if (map != null)
            {
                planner_.PrepareTracks(map);
                trackPrepared = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Get base map first!");
            }
        }

        private enum TrackPlannerMode { SETTING_PARKING_PLACE, ADDING_OBSTACLES, REMOVING_OBSTACLES, NONE }
        private TrackPlannerMode trackPlannerMode = TrackPlannerMode.NONE;

        private void button_AddObstacle_Click(object sender, RoutedEventArgs e)
        {
            if (trackPrepared)
            {
                trackPlannerMode = TrackPlannerMode.ADDING_OBSTACLES;
                button_AddObstacle.IsEnabled = false;
                button_RemoveObstacle.IsEnabled = true;
                button_SetParkingPlace.IsEnabled = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }
        }

        private void button_RemoveObstacle_Click(object sender, RoutedEventArgs e)
        {
            if (trackPrepared)
            {
                trackPlannerMode = TrackPlannerMode.REMOVING_OBSTACLES;
                button_AddObstacle.IsEnabled = true;
                button_RemoveObstacle.IsEnabled = false;
                button_SetParkingPlace.IsEnabled = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }
        }

        private void button_SetParkingPlace_Click(object sender, RoutedEventArgs e)
        {
            if (trackPrepared)
            {
                trackPlannerMode = TrackPlannerMode.SETTING_PARKING_PLACE;
                button_AddObstacle.IsEnabled = true;
                button_RemoveObstacle.IsEnabled = true;
                button_SetParkingPlace.IsEnabled = false;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Prepare track first!");
            }

        }

    }
}
