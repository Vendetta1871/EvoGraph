// See https://aka.ms/new-console-template for more information
namespace EvoGraph;

public static class Program
{
    public static void Main()
    {
        int[] a = [0, 2, 3, 3, 5, 10];
        CompactLabelsFast(a);
        foreach (var i in a)
            Console.Write($"{i}, ");
        Console.WriteLine("Hello, World!");
    }
    
    public static void CompactLabelsFast(int[] colors)
    {
        if (colors.Length == 0) return;

        // Находим максимальную метку
        int maxColor = 0;
        foreach (var c in colors)
            if (c > maxColor) maxColor = c;

        // Таблица отображений
        int[] map = new int[maxColor + 1];
        for (int i = 0; i <= maxColor; i++)
            map[i] = -1;

        // Заполняем новую нумерацию
        int nextLabel = 0;
        foreach (var c in colors)
            if (map[c] == -1)
                map[c] = nextLabel++;

        // Перенумеровываем
        for (int i = 0; i < colors.Length; i++)
            colors[i] = map[colors[i]];
    }
}