using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using UiPathTeams.Synap.OCR.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using System.Net;
using Newtonsoft.Json;
using System.Activities.Expressions;

namespace UiPathTeams.Synap.OCR
{

    [LocalizedDisplayName(nameof(Resources.Recognize_DisplayName))]
    [LocalizedDescription(nameof(Resources.Recognize_Description))]
    public class Recognize : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.Recognize_FilePath_DisplayName))]
        [LocalizedDescription(nameof(Resources.Recognize_FilePath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FilePath { get; set; }

        [LocalizedCategory(nameof(Resources.Input_Category))]
        [LocalizedDisplayName(nameof(Resources.Recognize_Language_DisplayName))]
        [LocalizedDescription(nameof(Resources.Recognize_Language_Description))]
        public InArgument<string> Language { get; set; }

        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> JsonOutput { get; set;  }

        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<SynapOCRResult> Result { get; set; }

        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<HttpStatusCode> StatusCode { get; set; }

        #endregion


        #region Constructors

        public Recognize()
        {
            Constraints.Add(ActivityConstraints.HasParentType<Recognize, SynapScope>(string.Format(Resources.ValidationScope_Error, Resources.SynapScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (FilePath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FilePath)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            var objectContainer = context.GetFromContext<IObjectContainer>(SynapScope.ParentContainerPropertyTag);
            var synap  = objectContainer.Get<SynapContext>();
            // Inputs
            var filepath = FilePath.Get(context);
            var language = Language.Get(context);

            var client = new UiPathHttpClient(synap.Endpoint);
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////
            if( string.IsNullOrEmpty(filepath) || !System.IO.File.Exists(filepath))
            {
                return (ctx) =>
                {
                    ctx.SetValue(StatusCode, HttpStatusCode.NotFound);
                    ctx.SetValue(JsonOutput, string.Empty);
                };
            }
            client.AddFile(filepath);
            client.AddField("api_key", synap.ApiKey);
            client.AddField("langs", "all");
            client.AddField("type", "upload");
            client.AddField("skew", "true");
            client.AddField("boxes_type", "all");
            //client.AddField("textout", "true");

            var json = await client.Upload();
            SynapOCRResponse resp  = JsonConvert.DeserializeObject<SynapOCRResponse>(json.body, new SynapOCRResponseConverter());
            // Outputs
            return (ctx) => {
                ctx.SetValue(StatusCode, json.status);
                if (json.status == HttpStatusCode.OK)
                {
                    ctx.SetValue(Result, resp.result);
                }
                else
                {
                    ctx.SetValue(Result, null);
                }
                ctx.SetValue(JsonOutput, json.body);
            };
        }

        #endregion
    }
}

