using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Activities.Statements;
using System.ComponentModel;
using UiPathTeams.Synap.OCR.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace UiPathTeams.Synap.OCR
{
    [LocalizedDisplayName(nameof(Resources.SynapScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.SynapScope_Description))]
    public class SynapScope : ContinuableAsyncNativeActivity
    {
        #region Properties

        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.SynapScope_Endpoint_DisplayName))]
        [LocalizedDescription(nameof(Resources.SynapScope_Endpoint_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Endpoint { get; set; }

        [LocalizedDisplayName(nameof(Resources.SynapScope_AccessKey_DisplayName))]
        [LocalizedDescription(nameof(Resources.SynapScope_AccessKey_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> AccessKey { get; set; }

        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        private SynapContext synap;
        #endregion


        #region Constructors

        public SynapScope(IObjectContainer objectContainer)
        {
            this.synap = new SynapContext();
            _objectContainer = objectContainer;
            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer> (ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public SynapScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Endpoint == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Endpoint)));
            if (AccessKey == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(AccessKey)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext  context, CancellationToken cancellationToken)
        {
            // Inputs
            var endpoint = Endpoint.Get(context);
            var accesskey = AccessKey.Get(context);

            this.synap.Endpoint = endpoint;
            this.synap.ApiKey = accesskey;
            this._objectContainer.Add<SynapContext>(this.synap);
            return (ctx) => {
                // Schedule child activities
                if (Body != null)
				    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
            };
        }

        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Cleanup();
        }

        #endregion


        #region Helpers
        
        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

