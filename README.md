# Digital Apprenticeships Service

## Employer Incentives

Licensed under the [MIT license](https://github.com/SkillsFundingAgency/das-employer-incentives/blob/master/LICENSE)

### Developer Setup

#### Requirements


#### Setup


##### Publish Database


##### Config


#### To run a local copy you will also require 


#### Run the solution


#### SonarCloud Analysis (Optional)

SonarCloud analysis can be performed using a docker container which can be built from the included dockerfile.

    Docker must be running Windows containers in this instance

An example of the docker run command to analyse the code base can be found below. 

For this docker container to be successfully created you will need:
* docker running Windows containers
* a user on SonarCloud.io with permission to run analysis
* a SonarQube.Analysis.xml file in the root of the git repository.

This file takes the format:

```xml
<SonarQubeAnalysisProperties  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.sonarsource.com/msbuild/integration/2015/1">
<Property Name="sonar.host.url">https://sonarcloud.io</Property>
<Property Name="sonar.login">[Your SonarCloud user token]</Property>
</SonarQubeAnalysisProperties>
```     

##### Example:

_docker run [OPTIONS] IMAGE COMMAND_

[Docker run documentation](https://docs.docker.com/engine/reference/commandline/run/)

```docker run --rm -v c:/projects/das-assessor-service:c:/projects/das-assessor-service -w c:/projects/das-assessor-service 3d9151a444b2 powershell -F c:/projects/das-assessor-service/sonarcloud/analyse.ps1```

###### Options:

|Option|Description|
|---|---|
|--rm| Remove any existing containers for this image
|-v| Bind the current directory of the host to the given directory in the container ($PWD may be different on your platform). This should be the folder where the code to be analysed is
|-w| Set the working directory

###### Command:

Execute the analyse.ps1 PowerShell script	    

### Getting Started
