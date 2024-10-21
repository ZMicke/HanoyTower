using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts;
using System.Diagnostics;

namespace HanoiTowers
{
    public partial class MainWindow : MetroWindow
    {
        private int delay = 150; // Оптимальная скорость анимации
        private HanoiTower[] towers;
        private HanoiSolver solver;
        private List<(int from, int to)> moves;
        private int currentMoveIndex = 0;
        private DispatcherTimer timer;

        private Stopwatch stopwatch; // Таймер для замера времени выполнения
        private List<double> timeData; // Список для хранения данных времени

        public MainWindow()
        {
            InitializeComponent();
            towers = new HanoiTower[] { new HanoiTower(Tower1), new HanoiTower(Tower2), new HanoiTower(Tower3) };
            solver = new HanoiSolver();

            InitializeTowerWithDisks(8); // Инициализация башни с 8 дисками

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(delay);
            timer.Tick += Timer_Tick;

            stopwatch = new Stopwatch();
            timeData = new List<double>();
            InitializeCharts(); // Инициализируем графики
        }

        // Инициализация башен и добавление дисков
        private void InitializeTowerWithDisks(int numberOfDisks)
        {
            foreach (var tower in towers)
            {
                tower.ClearDisks(); // Очищаем башни перед добавлением новых дисков
            }

            // Ширина дисков
            double maxDiskWidth = 200;
            double minDiskWidth = 50;
            double diskWidthStep = (maxDiskWidth - minDiskWidth) / numberOfDisks;

            Brush[] diskColors = new Brush[]
            {
                Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green,
                Brushes.Cyan, Brushes.Blue, Brushes.Purple, Brushes.Pink
            };

            // Размещение дисков на первой башне
            for (int i = 0; i < numberOfDisks; i++)
            {
                double diskWidth = maxDiskWidth - i * diskWidthStep;
                Rectangle disk = new Rectangle
                {
                    Width = diskWidth,
                    Height = 30,
                    Fill = diskColors[i % diskColors.Length],
                    RadiusX = 10,
                    RadiusY = 10
                };

                towers[0].AddDisk(disk); // Добавляем диск на первую башню

                // Позиционируем диск по вертикали, начиная снизу (от основания)
                double topPosition = towers[0].towerCanvas.Height - (towers[0].DiskCount() * 30);
                Canvas.SetTop(disk, topPosition);
                Canvas.SetLeft(disk, (towers[0].towerCanvas.Width / 2) - (disk.Width / 2)); // Центрируем диск
            }
        }

        // Инициализация графика времени
        private void InitializeCharts()
        {
            TimeChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Время выполнения",
                    Values = new ChartValues<double>()
                }
            };
            TimeChart.AxisX.Add(new Axis
            {
                Title = "Шаги",
                LabelFormatter = value => value.ToString()
            });
            TimeChart.AxisY.Add(new Axis
            {
                Title = "Время (мс)",
                LabelFormatter = value => value.ToString()
            });
        }

        // Обновление времени выполнения на графике
        private void ShowFinalCharts()
        {
            ((LineSeries)TimeChart.Series[0]).Values = new ChartValues<double>(timeData);
            MessageBox.Show("Алгоритм завершен! Графики обновлены.");
        }

        // Запуск решения задачи Ханойских башен
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DiskCountTextBox.Text, out int numberOfDisks) || numberOfDisks <= 0)
            {
                MessageBox.Show("Введите корректное количество дисков!");
                return;
            }

            solver.Reset();
            currentMoveIndex = 0;
            InitializeTowerWithDisks(numberOfDisks); // Инициализация башен с выбранным количеством дисков
            solver.Solve(numberOfDisks, 0, 2, 1); // Решаем задачу Ханойских башен
            moves = solver.GetMoves();
            timer.Start();
            stopwatch.Restart(); // Начинаем замер времени
        }

        // Обработка каждого шага при выполнении алгоритма
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (moves == null || currentMoveIndex >= moves.Count)
            {
                timer.Stop();
                stopwatch.Stop(); // Останавливаем замер времени

                // После завершения алгоритма показываем графики
                ShowFinalCharts();
                return;
            }

            var move = moves[currentMoveIndex];
            MoveDisk(move.from, move.to);
            currentMoveIndex++;

            // Сохраняем данные для будущего отображения
            timeData.Add(stopwatch.ElapsedMilliseconds);
        }

        // Метод для перемещения диска с одной башни на другую
        private void MoveDisk(int from, int to)
        {
            if (towers[from].DiskCount() <= 0) return;

            var disk = towers[from].RemoveDisk();
            if (disk == null) return;

            // Позиция по вертикали
            double targetTop = towers[to].GetNextDiskHeight();
            // Позиция по горизонтали
            double targetLeft = (towers[to].towerCanvas.Width / 2) - (disk.Width / 2);

            // Анимация перемещения по оси Y
            DoubleAnimation moveTop = new DoubleAnimation
            {
                From = Canvas.GetTop(disk),
                To = targetTop,
                Duration = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Анимация перемещения по оси X
            DoubleAnimation moveLeft = new DoubleAnimation
            {
                From = Canvas.GetLeft(disk),
                To = targetLeft,
                Duration = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            moveTop.Completed += (s, e) =>
            {
                Canvas.SetTop(disk, targetTop); // Устанавливаем финальное положение
                Canvas.SetLeft(disk, targetLeft);
                towers[to].AddDisk(disk); // Добавляем диск на целевую башню
            };

            disk.BeginAnimation(Canvas.TopProperty, moveTop);
            disk.BeginAnimation(Canvas.LeftProperty, moveLeft);
        }

        // Кнопка для выполнения шага вперед
        private void StepForward_Click(object sender, RoutedEventArgs e)
        {
            if (moves == null || currentMoveIndex >= moves.Count)
            {
                MessageBox.Show("Все шаги выполнены.");
                return;
            }

            var move = moves[currentMoveIndex];
            MoveDisk(move.from, move.to);
            currentMoveIndex++;

            timeData.Add(stopwatch.ElapsedMilliseconds); // Обновляем время для графика
        }

        // Кнопка для выполнения шага назад
        private void StepBack_Click(object sender, RoutedEventArgs e)
        {
            if (moves == null || currentMoveIndex <= 0)
            {
                MessageBox.Show("Нельзя вернуться дальше первого шага.");
                return;
            }

            currentMoveIndex--;
            var move = moves[currentMoveIndex];
            MoveDisk(move.to, move.from);

            timeData.Add(stopwatch.ElapsedMilliseconds); // Обновляем время для графика
        }

        // Кнопка сброса состояния башен и алгоритма
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DiskCountTextBox.Text, out int numberOfDisks) || numberOfDisks <= 0)
            {
                MessageBox.Show("Введите корректное количество дисков!");
                return;
            }

            InitializeTowerWithDisks(numberOfDisks); // Очищаем и заново создаем диски
            solver.Reset();
            currentMoveIndex = 0;
            timer.Stop();
            timeData.Clear(); // Сбрасываем данные времени
            MessageBox.Show("Башни сброшены. Начните новый цикл.");
        }

        // Выбор количества дисков
        private void ChooseButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiskCountTextBox.Text, out int numberOfDisks) && numberOfDisks > 0)
            {
                InitializeTowerWithDisks(numberOfDisks); // Инициализируем башни с выбранным количеством дисков
                MessageBox.Show($"Вы выбрали {numberOfDisks} дисков.");
            }
            else
            {
                MessageBox.Show("Введите корректное количество дисков!");
            }
        }
    }
}
