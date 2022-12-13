一个.NET 7.0示例项目，再尝试写点东西


### 编译构建

这里target目录定为输出目录

```shell
# 服务端构建
dotnet publish -c Release --output ./Docker/bin Gliese.sln
```

### 构建Docker镜像

```bash
# 构建docker镜像
cd Docker
sudo docker build -f Dockerfile -t polaris-www-server:latest .
# 测试执行构建的镜像
sudo docker run -p 8082:8082 polaris-www-server
# 仅在本地测试时使用，将aws凭证文件挂载到docker容器
sudo docker run -p 8082:8082 -v $HOME/.aws/credentials:/root/.aws/credentials:ro polaris-www-server
```
