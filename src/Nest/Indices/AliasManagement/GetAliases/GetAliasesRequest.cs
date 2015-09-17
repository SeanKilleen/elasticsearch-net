﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Newtonsoft.Json;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface IGetAliasesRequest : IIndicesOptionalPath<GetAliasesRequestParameters>
	{
		[JsonIgnore]
		string Alias { get; set; }
	}

	internal static class GetAliasesPathInfo
	{
		public static void Update(RequestPath<GetAliasesRequestParameters> pathInfo, IGetAliasesRequest request)
		{
			pathInfo.HttpMethod = HttpMethod.GET;
			pathInfo.Name = request.Alias ?? "*";
		}
	}
	
	public partial class GetAliasesRequest : IndicesOptionalPathBase<GetAliasesRequestParameters>, IGetAliasesRequest
	{
		public string Alias { get; set; }
		
		protected override void UpdatePathInfo(IConnectionSettingsValues settings, RequestPath<GetAliasesRequestParameters> pathInfo)
		{
			GetAliasesPathInfo.Update(pathInfo, this);
		}
	}

	[DescriptorFor("IndicesGetAliases")]
	public partial class GetAliasesDescriptor 
		: IndicesOptionalPathDescriptor<GetAliasesDescriptor, GetAliasesRequestParameters>, IGetAliasesRequest
	{

		private IGetAliasesRequest Self => this;

		string IGetAliasesRequest.Alias { get; set; }

		public GetAliasesDescriptor Alias(string alias)
		{
			Self.Alias = alias;
			return this;
		}

		protected override void UpdatePathInfo(IConnectionSettingsValues settings, RequestPath<GetAliasesRequestParameters> pathInfo)
		{
			GetAliasesPathInfo.Update(pathInfo, this);
		}
	}
}