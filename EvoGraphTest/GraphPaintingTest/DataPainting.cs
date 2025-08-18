namespace EvoGraphTest.GraphPaintingTest;

public class DataPainting
{
    public static int[][,] Data { get; private set; } = [];

    public static int[] Label { get; private set; } = [];
    
    public static void Init(int count, int size, bool greedy = true)
    {
        Data = new int[count][,];
        Label = new int[count];
        for (var i = 0; i < count; i++)
        {
            var extraEdgeProb = Random.Shared.NextDouble() * 0.5;
            Data[i] = GenerateRandomConnectedGraphs(size, extraEdgeProb);
            Label[i] = greedy ? GreedyColoring(Data[i]) : FindChromaticNumber(Data[i]);
        }
    }

    public static int[,] GenerateRandomConnectedGraphs(int n, double extraEdgeProb)
    {
        var adj = new int[n, n];

        // 1) Создаём случайное остовное дерево — гарантия связности
        //    способ: случайная перестановка вершин, соединяем последовательно
        var perm = RandomPermutation(n);
        for (var i = 1; i < n; i++)
        {
            var u = perm[i - 1];
            var v = perm[i];
            adj[u, v] = 1;
            adj[v, u] = 1;
        }

        // 2) Добавляем случайные дополнительные ребра с вероятностью extraEdgeProb
        for (var i = 0; i < n; i++)
        for (var j = i + 1; j < n; j++)
            if (adj[i, j] == 0) // ещё нет ребра
                if (Random.Shared.NextDouble() < extraEdgeProb)
                    adj[i, j] = adj[j, i] = 1;

        return adj;
    }

    private static int[] RandomPermutation(int n)
    {
        var a = new int[n];
        for (var i = 0; i < n; i++) a[i] = i;
        // Fisher–Yates shuffle
        for (var i = n - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }
    
    public static int FindChromaticNumber(int[,] adj)
    {
        int n = adj.GetLength(0);
        var colors = new int[n];
        int minColors = n;

        void Backtrack(int v, int usedColors)
        {
            if (v == n)
            {
                minColors = Math.Min(minColors, usedColors);
                return;
            }
            if (usedColors >= minColors) return; // pruning

            for (var c = 1; c <= usedColors + 1; c++)
            {
                if (!IsSafe(adj, colors, v, c)) continue;
                colors[v] = c;
                Backtrack(v + 1, Math.Max(usedColors, c));
                colors[v] = 0;
            }
        }

        Backtrack(0, 0);
        return minColors;
    }
    
    private static bool IsSafe(int[,] adj, int[] colors, int v, int color)
    {
        for (int i = 0; i < adj.GetLength(0); i++)
            if (adj[v, i] == 1 && colors[i] == color)
                return false;
        return true;
    }
    
    public static int GreedyColoring(int[,] adj)
    {
        int n = adj.GetLength(0);
        int[] colors = new int[n];
        colors[0] = 1;

        for (int u = 1; u < n; u++)
        {
            var used = new bool[n + 1];
            for (int v = 0; v < n; v++)
                if (adj[u, v] == 1 && colors[v] != 0)
                    used[colors[v]] = true;

            int color;
            for (color = 1; color <= n; color++)
                if (!used[color]) break;

            colors[u] = color;
        }

        int maxColor = 0;
        foreach (var c in colors) maxColor = Math.Max(maxColor, c);
        return maxColor;
    }
}