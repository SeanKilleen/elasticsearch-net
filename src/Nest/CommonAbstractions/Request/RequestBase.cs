using Elasticsearch.Net;
using Elasticsearch.Net.Connection.Configuration;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface IRequest
	{
		IRequestConfiguration Configuration { get; set; }
	}

	public interface IRequest<TParameters> : IRequest
		where TParameters : IRequestParameters, new()
	{
        /// <summary>
        /// Used to describe request parameters not part of the body. e.q query string or 
        /// connection configuration overrides
        /// </summary>
        TParameters Parameters { get; set; }

        RequestPath<TParameters> Path(IConnectionSettingsValues settings);
	}

    public abstract class RequestBase<TParameters> : IRequest<TParameters>
        where TParameters : IRequestParameters, new()
    {
        public RequestBase() { }

        public RequestBase(Func<RequestPath<TParameters>, RequestPath<TParameters>> pathSelector)
        {
            _path = pathSelector(new RequestPath<TParameters>());
        }

        private RequestPath<TParameters> _path;

        [JsonIgnore]
        protected IRequest<TParameters> Request => this;

		protected TOut Q<TOut>(string name) =>
			this.Request.Parameters.GetQueryStringValue<TOut>(name);

		protected void Q(string name, object value) =>
			this.Request.Parameters.AddQueryStringValue(name, value);

        /// <summary>
        /// Allows you to override connection settings on a per call basis
        /// </summary>
        [JsonIgnore]
        IRequestConfiguration IRequest.Configuration { get; set; }

        /// <summary>
        /// Describes parameters that are supplied on the querystring rather then the body of the request
        /// </summary>
        [JsonIgnore]
        TParameters IRequest<TParameters>.Parameters { get; set; } = new TParameters();
		
        /// <summary>
		/// Creates a PathInfo object from this request that we can use to dispatch into the low level client
		/// </summary>
		internal virtual RequestPath<TParameters> Path(IConnectionSettingsValues settings, TParameters queryString)
        {
            _path.RequestParameters = queryString;

			//if this request describes request specific connection overrides make sure they are carried 
			//over into the requestPath object
			var config = ((IRequest) this).Configuration;
			if (config != null)
			{
				IRequestParameters p = _path.RequestParameters;
				p.RequestConfiguration = config;
			}
			
			//ask subclasses to set the relevant pathInfo parameters
			SetRouteParameters(settings, _path);

			//update the pathInfo, is abstract and forces each subclass to at a minimum set the HttpMethod
			UpdateRequestPath(settings, _path);

			ValidateRequestPath(_path);

            return _path;
		}

        RequestPath<TParameters> IRequest<TParameters>.Path(IConnectionSettingsValues settings)
        {
            return this.Path(settings, this.Request.Parameters);
        }

        protected virtual void SetRouteParameters(IConnectionSettingsValues settings, RequestPath<TParameters> path) { }

		protected virtual void ValidateRequestPath(IRequestPath<TParameters> path) { }

		protected virtual void UpdateRequestPath(IConnectionSettingsValues settings, RequestPath<TParameters> pathInfo)
		{
			pathInfo.HttpMethod = pathInfo.RequestParameters.DefaultHttpMethod;
		}
	}

    public abstract class RequestDescriptorBase<TDescriptor, TParameters> : RequestBase<TParameters>, IDescriptor
		where TDescriptor : RequestDescriptorBase<TDescriptor, TParameters>
		where TParameters : FluentRequestParameters<TParameters>, new()
	{
        public RequestDescriptorBase() { }

        public RequestDescriptorBase(Func<RequestPath<TParameters>, RequestPath<TParameters>> pathSelector)
            : base(pathSelector)
        { }

		protected TDescriptor _requestParams(Action<TParameters> assigner)
		{
			assigner?.Invoke(this.Request.Parameters);
			return (TDescriptor)this;
		}

		/// <summary>
		/// Specify settings for this request alone, handy if you need a custom timeout or want to bypass sniffing, retries
		/// </summary>
		public TDescriptor RequestConfiguration(Func<RequestConfigurationDescriptor, IRequestConfiguration> configurationSelector)
		{
			configurationSelector.ThrowIfNull("configurationSelector");
			this.Request.Configuration = configurationSelector(new RequestConfigurationDescriptor());
			return (TDescriptor)this;
		}
		
		/// <summary>
		/// Hides the <see cref="Equals"/> method.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool Equals(object obj) => base.Equals(obj);

		/// <summary>
		/// Hides the <see cref="GetHashCode"/> method.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int GetHashCode() => base.GetHashCode();

		/// <summary>
		/// Hides the <see cref="ToString"/> method.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ToString() => base.ToString();
		
	}
}