using System;
using System.Collections.Generic;
using System.Drawing;

namespace CoreBrush
{
    public class HistoryManager : IDisposable
    {
        private readonly List<Bitmap> _stack = new List<Bitmap>();
        private readonly List<Bitmap> _redo = new List<Bitmap>();
        public int Capacity { get; set; } = 10;

        public void Clear()
        {
            foreach (var b in _stack) b.Dispose();
            foreach (var b in _redo) b.Dispose();
            _stack.Clear();
            _redo.Clear();
        }

        public void Push(Bitmap img)
        {
            // Push a copy to avoid external mutations
            _stack.Add(new Bitmap(img));
            // Trim capacity
            while (_stack.Count > Capacity)
            {
                _stack[0].Dispose();
                _stack.RemoveAt(0);
            }
            // New branch invalidates redo
            foreach (var b in _redo) b.Dispose();
            _redo.Clear();
        }

        public bool CanUndo => _stack.Count > 1;
        public bool CanRedo => _redo.Count > 0;

        public Bitmap Undo()
        {
            if (!CanUndo) throw new InvalidOperationException("Nada para desfazer.");
            // Pop current to redo
            var current = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            _redo.Add(current); // mantém bitmap para possível redo
            // Return a copy of previous
            return new Bitmap(_stack[_stack.Count - 1]);
        }

        public Bitmap Redo()
        {
            if (!CanRedo) throw new InvalidOperationException("Nada para refazer.");
            var img = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            // Move back to stack as new current
            _stack.Add(new Bitmap(img));
            return new Bitmap(img);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
