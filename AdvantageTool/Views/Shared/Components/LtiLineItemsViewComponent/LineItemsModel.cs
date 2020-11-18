using AdvantageTool.AssignmentGradeServices;
using AdvantageTool.Lti;
using System.Collections.Generic;

namespace AdvantageTool.Views.Shared.Components.LtiLineItemsViewComponent
{
    public class LineItemsModel
    {
        public string IdToken { get; set; }
        public List<MyLineItem> LineItems { get; set; }
        public string LineItemUrl { get; set; }
        public LtiResourceLinkRequest LtiRequest { get; set; }
        public Dictionary<string, string> Members { get; set; }
        public string Status { get; set; }
        public LineItemsModel(string idToken)
        {
            IdToken = idToken;
        }
    }

    public class MyLineItem
    {
        public string Header { get; set; }
        public LineItem AgsLineItem { get; set; }
        public List<Result> Results { get; set; }
    }
}
