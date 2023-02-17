FROM mcr.microsoft.com/dotnet/aspnet:7.0

# 指定RUN工作目录
WORKDIR /opt

# 拷贝程序
COPY Gliese/bin/Release/net7.0/linux-x64 /opt

# 启动程序
ENTRYPOINT ["dotnet", "/opt/Gliese.dll"]