using System;
using System.Collections.Generic;
using Kanyon.Core;
using Kanyon.Kubernetes.Apiextensions.V1;
using Kanyon.Kubernetes.Core.V1;
using Microsoft.OpenApi.Models;

namespace MSSqlOperator.Kanyon {
    public class V1CRDManifest : Manifest {
        public V1CRDManifest()
        {
            var databaseSpec = new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema> {
                                        { "databaseName", new OpenApiSchema { Type = "string" } },
                                        { "collation", new OpenApiSchema { Type = "string" } },
                                        { "gCStrategy", new OpenApiSchema { Type = "string" } },
                                        { "dataFiles", new OpenApiSchema {
                                            Type = "object",
                                            AdditionalProperties = new OpenApiSchema {
                                                Type = "array",
                                                Items = new OpenApiSchema {
                                                    Type = "object",
                                                    Properties = new Dictionary<string, OpenApiSchema> {
                                                        { "name", new OpenApiSchema { Type = "string" }},
                                                        { "path", new OpenApiSchema { Type = "string" }},
                                                        { "isPrimaryFile", new OpenApiSchema { Type = "boolean" }},
                                                    }
                                                }
                                            }
                                        }},
                                        { "databaseServerSelector", new OpenApiSchema
                                            {
                                                Type = "object",
                                                Properties = new Dictionary<string, OpenApiSchema> {
                                                    { "matchLabels", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } },
                                                    { "matchExpressions", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } }
                                                }
                                            }
                                        },
                                        { "logFiles", new OpenApiSchema
                                            {
                                                Type = "object",
                                                Items = new OpenApiSchema {
                                                    Type = "object",
                                                    Properties = new Dictionary<string, OpenApiSchema> {
                                                        { "name", new OpenApiSchema { Type = "string" }},
                                                        { "path", new OpenApiSchema { Type = "string" }},
                                                        { "isPrimaryFile", new OpenApiSchema { Type = "boolean" }},
                                                    }
                                                }
                                            }
                                        },
                                        { "backupFiles", new OpenApiSchema
                                            {
                                                Type = "object",
                                                Items = new OpenApiSchema {
                                                    Type = "object",
                                                    Properties = new Dictionary<string, OpenApiSchema> {
                                                        { "name", new OpenApiSchema { Type = "string" }},
                                                        { "path", new OpenApiSchema { Type = "string" }},
                                                        { "isPrimaryFile", new OpenApiSchema { Type = "boolean" }},
                                                    }
                                                }
                                            }
                                        },
                                    }
            };
            Add(new CustomResourceDefinition
            {
                metadata = new ObjectMeta
                {
                    name = "databases.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "Database",
                        plural = "databases",
                        shortNames = new[] { "db" },
                        singular = "database"
                    },
                    scope = "Namespaced",
                    versions = new [] {
                        new CustomResourceDefinitionVersion {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources {
                                status = new CustomResourceSubresourceStatus { }
                            },
                            schema = new CustomResourceValidation {
                                openAPIV3Schema = new OpenApiSchema {
                                    Type = "object",
                                    Properties = new Dictionary<string, OpenApiSchema> {
                                        { "spec", databaseSpec },
                                        { "status", new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                { "observedGeneration", new OpenApiSchema { Type = "number" } },
                                                { "LastUpdate", new OpenApiSchema { Type = "string", Format = "date-time" } },
                                                { "Reason", new OpenApiSchema { Type = "string" } },
                                                { "Message", new OpenApiSchema { Type = "string" } }
                                            }
                                        }}
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var databaseServerSpec = new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema> {
                                        { "adminUserName", new OpenApiSchema { Type = "string" }},
                                        { "serviceUrl", new OpenApiSchema { Type = "string" }},
                                        { "serviceSelector", new OpenApiSchema
                                            {
                                                Type = "object",
                                                Properties = new Dictionary<string, OpenApiSchema> {
                                                    { "matchLabels", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } },
                                                    { "matchExpressions", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } }
                                                }
                                            }
                                        },
                                        { "adminPasswordSecret", new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                { "value", new OpenApiSchema { Type = "string" }},
                                                { "secretKeyRef", new OpenApiSchema {
                                                    Type = "object",
                                                    Properties = new Dictionary<string, OpenApiSchema> {
                                                        { "key", new OpenApiSchema { Type = "string" }},
                                                        { "name", new OpenApiSchema { Type = "string" }},
                                                        { "optional", new OpenApiSchema { Type = "boolean" }},
                                                    }
                                                }}
                                            }
                                        }}
                                    }
            };
            Add(new CustomResourceDefinition
            {
                metadata = new ObjectMeta
                {
                    name = "databaseservers.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "DatabaseServer",
                        plural = "databaseservers",
                        shortNames = new[] { "dbms" },
                        singular = "databaseserver"
                    },
                    scope = "Namespaced",
                    versions = new [] {
                        new CustomResourceDefinitionVersion {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources {
                                status = new CustomResourceSubresourceStatus { }
                            },
                            schema = new CustomResourceValidation {
                                openAPIV3Schema = new OpenApiSchema {
                                    Type = "object",
                                    Properties = new Dictionary<string, OpenApiSchema> {
                                        { "spec", databaseServerSpec },
                                        { "status", new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                { "observedGeneration", new OpenApiSchema { Type = "number" } }
                                            }
                                        }}
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var deploymentScriptSpec = new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema> {
                                        { "script", new OpenApiSchema { Type = "string" }},
                                        { "databaseSelector", new OpenApiSchema
                                            {
                                                Type = "object",
                                                Properties = new Dictionary<string, OpenApiSchema> {
                                                    { "matchLabels", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } },
                                                    { "matchExpressions", new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } } }
                                                }
                                            }
                                        }
                                    }
            };
            Add(new CustomResourceDefinition
            {
                metadata = new ObjectMeta
                {
                    name = "deploymentscripts.mssql-operator.github.io"
                },
                spec = new CustomResourceDefinitionSpec
                {
                    group = "mssql-operator.github.io",
                    names = new CustomResourceDefinitionNames
                    {
                        kind = "DeploymentScript",
                        plural = "deploymentscripts",
                        shortNames = new[] { "script" },
                        singular = "deploymentscript"
                    },
                    scope = "Namespaced",
                    versions = new [] {
                        new CustomResourceDefinitionVersion {
                            name = "v1alpha1",
                            served = true,
                            storage = true,
                            subresources = new CustomResourceSubresources {
                                status = new CustomResourceSubresourceStatus { }
                            },
                            schema = new CustomResourceValidation {
                                openAPIV3Schema = new OpenApiSchema {
                                    Type = "object",
                                    Properties = new Dictionary<string, OpenApiSchema> {
                                        { "spec", deploymentScriptSpec },
                                        { "status", new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                { "observedGeneration", new OpenApiSchema { Type = "number" } },
                                                { "LastUpdate", new OpenApiSchema { Type = "string", Format = "date-time" } },
                                                { "Reason", new OpenApiSchema { Type = "string" } },
                                                { "Message", new OpenApiSchema { Type = "string" } },
                                                { "ExecutedDate", new OpenApiSchema { Type = "string", Format = "date-time" } },
                                            }
                                        }}
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}