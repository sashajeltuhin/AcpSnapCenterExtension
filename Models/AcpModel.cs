[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Scope = "namespace", Target = "ContivCommon.Models", Justification = "External contract")]

namespace DBCloning.Models
{
    using System;
    using System.Collections.Generic;

    public enum LoadBalancerUrlConfigurationType
    {
        Preserve = 0,
        Redirect = 1
    }

    public class ResourceBase
    {
        public string Href { get; set; }
    }

    public class PageResourceBase
    {
        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalPages { get; set; }
        public int totalItems { get; set; }
        public List<SOCCustomProperty> items { get; set; }
        public ResourceBase nextPage { get; set; }
        public ResourceBase previousPage { get; set; }
        public string href { get; set; }
    }

    public class SOCCustomProperty {
        public int id { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public CustomPropertyValueOptions valueOptions { get; set; }
        //public CustomPropertyDeveloperOptions developerOptions { get; set; }
        //public CustomPropertyApplicabilityOptionCollection applicability {get; set; }
        public string href { get; set; }
    }

    public class CustomPropertyValueOptions
    {
        public List<string> possibleValues { get; set; }
        public List<string> defaultValues { get; set; }
        public bool? allowCustomValues { get; set; }
    }
    public class CustomPropertyDeveloperOptions
    {
        public bool? isVisible { get; set; }
        public VisibilityOptions visibilityOptions { get; set; }
    }
    public class CustomPropertyApplicabilityOptionCollection
    {
        public CustomPropertyApplicationOptions applications { get; set; }
        public CustomPropertyApplicabilityOption computeServers { get; set; }
        public CustomPropertyApplicabilityOption databaseServers{ get; set; }
        public CustomPropertyApplicabilityOption resourcePolicies{ get; set; }
        public CustomPropertyApplicabilityOption storageQuotas{ get; set; }
    }

    public class VisibilityOptions
    {
        public bool? isEditableByDeveloper { get; set; }
        public bool? isRequiredForDeployment { get; set; }
    }
    public class CustomPropertyApplicationOptions
    {
        public bool? isComponentLevel { get; set; }
        public CustomPropertyApplicationComponentOptions applicationComponentLevelOptions { get; set; }
        public bool? isApplied { get; set; }
        public bool? allowMultipleValues { get; set; }
    }

    public class CustomPropertyApplicabilityOption
    {
        public bool? isApplied { get; set; }
        public bool? allowMultipleValues { get; set; }
    }

    public class CustomPropertyApplicationComponentOptions
    {
        public bool? userInterfaces { get; set; }
        public bool? javaWebApplications { get; set; }
        public bool? windowsServices { get; set; }
        public bool? linuxServices { get; set; }
        public bool? databases { get; set; }
        public bool? pods { get; set; }
    }

    public class StorageQuotaReference : ResourceBase
    {
        public string Name { get; set; }
    }

    public class ResourceAllocationPolicyReference : ResourceBase
    {
        public string Name { get; set; }
    }

    public class ComponentReference : ResourceBase
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Alias { get; set; }
    }

    public class Component : ComponentReference
    {
        public ResourceBase Version { get; set; }

        public StorageQuotaReference StorageQuota { get; set; }

        public ResourceAllocationPolicyReference ResourcePolicy { get; set; }

        public ResourceBase CustomProperties { get; set; }
    }

    public class CustomPropertyModel : ResourceBase
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool ArbitraryValuesAllowed { get; set; }

        public bool MultiSelectAllowed { get; set; }

        public bool Editable { get; set; }

        public List<string> Values { get; set; }

        public List<string> DefaultValues { get; set; }
    }

    public class CustomProperty : ResourceBase
    {
        public ResourceBase Version { get; set; }

        public ResourceBase ReferencedObject { get; set; }

        public CustomPropertyModel PropertyModel { get; set; }

        public List<string> Values { get; set; }
    }

    public class ApplicationVersion : ResourceBase
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public string Stage { get; set; }

        public string State { get; set; }

        public ResourceBase Application { get; set; }

        public bool? EnableStickySessions { get; set; }

        public bool? EnableSessionReplication { get; set; }

        public bool? EnableSslEnforcement { get; set; }

        public LoadBalancerUrlConfigurationType? LoadBalancerUrlConfiguration { get; set; }

        public bool InMaintenance { get; set; }

        public Uri Url { get; set; }

        public double LastHourUptime { get; set; }

        public double LastDayUptime { get; set; }

        public double LastMonthUptime { get; set; }

        public ResourceBase PreviousVersion { get; set; }

        public ResourceBase Components { get; set; }

        public ResourceBase Workloads { get; set; }

        public ResourceBase CustomProperties { get; set; }

        public ResourceBase ArchiveDownload { get; set; }

        public ResourceBase FilesDownload { get; set; }

        public ResourceBase ManifestDownload { get; set; }

        public ResourceBase Tenants { get; set; }
    }
}