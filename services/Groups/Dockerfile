FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore "Groups.csproj"

COPY . .
RUN dotnet publish "Groups.csproj" -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:8 AS runtime
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Groups::Amazon.Lambda.AspNetCoreServer.Hosting.APIGatewayHttpApiV2Handler::FunctionHandlerAsync"]
