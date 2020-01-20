# Introduction 
MSSQL-Operator allows MSSQL databases to be created declaratively against a registered database server.

# Overview
The MSSQL operator leverages DatabaseServer and Database objects to define how the databases get created. Credentials and DeploymentScript objects can also be used to influence the behavior of the creation.

## DatabaseServer
The DatabaseServer API object defines the connection information for the server where the database should be created. This DatabaseServer can reference a MSSQL server inside the cluster or outside the cluster. Inside the cluster, the serviceSelector field can be configured to select the service that exposes the MSSQL server. If the MSSQL server is outside the cluster (either in a HADR cluster, Azure, or other on-premise SQL servers), then the serviceUrl field can be used to provide a raw host name and port to connect to. In addition, an administrator username and password must be provided. This user needs to have dbcreator rights. The user name can be set in the adminUserName field, and the password can be set in the adminPasswordSecret field. This field takes either (preferably) a secretKeyRef that points to a Kubernetes secret with the user's password, or (less securely) the value field that takes the raw value of the password. Provided below is an example DatabaseServer object 
```
apiVersion: mssql.techpyramid.ws/v1alpha1
kind: DatabaseServer
metadata:
  name: dev-sql
  namespace: "default"
  labels:
    database-server: dev
spec:
  # serviceUrl: localhost,1433
  serviceSelector:
    matchLabels:
      tier: db
  adminUserName: sa
  adminPasswordSecret:
    # value: "NotAVerySecurePassword"
    secretKeyRef:
      name: mssql
      key: SA_PASSWORD
```

## Database
The Database API object defines the parameters to be used to create the database

## DeploymentScript
The DeploymentScript object allows the developer to specify a script to be run against the database sometime after creation. This can include Data Definition Language (DDL) statements and Data Manipulation Language (DML) statements to initialize the newly-created database to some desired state. Because the script is stored in the Kubernetes cluster's etcd data store, this is likely best used for simple state setups. More complex state manipulations will benefit more greatly from the restore functionality provided by the Database object.

# Getting Started
In order to run this operator in your cluster, there are three tasks that need to be performed:
1. First, the CRDs must be created in the cluster. These are located in the crds folder. For your convenience, both the v1 and the v1beta1 CRDs are provided. To deploy the definitions, simply run `kubectl apply -f crds/v1` or `kubectl apply -f crds/v1beta1` (depending on which API version you prefer to use)
2. Second, the RBAC permissions must be created to allow the operator to read the objects you will be creating. To deploy the RBAC permissions, run `kubectl apply -f deploy/rbac.yaml`
3. Finally, the operator deployment can be created. The deployment.yaml file under the deploy folder has a few minor configuration points related to telemetry. These are not necessary for successful operation, but can be helpful for monitoring. Update those configuration values as desired and then run `kubectl apply -f deploy/deployment.yaml`

# Build and Test
In order to build a development version of the container, simply run `docker build -f src/mssql-operator/Dockerfile src` from the root of the repository. For debugging, the MSSqlOperator.sln solution can be opened in Visual Studio 2019 

# Contribute
Contributions are absolutely welcome! MSSQL-Operator is licensed under the MIT license (see [LICENSE]). If you are unsure how you might be able to contribute, below are some ideas that are always welcomd.
1. Submit issues for feature requests and bug reports
2. Provide feedback on existing issues and pull requests
3. Create a Pull Request for new features
4. Write more (and better!) documentation

In all contributions, please be kind to each other. As the project matures, a more comprehensive code of conduct will be defined in order to welcome others to contribute.

More formal governance policies will be written as the project matures.
