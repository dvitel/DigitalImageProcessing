FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
RUN apk --update --no-cache add bash g++
COPY ./web/. ./web/
COPY ./iptool/. ./iptool/
RUN mkdir lib
RUN g++ -c -I./iptool/ -o ./lib/dip.dll.o -fPIC ./iptool/lib.cpp
RUN g++ -shared -o ./lib/dip.dll ./lib/dip.dll.o
RUN nm -D ./lib/dip.dll
RUN dotnet restore ./web/
RUN dotnet publish -c Debug -o ../out ./web/

# copy everything else and build app
# COPY ../web/. ./web/
# WORKDIR /app/aspnetapp
# RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS runtime
WORKDIR /app
RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories
RUN apk --update --no-cache add libgdiplus
RUN ln -s /usr/lib/libgdiplus.so.0 /usr/lib/libgdiplus.dll
COPY --from=build /app/out ./
RUN ls
ENTRYPOINT ["dotnet", "web.dll"]