FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Restore
# only run the restore in case the following files changed
COPY ["DatadogTakeHome.sln", "DatadogTakeHome.sln"]
COPY ["DatadogTakeHome/DatadogTakeHome.csproj", "DatadogTakeHome/"]
COPY ["DatadogTakeHome.Core/DatadogTakeHome.Core.csproj", "DatadogTakeHome.Core/"]
COPY ["DatadogTakeHome.Tests/DatadogTakeHome.Tests.csproj", "DatadogTakeHome.Tests/"]

RUN dotnet restore

# build
COPY . .

RUN dotnet build

FROM build as test
WORKDIR /app

# launch the tests
RUN dotnet test --logger:trx

