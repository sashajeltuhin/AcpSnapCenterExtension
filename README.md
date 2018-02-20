# ACP extension for DB cloning with NetApp SnapCenter

## Introduction
This automated solution simplifies the process of generating multiple copies of databases on-demand for pre-production purposes. The cloning of a pristine database for Dev/Test is instrumented
automatically at the application deployment time, using native ONTAP features like snapshots and FlexClones via REST API.

At the compute layer, the integrated solution relies on Apprenda deployment policy engine and the abstraction model that frees the developers from the need to know intricacies of database cloning and reduces the need in meetings. 
Apprenda policy engine allows the operators to securely segment the platform based on various needs, SDLC environments being one of them. 
The established policies determine where applications instances are deployed and what databases they are connecting to. 
The databases similar to the applications themselves are segmented based on the deployment policies ensuring secure separation of pre-production and production environments. ​
 Multiple servers can be setup to host cloned databases, in which case Apprenda will be controlling the placement of clones based on the CPU and memory utilization.​


 ## Architecture and Installation

 This automated database provisioning is conducted in two steps. First, Apprenda Extension for SnapCenter is invoked by Apprenda Deployment Pipeline. 
 It communicates with SnapCenter REST API to perform the desired type of cloning and mounting based on the metadata that accompanies the application. 
 At a later stage of the pipeline, Apprenda Bootstrapper for SnapCenter is called to update the application configuration file with the new connection information, 
 so that the application, once it is containerized by the platform, can connect to the cloned database. ​

 ![Architecture flow](https://github.com/sashajeltuhin/AcpSnapCenterExtension/blob/master/docs/process.PNG "Architecture Flow")

 ### Installation
* Create the following Custom Properties:
  * DBCloneType.  There are three options:​
    * CloneOriginal uses the original version of the database (a production copy or a specially prepared data set) for the cloning. The application consuming the database gets the most up-to-date data in this case.​
    * RestoreClone makes an attempt to restore the last available snapshot of the cloned DB.  This option is set as a default because the restore process is much faster than the initial cloning of the original. ​
			In both of these cases, the application starts from a clean slate as far as the data is concerned. All changes to the data that were made while the application was tested are overwritten by the recent production data or the original state of the earlier clone.​
			In some cases, it is, however, desirable not to refresh the data.​
    * KeepExisting option allows developers to maintain the state of the data between the test runs. ​
  * DBUser  (Visible and Editable by devs)
  * DBUserCreds (Visible and Editable by devs)
  * SnapPlugin
  * SnapCenterUrl
  * SnapCenterAdmin
  * SnapCenterPassword
  * SnapDBName   (Visible and Editable by devs)
  * SnapDBHost
  * SnapDBCloneHost
  * SnapPolicy
  * SnapMountScript
  * SnapDataLeafIP
  * SnapMountPath
* Deploy [the extension, "DBCloneExt.zip"](https://github.com/sashajeltuhin/AcpSnapCenterExtension/blob/master/Deploy/DBCloneExt.zip) to Apprenda
* In the operator portal, go to Configuration - Registry and update **DeveloperPortalExtServices** setting with the reference to the extension in the following format: `<application_name>(<version_number>)/<service_name>`
 If the setting does not exist, create it.
 NOTE: when upgrading the extension, make sure to temporary remove the setting value prior to the deployment. Once the new version is deployed, put the setting value back in the resgitry, referencing the new version number

* Deploy [the bootstrapper "BSP.zip"](https://github.com/sashajeltuhin/AcpSnapCenterExtension/blob/master/Deploy/BSP.zip) via Apprenda operator portal. 

​



​