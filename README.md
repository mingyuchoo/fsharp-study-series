# fsharp-study-series

## Prerequsits

Install dotnet 9 on your machine.

### In Windows

```pwsh
winget install dotnet-runtime-9
winget install dotnet-sdk-9
winget upgrade
```

### In Ubuntu 24.10

```pwsh
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

```pwsh
dotnet new console -lang F# -o MyFSharpApp
```

## How to run the project

```pwsh
cd MyFSharpApp
dotnet run
```


## References

- https://fsharp.org/
- https://dotnet.microsoft.com/ko-kr/learn/languages/fsharp-hello-world-tutorial/install
