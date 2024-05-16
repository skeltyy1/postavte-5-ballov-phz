using superpupernasralnauchebu.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class Game : Form
    {
        private int cellSize = 20;
        private int gridSize = 20;
        private int snakeLength = 1;
        private string saveFileName = "gameSave.txt";

        private Timer timer;
        private Random random;
        private List<Point> snake;
        private Point food;
        private Direction direction;
        private bool isGameOver;
        private bool GameStarted;

        public Game()
        {
            InitializeComponent();
            snake = new List<Point>();
            timer = new Timer();
            random = new Random();
            direction = Direction.Right;
            isGameOver = false;
        }
        private void Game_Load(object sender, EventArgs e)
        {

        }
        private void SetupGame()
        {
            newGameButton.Visible = false; // Скрываем кнопку New Game
            continueButton.Visible = false; // Скрываем кнопку  Continue Game
            label2.Visible = false; // Скрываем надпись с назвнанием игры
            label1.Visible = true; // Отображаем Прогресс
            GameStarted = true; // Включаем рендеринг игры
            snake.Clear();
            for (int i = 0; i < snakeLength; i++)
            {
                snake.Add(new Point(gridSize / 2 - i, gridSize / 2));
            }
            GenerateFood();

            timer.Interval = 150;
            timer.Tick += UpdateGame;
            timer.Start();
        }

        private void GenerateFood()
        {
            int x = random.Next(gridSize);
            int y = random.Next(gridSize);

            food = new Point(x, y);
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            MoveSnake();
            CheckCollisions();
            Invalidate();
        }

        private void MoveSnake()
        {
            Point newHead = snake[0];
            switch (direction)
            {
                case Direction.Up:
                    newHead.Y--;
                    break;
                case Direction.Down:
                    newHead.Y++;
                    break;
                case Direction.Left:
                    newHead.X--;
                    break;
                case Direction.Right:
                    newHead.X++;
                    break;
            }

            if (newHead == food)
            {
                snake.Insert(0, newHead);
                GenerateFood();
                timer.Interval -= 5; // Увеличиваем скорость игры
            }
            else
            {
                snake.Insert(0, newHead);
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void CheckCollisions()
        {
            Point head = snake[0];

            if (head.X < 0 || head.X >= gridSize || head.Y < 0 || head.Y >= gridSize)
            {
                EndGame();
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (head == snake[i])
                {
                    EndGame();
                    break;
                }
            }
        }

        private void EndGame()
        {
            isGameOver = true;
            timer.Stop();
            MessageBox.Show("Game Over! Your score: " + (snake.Count - snakeLength), "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (GameStarted)
            {
                label1.Text = "Прогресс: " + (snake.Count - snakeLength);
                base.OnPaint(e);
                Graphics canvas = e.Graphics;
                canvas.Clear(Color.LightGreen);

                // Рисуем границы поля
                canvas.DrawRectangle(Pens.Black, 0, 0, gridSize * cellSize, gridSize * cellSize);

                if (!isGameOver)
                {
                    foreach (Point cell in snake)
                    {
                        canvas.FillRectangle(Brushes.Green, cell.X * cellSize, cell.Y * cellSize, cellSize, cellSize);
                    }
                    canvas.DrawImage(Resources.apple, food.X * cellSize, food.Y * cellSize, cellSize, cellSize);
                }
                else
                {
                    string gameOverText = "Game Over!\nYour score: " + (snake.Count - snakeLength);
                    SizeF textSize = canvas.MeasureString(gameOverText, Font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    canvas.DrawString(gameOverText, Font, Brushes.Black, x, y);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up when direction != Direction.Down:
                    direction = Direction.Up;
                    break;
                case Keys.Down when direction != Direction.Up:
                    direction = Direction.Down;
                    break;
                case Keys.Left when direction != Direction.Right:
                    direction = Direction.Left;
                    break;
                case Keys.Right when direction != Direction.Left:
                    direction = Direction.Right;
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGame(); // Сохраняем игру при закрытии
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            ContinueGame();
        }

        private void NewGame()
        {
            // Очищаем сохраненные данные и начинаем новую игру
            File.Delete(saveFileName);
            SetupGame();
        }

        private void ContinueGame()
        {
            GameStarted = true;
            if (File.Exists(saveFileName))
            {
                // Если есть сохраненный прогресс, загружаем его
                LoadGame();
            }
            else
            {
                // Иначе начинаем новую игру
                SetupGame();
            }
        }

        private void SaveGame()
        {
            using (StreamWriter writer = new StreamWriter(saveFileName))
            {
                // Сохраняем состояние игры в файл
                writer.WriteLine(direction); // Направление
                writer.WriteLine(timer.Interval); // Таймер
                writer.WriteLine(snake.Count); // Размер
                foreach (Point cell in snake)
                {
                    writer.WriteLine(cell.X); // X
                    writer.WriteLine(cell.Y); // Y
                }
                writer.WriteLine(food.X); // Еда X
                writer.WriteLine(food.Y); // Еда Y
            }
        }

        private void LoadGame()
        {
            newGameButton.Visible = false; // Скрываем кнопку New Game
            continueButton.Visible = false; // Скрываем кнопку  Continue Game
            label2.Visible = false; // Скрываем надпись с назвнанием игры
            label1.Visible = true; // Отображаем Прогресс
            GameStarted = true; // Включаем рендеринг игры
            if (File.Exists(saveFileName))
            {
                using (StreamReader reader = new StreamReader(saveFileName))
                {
                    direction = (Direction)Enum.Parse(typeof(Direction), reader.ReadLine());
                    timer.Interval = int.Parse(reader.ReadLine());

                    // Очищаем старые данные
                    snake.Clear();

                    int snakeLength = int.Parse(reader.ReadLine());
                    for (int i = 0; i < snakeLength; i++)
                    {
                        int x = int.Parse(reader.ReadLine());
                        int y = int.Parse(reader.ReadLine());
                        snake.Add(new Point(x, y));
                    }

                    food = new Point(int.Parse(reader.ReadLine()), int.Parse(reader.ReadLine()));
                }
                isGameOver = false;
                timer.Interval = 150;
                timer.Tick += UpdateGame;
                timer.Start();
                Invalidate();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
