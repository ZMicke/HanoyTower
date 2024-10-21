using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace HanoiTowers
{
    public class HanoiTower
    {
        public Canvas towerCanvas;
        private double baseHeight = 20; // Высота основания башни
        private double baseWidth; // Ширина основания, которая будет динамически изменяться в зависимости от первого диска
        private double poleHeight; // Высота стержня
        private double diskHeight = 30; // Высота каждого диска
        private double poleWidth = 10; // Ширина стержня

        public HanoiTower(Canvas canvas)
        {
            this.towerCanvas = canvas;
            this.baseWidth = 230; // Начальная ширина основания
            UpdatePoleHeight(300); // Устанавливаем начальную высоту стержня
            DrawBaseAndPole();
        }

        // Метод для обновления высоты стержня (стержень растет только вверх)
        public void UpdatePoleHeight(double height)
        {
            poleHeight = height; // Устанавливаем новую высоту стержня
            DrawBaseAndPole(); // Перерисовываем основание и стержень
        }

        // Метод для установки ширины основания на основе самого большого диска
        public void SetBaseWidth(double maxDiskWidth)
        {
            baseWidth = maxDiskWidth + 20; // Основание шире самого большого диска на 20 пикселей
            DrawBaseAndPole(); // Перерисовываем основание
        }

        // Метод для отрисовки основания и стержня
        private void DrawBaseAndPole()
        {
            towerCanvas.Children.Clear(); // Очищаем канвас

            // Отрисовка основания башни (ширина основания больше первого диска)
            Rectangle towerBase = new Rectangle
            {
                Width = baseWidth, // Основание шире самого большого диска
                Height = baseHeight,
                Fill = Brushes.DarkGray
            };
            Canvas.SetTop(towerBase, towerCanvas.Height - baseHeight); // Располагаем основание внизу башни
            Canvas.SetLeft(towerBase, (towerCanvas.Width / 2) - (baseWidth / 2)); // Центрируем основание по горизонтали

            // Отрисовка стержня башни (растет только вверх)
            Rectangle towerPole = new Rectangle
            {
                Width = poleWidth,
                Height = poleHeight,
                Fill = Brushes.DarkGray
            };
            // Стержень располагаем так, чтобы он "рос" только вверх от основания
            Canvas.SetTop(towerPole, towerCanvas.Height - baseHeight - poleHeight); // Стержень растет вверх от основания
            Canvas.SetLeft(towerPole, (towerCanvas.Width / 2) - (poleWidth / 2)); // Центрируем стержень по горизонтали

            // Добавляем основание и стержень на канвас
            towerCanvas.Children.Add(towerBase);
            towerCanvas.Children.Add(towerPole);
        }

        // Метод для очистки дисков на башне
        public void ClearDisks()
        {
            // Очищаем только диски, оставляя стержень и основание
            var elementsToRemove = new List<UIElement>();
            foreach (UIElement element in towerCanvas.Children)
            {
                if (element is Rectangle disk && disk.Height == diskHeight)
                {
                    elementsToRemove.Add(disk);
                }
            }
            foreach (var element in elementsToRemove)
            {
                towerCanvas.Children.Remove(element);
            }
        }

        // Метод для добавления дисков
        public void AddDisk(Rectangle disk)
        {
            // Найдем позицию для следующего диска (самого верхнего)
            double nextDiskTop = GetNextDiskHeight();

            // Размещаем диск на башне
            Canvas.SetTop(disk, nextDiskTop);
            Canvas.SetLeft(disk, (towerCanvas.Width / 2) - (disk.Width / 2)); // Центрируем диск
            towerCanvas.Children.Add(disk);
        }

        // Метод для получения высоты для следующего диска
        public double GetNextDiskHeight()
        {
            // Считаем количество дисков, уже находящихся на башне
            int diskCount = 0;
            foreach (UIElement element in towerCanvas.Children)
            {
                if (element is Rectangle disk && disk.Height == diskHeight)
                {
                    diskCount++;
                }
            }

            // Возвращаем позицию для следующего диска (снизу вверх, начиная от основания)
            return towerCanvas.Height - baseHeight - (diskCount + 1) * diskHeight;
        }

        // Метод для удаления диска с башни
        public Rectangle RemoveDisk()
        {
            // Ищем самый верхний диск (по высоте)
            Rectangle topDisk = null;
            double topPosition = double.MaxValue;

            foreach (UIElement element in towerCanvas.Children)
            {
                if (element is Rectangle disk && disk.Height == diskHeight)
                {
                    double currentPosition = Canvas.GetTop(disk);
                    if (currentPosition < topPosition)
                    {
                        topPosition = currentPosition;
                        topDisk = disk;
                    }
                }
            }

            if (topDisk != null)
            {
                towerCanvas.Children.Remove(topDisk);
            }

            return topDisk;
        }

        // Метод для получения количества дисков на башне
        public int DiskCount()
        {
            int count = 0;
            foreach (UIElement element in towerCanvas.Children)
            {
                if (element is Rectangle disk && disk.Height == diskHeight)
                {
                    count++;
                }
            }
            return count;
        }
    }


}
