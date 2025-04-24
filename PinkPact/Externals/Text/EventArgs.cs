using System.Collections.ObjectModel;
using System.Windows;
using System;

using PinkPact.Controls;
using System.Linq;

namespace PinkPact.Text
{
    /// <summary>
    /// Represents data for a text update event.
    /// </summary>
    public class TextUpdatedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the text that is shown on the text block after the update is complete.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the Text property with all original formatting patterns applied to it.
        /// </summary>
        public string FormattedText { get; }

        /// <summary>
        /// Gets if the updated text was appended to the current text in the text block or not.
        /// </summary>
        public bool IsAppended { get; }

        /// <summary>
        /// Determines if the updated text was shown on a <see cref="FormattedTextBlock"/> instantly; that is, if the '<c>i</c>' modifier was used.
        /// </summary>
        public bool Instant { get; }

        /// <summary>
        /// Gets a read-only collection of all <see cref="FormattedTextEffectGroup"/>s applied on the text.
        /// </summary>
        public ReadOnlyCollection<FormattedTextEffectGroup> Effects { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUpdatedEventArgs"/> class, specifying the updated text, and if it was appended or not.
        /// </summary>
        public TextUpdatedEventArgs(FormattedText text, bool isAppended, bool instant)
        {
            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = text.Effects.AsReadOnly();
            IsAppended = isAppended;
            Instant = instant;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUpdatedEventArgs"/> class, specifying the updated text, and if it was appended or not, also using the supplied routed event identifier.
        /// </summary>
        public TextUpdatedEventArgs(FormattedText text, bool isAppended, bool instant, RoutedEvent routedEvent) : base(routedEvent)
        {
            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = text.Effects.AsReadOnly();
            IsAppended = isAppended;
            Instant = instant;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUpdatedEventArgs"/> class, specifying the updated text, and if it was appended or not, also using the supplied routed event identifier, and providing the opportunity to declare a different source for the event.
        /// </summary>
        public TextUpdatedEventArgs(FormattedText text, bool isAppended, bool instant, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = text.Effects.AsReadOnly();
            IsAppended = isAppended;
            Instant = instant;
        }
    }

    /// <summary>
    /// Represents a <see cref="LinkHandler"/> object that was added to its parent <see cref="FormattedTextBlock"/>.
    /// </summary>
    public class LinkHandlerAddedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the <see cref="LinkHandler"/> that was added to the parent <see cref="Controls.FormattedTextBlock"/>.
        /// </summary>
        public LinkHandler Handler { get; }

        /// <summary>
        /// Gets the index of the handler in the parent <see cref="Controls.FormattedTextBlock"/>.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object.
        /// </summary>
        internal LinkHandlerAddedEventArgs(LinkHandler handler, int index)
        {
            Handler = handler;
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object, also using the supplied routed event identifier.
        /// </summary>
        internal LinkHandlerAddedEventArgs(LinkHandler handler, int index, RoutedEvent routedEvent) : base(routedEvent)
        {
            Handler = handler;
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object, also using the supplied routed event identifier, and providing the opportunity to declare a different source for the event.
        /// </summary>
        internal LinkHandlerAddedEventArgs(LinkHandler handler, int index, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            Handler = handler;
            Index = index;
        }
    }

    /// <summary>
    /// Represents a collection <see cref="LinkHandler"/> objects that were cleared from their parent <see cref="FormattedTextBlock"/>.
    /// </summary>
    public class LinkHandlersClearedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the <see cref="LinkHandler"/> objects that were cleared from the parent <see cref="Controls.FormattedTextBlock"/>.
        /// </summary>
        public ReadOnlyCollection<LinkHandler> Handlers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object.
        /// </summary>
        internal LinkHandlersClearedEventArgs(LinkHandler[] handlers)
        {
            Handlers = Array.AsReadOnly(handlers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object, also using the supplied routed event identifier.
        /// </summary>
        internal LinkHandlersClearedEventArgs(LinkHandler[] handlers, RoutedEvent routedEvent) : base(routedEvent)
        {
            Handlers = Array.AsReadOnly(handlers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandlerAddedEventArgs"/> class, specifying the newly added <see cref="LinkHandler"/> object, and its index in parent <see cref="Controls.FormattedTextBlock"/> object, also using the supplied routed event identifier, and providing the opportunity to declare a different source for the event.
        /// </summary>
        internal LinkHandlersClearedEventArgs(LinkHandler[] handlers, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            Handlers = Array.AsReadOnly(handlers);
        }
    }

    /// <summary>
    /// Represents a character that was written individually to a <see cref="FormattedTextBlock"/>.
    /// </summary>
    public class CharacterWrittenEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the text should be shown on the text block when all characters are shown.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the Text property with all original formatting patterns applied to it.
        /// </summary>
        public string FormattedText { get; }

        /// <summary>
        /// Gets the text group in which the character belongs.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the character that was written on the text block.
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// Gets the character's index in the <see cref="Text"/> property.
        /// </summary>
        public int TotalIndex { get; }

        /// <summary>
        /// Gets the character's index in the <see cref="Group"/> property.
        /// </summary>
        public int GroupIndex { get; }

        /// <summary>
        /// Gets the effects applied on the character.
        /// </summary>
        public FormattedTextEffectGroup Effects { get; }

        internal CharacterWrittenEventArgs(char character, string group, int totalIndex, int groupIndex, FormattedTextEffectGroup effects, FormattedText text)
        {
            Character = character; 
            Group = group;
            TotalIndex = totalIndex;
            GroupIndex = groupIndex;

            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = effects;
        }

        internal CharacterWrittenEventArgs(RoutedEvent routedEvent, char character, string group, int totalIndex, int groupIndex, FormattedTextEffectGroup effects, FormattedText text)
        {
            Character = character;
            Group = group;
            TotalIndex = totalIndex;
            GroupIndex = groupIndex;

            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = effects;

            RoutedEvent = routedEvent;
        }

        internal CharacterWrittenEventArgs(RoutedEvent routedEvent, object source, char character, string group, int totalIndex, int groupIndex, FormattedTextEffectGroup effects, FormattedText text)
        {
            Character = character;
            Group = group;
            TotalIndex = totalIndex;
            GroupIndex = groupIndex;

            Text = text.Text;
            FormattedText = text.AsFormatted();
            Effects = effects;

            RoutedEvent = routedEvent;
            Source = source;
        }
    }
}