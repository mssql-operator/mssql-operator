using System;
using System.Collections.Generic;
using System.Text;
using OperatorSharp.CustomResources;
using OperatorSharp.CustomResources.Metadata;

namespace MSSqlOperator.DeploymentScripts
{
    [ApiVersion("mssql.techpyramid.ws/v1alpha1")]
    [Kind("DeploymentScript")]
    [PluralName("deploymentscripts")]
    [ResourceScope(ResourceScopes.Namespaced)]
    [ShortName("script")]
    public class DeploymentScriptResource : CustomResource<DeploymentScriptSpec, DeploymentScriptStatus>
    {
    }
}
