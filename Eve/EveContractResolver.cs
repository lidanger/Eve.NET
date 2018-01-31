﻿using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Eve
{
	/// <summary>
	/// Does not serialize remote (Eve) meta fields (which are read only for the API)
	/// </summary>
	public class EveContractResolver : DefaultContractResolver
	{
        public EveContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy();
        }
		protected override JsonProperty CreateProperty (MemberInfo member, MemberSerialization memberSerialization)
		{
			var property =	base.CreateProperty (member, memberSerialization);
			// if the property is flagged with RemoteAttribute it's a meta field so don't serialize it.
			property.ShouldSerialize = 
				instance => {
                    var r = member.GetCustomAttributes (typeof(RemoteAttribute), false);
                    return r.GetEnumerator ().MoveNext () == false;
                };
			return property;
		}
    }
}