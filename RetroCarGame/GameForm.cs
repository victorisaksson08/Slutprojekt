using System.Drawing.Drawing2D;

namespace RetroCarGame
{
    public class GameForm : Form
    {
        // Game constants
        private const int WINDOW_WIDTH = 500;
        private const int WINDOW_HEIGHT = 700;
        private const int LANE_COUNT = 5;
        private const int CAR_WIDTH = 50;
        private const int CAR_HEIGHT = 90;
        private const int ROAD_MARGIN = 25;
        private const int LANE_WIDTH = (WINDOW_WIDTH - 2 * ROAD_MARGIN) / LANE_COUNT;

        // Game state
        private int playerLane = 2; // Start in middle lane (0-indexed)
        private int score = 0;
        private int highScore = 0;
        private bool gameOver = false;
        private bool gameStarted = false;
        private double baseSpeed = 4.0;
        private double currentSpeed = 4.0;
        private int roadLineOffset = 0;

        // Traffic
        private List<TrafficCar> trafficCars = new List<TrafficCar>();
        private Random random = new Random();
        private int spawnCooldown = 0;

        // Timer
        private System.Windows.Forms.Timer gameTimer;

        // Colors - Retro palette
        private readonly Color roadColor = Color.FromArgb(40, 40, 40);
        private readonly Color grassColor = Color.FromArgb(34, 139, 34);
        private readonly Color lineColor = Color.FromArgb(255, 255, 100);
        private readonly Color playerCarColor = Color.FromArgb(255, 50, 50);
        private readonly Color hudColor = Color.FromArgb(255, 255, 255);
        private readonly Color skyColor = Color.FromArgb(20, 20, 50);

        // Traffic car colors
        private readonly Color[] trafficColors = new Color[]
        {
            Color.FromArgb(0, 150, 255),
            Color.FromArgb(255, 165, 0),
            Color.FromArgb(0, 200, 100),
            Color.FromArgb(200, 0, 200),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(100, 200, 255),
            Color.FromArgb(255, 100, 150)
        };

        // Input handling
        private bool moveLeftPressed = false;
        private bool moveRightPressed = false;
        private int moveCooldown = 0;

        public GameForm()
        {
            InitializeComponent();
            SetupGame();
        }

        private void InitializeComponent()
        {
            this.Text = "Retro Car Racer";
            this.ClientSize = new Size(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = skyColor;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
            this.Paint += GameForm_Paint;
        }

        private void SetupGame()
        {
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void ResetGame()
        {
            playerLane = 2;
            score = 0;
            currentSpeed = baseSpeed;
            trafficCars.Clear();
            gameOver = false;
            gameStarted = true;
            spawnCooldown = 0;
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!gameStarted || gameOver)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    ResetGame();
                }
                return;
            }

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                moveLeftPressed = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                moveRightPressed = true;
        }

        private void GameForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                moveLeftPressed = false;
                moveCooldown = 0;
            }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                moveRightPressed = false;
                moveCooldown = 0;
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!gameStarted || gameOver)
            {
                Invalidate();
                return;
            }

            // Handle player movement with cooldown
            if (moveCooldown > 0)
                moveCooldown--;

            if (moveCooldown == 0)
            {
                if (moveLeftPressed && playerLane > 0)
                {
                    playerLane--;
                    moveCooldown = 10;
                }
                else if (moveRightPressed && playerLane < LANE_COUNT - 1)
                {
                    playerLane++;
                    moveCooldown = 10;
                }
            }

            // Update road lines animation
            roadLineOffset += (int)currentSpeed;
            if (roadLineOffset >= 40)
                roadLineOffset = 0;

            // Spawn traffic
            spawnCooldown--;
            if (spawnCooldown <= 0)
            {
                SpawnTraffic();
                int minCooldown = Math.Max(20, 60 - (score / 5));
                int maxCooldown = Math.Max(40, 90 - (score / 3));
                spawnCooldown = random.Next(minCooldown, maxCooldown);
            }

            // Move traffic
            for (int i = trafficCars.Count - 1; i >= 0; i--)
            {
                trafficCars[i].Y += currentSpeed;

                // Remove off-screen cars and add score
                if (trafficCars[i].Y > WINDOW_HEIGHT + CAR_HEIGHT)
                {
                    trafficCars.RemoveAt(i);
                    score++;
                    // Increase speed every 5 points
                    currentSpeed = baseSpeed + (score / 5) * 0.5;
                }
            }

            // Check collisions
            Rectangle playerRect = GetPlayerRect();
            foreach (var car in trafficCars)
            {
                Rectangle carRect = new Rectangle(
                    ROAD_MARGIN + car.Lane * LANE_WIDTH + (LANE_WIDTH - CAR_WIDTH) / 2,
                    (int)car.Y,
                    CAR_WIDTH,
                    CAR_HEIGHT);

                if (playerRect.IntersectsWith(carRect))
                {
                    gameOver = true;
                    if (score > highScore)
                        highScore = score;
                    break;
                }
            }

            Invalidate();
        }

        private void SpawnTraffic()
        {
            int lane = random.Next(LANE_COUNT);

            // Avoid spawning on top of existing cars
            bool laneOccupied = trafficCars.Any(c => c.Lane == lane && c.Y < 100);
            if (laneOccupied)
            {
                // Try another lane
                lane = random.Next(LANE_COUNT);
                laneOccupied = trafficCars.Any(c => c.Lane == lane && c.Y < 100);
                if (laneOccupied) return;
            }

            Color color = trafficColors[random.Next(trafficColors.Length)];
            trafficCars.Add(new TrafficCar(lane, -CAR_HEIGHT, color));
        }

        private Rectangle GetPlayerRect()
        {
            int x = ROAD_MARGIN + playerLane * LANE_WIDTH + (LANE_WIDTH - CAR_WIDTH) / 2;
            int y = WINDOW_HEIGHT - CAR_HEIGHT - 40;
            return new Rectangle(x, y, CAR_WIDTH, CAR_HEIGHT);
        }

        private void GameForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None; // Retro pixel look
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            DrawRoad(g);
            DrawRoadLines(g);

            if (!gameStarted)
            {
                DrawStartScreen(g);
                return;
            }

            DrawTraffic(g);
            DrawPlayerCar(g);
            DrawHUD(g);

            if (gameOver)
            {
                DrawGameOver(g);
            }
        }

        private void DrawRoad(Graphics g)
        {
            // Grass on sides
            g.FillRectangle(new SolidBrush(grassColor), 0, 0, ROAD_MARGIN, WINDOW_HEIGHT);
            g.FillRectangle(new SolidBrush(grassColor), WINDOW_WIDTH - ROAD_MARGIN, 0, ROAD_MARGIN, WINDOW_HEIGHT);

            // Road
            g.FillRectangle(new SolidBrush(roadColor), ROAD_MARGIN, 0, WINDOW_WIDTH - 2 * ROAD_MARGIN, WINDOW_HEIGHT);

            // Road edges (white lines)
            using (Pen edgePen = new Pen(Color.White, 3))
            {
                g.DrawLine(edgePen, ROAD_MARGIN, 0, ROAD_MARGIN, WINDOW_HEIGHT);
                g.DrawLine(edgePen, WINDOW_WIDTH - ROAD_MARGIN, 0, WINDOW_WIDTH - ROAD_MARGIN, WINDOW_HEIGHT);
            }
        }

        private void DrawRoadLines(Graphics g)
        {
            using (Pen linePen = new Pen(lineColor, 2))
            {
                linePen.DashStyle = DashStyle.Custom;
                linePen.DashPattern = new float[] { 10, 10 };

                for (int i = 1; i < LANE_COUNT; i++)
                {
                    int x = ROAD_MARGIN + i * LANE_WIDTH;
                    // Animated dashed lines
                    for (int y = -40 + roadLineOffset; y < WINDOW_HEIGHT; y += 40)
                    {
                        g.FillRectangle(new SolidBrush(lineColor), x - 1, y, 3, 20);
                    }
                }
            }
        }

        private void DrawPlayerCar(Graphics g)
        {
            Rectangle rect = GetPlayerRect();
            DrawCar(g, rect, playerCarColor, true);
        }

        private void DrawTraffic(Graphics g)
        {
            foreach (var car in trafficCars)
            {
                int x = ROAD_MARGIN + car.Lane * LANE_WIDTH + (LANE_WIDTH - CAR_WIDTH) / 2;
                Rectangle rect = new Rectangle(x, (int)car.Y, CAR_WIDTH, CAR_HEIGHT);
                DrawCar(g, rect, car.Color, false);
            }
        }

        private void DrawCar(Graphics g, Rectangle rect, Color color, bool isPlayer)
        {
            // Car body
            g.FillRectangle(new SolidBrush(color), rect.X + 5, rect.Y + 10, rect.Width - 10, rect.Height - 20);

            // Car top (cabin)
            Color darkerColor = Color.FromArgb(
                Math.Max(0, color.R - 60),
                Math.Max(0, color.G - 60),
                Math.Max(0, color.B - 60));
            g.FillRectangle(new SolidBrush(darkerColor), rect.X + 10, rect.Y + 25, rect.Width - 20, rect.Height - 50);

            // Windshield
            Color windshieldColor = Color.FromArgb(100, 180, 255);
            if (isPlayer)
            {
                // Rear windshield (we see player from above/behind)
                g.FillRectangle(new SolidBrush(windshieldColor), rect.X + 12, rect.Y + 15, rect.Width - 24, 15);
            }
            else
            {
                // Front windshield (traffic coming toward us)
                g.FillRectangle(new SolidBrush(windshieldColor), rect.X + 12, rect.Y + rect.Height - 35, rect.Width - 24, 15);
            }

            // Wheels
            Color wheelColor = Color.FromArgb(30, 30, 30);
            // Left wheels
            g.FillRectangle(new SolidBrush(wheelColor), rect.X, rect.Y + 15, 6, 18);
            g.FillRectangle(new SolidBrush(wheelColor), rect.X, rect.Y + rect.Height - 33, 6, 18);
            // Right wheels
            g.FillRectangle(new SolidBrush(wheelColor), rect.X + rect.Width - 6, rect.Y + 15, 6, 18);
            g.FillRectangle(new SolidBrush(wheelColor), rect.X + rect.Width - 6, rect.Y + rect.Height - 33, 6, 18);

            // Headlights/taillights
            if (isPlayer)
            {
                // Red taillights
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), rect.X + 8, rect.Y + rect.Height - 14, 10, 6);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), rect.X + rect.Width - 18, rect.Y + rect.Height - 14, 10, 6);
            }
            else
            {
                // Yellow headlights
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 150)), rect.X + 8, rect.Y + rect.Height - 14, 10, 6);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 150)), rect.X + rect.Width - 18, rect.Y + rect.Height - 14, 10, 6);
            }

            // Outline for retro feel
            using (Pen outlinePen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(outlinePen, rect.X + 5, rect.Y + 10, rect.Width - 10, rect.Height - 20);
            }
        }

        private void DrawHUD(Graphics g)
        {
            using (Font scoreFont = new Font("Consolas", 16, FontStyle.Bold))
            using (Font speedFont = new Font("Consolas", 12, FontStyle.Bold))
            {
                // Score
                string scoreText = $"SCORE: {score}";
                g.DrawString(scoreText, scoreFont, new SolidBrush(hudColor), 10, 10);

                // High score
                string highScoreText = $"HIGH: {highScore}";
                g.DrawString(highScoreText, scoreFont, new SolidBrush(Color.Gold), WINDOW_WIDTH - 160, 10);

                // Speed indicator
                int speedLevel = (int)((currentSpeed - baseSpeed) / 0.5) + 1;
                string speedText = $"SPEED: {speedLevel}x";
                g.DrawString(speedText, speedFont, new SolidBrush(Color.Cyan), 10, 40);
            }
        }

        private void DrawStartScreen(Graphics g)
        {
            // Title
            using (Font titleFont = new Font("Consolas", 28, FontStyle.Bold))
            using (Font subFont = new Font("Consolas", 14, FontStyle.Regular))
            using (Font controlFont = new Font("Consolas", 11, FontStyle.Regular))
            {
                string title = "RETRO RACER";
                SizeF titleSize = g.MeasureString(title, titleFont);
                float titleX = (WINDOW_WIDTH - titleSize.Width) / 2;

                // Title shadow
                g.DrawString(title, titleFont, new SolidBrush(Color.FromArgb(150, 0, 0)), titleX + 3, 153);
                g.DrawString(title, titleFont, new SolidBrush(Color.Red), titleX, 150);

                // Subtitle
                string subtitle = "DODGE THE TRAFFIC!";
                SizeF subSize = g.MeasureString(subtitle, subFont);
                g.DrawString(subtitle, subFont, new SolidBrush(Color.Yellow),
                    (WINDOW_WIDTH - subSize.Width) / 2, 210);

                // Draw a sample car
                Rectangle sampleCar = new Rectangle(WINDOW_WIDTH / 2 - 25, 280, CAR_WIDTH, CAR_HEIGHT);
                DrawCar(g, sampleCar, playerCarColor, true);

                // Controls
                string[] controls = new string[]
                {
                    "CONTROLS:",
                    "",
                    "LEFT/RIGHT ARROW or A/D",
                    "to switch lanes",
                    "",
                    "Dodge traffic to score!",
                    "Speed increases every 5 points",
                    "",
                    "Press ENTER or SPACE to start!"
                };

                float startY = 400;
                foreach (string line in controls)
                {
                    SizeF lineSize = g.MeasureString(line, controlFont);
                    g.DrawString(line, controlFont, new SolidBrush(hudColor),
                        (WINDOW_WIDTH - lineSize.Width) / 2, startY);
                    startY += 22;
                }
            }
        }

        private void DrawGameOver(Graphics g)
        {
            // Semi-transparent overlay
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), 0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

            using (Font gameOverFont = new Font("Consolas", 32, FontStyle.Bold))
            using (Font scoreFont = new Font("Consolas", 18, FontStyle.Bold))
            using (Font restartFont = new Font("Consolas", 14, FontStyle.Regular))
            {
                // Game Over text
                string gameOverText = "GAME OVER";
                SizeF goSize = g.MeasureString(gameOverText, gameOverFont);
                g.DrawString(gameOverText, gameOverFont, new SolidBrush(Color.Red),
                    (WINDOW_WIDTH - goSize.Width) / 2, 220);

                // Final score
                string finalScore = $"SCORE: {score}";
                SizeF fsSize = g.MeasureString(finalScore, scoreFont);
                g.DrawString(finalScore, scoreFont, new SolidBrush(Color.White),
                    (WINDOW_WIDTH - fsSize.Width) / 2, 290);

                // High score
                string hsText = $"HIGH SCORE: {highScore}";
                SizeF hsSize = g.MeasureString(hsText, scoreFont);
                g.DrawString(hsText, scoreFont, new SolidBrush(Color.Gold),
                    (WINDOW_WIDTH - hsSize.Width) / 2, 330);

                // New high score notice
                if (score == highScore && score > 0)
                {
                    string newHS = "NEW HIGH SCORE!";
                    SizeF nhsSize = g.MeasureString(newHS, scoreFont);
                    g.DrawString(newHS, scoreFont, new SolidBrush(Color.Yellow),
                        (WINDOW_WIDTH - nhsSize.Width) / 2, 370);
                }

                // Restart prompt
                string restart = "Press ENTER or SPACE to restart";
                SizeF rSize = g.MeasureString(restart, restartFont);
                g.DrawString(restart, restartFont, new SolidBrush(Color.LightGray),
                    (WINDOW_WIDTH - rSize.Width) / 2, 430);
            }
        }
    }

    public class TrafficCar
    {
        public int Lane { get; set; }
        public double Y { get; set; }
        public Color Color { get; set; }

        public TrafficCar(int lane, double y, Color color)
        {
            Lane = lane;
            Y = y;
            Color = color;
        }
    }
}
