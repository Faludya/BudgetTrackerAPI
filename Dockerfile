# Use the .NET 8 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and restore
COPY ./BudgetTrackerAPI.sln .
COPY ./BudgetTrackerAPI ./BudgetTrackerAPI/
COPY ./Models ./Models/
COPY ./Repositories ./Repositories/
COPY ./Services ./Services/

RUN dotnet restore "BudgetTrackerAPI/BudgetTrackerAPI.csproj"

# Build and publish the project
WORKDIR /src/BudgetTrackerAPI
RUN dotnet publish "BudgetTrackerAPI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BudgetTrackerAPI.dll"]
