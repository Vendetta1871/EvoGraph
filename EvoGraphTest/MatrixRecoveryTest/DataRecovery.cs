namespace EvoGraphTest.MatrixRecoveryTest;

public static class DataRecovery
{
    public static int[][,] Label { get; private set; } = []; 
    public static int[][,] Data { get; private set; } = [];
    
    public static int[] Clusters { get; private set; } = [];
    
    public static void Init(int count, int size, double p0, double p1, double pMiss)
    {
        Label = new int[count][,];
        Data = new int[count][,];
        Clusters = new int[count];
        for (var i = 0; i < count; i++)
        {
            Clusters[i] = Random.Shared.Next(1, size + 1);
            Label[i] = GenerateEquivalenceMatrix(size, Clusters[i]);
            Data[i] = NoiseMatrix(Label[i], size, p0, p1, pMiss);
        }
    }

    private static int[,] GenerateEquivalenceMatrix(int size, int classCount)
    {
        var classes = new int[size];

        // Заполняем так, чтобы каждый класс встретился хотя бы раз
        for (var i = 0; i < classCount; i++) classes[i] = i;

        // Остальные элементы распределяем случайно
        for (var i = classCount; i < size; i++) classes[i] = Random.Shared.Next(classCount);

        // Перемешиваем, чтобы классы шли в случайном порядке
        classes = classes.OrderBy(x => Random.Shared.Next()).ToArray();
        
        var matrix = new int[size, size];
        for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                matrix[i, j] = classes[i] == classes[j] ? 1 : 0;

        return matrix;
    }


    private static int[,] NoiseMatrix(int[,] mat, int size,  double p0, double p1, double pMiss)
    {
        var ans = new int[size, size];
        for (var i = 0; i < size; i++)
        for (var j = 0; j < size; j++)
        {
            ans[i, j] = mat[i, j];
            if (Random.Shared.NextDouble() < pMiss) ans[i, j] = -1;
  
            var rnd = Random.Shared.NextDouble();
            if (mat[i, j] == 0 && rnd < p0) ans[i, j] = 1;
            if (mat[i, j] == 1 && rnd < p1) ans[i, j] = 0;
        }

        return ans;
    }
}