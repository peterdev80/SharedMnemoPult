using System;
using System.Collections.Generic;

namespace fmslstrap.Interface
{
    internal class MenuItem
    {
        private MenuItem _parent;
        private readonly List<MenuItem> _subitems = new List<MenuItem>();

        public event Action OnInvoke;
        public event Action<MenuItem> OnChanged;

        public string Caption { get; set; }

        public bool IsSubmenu { get; set; }

        public bool IsBold { get; set; }

        public MenuItem[] Submenu => _subitems.ToArray();

        public MenuItem Parent
        {
            get { return _parent; }
            set
            {
                if (value == null && _parent == null)
                    return;

                if (value == null && _parent != null)
                {
                    lock (_parent._subitems)
                    {
                        _parent._subitems.Remove(this);

                        _parent.RaiseOnChanged();

                        _parent = null;

                        return;
                    }
                }

                if (value != null && _parent == null)
                {
                    _parent = value;

                    lock (_parent._subitems)
                    {
                        _parent._subitems.Add(this);
                        _parent.RaiseOnChanged();

                        // ReSharper disable once RedundantJumpStatement
                        return;
                    }
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public void InsertSubItem(MenuItem Item, int Index)
        {
            lock (_subitems)
            {
                _subitems.Insert(Index, Item);
            }

            RaiseOnChanged();
        }

        public void RaiseOnChanged()
        {
            OnChanged?.Invoke(this);

            _parent?.RaiseOnChanged();
        }

        public void RaiseOnInvoke()
        {
            OnInvoke?.Invoke();
        }
    }

    internal class MenuItemSeparator : MenuItem
    {
    }
}
