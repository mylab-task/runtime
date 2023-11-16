ARG PROJ=MyLab.Task.Runtime

# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG PROJ
WORKDIR /src

# copy csproj and restore as distinct layers
COPY . .
RUN dotnet restore

# copy everything else and build app
WORKDIR ./${PROJ}
RUN dotnet publish -c release -o /app --no-restore -p:ExcludePack=true

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
ARG PROJ
ENV PROJ_FILE=$PROJ
WORKDIR /app
COPY --from=build /app /app
EXPOSE 80

RUN apt update -y && apt install -y wget

COPY ./Scripts/ /usr/local/bin/

RUN find /usr/local/bin -name "*.sh" -type f -exec chmod +x {} \;

RUN mkdir -p /etc/task-runtime/assets

ENTRYPOINT ["sh", "-c", "dotnet /app/${PROJ_FILE}.dll"]