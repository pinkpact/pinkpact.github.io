using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System;

using PinkPact.Helpers;
using PinkPact.Animations;
using Txt = PinkPact.Text;
using System.Security.Policy;
using System.Windows.Threading;

namespace PinkPact.Controls
{
    /// <summary>
    /// Represents a text block which supports the use of formatted text.
    /// </summary>
    public class FormattedTextBlock : TextBlock
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when the text in a <see cref="FormattedTextBlock"/> changes.
        /// </summary>
        public delegate void TextUpdatedEventHandler(object sender, Txt.TextUpdatedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Txt.LinkHandler"/> is added to a <see cref="FormattedTextBlock"/>.
        /// </summary>
        public delegate void LinkHandlerAddedEventHandler(object sender, Txt.LinkHandlerAddedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when all <see cref="Txt.LinkHandler"/> objects are cleared from a <see cref="FormattedTextBlock"/>.
        /// </summary>
        public delegate void LinkHandlersClearedEventHandler(object sender, Txt.LinkHandlersClearedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a character is written to a <see cref="FormattedTextBlock"/>.
        /// </summary>
        public delegate void CharacterWrittenEventHandler(object sender, Txt.CharacterWrittenEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the string of formatted text that will be displayed on this <see cref="FormattedTextBlock"/>.
        /// <para>
        /// A formatting pattern will appear as <c>(</c>{<c>effectType</c>}{<c>value</c>}<c>,</c>etc.<c>)[</c>{<c>affectedText</c>}<c>]</c>. Any brackets inside <c>affectedText</c> have to be escaped with \.
        /// </para>
        /// <para>
        /// <c>effectType</c> can be the following:
        /// <list type="bullet">
        /// <item>s (double): <c>affectedText</c> will shake with the intensity of <c>value</c>.</item>
        /// <item>w (double): <c>affectedText</c> will wave with the intensity of <c>value</c>.</item>
        /// <item>f (double): The font size of <c>affectedText</c> will be amplified by <c>value</c>.</item>
        /// <item>c (color(s)): <c>afftectedText</c> will have the color <c>value</c>. Two colors can be specified through a dash; this will animate <c>value</c> between the two colors.</item>
        /// <item>b (no value): <c>afftectedText</c> will be bold.</item>
        /// <item>i (no value): <c>afftectedText</c> will be italic.</item>
        /// <item>p (no value): In a <see cref="FormattedTextBlock"/>, instead of a standard fade animation, <c>afftectedText</c> will have a "pin" animation.</item>
        /// <item>fd (int): In a <see cref="FormattedTextBlock"/>, <c>afftectedText</c> will fade in <c>value</c> milliseconds.</item>
        /// <item>ff (string): <c>afftectedText</c> will use the font <c>value</c>. The value must follow the format '<c>`</c>{<c>fontName</c>}<c>`</c>'</item>
        /// </list>
        /// </para>
        /// <para>
        /// Any desired modifiers will be specified at the beginning of the string. Modifiers appear as <c>&lt;</c>{<c>modifierType</c>}<c>,</c>etc.<c>&gt;</c>{<c>text</c>}.
        /// </para>
        /// <para>
        /// <c>modifierType</c> can be the following:
        /// <list type="bullet">
        /// <item>- : No modifiers. If used, no other modifiers can be specified.</item>
        /// <item>i : In a <see cref="FormattedTextBlock"/>, <c>text</c> will appear instantly instead of fading in. Previous text will also disappear instantly instead of fading out.</item>
        /// <item>r : In a <see cref="FormattedTextBlock"/>, each character of <c>text</c> will appear instantly instead of fading in. Different to the 'i' modifier, the character dealy will still apply.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <returns>
        /// The unformatted text that is displayed on this <see cref="FormattedTextBlock"/>. To get the formatted version, use the <see cref="GetFormattedText"/> method instead.
        /// </returns>
        public new string Text
        {
            get => GetValue(TextProperty) as string;
            set
            {
                text = Txt.FormattedText.Parse(value);
                SetValue(TextProperty, text.Text);
                AddToQueue(text, false);
            }
        }

        /// <summary>
        /// Gets the text that is curretly visible in this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public string CurrentText
        {
            get => executingText.Text;
        }

        /// <summary>
        /// Determines, when text fades in, if characters will appear one after another.
        /// </summary>
        /// <returns>
        /// The value indicating if characters will appear one after another. If <see langword="true"/>, the FadingDuration property will represent the interval between characters. This default is <see langword="true"/>.
        /// </returns>
        public bool SequentialFading { get; set; } = true;

        /// <summary>
        /// Gets or sets the time, in milliseconds, for text changes to fade in or out.
        /// </summary>
        /// <returns>
        /// The amount of time, in milliseconds, for text changes to fade in or out. If the SequentialFading property is <see langword="true"/>, represents the interval between appearing characters. The default is 200.
        /// </returns>
        public int FadingDuration { get; set; } = 50;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to the text contents of the <see cref="FormattedTextBlock"/>.
        /// <para>
        /// Text fragments whose color was changed by an effect will not be affected by the Foreground property.
        /// </para>
        /// </summary>
        /// <returns>
        /// The brush used to apply to the text contents. The default is <see cref="Brushes.AliceBlue"/>.
        /// </returns>
        public new Brush Foreground
        {
            get => GetValue(ForegroundProperty) as Brush;
            set
            {
                SetValue(ForegroundProperty, value);

                //Get a clone of the foreground in case the current value is frozen
                var foreground = Foreground.Clone();

                //Transform the inlines into an array for easier access
                var inlines = Inlines.ToArray();
                for (int group = 0; group < inlines.Length; group++)
                {
                    //If the group has a color modifier, don't override it
                    if (group % 2 == 1 && executingText.Effects[group / 2].EffectGroup.Where(x => x.Type.Equals(Txt.FormattedTextEffectType.Color)).Any()) continue;

                    for (int effect = 0; effect < inlines[group].TextEffects.Count; effect++)
                    {
                        //Save the initial opacity and reuse it after the foreground was changed.
                        var opacity = inlines[group].TextEffects[effect].Foreground.Opacity;
                        inlines[group].TextEffects[effect].Foreground = foreground;
                        inlines[group].TextEffects[effect].Foreground.Opacity = opacity;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="Txt.LinkHandler"/> objects which represent the handlers of the currently showing text.
        /// </summary>
        public ReadOnlyCollection<Txt.LinkHandler> Handlers { get; }

        /// <summary>
        /// Determines if this <see cref="FormattedTextBlock"/> is currently fading text in.
        /// </summary>
        public bool IsFading
        {
            get => fading;
        }

        /// <summary>
        /// Determines if this <see cref="FormattedTextBlock"/> has to show future text in the queue.
        /// </summary>
        public bool UpToDate => textQueue.Count == 0;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a text update is complete. That is, when text finishes fading into the text block.
        /// </summary>
        public event TextUpdatedEventHandler TextUpdated
        {
            add => AddHandler(TextUpdatedEvent, value);
            remove => RemoveHandler(TextUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when a <see cref="Txt.LinkHandler"/> is added to this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event LinkHandlerAddedEventHandler LinkHandlerAdded
        {
            add => AddHandler(LinkHandlerAddedEvent, value);
            remove => RemoveHandler(LinkHandlerAddedEvent, value);
        }

        /// <summary>
        /// Occurs when all <see cref="Txt.LinkHandler"/> objects are cleared from this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event LinkHandlersClearedEventHandler LinkHandlersCleared
        {
            add => AddHandler(LinkHandlersClearedEvent, value);
            remove => RemoveHandler(LinkHandlersClearedEvent, value);
        }

        /// <summary>
        /// Occurs after a character has been fully written to this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event CharacterWrittenEventHandler CharacterWritten
        {
            add => AddHandler(CharacterWrittenEvent, value);
            remove => RemoveHandler(CharacterWrittenEvent, value);
        }

        /// <summary>
        /// Occurs before a character appears on this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event CharacterWrittenEventHandler PreviewCharacterWritten
        {
            add => AddHandler(PreviewCharacterWrittenEvent, value);
            remove => RemoveHandler(PreviewCharacterWrittenEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="FormattedTextBlock"/> is clicked.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Fields

        public static readonly new DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FormattedTextBlock), new PropertyMetadata(""));
        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(FormattedTextBlock), new PropertyMetadata(Brushes.AliceBlue.Clone()));
        public static readonly RoutedEvent TextUpdatedEvent = EventManager.RegisterRoutedEvent("TextUpdated", RoutingStrategy.Bubble, typeof(TextUpdatedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent CharacterWrittenEvent = EventManager.RegisterRoutedEvent("CharacterWritten", RoutingStrategy.Bubble, typeof(CharacterWrittenEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent PreviewCharacterWrittenEvent = EventManager.RegisterRoutedEvent("PreviewCharacterWritten", RoutingStrategy.Bubble, typeof(CharacterWrittenEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent LinkHandlerAddedEvent = EventManager.RegisterRoutedEvent("LinkHandlerAdded", RoutingStrategy.Bubble, typeof(LinkHandlerAddedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent LinkHandlersClearedEvent = EventManager.RegisterRoutedEvent("LinkHandlersCleared", RoutingStrategy.Bubble, typeof(LinkHandlersClearedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FormattedTextBlock));

        readonly List<Txt.LinkHandler> handlers = new List<Txt.LinkHandler>();

        readonly DoubleAnimation anim = new DoubleAnimation()
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        readonly Dictionary<Txt.FormattedText, bool> textQueue = new Dictionary<Txt.FormattedText, bool>();
        Txt.FormattedText text, executingText;
        bool wasMouseDown = true,
             fading = false,
             forcing = false,
             instantForcing = false;

        double lastLineSize = 0;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedTextBlock"/> class.
        /// </summary>
        public FormattedTextBlock()
        {
            Effect = new DropShadowEffect() { BlurRadius = 20, ShadowDepth = 10, RenderingBias = RenderingBias.Performance };
            Handlers = handlers.AsReadOnly();
        }

        /// <summary>
        /// Gets the Text property with all original formatting patterns applied to it.
        /// <para>
        /// If <paramref name="current"/> is <see langword="true"/>, the Text property is replaced with CurrentText.
        /// </para>
        /// </summary>
        public string GetFormattedText(bool current)
        {
            return current ? executingText.AsFormatted() : text.AsFormatted();
        }

        /// <summary>
        /// Appends a string of formatted text to this <see cref="FormattedTextBlock"/>.
        /// <para>
        /// A formatting pattern will appear as <c>(</c>{<c>effectType</c>}{<c>value</c>}<c>,</c>etc.<c>)[</c>{<c>affectedText</c>}<c>]</c>. Any brackets inside <c>affectedText</c> have to be escaped with \.
        /// </para>
        /// <para>
        /// <c>effectType</c> can be the following:
        /// <list type="bullet">
        /// <item>s (double): <c>affectedText</c> will shake with the intensity of <c>value</c>.</item>
        /// <item>w (double): <c>affectedText</c> will wave with the intensity of <c>value</c>.</item>
        /// <item>f (double): The font size of <c>affectedText</c> will be amplified by <c>value</c>.</item>
        /// <item>c (color(s)): <c>afftectedText</c> will have the color <c>value</c>. Two colors can be specified through a dash; this will animate <c>value</c> between the two colors.</item>
        /// <item>b (no value): <c>afftectedText</c> will be bold.</item>
        /// <item>i (no value): <c>afftectedText</c> will be italic.</item>
        /// <item>p (no value): In a <see cref="FormattedTextBlock"/>, instead of a standard fade animation, <c>afftectedText</c> will have a "pin" animation.</item>
        /// <item>fd (int): In a <see cref="FormattedTextBlock"/>, <c>afftectedText</c> will fade in <c>value</c> milliseconds.</item>
        /// <item>ff (string): <c>afftectedText</c> will use the font <c>value</c>. The value must follow the format '<c>`</c>{<c>fontName</c>}<c>`</c>'</item>
        /// </list>
        /// </para>
        /// <para>
        /// Any desired modifiers will be specified at the beginning of the string. Modifiers appear as <c>&lt;</c>{<c>modifierType</c>}<c>,</c>etc.<c>&gt;</c>{<c>text</c>}.
        /// </para>
        /// <para>
        /// <c>modifierType</c> can be the following:
        /// <list type="bullet">
        /// <item>- : No modifiers. If used, no other modifiers can be specified.</item>
        /// <item>i : In a <see cref="FormattedTextBlock"/>, <c>text</c> will appear instantly instead of fading in. Previous text will also disappear instantly instead of fading out.</item>
        /// <item>r : In a <see cref="FormattedTextBlock"/>, each character of <c>text</c> will appear instantly instead of fading in. Different to the 'i' modifier, the character dealy will still apply.</item>
        /// </list>
        /// </para>
        /// </summary>
        public void Append(string formattedText)
        {
            //Parse the new text
            var newText = Txt.FormattedText.Parse(formattedText);

            //Set the Text property from this method to not trigger the queue from the setter
            text += newText;
            SetValue(TextProperty, text.Text);

            //Add the new text to the queue
            AddToQueue(newText, true);
        }

        /// <summary>
        /// Appends a string of formatted text to this <see cref="FormattedTextBlock"/> and waits for it to fully apppear.
        /// <para>
        /// A formatting pattern will appear as <c>(</c>{<c>effectType</c>}{<c>value</c>}<c>,</c>etc.<c>)[</c>{<c>affectedText</c>}<c>]</c>. Any brackets inside <c>affectedText</c> have to be escaped with \.
        /// </para>
        /// <para>
        /// <c>effectType</c> can be the following:
        /// <list type="bullet">
        /// <item>s (double): <c>affectedText</c> will shake with the intensity of <c>value</c>.</item>
        /// <item>w (double): <c>affectedText</c> will wave with the intensity of <c>value</c>.</item>
        /// <item>f (double): The font size of <c>affectedText</c> will be amplified by <c>value</c>.</item>
        /// <item>c (color(s)): <c>afftectedText</c> will have the color <c>value</c>. Two colors can be specified through a dash; this will animate <c>value</c> between the two colors.</item>
        /// <item>b (no value): <c>afftectedText</c> will be bold.</item>
        /// <item>i (no value): <c>afftectedText</c> will be italic.</item>
        /// <item>p (no value): In a <see cref="FormattedTextBlock"/>, instead of a standard fade animation, <c>afftectedText</c> will have a "pin" animation.</item>
        /// <item>fd (int): In a <see cref="FormattedTextBlock"/>, <c>afftectedText</c> will fade in <c>value</c> milliseconds.</item>
        /// <item>ff (string): <c>afftectedText</c> will use the font <c>value</c>. The value must follow the format '<c>`</c>{<c>fontName</c>}<c>`</c>'</item>
        /// </list>
        /// </para>
        /// <para>
        /// Any desired modifiers will be specified at the beginning of the string. Modifiers appear as <c>&lt;</c>{<c>modifierType</c>}<c>,</c>etc.<c>&gt;</c>{<c>text</c>}.
        /// </para>
        /// <para>
        /// <c>modifierType</c> can be the following:
        /// <list type="bullet">
        /// <item>- : No modifiers. If used, no other modifiers can be specified.</item>
        /// <item>i : In a <see cref="FormattedTextBlock"/>, <c>text</c> will appear instantly instead of fading in. Previous text will also disappear instantly instead of fading out.</item>
        /// <item>r : In a <see cref="FormattedTextBlock"/>, each character of <c>text</c> will appear instantly instead of fading in. Different to the 'i' modifier, the character dealy will still apply.</item>
        /// </list>
        /// </para>
        /// </summary>
        public async Task AppendAsync(string formattedText)
        {
            Append(formattedText);
            await AwaitTextQueue();

            var lastShownInline = Inlines.LastInline as Run;
            while (Regex.Replace(lastShownInline.Text, @"\s", "").Length == 0 && lastShownInline != Inlines.FirstInline)

                lastShownInline = lastShownInline.PreviousInline as Run;

            while (lastShownInline.TextEffects.Count > 0 &&
                   lastShownInline.TextEffects[lastShownInline.TextEffects.Count - 1].Foreground.Opacity < (Opacity - Opacity / 3)) 
                
                await Task.Delay(1);
        }

        /// <summary>
        /// Sets the string of formatted text that will be displayed on this <see cref="FormattedTextBlock"/> and waits to be shown onto it.
        /// <para>
        /// A formatting pattern will appear as <c>(</c>{<c>effectType</c>}{<c>value</c>}<c>,</c>etc.<c>)[</c>{<c>affectedText</c>}<c>]</c>. Any brackets inside <c>affectedText</c> have to be escaped with \.
        /// </para>
        /// <para>
        /// <c>effectType</c> can be the following:
        /// <list type="bullet">
        /// <item>s (double): <c>affectedText</c> will shake with the intensity of <c>value</c>.</item>
        /// <item>w (double): <c>affectedText</c> will wave with the intensity of <c>value</c>.</item>
        /// <item>f (double): The font size of <c>affectedText</c> will be amplified by <c>value</c>.</item>
        /// <item>c (color(s)): <c>afftectedText</c> will have the color <c>value</c>. Two colors can be specified through a dash; this will animate <c>value</c> between the two colors.</item>
        /// <item>b (no value): <c>afftectedText</c> will be bold.</item>
        /// <item>i (no value): <c>afftectedText</c> will be italic.</item>
        /// <item>p (no value): In a <see cref="FormattedTextBlock"/>, instead of a standard fade animation, <c>afftectedText</c> will have a "pin" animation.</item>
        /// <item>fd (int): In a <see cref="FormattedTextBlock"/>, <c>afftectedText</c> will fade in <c>value</c> milliseconds.</item>
        /// <item>ff (string): <c>afftectedText</c> will use the font <c>value</c>. The value must follow the format '<c>`</c>{<c>fontName</c>}<c>`</c>'</item>
        /// </list>
        /// </para>
        /// <para>
        /// Any desired modifiers will be specified at the beginning of the string. Modifiers appear as <c>&lt;</c>{<c>modifierType</c>}<c>,</c>etc.<c>&gt;</c>{<c>text</c>}.
        /// </para>
        /// <para>
        /// <c>modifierType</c> can be the following:
        /// <list type="bullet">
        /// <item>- : No modifiers. If used, no other modifiers can be specified.</item>
        /// <item>i : In a <see cref="FormattedTextBlock"/>, <c>text</c> will appear instantly instead of fading in. Previous text will also disappear instantly instead of fading out.</item>
        /// <item>r : In a <see cref="FormattedTextBlock"/>, each character of <c>text</c> will appear instantly instead of fading in. Different to the 'i' modifier, the character dealy will still apply.</item>
        /// </list>
        /// </para>
        /// </summary>
        public async Task SetAsync(string formattedText)
        {
            Text = formattedText;
            await AwaitTextQueue();

            var lastShownInline = Inlines.LastInline as Run;
            while (Regex.Replace(lastShownInline.Text, @"\s", "").Length == 0 && lastShownInline != Inlines.FirstInline)

                lastShownInline = lastShownInline.PreviousInline as Run;

            while (lastShownInline.TextEffects.Count > 0 &&
                   lastShownInline.TextEffects[lastShownInline.TextEffects.Count - 1].Foreground.Opacity < (Opacity - Opacity / 3))

                await Task.Delay(1);
        }

        /// <summary>
        /// Forces and waits for the currently appearing text to fully show all of its characters at once. If <paramref name="instant"/> is <see langword="true"/>, also bypasses the fading effect.
        /// </summary>
        /// <param name="instant"></param>
        public async Task ForceShow(bool instant = false)
        {
            if (!IsFading) return;

            if (instant) instantForcing = true;
            else forcing = true;

            while (IsFading) await Task.Delay(1);
            if (!instant) await Task.Delay(FadingDuration);
        }

        /// <summary>
        /// Clears all data used by this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public void Destroy()
        {
            ClearInstant();

            text?.Effects.Clear();
            executingText?.Effects.Clear();

            foreach (var t in textQueue) t.Key.Effects.Clear();
        }

        /// <summary>
        /// Clears the text queue of any pending updates, setting the Text property of this <see cref="FormattedTextBlock"/> to the last update.
        /// </summary>
        public void ClearQueue()
        {
            //Keep the current text's add value
            var execAdd = textQueue[executingText];

            //Clear the queue and re-add the text with its corresponding add value
            textQueue.Clear();
            textQueue.Add(executingText, execAdd);

            //Set the text property to match with the executing text
            text = executingText;
            SetValue(TextProperty, text.Text);
        }

        /// <summary>
        /// Waits for the text queue to finish. When this task finishes, all text updates will have been processed.
        /// </summary>
        public async Task AwaitTextQueue()
        {
            while (textQueue.Count >= 1 || IsFading) await Task.Delay(1);
        }

        /// <summary>
        /// Fades in a <see cref="FormattedText"/> object to this text block. Fades and clears out the previous text if <paramref name="add"/> is <see langword="false"/>.
        /// <para>
        /// Each letter appears after <paramref name="interval"/> milliseconds if <paramref name="allAtOnce"/> is <see langword="false"/>, otherwise, <paramref name="interval"/> servers as the total duration for the whole text to appear.
        /// </para>
        /// </summary>
        async Task FadeText(Txt.FormattedText text, bool allAtOnce = false, bool add = false)
        {
            bool instant = text.Modifiers.Contains(Txt.FormattedTextModifier.Instant),
                 noFade = text.Modifiers.Contains(Txt.FormattedTextModifier.RemoveFade);

            int overridenFade = -1,
                index = 0;

            fading = true;

            //Clear the text block through an animation, or instantly
            if (!add)
            {
                if (instant) ClearInstant();
                else await Clear(FadingDuration, !allAtOnce);
            }

            //Get all groups in the string, including the non affected ones
            var groups = GetGroups(text);

            // Add all inlines before fading in so as to keep the text boxes' width fixed

            Inline prevInline = add ? Inlines.LastInline : null;
            double lineSize = add ? lastLineSize : lastLineSize = 0;

            for (int group = 0; group < groups.Length; group++)
            {

                //Create the run with the according text
                var run = new Run(groups[group])
                {
                    //Populate the run with effects for each character
                    TextEffects = new TextEffectCollection(Enumerable.Range(0, groups[group].Length).Select((x, i) => { var e = new TextEffect() { PositionCount = 1, PositionStart = GetCurrentStartingPosition() + i, Transform = new TransformGroup(), Foreground = Foreground.Clone() }; e.Foreground.Opacity = instant ? Opacity : 0; return e; }))
                };

                // Set the by-run values here, so as not set them every character
                if (group % 2 == 1)
                {
                    if (text.Effects[group / 2].EffectGroup.Any(x => x.Type == Txt.FormattedTextEffectType.FontSize))
                    {
                        //Get the effect's value and set the font size
                        var value = (double)text.Effects[group / 2].EffectGroup.First(x => x.Type == Txt.FormattedTextEffectType.FontSize).Value;
                        run.FontSize = FontSize * value;
                    }

                    if (text.Effects[group / 2].EffectGroup.Any(x => x.Type == Txt.FormattedTextEffectType.Bold)) run.FontWeight = FontWeights.Bold;

                    if (text.Effects[group / 2].EffectGroup.Any(x => x.Type == Txt.FormattedTextEffectType.Italic)) run.FontStyle = FontStyles.Italic;
                    
                    if (text.Effects[group / 2].EffectGroup.Any(x => x.Type == Txt.FormattedTextEffectType.Font)) run.FontFamily = text.Effects[group / 2].EffectGroup.First(x => x.Type == Txt.FormattedTextEffectType.Font).Value as FontFamily;
                }

                //Add the run to the text block
                Inlines.Add(run);
            }

            // Now we iterate through the inlines to fade the text in

            prevInline = add ? prevInline.NextInline : Inlines.FirstInline;

            for (int group = 0; group < groups.Length; group++)
            {
                if (prevInline == null) break;

                var run = prevInline as Run;
                prevInline = prevInline.NextInline;

                //Iterate through the TextEffect array that will affect every character only if it's an affected group
                for (int charEffect = 0; charEffect < groups[group].Length; charEffect++)
                {
                    //Modify the effect's start position accordingly
                    var effect = run.TextEffects[charEffect];
                    var transform = effect.Transform as TransformGroup;

                    bool doPin = false;

                    //Add the scale to the effect's transform group
                    var translation = new TranslateTransform();
                    var scale = new ScaleTransform();

                    transform.Children.Add(translation);
                    transform.Children.Add(scale);

                    //Raise the preview character written event
                    if (!instant && !allAtOnce && !forcing && !instantForcing) RaiseEvent(new Txt.CharacterWrittenEventArgs(PreviewCharacterWrittenEvent, this, groups[group][charEffect], groups[group], index++, charEffect, group % 2 != 1 ? default : text.Effects[group / 2], text));

                    //Ignore whitespace characters
                    if (char.IsWhiteSpace(groups[group][charEffect])) continue;

                    //Iterate through the effect group to add animation to the effect according to the effect type (this can only loop up to 9 times)

                    for (int effectGroup = 0; group % 2 == 1 && effectGroup < text.Effects[group / 2].EffectGroup.Length; effectGroup++)
                    {
                        double value;
                        switch (text.Effects[group / 2].EffectGroup[effectGroup].Type)
                        {
                            case Txt.FormattedTextEffectType.Shake:
                            {
                                //Create a variable to store the sense of the shake and get the effect's value
                                value = (double)text.Effects[group / 2].EffectGroup[effectGroup].Value;

                                //Add the translation to the transform group and set the animation's duration
                                transform.Children.Add(new TranslateTransform());

                                var random_anim = new RandomDoubleAnimation(-value, value, TimeSpan.FromMilliseconds(50))
                                {
                                    RepeatBehavior = RepeatBehavior.Forever
                                };

                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, random_anim);
                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, random_anim);

                                break;
                            }

                            case Txt.FormattedTextEffectType.Wave:
                            {
                                //Get the effect's value
                                value = (double)text.Effects[group / 2].EffectGroup[effectGroup].Value;

                                //Set up the animation and specify and add the delay
                                anim.Duration = TimeSpan.FromSeconds(0.5);
                                anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                                anim.BeginTime = allAtOnce || instant || forcing || instantForcing ? TimeSpan.FromMilliseconds(50 * charEffect) : TimeSpan.FromMilliseconds(0);
                                anim.From = -value;
                                anim.To = value;

                                //Create a clock for the animation to correct the starting value of the translation
                                var clock = anim.CreateClock();
                                clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                                //Add the translation using the correct Y value and begin the animation
                                transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                                break;
                            }

                            case Txt.FormattedTextEffectType.Color:
                            {
                                //Check if the value is a single color first
                                if (text.Effects[group / 2].EffectGroup[effectGroup].Value.GetType().Equals(typeof(Color)))
                                {
                                    effect.Foreground = new SolidColorBrush((Color)text.Effects[group / 2].EffectGroup[effectGroup].Value);
                                    break;
                                }

                                //Set up the animation
                                var colorAnim = text.Effects[group / 2].EffectGroup[effectGroup].Value as ColorAnimation;
                                colorAnim.BeginTime = allAtOnce || instant || forcing || instantForcing ? TimeSpan.FromMilliseconds(50 * charEffect) : TimeSpan.FromMilliseconds(0);
                                colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                                colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                                colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                                colorAnim.AutoReverse = true;

                                //Create a clock for the animation to correct the starting value of the translation
                                var colorClock = colorAnim.CreateClock();
                                colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                                //Create the brush, assign it and begin the animation
                                effect.Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                                effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                                break;
                            }

                            case Txt.FormattedTextEffectType.Link:
                            {
                                //Only add event handlers to the run on the first character
                                if (charEffect != 0) break;

                                //Get the link handler and add it to the instance's list
                                var handler = text.Effects[group / 2].EffectGroup[effectGroup].Value as Txt.LinkHandler;
                                handler.Run = run;

                                handlers.Add(handler);

                                //Add tags to the run
                                run.Cursor = Cursors.Hand;
                                run.Tag = new object[] { false, handler, text.Effects[group / 2], new List<Transform>(), true };
                                run.MouseLeftButtonDown += OnRunMouseDown;
                                run.MouseLeftButtonUp += OnRunMouseUp;
                                run.MouseEnter += OnRunMouseEnter;
                                run.MouseLeave += OnRunMouseLeave;

                                //Raise the link added event
                                RaiseEvent(new Txt.LinkHandlerAddedEventArgs(handler, handlers.Count - 1, LinkHandlerAddedEvent, this));

                                break;
                            }

                            case Txt.FormattedTextEffectType.FadeDuration:
                            {
                                overridenFade = (int)text.Effects[group / 2].EffectGroup[effectGroup].Value;
                                break;
                            }

                            case Txt.FormattedTextEffectType.Pin:
                            {
                                doPin = true;
                                break;
                            }
                        }
                    }

                    //Fade in the character the effect is applied on, depending on the conditions

                    int duration = overridenFade > 0 ? overridenFade : FadingDuration;
                    effect.Foreground.Opacity = noFade || instant || instantForcing ? Opacity : 0;

                    if (!instant && !noFade && !instantForcing)
                    {
                        effect.Foreground.AnimateBrushOpacity(0, Opacity, duration * 5, typeof(SineEase), EasingMode.EaseOut);
                        if (!doPin) translation.AnimatePositionY(-4.5, 0, duration * 5, typeof(SineEase), EasingMode.EaseOut);
                        else
                        {
                            translation.AnimatePositionX(10, 0, duration * 7, typeof(CubicEase), EasingMode.EaseOut);
                            translation.AnimatePositionY(10, 0, duration * 7, typeof(CubicEase), EasingMode.EaseOut);

                            scale.AnimateHeight(2.5, 1, duration * 5, typeof(CubicEase), EasingMode.EaseOut);
                            scale.AnimateWidth(2.5, 1, duration * 5, typeof(CubicEase), EasingMode.EaseOut);
                        }
                    }

                    if (!allAtOnce && !instant && !forcing && !instantForcing)
                    {
                        // Raise the character written event
                        RaiseEvent(new Txt.CharacterWrittenEventArgs(CharacterWrittenEvent, this, groups[group][charEffect], groups[group], index - 1, charEffect, group % 2 != 1 ? default : text.Effects[group / 2], text));
                        await Task.Delay(duration);
                    }

                    overridenFade = -1;
                }
            }

            if (allAtOnce && !instant && !instantForcing) await Task.Delay(FadingDuration);

            fading = forcing = instantForcing = false;

            RaiseEvent(new Txt.TextUpdatedEventArgs(executingText, add, instant, TextUpdatedEvent, this));
            text?.Effects.Clear();
        }

        /// <summary>
        /// Clears this text block's inlines by fading them out, also stopping all the inlines' effects' animations.
        /// </summary>
        async Task Clear(int duration, bool sequential)
        {
            //Fade the text block out to clear runs
            var opacity = Opacity;
            if (Inlines.Count > 0) await this.AnimateOpacityAsync(Opacity, 0, sequential ? 150 : duration, typeof(SineEase), EasingMode.EaseOut);

            //Go through all the runs to stop animations on all effects
            foreach (var inline in Inlines)
            {
                for (int effectIndex = 0; effectIndex < (inline as Run).TextEffects.Count; effectIndex++)
                {
                    var effect = (inline as Run).TextEffects[effectIndex];
                    var transform = effect.Transform as TransformGroup;

                    //Stop the color animation
                    effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                    //Remove the event handlers
                    inline.MouseLeave -= OnRunMouseLeave;
                    inline.MouseLeftButtonDown -= OnRunMouseDown;
                    inline.MouseLeftButtonUp -= OnRunMouseUp;

                    //Stop the translate animations
                    for (int transformIndex = 0; transformIndex < transform.Children.Count; transformIndex++)
                    {
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
                    }

                    transform.Children.Clear();
                }

                if (inline.Tag?.GetType() == typeof(object[])) ((inline.Tag as object[])[3] as List<Transform>).Clear();
                (inline as Run).TextEffects.Clear();
            }

            //Clear the LinkHandlers and raise the event
            RaiseEvent(new Txt.LinkHandlersClearedEventArgs(handlers.ToArray(), LinkHandlersClearedEvent, this));
            handlers.Clear();

            //Clear the runs and restore the opacity
            Inlines.Clear();
            Opacity = opacity;
        }

        /// <summary>
        /// Immediately clears this text block's inlines, also stopping all the inlines' effects' animations.
        /// </summary>
        private void ClearInstant(bool raise = true)
        {
            //Fade the text block out to clear runs
            var opacity = Opacity;
            Opacity = 0;

            //Go through all the runs to stop animations on all effects
            foreach (var inline in Inlines)
            {
                for (int effectIndex = 0; effectIndex < (inline as Run).TextEffects.Count; effectIndex++)
                {
                    var effect = (inline as Run).TextEffects[effectIndex];
                    var transform = effect.Transform as TransformGroup;

                    //Stop the color animation
                    effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                    //Remove the event handlers
                    inline.MouseLeave -= OnRunMouseLeave;
                    inline.MouseLeftButtonDown -= OnRunMouseDown;
                    inline.MouseLeftButtonUp -= OnRunMouseUp;

                    //Stop the translate animations
                    for (int transformIndex = 0; transformIndex < transform.Children.Count; transformIndex++)
                    {
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
                    }

                    transform.Children.Clear();
                }

                if (inline.Tag?.GetType() == typeof(object[])) ((inline.Tag as object[])[3] as List<Transform>).Clear();
                (inline as Run).TextEffects.Clear();
            }

            //Clear the LinkHandlers and raise the event
            if (raise) RaiseEvent(new Txt.LinkHandlersClearedEventArgs(handlers.ToArray(), LinkHandlersClearedEvent, this));
            handlers.Clear();

            //Clear the runs and restore the opacity
            Inlines.Clear();
            Opacity = opacity;
        }

        /// <summary>
        /// Gets all groups of text in a <see cref="FormattedText"/> object, including groups that have no effects.
        /// </summary>
        string[] GetGroups(Txt.FormattedText text)
        {
            return text.Effects.Take(text.Effects.Count - 1).Select((x, i) => new string[] { x.Text, text.Text.Substring(x.Index + x.Text.Length, text.Effects[i + 1].Index - (x.Index + x.Text.Length)) })

                                                                  //Flatten the groups
                                                                  .SelectMany(x => x)

                                                                  //Add the last effect and the remaining unaffected group
                                                                  .Concat(text.Effects.Count > 0 ? new string[] { text.Effects[text.Effects.Count - 1].Text, text.Text.Substring(text.Effects[text.Effects.Count - 1].Index + text.Effects[text.Effects.Count - 1].Text.Length, text.Text.Length - (text.Effects[text.Effects.Count - 1].Index + text.Effects[text.Effects.Count - 1].Text.Length)) } : new string[0])

                                                                  //Add the first unaffected group if it exists
                                                                  .Prepend(text.Effects.Count > 0 ? text.Text.Substring(0, text.Effects[0].Index) : text.Text)

                                                                  //Transform to array
                                                                  .ToArray();
        }

        /// <summary>
        /// Gets the starting positon for the first character a <see cref="Run"/> in the context of a <see cref="TextEffect"/>.
        /// </summary>
        int GetCurrentStartingPosition()
        {
            return Inlines.Select(x => x as Run).Aggregate(1, (total, next) => total + MathHelper.Clamp(next.TextEffects.Count - 1, 0) + 3 - Convert.ToInt32(next.Text.Length == 0));
        }

        /// <summary>
        /// Adds a task to the text queue. The task will be executed only if the queue is empty.
        /// </summary>
        void AddToQueue(Txt.FormattedText text, bool add)
        {
            textQueue.Add(text, add);

            //Only execute if the queue was empty
            if (textQueue.Count == 1) RunQueue(text, add);
        }

        /// <summary>
        /// Runs the text queue until there are no tasks left to complete.
        /// </summary>
        async void RunQueue(Txt.FormattedText text, bool add)
        {
            while (!IsLoaded) await Task.Delay(1);

            executingText = text;
            await FadeText(text, !SequentialFading, add);

            //Remove from the queue and run again until the queue is empty
            if (textQueue.ContainsKey(text)) textQueue.Remove(text);
            if (textQueue.Count >= 1) RunQueue(textQueue.First().Key, textQueue.First().Value);
        }

        /// <summary>
        /// Called when the left mouse button is depressed onto a <see cref="Run"/>. Applies a link effect's click animation to the <see cref="Run"/>.
        /// </summary>
        void OnRunMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as object[];

            //Tag 0 means the left mouse button was pressed over the run
            tags[0] = true;

            (tags[1] as Txt.LinkHandler).MouseDown?.Invoke(tags[1] as Txt.LinkHandler);
        }

        /// <summary>
        /// Called when the left mouse button is raised from a <see cref="Run"/>.
        /// </summary>
        void OnRunMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as object[];

            (tags[1] as Txt.LinkHandler).MouseUp?.Invoke(tags[1] as Txt.LinkHandler);

            // If the mouse wasn't down in thhe first place, then the click shouldn't occur
            if (!((bool)tags[0])) return;

            //Invoke the click handler
            var handler = tags[1] as Txt.LinkHandler;
            if (!(handler?.Click is null)) handler.Click?.Invoke(handler);
        }

        /// <summary>
        /// Called when the mouse enters a <see cref="Run"/>.
        /// </summary>
        void OnRunMouseEnter(object sender, MouseEventArgs e)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as object[];

            //Tag 3 is a list of all link transforms on the run
            var transforms = tags[3] as List<Transform>;

            //Apply the handler's effect
            double value;
            var handler = tags[1] as Txt.LinkHandler;

            //Go through all characters to apply the effect
            for (int charEffect = 0; !handler.ClickEffect.Equals(default(Txt.FormattedTextEffect)) && charEffect < run.Text.Length; charEffect++)
            {
                //Get the transform group of the effect
                var transform = run.TextEffects[charEffect].Transform as TransformGroup;

                //Stop the color animation
                run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                //Stop the translate animations and remove them
                int count = transform.Children.Count;
                for (int transformIndex = 2; transformIndex < count; transformIndex++)
                {
                    transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                    transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);

                    transform.Children.RemoveAt(transformIndex);
                }

                //Begin new animations
                switch (handler.ClickEffect.Type)
                {
                    case Txt.FormattedTextEffectType.Shake:
                    {
                        //Create a variable to store the sense of the shake and get the effect's value
                        value = (double)handler.ClickEffect.Value;

                        //Add the translation to the transform group and set the animation's duration
                        transform.Children.Add(new TranslateTransform());
                        transforms.Add(transform.Children[transform.Children.Count - 1]);

                        var random_anim = new RandomDoubleAnimation(-value, value, TimeSpan.FromMilliseconds(50))
                        {
                            RepeatBehavior = RepeatBehavior.Forever
                        };

                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, random_anim);
                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, random_anim);

                        break;
                    }

                    case Txt.FormattedTextEffectType.Wave:
                    {
                        //Get the effect's value
                        value = (double)handler.ClickEffect.Value;

                        //Set up the animation and specify and add the delay
                        anim.Duration = TimeSpan.FromSeconds(0.5);
                        anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                        anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                        anim.From = -value;
                        anim.To = value;

                        //Create a clock for the animation to correct the starting value of the translation
                        var clock = anim.CreateClock();
                        clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                        //Add the translation using the correct Y value and begin the animation
                        transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                        transforms.Add(transform.Children[transform.Children.Count - 1]);

                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                        break;
                    }

                    case Txt.FormattedTextEffectType.Color:
                    {
                        //Check if the value is a single color first
                        if (handler.ClickEffect.Value.GetType().Equals(typeof(Color)))
                        {
                            run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)handler.ClickEffect.Value);
                            break;
                        }

                        //Set up the animation
                        var colorAnim = handler.ClickEffect.Value as ColorAnimation;
                        colorAnim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                        colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                        colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                        colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                        colorAnim.AutoReverse = true;

                        //Create a clock for the animation to correct the starting value of the translation
                        var colorClock = colorAnim.CreateClock();
                        colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                        //Create the brush, assign it and begin the animation
                        run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                        run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                        break;
                    }
                        
                    case Txt.FormattedTextEffectType.FontSize:
                    {
                        value = (double)handler.ClickEffect.Value;
                        run.FontSize = FontSize * value;

                        break;
                    }

                    case Txt.FormattedTextEffectType.Bold:
                    {
                        run.FontWeight = FontWeights.Bold;
                        break;
                    }

                    case Txt.FormattedTextEffectType.Italic:
                    {
                        run.FontStyle = FontStyles.Italic;
                        break;
                    }

                    case Txt.FormattedTextEffectType.Font:
                    {
                        run.FontFamily = handler.ClickEffect.Value as FontFamily;
                        break;
                    }
                }
            }

            (tags[1] as Txt.LinkHandler).MouseEnter?.Invoke(tags[1] as Txt.LinkHandler);
        }

        /// <summary>
        /// Called when the mouse leaves a <see cref="Run"/>.
        /// </summary>
        void OnRunMouseLeave(object sender, MouseEventArgs e)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as object[];

            // Unset the press tag
            tags[0] = false;

            //Stop the animations on the link's transforms
            var transforms = tags[3] as List<Transform>;
            for (int transformIndex = 0; transformIndex < transforms.Count; transformIndex++)
            {
                transforms[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                transforms[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
            }

            //Reset the run's bold, italic and family states
            run.FontWeight = FontWeight;
            run.FontFamily = FontFamily;
            run.FontStyle = FontStyle;

            //Get the effect group and re-add the animations to each effect
            var group = (Txt.FormattedTextEffectGroup)tags[2];
            for (int charEffect = 0; charEffect < run.Text.Length; charEffect++)
            {
                //Stop the foreground animation, if needed
                run.TextEffects[charEffect].Foreground?.BeginAnimation(SolidColorBrush.ColorProperty, null);

                //Remove the effect's link transforms
                var transform = run.TextEffects[charEffect].Transform as TransformGroup;
                while (transform.Children.Count > 2) transform.Children.RemoveAt(transform.Children.Count - 1);

                //Iterate through the effect group to read animation to the effect according to the effect type (this can only loop up to 6 times)
                for (int effectGroup = 0; effectGroup < group.EffectGroup.Length; effectGroup++)
                {
                    double value;
                    switch (group.EffectGroup[effectGroup].Type)
                    {
                        case Txt.FormattedTextEffectType.Shake:
                        {
                            //Create a variable to store the sense of the shake and get the effect's value
                            value = (double)group.EffectGroup[effectGroup].Value;

                            //Add the translation to the transform group and set the animation's duration
                            transform.Children.Add(new TranslateTransform());

                            var random_anim = new RandomDoubleAnimation(-value, value, TimeSpan.FromMilliseconds(50))
                            {
                                RepeatBehavior = RepeatBehavior.Forever
                            };

                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, random_anim);
                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, random_anim);

                            break;
                        }

                        case Txt.FormattedTextEffectType.Wave:
                        {
                            //Get the effect's value
                            value = (double)group.EffectGroup[effectGroup].Value;

                            //Set up the animation and specify and add the delay
                            anim.Duration = TimeSpan.FromSeconds(0.5);
                            anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                            anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                            anim.From = -value;
                            anim.To = value;

                            //Create a clock for the animation to correct the starting value of the translation
                            var clock = anim.CreateClock();
                            clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                            //Add the translation using the correct Y value and begin the animation
                            transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                            break;
                        }

                        case Txt.FormattedTextEffectType.Color:
                        {
                            //Check if the value is a single color first
                            if (group.EffectGroup[effectGroup].Value.GetType().Equals(typeof(Color)))
                            {
                                run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)group.EffectGroup[effectGroup].Value);
                                break;
                            }

                            //Set up the animation
                            var colorAnim = group.EffectGroup[effectGroup].Value as ColorAnimation;
                            colorAnim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                            colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                            colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                            colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                            colorAnim.AutoReverse = true;

                            //Create a clock for the animation to correct the starting value of the translation
                            var colorClock = colorAnim.CreateClock();
                            colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                            //Create the brush, assign it and begin the animation
                            run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                            run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                            break;
                        }

                        case Txt.FormattedTextEffectType.FontSize:
                        {
                            value = (double)group.EffectGroup[effectGroup].Value;
                            run.FontSize = FontSize * value;

                            break;
                        }

                        case Txt.FormattedTextEffectType.Bold:
                        {
                            run.FontWeight = FontWeights.Bold;
                            break;
                        }

                        case Txt.FormattedTextEffectType.Italic:
                        {
                            run.FontStyle = FontStyles.Italic;
                            break;
                        }

                        case Txt.FormattedTextEffectType.Font:
                        {
                            run.FontFamily = group.EffectGroup[effectGroup].Value as FontFamily;
                            break;
                        }
                    }
                }
            }

            (tags[1] as Txt.LinkHandler).MouseLeave?.Invoke(tags[1] as Txt.LinkHandler);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            wasMouseDown = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (!wasMouseDown) return;
            wasMouseDown = false;
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Text = Text;
        }
    }
}

#region RUN TAG INFO
#if false

- all Run objects of a FormattedTextBlock object (which have the link effect) have the same tag, a List<object>

tag[0]: a bool which confirms if the left mouse button was pressed on the Run (used for MouseLeftButtonUp and MouseLeave events)

tag[1]: a LinkHandler (used for ease of access to the click effect)

tag[2]: the FormattedTextEffectGroup which is applied to all the Run's characters (used to keep track of the original effects)

tag[3]: a List<Transform> which holds all transforms the link effect applied over the run (used to remove them from the run later)

#endif
#endregion