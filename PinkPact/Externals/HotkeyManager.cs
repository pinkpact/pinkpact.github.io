using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace PinkPact.Externals
{
    /// <summary>
    /// Manages hotkeys in a WPF application.
    /// </summary>
    public static class HotkeyManager
    {
        static HashSet<Hotkey> hotkeys = new HashSet<Hotkey>();

        static HotkeyManager()
        {
            ((Action)(async () =>
            {
                while (true)
                {
                    foreach (var hotkey in hotkeys) hotkey.Check();
                    await Task.Delay(10);
                }
            }))();
        }

        /// <summary>
        /// Adds a new anonymous hotkey. When all <paramref name="keys"/> are pressed in order, <paramref name="action"/> will execute.
        /// </summary>
        public static void Add(Action action, params Key[] keys)
        {
            hotkeys.Add(new Hotkey(action, keys));
        }

        /// <summary>
        /// Adds a new anonymous hotkey. When all <paramref name="keys"/> are pressed in order, <paramref name="action"/> will execute only if <paramref name="predicate"/>(<paramref name="keys"/>) returns <see langword="true"/>.
        /// </summary>
        public static void Add(Action action, Func<Key[], bool> predicate, params Key[] keys)
        {
            hotkeys.Add(new Hotkey(action, predicate, keys));
        }

        /// <summary>
        /// Adds a new hotkey named <paramref name="alias"/>. When all <paramref name="keys"/> are pressed in order, <paramref name="action"/> will execute.
        /// <para>Note: Aliases are not exclusive. This means that two or more hotkeys may have the same <paramref name="alias"/>.</para>
        /// </summary>
        public static void Add(string alias, Action action, params Key[] keys)
        {
            hotkeys.Add(new Hotkey(action, keys) { Alias = alias });
        }

        /// <summary>
        /// Adds a new hotkey named <paramref name="alias"/>. When all <paramref name="keys"/> are pressed in order, <paramref name="action"/> will execute only if <paramref name="predicate"/>(<paramref name="keys"/>) returns <see langword="true"/>.
        /// <para>Note: Aliases are not exclusive. This means that two or more hotkeys may have the same <paramref name="alias"/>.</para>
        /// </summary>
        public static void Add(string alias, Action action, Func<Key[], bool> predicate, params Key[] keys)
        {
            hotkeys.Add(new Hotkey(action, predicate, keys) { Alias = alias });
        }

        /// <summary>
        /// Removes all hotkeys represented by the key combination <paramref name="keys"/>.
        /// </summary>
        public static void Remove(params Key[] keys)
        {
            hotkeys = hotkeys.Where(x => !x.Keys.SequenceEqual(keys)).ToHashSet();
        }

        /// <summary>
        /// Removes all hotkeys with the specified <paramref name="alias"/>.
        /// </summary>
        public static void Remove(string alias)
        {
            hotkeys = hotkeys.Where(x => x.Alias != alias).ToHashSet();
        }

        /// <summary>
        /// Removes all hotkeys represented by the key combination <paramref name="keys"/> that execute the action <paramref name="action"/>.
        /// </summary>
        public static void Remove(Action action, params Key[] keys)
        {
            hotkeys = hotkeys.Where(x => !x.Keys.SequenceEqual(keys) && x.Action != action).ToHashSet();
        }

        /// <summary>
        /// Disables all hotkeys with the specified <paramref name="alias"/>.
        /// </summary>
        public static void Disable(string alias)
        {
            foreach (var h in hotkeys.Where(x => x.Alias == alias)) h.Disabled = true;
        }

        /// <summary>
        /// Disables all hotkeys represented by the key combination <paramref name="keys"/>.
        /// </summary>
        public static void Disable(params Key[] keys)
        {
            foreach (var h in hotkeys.Where(x => x.Keys.SequenceEqual(keys))) h.Disabled = true;
        }

        /// <summary>
        /// Enables all hotkeys with the specified <paramref name="alias"/>.
        /// </summary>
        public static void Enable(string alias)
        {
            foreach (var h in hotkeys.Where(x => x.Alias == alias)) h.Disabled = false;
        }

        /// <summary>
        /// Enables all hotkeys represented by the key combination <paramref name="keys"/>.
        /// </summary>
        public static void Enable(params Key[] keys)
        {
            foreach (var h in hotkeys.Where(x => x.Keys.SequenceEqual(keys))) h.Disabled = false;
        }

        /// <summary>
        /// Gets all hotkeys represented by the key combination <paramref name="keys"/> as <see cref="Hotkey"/> objects.
        /// </summary>
        public static Hotkey[] Get(params Key[] keys)
        {
            return hotkeys.Where(x => x.Keys.SequenceEqual(keys)).ToArray();
        }

        /// <summary>
        /// Gets all hotkeys with the alias <paramref name="alias"/> as <see cref="Hotkey"/> objects.
        /// </summary>
        public static Hotkey[] Get(string alias)
        {
            return hotkeys.Where(x => x.Alias == alias).ToArray();
        }

        /// <summary>
        /// Waits for the first hotkey with the specified <paramref name="alias"/> to be triggered.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task Await(string alias)
        {
            var hotkey = hotkeys.FirstOrDefault(x => x.Alias == alias) ?? throw new InvalidOperationException("No hotkey with alias " + alias + " exists.");
           
            while (hotkey.Keys.Select(x => Keyboard.IsKeyDown(x)).Any(x => x)) await Task.Delay(5);
            while (!hotkey.Triggered) await Task.Delay(5);
        }

        /// <summary>
        /// Waits for the hotkey represented by the key combination <paramref name="keys"/> to be triggered.
        /// </summary>
        public static async Task Await(params Key[] keys)
        {
            var hotkey = new Hotkey(null, keys);
            while (keys.Select(x => Keyboard.IsKeyDown(x)).Any(x => x)) await Task.Delay(5);
            while (!hotkey.Check()) await Task.Delay(5);
        }

        /// <summary>
        /// Represents a hotkey.
        /// </summary>
        public class Hotkey
        {
            /// <summary>
            /// Gets or sets the action represented by this <see cref="Hotkey"/>.
            /// </summary>
            public Action Action { get; set; }

            /// <summary>
            /// Gets or sets the predicate necessary to be passed for this hotkey to be active.
            /// </summary>
            public Func<Key[], bool> Predicate { get; set; }

            /// <summary>
            /// Gets or sets the hotkeys represented by this <see cref="Hotkey"/>.
            /// </summary>
            public Key[] Keys { get; set; }

            /// <summary>
            /// Gets or sets the alias of this hotkey.
            /// </summary>
            public string Alias { get; set; }

            /// <summary>
            /// Determines if this hotkey is disabled or not. Disabled hotkeys cannot be active.
            /// </summary>
            public bool Disabled { get; set; }

            /// <summary>
            /// Determines if the last hotkey check yielded <see langword="true"/> or not.
            /// </summary>
            public bool Triggered { get; set; }

            readonly Dictionary<Key, KeyHandler> handlers = new Dictionary<Key, KeyHandler>();

            /// <summary>
            /// Creates a new instance of the <see cref="Hotkey"/> class using the specified <paramref name="keys"/> and <paramref name="action"/>.
            /// </summary>
            public Hotkey(Action action, params Key[] keys)
            {
                Keys = keys;
                Action = action;

                foreach (var key in keys) handlers.Add(key, new KeyHandler(key));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Hotkey"/> class using the specified <paramref name="keys"/>, <paramref name="action"/> and <paramref name="predicate"/>.
            /// </summary>
            public Hotkey(Action action, Func<Key[], bool> predicate, params Key[] keys)
            {
                Keys = keys;
                Action = action;
                Predicate = predicate;

                foreach (var key in keys) handlers.Add(key, new KeyHandler(key));
            }

            /// <summary>
            /// Checks if the hotkey is active. The hotkey is active if it is not disabled, all of its keys have been pressed in order, and Predicate(Keys) returns <see langword="true"/>.
            /// </summary>
            public bool Check()
            {
                // Here just do a general check on the keys
                // That is, if a key is not down, instant return.
                // In the end, at least 1 key must've JUST been pressed for the hotkey to work.
                // Each key's hold counter is reset ONLY when it's let go, so '1' is equivalent to an immediate press.
                // By holding it longer, the counter will just keep going, and so it won't be 1.
                // This prevents the holding issue.

                bool pressed = false;
                for (int i = 0; i < Keys.Length; i++)
                {
                    var handler = handlers[Keys[i]];
                    if (Keyboard.IsKeyDown(Keys[i])) handler.CheckState++;
                    if (Keyboard.IsKeyUp(Keys[i]))
                    {
                        handler.CheckState = 0;
                        return false;
                    }

                    pressed |= handler.CheckState == 1;
                }

                // This checks the order.
                // Realistically, when a human presses the keys, each key will have a lower hold counter than the previous.
                // So if that isn't respected, we'll know that the keys weren't pressed in order.

                for (int i = 1; i < Keys.Length; i++) if (handlers[Keys[i]].CheckState >= handlers[Keys[i - 1]].CheckState) return false;

                if (pressed && !Disabled && (Predicate?.Invoke(Keys) ?? true))
                {
                    Action?.Invoke();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Represents a local key handler.
        /// </summary>
        class KeyHandler
        {
            /// <summary>
            /// The <c>Key</c> that is assigned to this <c>KeyHandler</c>.
            /// </summary>
            public Key Key
            {
                get => key;
                set
                {
                    key = value;
                    state = 0;
                }
            }

            /// <summary>
            /// Checks if the <c>Key</c> is in a toggled state.
            /// <para>
            /// Remark: the <c>Toggled</c> state is dependent to the instance of the <c>KeyHandler</c>, not the actual state of the key.
            /// </para>
            /// </summary>
            public bool Toggled { get; set; } = false;

            /// <summary>
            /// Gets or sets the "check" state of his <see cref="KeyHandler"/>.<br/>Primarily used as a unit of time for how long the key represented by this handler has been pressed.
            /// </summary>
            public long CheckState
            {
                get => state;
                set => state = value;
            }

            long state = 0;
            Key key = Key.None;

            /// <summary>
            /// Creates a new instance of the <c>KeyHandler</c> class.
            /// </summary>
            /// <param name="k"></param>
            public KeyHandler(Key k) => Key = k;

            /// <summary>
            /// Checks if <c>Key</c> is in a toggled state (dependent to its instance).
            /// <para>
            /// Returns: <c>Toggled</c>
            /// </para>
            /// </summary>
            /// <returns></returns>
            public bool CheckToggle()
            {
                if (Keyboard.IsKeyDown(Key)) if (state == 0) { state++; Toggled = true; } else if (state == -1) { Toggled = false; }
                if (Keyboard.IsKeyUp(Key)) if (Toggled) state = -1; else state = 0;
                return Toggled;
            }
        }
    }
}
