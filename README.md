# fsharp-study-series

## Prerequsits

Install dotnet 9 on your machine.

### In Windows

```pwsh
winget install dotnet-runtime-9
winget install dotnet-sdk-9
winget upgrade
```

### In macOS

- https://dotnet.microsoft.com/en-us/download/dotnet/

```zsh
brew install mono-libgdiplus wget
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh
```

### In Ubuntu 24.10

```bash
sudo apt install -y ca-certificates \
  libc6 \
  libgcc-s1 \
  libicu74 \
  liblttng-ust1 \
  libssl3 \
  libstdc++6 \
  libunwind8 \
  zlib1g
  
sudo apt install -y dotnet-sdk-9.0
sudo apt install -y dotnet-runtime-9.0 # or aspnetcore-runtime-9.0
```

## Create a project

### Console project

```pwsh
dotnet new console -lang F# -o MyFSharpConsoleApp
cd MyFSharpConsoleApp
dotnet run
```

### Web project

```pwsh
dotnet new web -lang F# -o MyFSharpWebApp
cd MyFSharpWebApp
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
dotnet run
```
Connect to `http://localhost:5050`

## How to run the project

```pwsh
```


## References

- https://fsharp.org/
- https://dotnet.microsoft.com/ko-kr/learn/languages/fsharp-hello-world-tutorial/install
