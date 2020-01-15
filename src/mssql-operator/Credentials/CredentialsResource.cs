using System;
using System.Collections.Generic;
using System.Text;
using OperatorSharp.CustomResources;
using OperatorSharp.CustomResources.Metadata;

namespace MSSqlOperator.Credentials
{
    [ApiVersion("mssql.techpyramid.ws/v1alpha1")]
    [Kind("Credentials")]
    [PluralName("Credentials")]
    [ShortName("creds")]
    [ResourceScope(ResourceScopes.Namespaced)]
    public class CredentialsResource : CustomResource<CredentialsSpec, CredentialsStatus>
    {
    }
}
