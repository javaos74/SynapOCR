using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using UiPathTeams.Synap.OCR;
using UiPathTeams.Synap.OCR.Design.Designers;
using UiPathTeams.Synap.OCR.Design.Properties;

namespace UiPath.Synap.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(SynapScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(SynapScope), new DesignerAttribute(typeof(SynapScopeDesigner)));
            builder.AddCustomAttributes(typeof(SynapScope), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(Recognize), categoryAttribute);
            builder.AddCustomAttributes(typeof(Recognize), new DesignerAttribute(typeof(RecognizeDesigner)));
            builder.AddCustomAttributes(typeof(Recognize), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
