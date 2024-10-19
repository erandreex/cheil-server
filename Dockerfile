# Usa una imagen base para el SDK de .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Establece el directorio de trabajo
WORKDIR /app

# Copia el archivo de proyecto y restaura las dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto de los archivos de la aplicación
COPY . ./

# Compila la aplicación
RUN dotnet publish -c Release -o out

# Usa una imagen base para el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Establece el directorio de trabajo
WORKDIR /app

# Copia los archivos de la aplicación desde la etapa de compilación
COPY --from=build /app/out .

# Expone el puerto donde la aplicación escuchará
EXPOSE 8080

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "server.dll"]
