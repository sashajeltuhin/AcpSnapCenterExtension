using Apprenda.API.Extension.Bootstrapping;
using Apprenda.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using DBCloning.Models;

namespace DBCloneBootstrapper
{

    public class DBCloneBSP : BootstrapperBase
    {
        private static readonly ILogger log =
                LogManager.Instance().GetLogger(typeof(DBCloneBSP));
        public override BootstrappingResult Bootstrap(BootstrappingRequest request)
        {
            try
            {
                log.Info("DBClone Bootstrapper starting");
                
                var prop = request.Properties.First(p => p.Name == CustomProperties.DBCloneType);
                if (prop == null || prop.Values == null || string.IsNullOrEmpty(prop.Values.First()) || prop.Values.First().ToLower() == "none")
                {
                    log.Info("DB Cloning not requested");
                    return BootstrappingResult.Success();
                }

                var propDBServer = request.Properties.First(p => p.Name == CustomProperties.SnapDBCloneHost);
                if (propDBServer == null || propDBServer.Values == null || string.IsNullOrEmpty(propDBServer.Values.First()))
                {
                    log.Error($"Cloning requested but the DB host name is missing. Configure the host name or do not request connection to the cloned DB by setting DBCloneType property to 'none'");
                    return BootstrappingResult.Failure(new[] { $"Cloning requested but the DB host name is missing" });
                }
                string serverName = propDBServer.Values.First();

                var propDBName = request.Properties.First(p => p.Name == CustomProperties.SnapDBName);
                if (propDBName == null || propDBName.Values == null || string.IsNullOrEmpty(propDBName.Values.First()))
                {
                    log.Error($"Cloning requested but the cloned DB name is missing. Configure the DB name or do not request connection to the cloned DB by setting DBCloneType property to 'none'");
                    return BootstrappingResult.Failure(new[] { $"Cloning requested but the cloned DB name is missing." });
                }
                string dbName = propDBName.Values.First();

                var propDBUser = request.Properties.First(p => p.Name == CustomProperties.DBUser);
                if (propDBUser == null || propDBUser.Values == null || string.IsNullOrEmpty(propDBUser.Values.First()))
                {
                    log.Error($"Cloning requested but the DB user is not configured. Configure the DB user or do not request connection to the cloned DB by setting DBCloneType property to 'none'");
                    return BootstrappingResult.Failure(new[] { $"Cloning requested but the DB user is not configured." });
                }
                string dbUser = propDBUser.Values.First();

                var propDBUserPass = request.Properties.First(p => p.Name == CustomProperties.DBUserCreds);
                if (propDBUserPass == null || propDBUserPass.Values == null || string.IsNullOrEmpty(propDBUserPass.Values.First()))
                {
                    log.Error($"Cloning requested but the DB user password is not set. Configure the DB user password or do not request connection to the cloned DB by setting DBCloneType property to 'none'");
                    return BootstrappingResult.Failure(new[] { $"Cloning requested but the DB user is not configured." });
                }
                string dbUserPass = propDBUserPass.Values.First();
                log.Info($"Retrieving web config for modification from path {request.ComponentPath}");
                string[] configFiles = Directory.GetFiles(request.ComponentPath, "Web.config", SearchOption.AllDirectories);
                if (configFiles.Length <= 0)
                {
                    log.Error($"Config file not found in  {request.ComponentPath}");
                    return BootstrappingResult.Failure(new[] { "Failed to find a web.config for application " + request.ApplicationAlias });
                }

                string filePath = configFiles[0];
                //string cloneDB = SnapSession.BuildCloneName(dbName, request.ApplicationAlias);
                UpdateConnectionString(filePath, serverName, dbUser, dbName, dbUserPass);

                log.Info("Connection string mod for MySql is complete");

            }
            catch (Exception ex)
            {
                log.Error($"Error updating connection info for application {request.ApplicationAlias}. Reason: {ex.Message}");
                return BootstrappingResult.Failure(new[] { $"Error updating connection info for application {request.ApplicationAlias}. Reason: {ex.Message}" });
            }

            return BootstrappingResult.Success();
        }

        private static void UpdateConnectionString(string filePath, string serverName, string dbUser, string dbName,  string pass)
        {          
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            string connectionString = string.Format("server={0};user={1};database={2};port=3306;password={3}", serverName, dbUser, dbName, pass);
            log.Info($"The new connection string is: {connectionString}. Proceeding to web config mod...");

            XmlElement mysqlConnection = (XmlElement)xmlDoc.SelectSingleNode("//configuration/connectionStrings/add[@name = 'MySqlServer']");
            if (mysqlConnection != null)
            {
                // modify the existing value
                mysqlConnection.Attributes["connectionString"].Value = connectionString;
            }
            else
            {
                //create new node
                XmlNode rootNode = xmlDoc.SelectSingleNode("//configuration");
                if (rootNode == null)
                {
                    throw new Exception("The config file is missing root configuration node");
                }
                XmlNode connectionNode = xmlDoc.SelectSingleNode("//configuration/connectionStrings");
                if (connectionNode == null)
                {
                    rootNode.AppendChild(CreateNewNode("connectionStrings", xmlDoc));
                    connectionNode = xmlDoc.SelectSingleNode("//configuration/connectionStrings");
                }

                connectionNode.AppendChild(CreateNewElement("name", "MySqlServer", "connectionString", connectionString, xmlDoc));
            }


            xmlDoc.Save(filePath);
        }

        private static XmlNode CreateNewElement(string attribute1Name, string attribute1Value, string attribute2Name, string attribute2Value, XmlDocument xmlDoc)
        {
            XmlNode newElement = xmlDoc.CreateNode(XmlNodeType.Element, "add", null);
            XmlAttribute attribute1 = xmlDoc.CreateAttribute(attribute1Name);
            attribute1.Value = attribute1Value;
            newElement.Attributes.Append(attribute1);
            XmlAttribute attribute2 = xmlDoc.CreateAttribute(attribute2Name);
            attribute2.Value = attribute2Value;
            newElement.Attributes.Append(attribute2);
            return newElement;
        }

        private static XmlNode CreateNewNode(string nodeName, XmlDocument xmlDoc)
        {
            XmlNode newElement = xmlDoc.CreateNode(XmlNodeType.Element, nodeName, null);
            return newElement;
        }
    }
}
