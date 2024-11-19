# Base dotnet image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY eu-west-2-bundle.pem /eu-west-2-bundle.pem

# Add curl to template.
# CDP PLATFORM HEALTHCHECK REQUIREMENT
RUN apt update && \
    apt install curl -y && \
    rm -rf /var/lib/apt/lists/*
    

RUN curl -L -o /tmp/zulu.tar.gz https://cdn.azul.com/zulu/bin/zulu21.36.17-ca-jdk21.0.4-linux_x64.tar.gz \
    && mkdir -p /usr/lib/jvm \
    && tar -xvf /tmp/zulu.tar.gz -C /usr/lib/jvm \
    && rm /tmp/zulu.tar.gz \
    && ln -s /usr/lib/jvm/zulu21.36.17-ca-jdk21.0.4-linux_x64 /usr/lib/jvm/default-java


RUN ls -l /usr/lib/jvm/default-java

# Set environment variables
ENV JAVA_HOME=/usr/lib/jvm/default-java
ENV PATH="$JAVA_HOME/bin:$PATH"

RUN echo $PATH

# Install Liquibase
RUN curl -sL https://github.com/liquibase/liquibase/releases/download/v4.23.0/liquibase-4.23.0.tar.gz \
    | tar -xz -C /opt \
    && ln -s /opt/liquibase /usr/local/bin/liquibase 


# Verify installation
RUN java -version && liquibase --version

# Build stage image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
WORKDIR "/src"

# unit test and code coverage
RUN dotnet test CdpDotnetAuroraSpike.Test

FROM build AS publish
RUN dotnet publish CdpDotnetAuroraSpike -c Release -o /app/publish /p:UseAppHost=false


ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8085
ENTRYPOINT ["dotnet", "CdpDotnetAuroraSpike.dll"]
