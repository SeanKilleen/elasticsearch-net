﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Newtonsoft.Json;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public interface IBulkAliasRequest : IRequest<BulkAliasRequestParameters>
	{
		[JsonProperty("actions")]
		IList<IAliasAction> Actions { get; set; }
	}

	public partial class BulkAliasRequest : RequestBase<BulkAliasRequestParameters>, IBulkAliasRequest
	{
		public IList<IAliasAction> Actions { get; set; }
	}


	[DescriptorFor("IndicesUpdateAliases")]
	public partial class BulkAliasDescriptor : RequestDescriptorBase<BulkAliasDescriptor, BulkAliasRequestParameters>, IBulkAliasRequest
	{
		public BulkAliasDescriptor Add(IAliasAction action) => 
			Fluent.Assign<BulkAliasDescriptor, IBulkAliasRequest>(this, a=> a.Actions.AddIfNotNull(action));

		IList<IAliasAction> IBulkAliasRequest.Actions { get; set; } = new List<IAliasAction>();

		public BulkAliasDescriptor Add(Func<AliasAddDescriptor, AliasAddDescriptor> addSelector) => Add(addSelector?.Invoke(new AliasAddDescriptor()));

		public BulkAliasDescriptor Remove(Func<AliasRemoveDescriptor, AliasRemoveDescriptor> removeSelector)=> Add(removeSelector?.Invoke(new AliasRemoveDescriptor()));
	}
}