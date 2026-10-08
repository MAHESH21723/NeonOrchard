using System;
using System.Collections.Generic;

namespace NeonOrchard
{
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int x, y;
        public Cell(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(Cell b) => x == b.x && y == b.y;
        public override bool Equals(object obj) => obj is Cell b && Equals(b);
        public override int GetHashCode() => x * 397 ^ y;
        public static Cell operator +(Cell a, Cell b) => new Cell(a.x + b.x, a.y + b.y);
        public static bool operator ==(Cell a, Cell b) => a.Equals(b);
        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);
    }

    public enum StepResult { Moved, Ate, Crashed, Complete }

    public sealed class Stage
    {
        public const int Count = 24;
        public int Index, Target;
        public float Interval;
        public string Name, World;
        public readonly HashSet<Cell> Rocks = new HashSet<Cell>();
        static readonly string[] Names = { "First light", "Fresh start", "Little loops", "Open meadow", "Sweet corners", "Morning glow", "Twin groves", "Garden path", "The crossing", "Side streets", "Four corners", "Green retreat", "Moon garden", "Slow river", "Night bloom", "Hidden paths", "Crystal grove", "After hours", "Golden hour", "Tight turns", "The long way", "Final harvest", "Last light", "Orchard master" };
        static readonly string[] Worlds = { "MEADOW", "GROVE", "MOONLIGHT", "GOLDEN HOUR" };
        public static Stage Create(int index)
        {
            index = Math.Max(0, Math.Min(Count - 1, index));
            var s = new Stage { Index = index, Name = Names[index], World = Worlds[index / 6], Target = 5 + index / 2, Interval = Math.Max(.125f, .235f - index * .004f) };
            if (index >= 3)
            {
                int length = 2 + index / 8;
                for (int i = 0; i < length; i++)
                {
                    s.Rocks.Add(new Cell(4 + i, 5));
                    s.Rocks.Add(new Cell(13 - i, 16));
                    if (index >= 9) { s.Rocks.Add(new Cell(4, 14 + i)); s.Rocks.Add(new Cell(13, 7 - i)); }
                }
            }
            if (index >= 12)
                for (int i = 0; i < 3; i++) { s.Rocks.Add(new Cell(8 + i, 3)); s.Rocks.Add(new Cell(7 + i, 18)); }
            if (index >= 18) { s.Rocks.Add(new Cell(2, 10)); s.Rocks.Add(new Cell(2, 11)); s.Rocks.Add(new Cell(15, 10)); s.Rocks.Add(new Cell(15, 11)); }
            return s;
        }
    }

    public sealed class SnakeModel
    {
        public const int Width = 18, Height = 22;
        public readonly List<Cell> Body = new List<Cell>();
        public readonly HashSet<Cell> Rocks = new HashSet<Cell>();
        public Cell Direction { get; private set; } = new Cell(1, 0);
        public Cell Food { get; private set; }
        public int Eaten { get; private set; }
        public int Score => Eaten * 100;
        public bool Over { get; private set; }
        public bool Endless { get; private set; }
        public Stage Stage { get; private set; }
        public float Interval => Math.Max(.085f, Stage.Interval - (Endless ? Eaten * .002f : 0));
        public int FreeCellCount => Width * Height - Rocks.Count - Body.Count;
        readonly Queue<Cell> turns = new Queue<Cell>(2);
        readonly Random random;
        public SnakeModel(int seed) { random = new Random(seed); }
        public void Reset(int level, bool endless)
        {
            Stage = Stage.Create(level); Endless = endless;
            Body.Clear(); Rocks.Clear(); turns.Clear(); Eaten = 0; Over = false; Direction = new Cell(1, 0);
            Body.Add(new Cell(7, 10)); Body.Add(new Cell(6, 10)); Body.Add(new Cell(5, 10)); Body.Add(new Cell(4, 10));
            if (!endless) Rocks.UnionWith(Stage.Rocks);
            PlaceFood();
        }
        public bool Turn(Cell d)
        {
            if (Over || Math.Abs(d.x) + Math.Abs(d.y) != 1 || turns.Count >= 2) return false;
            Cell previous = Direction;
            foreach (var q in turns) previous = q;
            if (d == previous || d.x == -previous.x && d.y == -previous.y) return false;
            turns.Enqueue(d); return true;
        }
        public StepResult Step()
        {
            if (Over) return StepResult.Crashed;
            if (turns.Count > 0) Direction = turns.Dequeue();
            Cell next = Body[0] + Direction;
            bool eat = next == Food;
            int occupied = Body.Count - (eat ? 0 : 1);
            if (!Inside(next) || Rocks.Contains(next)) return Crash();
            for (int i = 0; i < occupied; i++) if (Body[i] == next) return Crash();
            Body.Insert(0, next);
            if (!eat) { Body.RemoveAt(Body.Count - 1); return StepResult.Moved; }
            Eaten++;
            if ((!Endless && Eaten >= Stage.Target) || FreeCellCount == 0) { Over = true; return StepResult.Complete; }
            PlaceFood(); return StepResult.Ate;
        }
        StepResult Crash() { Over = true; return StepResult.Crashed; }
        void PlaceFood()
        {
            // Sampling free cells guarantees termination even on an almost full board.
            int candidate = random.Next(FreeCellCount);
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
            {
                var c = new Cell(x, y);
                if (Rocks.Contains(c) || Body.Contains(c)) continue;
                if (candidate-- == 0) { Food = c; return; }
            }
        }
        public static bool Inside(Cell c) => c.x >= 0 && c.x < Width && c.y >= 0 && c.y < Height;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int unlocked, best, fruit, skin, runs;
        public int[] stars = new int[Stage.Count];
        public bool music = true, sound = true, haptics = false, buttons = true;
        public void Sanitize()
        {
            unlocked = Math.Max(0, Math.Min(Stage.Count - 1, unlocked));
            best = Math.Max(0, best); fruit = Math.Max(0, fruit); runs = Math.Max(0, runs); skin = Math.Max(0, Math.Min(3, skin));
            if (stars == null || stars.Length != Stage.Count) stars = new int[Stage.Count];
            for (int i = 0; i < stars.Length; i++) stars[i] = Math.Max(0, Math.Min(3, stars[i]));
            if (skin > 0 && fruit < skin * 40) skin = 0;
        }
    }
}
