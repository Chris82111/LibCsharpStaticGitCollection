# LibCsharpStaticGitCollection

<div align="center">

  [![Shields](https://img.shields.io/badge/.NET-8.0-5C2D91)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0 "Download .NET 8.0")
  [![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91)](https://visualstudio.microsoft.com/de/ "Download Visual Studio")
  [![Git for Windows](https://img.shields.io/badge/dependency-Git_for_Windows-F1502F)](https://github.com/git-for-windows/git "Link to repository")

</div>

## Update 

The version of Git for Windows (MinGit) can be specified in the file [MinGitLib.props](LibCsharpStaticGitCollection/Lib/MinGitLib.props).

## Bild Requirement

### Docker

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