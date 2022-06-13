#Build image where we get the apps
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

COPY . .

RUN dotnet publish -c Release -warnaserror -o ./publish

#Image to run the docker
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as base

RUN apt-get update && apt install -y zlib1g-dev

RUN echo "Copying Data"
COPY ./bin/Data Data

RUN echo "Copying Settngs"
COPY ./bin/settings.ini .

RUN echo "Copying zlib"
COPY ./bin/zlib.dll .
COPY ./bin/zlib32.dll .
COPY ./bin/zlib64.dll .

RUN echo "Copying Muls"
RUN mkdir Muls
#COPY ./bin/Muls Muls

COPY --from=build /publish/ .

ENTRYPOINT ["dotnet", "CorexUO.dll"]