dotnet restore --verbosity normal
dotnet build --configuration Release --verbosity normal .\src\Mockaco\Mockaco.csproj
dotnet test --configuration Release --verbosity normal .\test\Mockaco.AspNetCore.Tests\Mockaco.AspNetCore.Tests.csproj
dotnet pack --configuration Nuget --output ./nupkg
docker build -f ./src/Mockaco/Docker/Dockerfile -t mockaco:local .

$containerName = [guid]::NewGuid().ToString()
try {
    docker run --name $containerName -d -p 5000:5000 -v ${PSScriptRoot}/src/Mockaco/Mocks:/app/Mocks mockaco:local
    Start-Sleep -Seconds 5
    docker run --rm -v ${PSScriptRoot}/test/_postman:/etc/newman -t postman/newman:alpine run Mockaco.postman_collection.json
}
finally {
    docker container stop $containerName
    docker container rm $containerName
}
