using System.Collections.Generic;

namespace AdvantageTool.AssignmentGradeServices
{
    /// <inheritdoc />
    /// <summary>
    /// A list of LineItems.
    /// </summary>
    public class LineItemContainer : List<LineItem>
    {
        /// <inheritdoc />
        /// <summary>
        /// Create an empty LineItemContainer
        /// </summary>
        public LineItemContainer()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a LineItemContainer that contains the line items.
        /// </summary>
        /// <param name="lineItems"></param>
        public LineItemContainer(IEnumerable<LineItem> lineItems) : base(lineItems)
        {
        }
    }
}
