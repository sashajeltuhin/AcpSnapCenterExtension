using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocModels
{
    public class ResourceBase
    {
        public string Href { get; set; }
    }

    public class NodePageResourceBase
    {
        public int currentPage { get; set; }
        public int pageSize { get; set; }
        public int totalPages { get; set; }
        public int totalItems { get; set; }
        public List<Node> items { get; set; }
        public ResourceBase nextPage { get; set; }
        public ResourceBase previousPage { get; set; }
        public string href { get; set; }
    }

    public class Node
    {
        public string name { get; set; }
        public string href { get; set; }
        public List<NodeRole> nodeRoles { get; set; }
        public string cloudName { get; set; }
        public ResourceBase workloads { get; set; }
    }

    public class NodeRole {
        public string nodeRole { get; set; }
        public string databaseProvider { get; set; }
        public List<NodeRole> nodeRoles { get; set; }
        public string cloudName { get; set; }
        public ResourceBase workloads { get; set; }
        public bool isContainerRunning { get; set; }
        public bool isWindowsUiHost { get; set; }
        public string osVersion { get; set; }
        public string apprendaPlatformVersion { get; set; }
        public bool isGhost { get; set; }
        public int totalMemory { get; set; }
        public int totalActualMemory { get; set; }
        public int clockSpeed { get; set; }
        public int processorCount { get; set; }
        public int allocatedMemory { get; set; }
        public int totalStorage { get; set; }
        public int allocatedStorage { get; set; }
        public int allocatedCpu { get; set; }
        public string architecture { get; set; }
        public ResourceAllocationReport resourceAllocation { get; set; }
    }

    public class ResourceAllocationReport {
        public int allocatedCpu { get; set; }
        public int allocatedMemory { get; set; }
        public int allocatedStorage { get; set; }
    }
}
