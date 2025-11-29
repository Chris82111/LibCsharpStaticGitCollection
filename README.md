# LibCsharpStaticGitCollection

<div align="center">

  [![Shields](https://img.shields.io/badge/.NET-8.0-5C2D91)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0 "Download .NET 8.0")
  [![Visual Studio](https://img.shields.io/badge/IDE-Visual_Studio-5C2D91)](https://visualstudio.microsoft.com/ "Download Visual Studio")
  ![Linux x64](https://img.shields.io/badge/Linux-x64-009639)
  ![Windows x64](https://img.shields.io/badge/Windows-x64-0067C0)

  [![Docker Engine](https://img.shields.io/badge/dependency-Docker_Engine-0d4dF2)](https://docs.docker.com/engine/install "Link to web page")
  [![WSL](https://img.shields.io/badge/dependency_(Windows)-WSL-00BCF2)](https://learn.microsoft.com/en-us/windows/wsl/install "Link to web page")

  [![Git for Windows](https://img.shields.io/badge/dependency_(Windows)-Git_for_Windows-F1502F)](https://github.com/git-for-windows/git "Link to repository")
  [![zlib, dependency for Git for Linux](https://img.shields.io/badge/dependency_(Linux)-zlib-2C652C)](https://zlib.net/ "Link to web page")
  [![OpenSSL, dependency for Git for Linux](https://img.shields.io/badge/dependency_(Linux)-OpenSSL-731513)](https://github.com/openssl/openssl/ "Link to repository")
  [![libpsl, dependency for Git for Linux](https://img.shields.io/badge/dependency_(Linux)-libpsl-394E79)](https://www.linuxfromscratch.org/blfs/view/svn/basicnet/libpsl.html "Link to web page")
  [![curl, dependency for Git for Linux](https://img.shields.io/badge/dependency_(Linux)-curl-093754)](https://curl.se/download.html "Link to web page")
  [![Git](https://img.shields.io/badge/dependency_(Linux)-Git_(source_code)-F1502F)](https://git-scm.com/install/source "Link to web page")

</div>

## Update 

The version of Git for Windows (MinGit) can be specified in the file [GitLinux.props](LibCsharpStaticGitCollection/Lib/GitLinux.props).
The individual versions of Git for Linux can be specified in the [Dockerfile](LibCsharpStaticGitCollection/Lib/Dockerfile) file.

## Bild Requirement

Docker is required for the build process; WSL is also required on Windows.
The build process takes approximately 9 minutes and 15 seconds.

- `dotnet build /p:Configuration=Debug`
- `dotnet build /p:Configuration=Release`

### Docker

The entire build process requires many prerequisites. To always provide the same build environment and avoid complications with the host operating system, the build is performed in a container. For this reason, Docker is required to create the static Git for Linux.

#### Docker on Windows

1. See the Microsoft description: [How to install Linux on Windows with WSL](https://learn.microsoft.com/en-us/windows/wsl/install)
2. Start PowerShell in administrator mode.
3. Install WSL: `wsl --install`
4. Restart the computer
5. The installation program will start automatically, follow the descriptions
6. Open the Run dialog box with Windows key + R and enter `wsl`
7. See the Docker description: [Install Docker Engine](https://docs.docker.com/engine/install)
8. Follow the description.
9. `wsl sudo sudo usermod -aG docker $USER`
10. `wsl --shutdown`
11. Verify the installation,
    1. from inside WSL: `docker run hello-world`
    2. from cmd/PowerShell: `wsl docker run hello-world`

## Influencing the Build Process

There are various ways to disable individual functions during the build:

1. The file `LibraryConfigDefaults.props` can be modified, but it is tracked by Git.
2. A new file `LibraryConfigOverrides.props` can be created next to the file `LibraryConfigDefaults.props` with the following content:
   
   ```cs
   <Project>
     <PropertyGroup>
       <EnableStaticGitUseLinux>false</EnableStaticGitUseLinux>
       <EnableStaticGitUseWindows>false</EnableStaticGitUseWindows>
     </PropertyGroup>
   </Project>
   ```
   
3. An environment variable can override the properties
4. A custom property can override the properties: \
   `dotnet build -c Debug -p:EnableStaticGitUseLinux=false -p:EnableStaticGitUseWindows=false`