# 发版与更新检查

## releaseUrl 和 downloadUrl 的区别

`releaseUrl` 是发布详情页地址，适合放版本说明、多个安装包入口、历史版本等内容。

`downloadUrl` 是直接安装包地址，点击后通常会直接下载文件。

当前程序会优先打开当前系统匹配到的 `packages[].downloadUrl`。如果没有匹配包，则退回到顶层 `downloadUrl`，最后再退回到 `releaseUrl`。

## 版本号

当前应用版本在 `EncryptTool.csproj` 中维护：

```xml
<Version>1.0.0</Version>
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
```

每次发版时递增版本号，例如 `1.0.1`。

## 更新清单地址

应用启动后会读取程序集元数据里的 `UpdateManifestUrl`，下载远端 JSON，并和当前 `Version` 比较。未配置地址时不会检查更新。

推荐在发布命令里传入：

```powershell
dotnet publish .\EncryptTool.csproj -p:PublishProfile=win-x64 -p:UpdateManifestUrl=https://your-domain.com/encrypttool/release.json
```

也可以直接写到 `EncryptTool.csproj`：

```xml
<UpdateManifestUrl>https://your-domain.com/encrypttool/release.json</UpdateManifestUrl>
```

## 多平台更新清单

参考 `release-manifest.example.json`：

```json
{
  "version": "1.0.1",
  "downloadUrl": "https://your-domain.com/encrypttool/releases/1.0.1/EncryptTool-1.0.1-win-x64.zip",
  "releaseUrl": "https://your-domain.com/encrypttool/releases/1.0.1/",
  "releaseNotes": "Fix known issues and improve UI experience.",
  "forceUpdate": false,
  "packages": [
    {
      "rid": "win-x64",
      "fileName": "EncryptTool-1.0.1-win-x64.zip",
      "downloadUrl": "https://your-domain.com/encrypttool/releases/1.0.1/EncryptTool-1.0.1-win-x64.zip"
    },
    {
      "rid": "linux-x64",
      "fileName": "EncryptTool-1.0.1-linux-x64.tar.gz",
      "downloadUrl": "https://your-domain.com/encrypttool/releases/1.0.1/EncryptTool-1.0.1-linux-x64.tar.gz"
    }
  ]
}
```

程序会根据当前运行环境自动匹配 RID：

- Windows 64 位：`win-x64`
- Windows 32 位：`win-x86`
- Linux 64 位：`linux-x64`
- macOS Apple Silicon：`osx-arm64`
- macOS Intel：`osx-x64`

## 本地发布命令

Windows x64：

```powershell
dotnet publish .\EncryptTool.csproj -p:PublishProfile=win-x64 -p:UpdateManifestUrl=https://your-domain.com/encrypttool/release.json
```

Windows x86：

```powershell
dotnet publish .\EncryptTool.csproj -p:PublishProfile=FolderProfile -p:UpdateManifestUrl=https://your-domain.com/encrypttool/release.json
```

Linux x64：

```powershell
dotnet publish .\EncryptTool.csproj -p:PublishProfile=linux-x64 -p:UpdateManifestUrl=https://your-domain.com/encrypttool/release.json
```

发布完成后，把对应目录打包：

- `bin\Release\net8.0\publish\win-x64\` 打成 `EncryptTool-1.0.1-win-x64.zip`
- `bin\Release\net8.0\publish\win-x86\` 打成 `EncryptTool-1.0.1-win-x86.zip`
- `bin\Release\net8.0\publish\linux-x64\` 打成 `EncryptTool-1.0.1-linux-x64.tar.gz`

## 上传到腾讯云 Linux 服务器

假设服务器目录为 `/var/www/encrypttool`：

```bash
sudo mkdir -p /var/www/encrypttool/releases/1.0.1
sudo chown -R $USER:$USER /var/www/encrypttool
```

从本机上传文件，示例：

```powershell
scp .\EncryptTool-1.0.1-win-x64.zip root@your-server-ip:/var/www/encrypttool/releases/1.0.1/
scp .\EncryptTool-1.0.1-linux-x64.tar.gz root@your-server-ip:/var/www/encrypttool/releases/1.0.1/
scp .\release.json root@your-server-ip:/var/www/encrypttool/release.json
```

Nginx 站点示例：

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location /encrypttool/ {
        alias /var/www/encrypttool/;
        autoindex on;
    }
}
```

重载 Nginx：

```bash
sudo nginx -t
sudo systemctl reload nginx
```

最终清单地址类似：

```text
https://your-domain.com/encrypttool/release.json
```

每次发版时，只需要上传新的安装包，并更新服务器上的 `release.json`。
