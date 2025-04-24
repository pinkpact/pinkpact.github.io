using System;
using System.Windows.Documents;

namespace PinkPact.Text
{
    /// <summary>
    /// Represents a handler for links in <see cref="FormattedText"/> objects.
    /// </summary>
    public class LinkHandler
    {
        /// <summary>
        /// Gets the index of the first character of the text over which this effect group is applied to in the unformatted string.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the text over which this link is applied to.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the <see cref="System.Windows.Documents.Run"> over which this link is applied to.
        /// </summary>
        public Run Run { get; internal set; }

        /// <summary>
        /// Gets the effect which will be applied when clicking on the affected text of the Owner object.
        /// </summary>
        public FormattedTextEffect ClickEffect { get; }

        /// <summary>
        /// Gets or sets the click action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action<LinkHandler> Click { get; set; }

        /// <summary>
        /// Gets or sets the mouse enter action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action<LinkHandler> MouseEnter { get; set; }

        /// <summary>
        /// Gets or sets the mouse leave action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action<LinkHandler> MouseLeave { get; set; }

        /// <summary>
        /// Gets or sets the mouse down action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action<LinkHandler> MouseDown { get; set; }

        /// <summary>
        /// Gets or sets the mouse up action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action<LinkHandler> MouseUp { get; set; }

        /// <summary>
        /// Gets a value indicating if this <see cref="LinkHandler"/> is active. That is, if the hover effect and handler execute.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandler"/> class from an owner and the speicfied action the link refers to.
        /// </summary>
        public LinkHandler(FormattedTextEffect effect, Run run, string text, int index, Action<LinkHandler> handler)
        {
            Run = run;
            Text = text;
            Index = index;
            Click = handler;
            ClickEffect = effect;
        }
    }
}
