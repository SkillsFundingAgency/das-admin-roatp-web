## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

## RoATP Admin Web
<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status%2Fdas-admin-roatp-web?repoName=SkillsFundingAgency%2Fdas-admin-roatp-web&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=4161&repoName=SkillsFundingAgency%2Fdas-admin-roatp-web&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-admin-roatp-web&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-admin-roatp-web)
[![Confluence Page](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/304644526/Register+of+Apprenticeship+Training+Providers+RoATP)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## About
This web solution is part of RoATP project. RoATP requires admin users to manage Training Providers register on the RoATP system.

## How It Works

RoATP Admin Web UI landing page have links to these web applications, base on the claims of the signed in user. The users can click on the links to navigate to the corresponding application. 
The applications will be opened in a new tab and the user will be automatically signed in using DFE Sign-in.

- Roatp GatewayAssessor
- Roatp FinancialAssessor
- Roatp ApplicationOversight
- Roatp Assessor 


There are 5 types of admin users, pure admin users who mainly manage manage Training Providers register. The users have to have a working DFE Sign-in to authenticate and they should have at least one of the following claims: 
- APR (Roatp Admin role)
- GAC (Roatp GatewayAssessor role)
- FHC (Roatp FinancialAssessor role)
- AOV (Roatp ApplicationOversight role)
- AAC (Roatp Assessor role)

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Visual studio or similar IDE 
* .Net 10.0 SDK
* An Azure Active Directory account with the appropriate roles as per the [config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-tools-servicebus-support/SFA.DAS.Tools.Servicebus.Support.json).
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/AdminRoatp) should be available either running locally or accessible in an Azure tenancy.

### Configuration
- Create a `Configuration `table in your (Development) local storage account.
- Obtain the [SFA.DAS.AdminRoatp.Web.json](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-admin-roatp-web/SFA.DAS.AdminRoatp.Web.json) from the `das-employer-config`.
- Add a row to the Configuration table with fields: 
  - PartitionKey: LOCAL
  - RowKey: SFA.DAS.AdminRoatp.Web_1.0
  - Data: {The contents of the `SFA.DAS.AdminRoatp.Web.json` file}

In the web project, if not exist already, add `AppSettings.Development.json` file with following content:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "ConfigNames": "SFA.DAS.AdminRoatp.Web,SFA.DAS.Provider.DfeSignIn",
  "EnvironmentName": "LOCAL",
  "ResourceEnvironmentName": "LOCAL",
  "Version": "1.0",
  "cdn": {
    "url": "https://das-at-frnt-end.azureedge.net"
  }
}  
```

Make sure you have DFESignin configuration set up from [das-employer-config repository]
(https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-shared-config/SFA.DAS.Provider.DfeSignIn.json)
using: 
* PartitionKey: LOCAL
* RowKey: SFA.DAS.Provider.DfeSignIn_1.0
* Data: (config as above)

## Technologies
* .Net 10.0
* NUnit
* Moq
* FluentAssertions
* RestEase
* MediatR
