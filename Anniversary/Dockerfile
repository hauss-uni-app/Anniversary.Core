#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#
#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
#WORKDIR /code
#COPY *.csproj ./
#RUN dotnet restore 
#COPY . ./
#RUN dotnet publish  -c Release -o out
#
#FROM base AS final
#WORKDIR /app
#COPY --from=build /code/out ./
#ENTRYPOINT ["dotnet", "Anniversary.dll"]

#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
#WORKDIR /src
#COPY ["Anniversary/Anniversary.csproj", "Anniversary/"]
#RUN dotnet restore "Anniversary/Anniversary.csproj"
#COPY . .
#WORKDIR "/src/Anniversary"
#RUN dotnet build "Anniversary.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Anniversary.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Anniversary.dll"]


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Anniversary.dll"]