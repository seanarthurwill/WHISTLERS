FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore "Organizations.csproj"

COPY . .
RUN dotnet publish "Organizations.csproj" -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:8 AS runtime
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["Organizations::Amazon.Lambda.AspNetCoreServer.Hosting.APIGatewayHttpApiV2Handler::FunctionHandlerAsync"]
