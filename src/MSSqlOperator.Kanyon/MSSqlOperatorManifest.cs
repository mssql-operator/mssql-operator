using System;
using System.Collections.Generic;
using System.Text;
using Kanyon.Core;
using Kanyon.Kubernetes.Apiextensions.V1;
using Kanyon.Kubernetes.Apps.V1;
using Kanyon.Kubernetes.Core.V1;
using Kanyon.Kubernetes.Rbac.V1;

namespace MSSqlOperator.Kanyon
{
    public class MSSqlOperatorManifest : Manifest
    {
        public MSSqlOperatorManifest()
        {
            Add(new CustomResourceDefinition()
            {
                metadata = new ObjectMeta
                {
                    name = "databases.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec()
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "Database",
                        plural = "databases",
                        shortNames = new[] { "db" },
                        singular = "database",
                    },
                    preserveUnknownFields = true,
                    scope = "Namespaced",
                    versions = new[]
                    {
                        new CustomResourceDefinitionVersion
                        {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources
                            {
                                status = new CustomResourceSubresourceStatus()
                            }
                        }
                    }
                }
            });

            Add(new CustomResourceDefinition()
            {
                metadata = new ObjectMeta
                {
                    name = "databaseservers.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec()
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "DatabaseServer",
                        plural = "databaseservers",
                        shortNames = new[] { "dbms" },
                        singular = "databaseserver",
                    },
                    preserveUnknownFields = true,
                    scope = "Namespaced",
                    versions = new[]
                    {
                        new CustomResourceDefinitionVersion
                        {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources
                            {
                                status = new CustomResourceSubresourceStatus()
                            }
                        }
                    }
                }
            });

            Add(new CustomResourceDefinition()
            {
                metadata = new ObjectMeta
                {
                    name = "deploymentscripts.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec()
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "DeploymentScript",
                        plural = "deploymentscripts",
                        shortNames = new[] { "script" },
                        singular = "deploymentscript",
                    },
                    preserveUnknownFields = true,
                    scope = "Namespaced",
                    versions = new[]
                    {
                        new CustomResourceDefinitionVersion
                        {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources
                            {
                                status = new CustomResourceSubresourceStatus()
                            }
                        }
                    }
                }
            });

            Add(new Namespace
            {
                metadata = new ObjectMeta
                {
                    name = "mssql-operator"
                }
            });

            Add(new ServiceAccount
            {
                metadata = new ObjectMeta
                {
                    name = "mssql-operator-serviceaccount",
                    @namespace = "mssql-operator"
                }
            });

            Add(new ClusterRole
            {
                metadata = new ObjectMeta
                {
                    name = "mssql-operator-clusterrole"
                },
                rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        apiGroups = new[] { "mssql-operator.github.io" },
                        resources = new[] { "databases", "databaseservers", "deploymentscripts" },
                        verbs = new[] { "get", "list", "watch", "patch" },
                    },
                    new PolicyRule
                    {
                        apiGroups = new[] { "" },
                        resources = new[] { "secrets", "services" },
                        verbs = new[] { "get", "list" }
                    }
                }
            });

            Add(new ClusterRoleBinding
            {
                metadata = new ObjectMeta
                {
                    name = "mssql-operator-clusterrole-binding"
                },
                roleRef = new RoleRef
                {
                    apiGroup = "rbac.authorization.k8s.io",
                    kind = "ClusterRole",
                    name = "mssql-operator-clusterrole"
                },
                subjects = new List<Subject>
                {
                    new Subject
                    {
                        kind = "ServiceAccount",
                        name = "mssql-operator-serviceaccount",
                        @namespace = "mssql-operator"
                    }
                }
            });

            var labels = new { app = "mssql-operator" };

            Add(Deployment);
        }

        public Deployment Deployment { get; set; } = new Deployment
        {
            metadata = new ObjectMeta
            {
                name = "mssql-operator",
                @namespace = "mssql-operator",
                labels = new { app = "mssql-operator" }
            },
            spec = new DeploymentSpec
            {
                selector = new LabelSelector
                {
                    matchLabels = new { app = "mssql-operator" }
                },
                template = new PodTemplateSpec
                {
                    metadata = new ObjectMeta
                    {
                        labels = new { app = "mssql-operator" }
                    },
                    spec = new PodSpec
                    {
                        restartPolicy = "Always",
                        serviceAccountName = "mssql-operator-serviceaccount",
                        containers = new List<Container>
                            {
                                new Container
                                {
                                    name = "mssql-operator",
                                    image = "techpyramid/mssql-operator:nightly",
                                    imagePullPolicy = "IfNotPresent"
                                }
                            }
                    }
                }
            }
        };
    }
}
