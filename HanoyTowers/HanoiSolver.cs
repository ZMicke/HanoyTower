public class HanoiSolver
{
    private List<(int from, int to)> moves;

    public HanoiSolver()
    {
        moves = new List<(int from, int to)>();
    }

    // Сброс состояния
    public void Reset()
    {
        moves.Clear();
    }

    // Основной алгоритм решения задачи Ханойских башен
    public void Solve(int n, int from, int to, int aux)
    {
        if (n == 0) return;

        Solve(n - 1, from, aux, to);
        moves.Add((from, to)); // Добавляем движение в список
        Solve(n - 1, aux, to, from);
    }

    // Возвращаем список ходов
    public List<(int from, int to)> GetMoves()
    {
        return moves;
    }
}
