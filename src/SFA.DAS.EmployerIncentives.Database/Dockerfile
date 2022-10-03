FROM mcr.microsoft.com/mssql/server:2019-latest
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Pa55word!
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433

#
## Elevate to root to install required packages
USER root

## Get CLI tool to do dacpac deployment
RUN apt-get update \
    && apt-get install unzip -y

## Install SQLPackage for Linux and make it executable
RUN wget -progress=bar:force -q -O sqlpackage.zip https://aka.ms/sqlpackage-linux \
    && unzip -qq sqlpackage.zip -d /opt/sqlpackage \
    && chmod +x /opt/sqlpackage/sqlpackage

## TODO : Put following in separate image

## Add the DACPAC to the image
COPY SFA.DAS.EmployerIncentives.Database.dacpac /tmp/db/db.dacpac

USER mssql

## Configure the required environmental variables
ENV DB_NAME=Model

# Launch SQL Server, confirm startup is complete, deploy the DACPAC, then terminate SQL Server.
# See https://stackoverflow.com/a/51589787/488695
RUN (/opt/mssql/bin/sqlservr --accept-eula & ) | grep -q "Service Broker manager has started" \
    && /opt/sqlpackage/sqlpackage /a:Publish /tsn:localhost /tdn:$DB_NAME /tu:sa /tp:$SA_PASSWORD /sf:/tmp/db/db.dacpac \
    && pkill sqlservr