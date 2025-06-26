using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

class Program
{
    static Random rnd = new Random();

    static int Main()
    {
        const int seed = 12345;

        for (int i = 1; i < 101; i++)
        {
            rnd = new Random(seed + i);

            if (!(RandomizedTest(GenerateInts()) && RandomizedTest(GenerateStrings())))
            {
                WriteLine($"\u274C Randomized test #{i} failed.");
                return 1;
            }
        }
        WriteLine("\u2705 Randomized tests passed.");

        if (!AllCasesTest())
        {
            WriteLine("\u274C All cases test failed.");
            return 1;
        }
        WriteLine("\u2705 All cases test passed.");

        return 0;
    }

    /// <summary>Performs a series of <see cref="Insertion{T}"/> and <see cref="Deletion{T}"/>, runs the <see cref="Validate{T}"/> check.</summary>
    /// <returns>False if the validation failed.</returns>
    static bool RandomizedTest<T>(List<T> input) where T : IComparable<T>
    {
        var tree = new AVLTree<T>();
        var dict = new Dictionary<T, int>();
        for (int i = 0; i < rnd.Next(40, 71); i++)
        {
            if (rnd.Next(2) != 0) Insertion(ref tree, ref dict, input);
            else Deletion(ref tree, ref dict, input);
        }

        if (!Validate(tree, dict)) return false;
        if (!tree.IsAVL())
        {
            WriteIssue("The current tree in not a valid AVL tree.");
            return false;
        }

        return true;
    }


    /// <summary>Checks <see cref="AVLTree{T}.Count"/>, <see cref="AVLTree{T}.KeyCount(T)"/>, <see cref="AVLTree{T}.Min()"/>, <see cref="AVLTree{T}.Max()"/> for correctness.</summary>
    /// <returns>False if one of the parameters is incorrect.</returns>
    static bool Validate<T>(AVLTree<T> tree, Dictionary<T, int> expectedCounts) where T : IComparable<T>
    {
        int expectedCount = expectedCounts.Values.Sum();
        if (tree.Count != expectedCount)
        {
            WriteIssue($"Count mismatch: expected {expectedCount}, outcome {tree.Count}.");
            return false;
        }

        foreach (var kvp in expectedCounts)
        {
            int kc = tree.KeyCount(kvp.Key);
            if (kc != kvp.Value)
            {
                WriteIssue($"KeyCount mismatch for {kvp.Key}: expected {kvp.Value}, outcome {kc}.");
                return false;
            }
        }

        if (expectedCounts.Count > 0)
        {
            var expectedMin = expectedCounts.Keys.Min();
            var expectedMax = expectedCounts.Keys.Max();
            var min = tree.Min();
            var max = tree.Max();
            if (!Equals(min, expectedMin))
            {
                WriteIssue($"Min mismatch: expected {expectedMin}, outcome {min}.");
                return false;
            }
            if (!Equals(max, expectedMax))
            {
                WriteIssue($"Max mismatch: expected {expectedMax}, outcome {max}.");
                return false;
            }
        }

        return true;
    }

    /// <summary>Performs a series of <see cref="AVLTree{T}.Insert(T, int)"/> and insertions to <see cref="Dictionary{T, int}"/> with the same input values.</summary>
    static void Insertion<T>(ref AVLTree<T> tree, ref Dictionary<T, int> dict, List<T> input) where T : IComparable<T>
    {
        var grouped = input.GroupBy(x => x).OrderBy(_ => rnd.Next()).Take(rnd.Next(input.Count / 3, input.Count));
        foreach (var group in grouped)
        {
            tree.Insert(group.Key, group.Count());
            dict[group.Key] = dict.GetValueOrDefault(group.Key) + group.Count();
        }
    }

    /// <summary>Performs a series of <see cref="AVLTree{T}.Delete(T, bool)"/> and <see cref="Dictionary{T, int}.Remove(T)"/> with the same input keys.</summary>
    static void Deletion<T>(ref AVLTree<T> tree, ref Dictionary<T, int> dict, List<T> input) where T : IComparable<T>
    {
        var toDelete = input.OrderBy(_ => rnd.Next()).Take(input.Count - rnd.Next(input.Count / 2)).ToList();
        foreach (var key in toDelete)
        {
            if (dict.ContainsKey(key))
            {
                if (rnd.Next(5) == 0)
                {
                    tree.Delete(key, true);
                    dict.Remove(key);
                }
                else
                {
                    tree.Delete(key);
                    dict[key]--;
                    if (dict[key] <= 0) dict.Remove(key);
                }
            }
        }
    }

    /// <returns>A randomized list of integers.</returns>
    static List<int> GenerateInts()
    {
        return Enumerable.Range(0, rnd.Next(3, 54))
                         .Select(_ => rnd.Next(-10, 101)) // small range to test repetition workarounds
                         .ToList();
    }

    /// <returns>A randomized list of strings.</returns>
    static List<string> GenerateStrings()
    {
        var list = new List<string>();
        int count = rnd.Next(3, 54);
        for (int i = 0; i < count; i++)
        {
            int len = rnd.Next(1, 8);
            char[] chars = new char[len];
            for (int j = 0; j < len; j++)
                chars[j] = (char)rnd.Next('a', 'z' + 1);
            list.Add(new string(chars));
        }
        return list;
    }

    /// <summary>Writes a <paramref name="message"/> into the console in a red font.</summary>
    static void WriteIssue(string message)
    {
        ForegroundColor = ConsoleColor.Red;
        WriteLine(message);
        ResetColor();
    }

    /// <summary>Tests individual deletion, insertion, rotation cases.</summary>
    /// <returns>False if one of the tests failed.</returns>
    static bool AllCasesTest()
    {
        var tree = new AVLTree<int>();

        // Exception test
        bool expectedException = false;
        try
        {
            var min = tree.Min();
        }
        catch (InvalidOperationException)
        {
            expectedException = true;
        }
        catch (Exception ex)
        {
            WriteIssue($"Unexpected exception caught extracting Min() from an empty tree: {ex.GetType().Name} - {ex.Message}");
            return false;
        }

        if (!expectedException)
        {
            WriteIssue("Expected InvalidOperationException was not thrown.");
            return false;
        }

        // Deletions test
        try
        {
            tree.Delete(1);
            tree.Insert(10);
            tree.Delete(1);
        }
        catch (Exception ex)
        {
            WriteIssue($"Deletion of nonexistent key test failed: {ex.GetType().Name} - {ex.Message}");
        }

        if (!tree.IsAVL())
        {
            WriteIssue("Deletion of nonexistent key test failed.");
            return false;
        }

        // Insertion test
        tree.Insert(10, 2);
        if (tree.KeyCount(10) != 3 || !tree.IsAVL())
        {
            WriteIssue("Insertion of duplicate key test failed.");
            return false;
        }

        // Rotations tests
        tree.Insert(11);
        tree.Insert(12);
        if (!tree.IsAVL())
        {
            WriteIssue("Left rotation test failed.");
            return false;
        }

        tree.Insert(9);
        tree.Insert(8);
        tree.Insert(7);
        tree.Insert(6);
        if (!tree.IsAVL())
        {
            WriteIssue("Right rotation test failed.");
            return false;
        }

        tree = new AVLTree<int>();
        tree.Insert(3);
        tree.Insert(1);
        tree.Insert(2);
        if (!tree.IsAVL())
        {
            WriteIssue("Left-right rotation test failed.");
            return false;
        }

        tree = new AVLTree<int>();
        tree.Insert(1);
        tree.Insert(3);
        tree.Insert(2);
        if (!tree.IsAVL())
        {
            WriteIssue("Right-left rotation test failed.");
            return false;
        }

        return true;
    }
}