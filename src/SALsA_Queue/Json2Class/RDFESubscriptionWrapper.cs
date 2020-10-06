using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALsA.LivesiteAutomation.Json2Class.RDFESubscriptionWrapper
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Subscription
    {

        private string[] locationConstraintField;

        private SubscriptionStorageService[] storageServiceField;

        private SubscriptionPrincipal[] principalField;

        private SubscriptionHostedService[] hostedServiceField;

        private string[] featureField;

        private string[] subscriptionCertificateField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LocationConstraint")]
        public string[] LocationConstraint
        {
            get
            {
                return this.locationConstraintField;
            }
            set
            {
                this.locationConstraintField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StorageService")]
        public SubscriptionStorageService[] StorageService
        {
            get
            {
                return this.storageServiceField;
            }
            set
            {
                this.storageServiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Principal")]
        public SubscriptionPrincipal[] Principal
        {
            get
            {
                return this.principalField;
            }
            set
            {
                this.principalField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HostedService")]
        public SubscriptionHostedService[] HostedService
        {
            get
            {
                return this.hostedServiceField;
            }
            set
            {
                this.hostedServiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Feature")]
        public string[] Feature
        {
            get
            {
                return this.featureField;
            }
            set
            {
                this.featureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SubscriptionCertificate")]
        public string[] SubscriptionCertificate
        {
            get
            {
                return this.subscriptionCertificateField;
            }
            set
            {
                this.subscriptionCertificateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionStorageService
    {

        private string locationConstraintField;

        private string[] extendedPropertiesField;

        private string[] textField;

        /// <remarks/>
        public string LocationConstraint
        {
            get
            {
                return this.locationConstraintField;
            }
            set
            {
                this.locationConstraintField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExtendedProperties")]
        public string[] ExtendedProperties
        {
            get
            {
                return this.extendedPropertiesField;
            }
            set
            {
                this.extendedPropertiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionPrincipal
    {

        private string userField;

        /// <remarks/>
        public string User
        {
            get
            {
                return this.userField;
            }
            set
            {
                this.userField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionHostedService
    {

        private string lastCreateDeploymentInStagingSlotTrackingField;
        private string lastCreateDeploymentInProductionSlotTrackingField;

        private string[] extendedPropertiesField;

        private string[] certificateIdentityField;

        private SubscriptionHostedServiceDeployment deploymentField;

        private string[] textField;

        /// <remarks/>
        public string LastCreateDeploymentInStagingSlotTracking
        {
            get
            {
                return this.lastCreateDeploymentInStagingSlotTrackingField;
            }
            set
            {
                this.lastCreateDeploymentInStagingSlotTrackingField = value;
            }
        }

        /// <remarks/>
        public string LastCreateDeploymentInProductionSlotTracking
        {
            get
            {
                return this.lastCreateDeploymentInProductionSlotTrackingField;
            }
            set
            {
                this.lastCreateDeploymentInProductionSlotTrackingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ExtendedProperties")]
        public string[] ExtendedProperties
        {
            get
            {
                return this.extendedPropertiesField;
            }
            set
            {
                this.extendedPropertiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CertificateIdentity")]
        public string[] CertificateIdentity
        {
            get
            {
                return this.certificateIdentityField;
            }
            set
            {
                this.certificateIdentityField = value;
            }
        }

        /// <remarks/>

        public SubscriptionHostedServiceDeployment Deployment
        {
            get
            {
                return this.deploymentField;
            }
            set
            {
                this.deploymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionHostedServiceDeployment
    {

        private ServiceConfiguration serviceConfigurationField;

        private SubscriptionHostedServiceDeploymentLastChangingOperationTracking lastChangingOperationTrackingField;

        private string persistentVMDowntimeInfoField;

        private SubscriptionHostedServiceDeploymentRole[] roleField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
        public ServiceConfiguration ServiceConfiguration
        {
            get
            {
                return this.serviceConfigurationField;
            }
            set
            {
                this.serviceConfigurationField = value;
            }
        }

        /// <remarks/>
        public SubscriptionHostedServiceDeploymentLastChangingOperationTracking LastChangingOperationTracking
        {
            get
            {
                return this.lastChangingOperationTrackingField;
            }
            set
            {
                this.lastChangingOperationTrackingField = value;
            }
        }

        /// <remarks/>
        public string PersistentVMDowntimeInfo
        {
            get
            {
                return this.persistentVMDowntimeInfoField;
            }
            set
            {
                this.persistentVMDowntimeInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Role")]
        public SubscriptionHostedServiceDeploymentRole[] Role
        {
            get
            {
                return this.roleField;
            }
            set
            {
                this.roleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration", IsNullable = false)]
    public partial class ServiceConfiguration
    {

        private ServiceConfigurationRole[] roleField;

        private ServiceConfigurationNetworkConfiguration networkConfigurationField;

        private string xmlns_xsdField;

        private string xmlns_xsiField;

        private string serviceNameField;

        private byte osFamilyField;

        private string osVersionField;

        private string schemaVersionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Role")]
        public ServiceConfigurationRole[] Role
        {
            get
            {
                return this.roleField;
            }
            set
            {
                this.roleField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationNetworkConfiguration NetworkConfiguration
        {
            get
            {
                return this.networkConfigurationField;
            }
            set
            {
                this.networkConfigurationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string xmlns_xsd
        {
            get
            {
                return this.xmlns_xsdField;
            }
            set
            {
                this.xmlns_xsdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string xmlns_xsi
        {
            get
            {
                return this.xmlns_xsiField;
            }
            set
            {
                this.xmlns_xsiField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string serviceName
        {
            get
            {
                return this.serviceNameField;
            }
            set
            {
                this.serviceNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte osFamily
        {
            get
            {
                return this.osFamilyField;
            }
            set
            {
                this.osFamilyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string osVersion
        {
            get
            {
                return this.osVersionField;
            }
            set
            {
                this.osVersionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string schemaVersion
        {
            get
            {
                return this.schemaVersionField;
            }
            set
            {
                this.schemaVersionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationRole
    {

        private ServiceConfigurationRoleConfigurationSettings configurationSettingsField;

        private ServiceConfigurationRoleInstances instancesField;

        private ServiceConfigurationRoleCertificates certificatesField;

        private string nameField;

        /// <remarks/>
        public ServiceConfigurationRoleConfigurationSettings ConfigurationSettings
        {
            get
            {
                return this.configurationSettingsField;
            }
            set
            {
                this.configurationSettingsField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationRoleInstances Instances
        {
            get
            {
                return this.instancesField;
            }
            set
            {
                this.instancesField = value;
            }
        }

        /// <remarks/>

        public ServiceConfigurationRoleCertificates Certificates
        {
            get
            {
                return this.certificatesField;
            }
            set
            {
                this.certificatesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationRoleConfigurationSettings
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationRoleInstances
    {

        private byte countField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte count
        {
            get
            {
                return this.countField;
            }
            set
            {
                this.countField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationRoleCertificates
    {

        private ServiceConfigurationRoleCertificatesCertificate certificateField;

        /// <remarks/>
        public ServiceConfigurationRoleCertificatesCertificate Certificate
        {
            get
            {
                return this.certificateField;
            }
            set
            {
                this.certificateField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationRoleCertificatesCertificate
    {

        private string nameField;

        private string thumbprintField;

        private string thumbprintAlgorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string thumbprint
        {
            get
            {
                return this.thumbprintField;
            }
            set
            {
                this.thumbprintField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string thumbprintAlgorithm
        {
            get
            {
                return this.thumbprintAlgorithmField;
            }
            set
            {
                this.thumbprintAlgorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfiguration
    {

        private ServiceConfigurationNetworkConfigurationVirtualNetworkSite virtualNetworkSiteField;

        private ServiceConfigurationNetworkConfigurationAddressAssignments addressAssignmentsField;

        private ServiceConfigurationNetworkConfigurationAccessControls accessControlsField;

        private ServiceConfigurationNetworkConfigurationEndpointAcls endpointAclsField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationVirtualNetworkSite VirtualNetworkSite
        {
            get
            {
                return this.virtualNetworkSiteField;
            }
            set
            {
                this.virtualNetworkSiteField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAddressAssignments AddressAssignments
        {
            get
            {
                return this.addressAssignmentsField;
            }
            set
            {
                this.addressAssignmentsField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAccessControls AccessControls
        {
            get
            {
                return this.accessControlsField;
            }
            set
            {
                this.accessControlsField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationEndpointAcls EndpointAcls
        {
            get
            {
                return this.endpointAclsField;
            }
            set
            {
                this.endpointAclsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationVirtualNetworkSite
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignments
    {

        private ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddress[] instanceAddressField;

        private ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPs reservedIPsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InstanceAddress")]
        public ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddress[] InstanceAddress
        {
            get
            {
                return this.instanceAddressField;
            }
            set
            {
                this.instanceAddressField = value;
            }
        }

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPs ReservedIPs
        {
            get
            {
                return this.reservedIPsField;
            }
            set
            {
                this.reservedIPsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddress
    {

        private ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnets subnetsField;

        private string roleNameField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnets Subnets
        {
            get
            {
                return this.subnetsField;
            }
            set
            {
                this.subnetsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string roleName
        {
            get
            {
                return this.roleNameField;
            }
            set
            {
                this.roleNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnets
    {

        private ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnetsSubnet subnetField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnetsSubnet Subnet
        {
            get
            {
                return this.subnetField;
            }
            set
            {
                this.subnetField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignmentsInstanceAddressSubnetsSubnet
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPs
    {

        private ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPsReservedIP reservedIPField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPsReservedIP ReservedIP
        {
            get
            {
                return this.reservedIPField;
            }
            set
            {
                this.reservedIPField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAddressAssignmentsReservedIPsReservedIP
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAccessControls
    {

        private ServiceConfigurationNetworkConfigurationAccessControlsAccessControl accessControlField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationAccessControlsAccessControl AccessControl
        {
            get
            {
                return this.accessControlField;
            }
            set
            {
                this.accessControlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAccessControlsAccessControl
    {

        private ServiceConfigurationNetworkConfigurationAccessControlsAccessControlRule[] ruleField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Rule")]
        public ServiceConfigurationNetworkConfigurationAccessControlsAccessControlRule[] Rule
        {
            get
            {
                return this.ruleField;
            }
            set
            {
                this.ruleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationAccessControlsAccessControlRule
    {

        private ushort orderField;

        private string actionField;

        private string remoteSubnetField;

        private string descriptionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort order
        {
            get
            {
                return this.orderField;
            }
            set
            {
                this.orderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string remoteSubnet
        {
            get
            {
                return this.remoteSubnetField;
            }
            set
            {
                this.remoteSubnetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationEndpointAcls
    {

        private ServiceConfigurationNetworkConfigurationEndpointAclsEndpointAcl endpointAclField;

        /// <remarks/>
        public ServiceConfigurationNetworkConfigurationEndpointAclsEndpointAcl EndpointAcl
        {
            get
            {
                return this.endpointAclField;
            }
            set
            {
                this.endpointAclField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration")]
    public partial class ServiceConfigurationNetworkConfigurationEndpointAclsEndpointAcl
    {

        private string roleField;

        private string endPointField;

        private string accessControlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string role
        {
            get
            {
                return this.roleField;
            }
            set
            {
                this.roleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string endPoint
        {
            get
            {
                return this.endPointField;
            }
            set
            {
                this.endPointField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string accessControl
        {
            get
            {
                return this.accessControlField;
            }
            set
            {
                this.accessControlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionHostedServiceDeploymentLastChangingOperationTracking
    {

        private string errorDetailField;

        private string[] textField;

        /// <remarks/>
        public string ErrorDetail
        {
            get
            {
                return this.errorDetailField;
            }
            set
            {
                this.errorDetailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionHostedServiceDeploymentRole
    {

        private SubscriptionHostedServiceDeploymentRoleRoleInstance[] roleInstanceField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RoleInstance")]
        public SubscriptionHostedServiceDeploymentRoleRoleInstance[] RoleInstance
        {
            get
            {
                return this.roleInstanceField;
            }
            set
            {
                this.roleInstanceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubscriptionHostedServiceDeploymentRoleRoleInstance
    {

        private string[] instanceEndpointField;

        private string vmField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InstanceEndpoint")]
        public string[] InstanceEndpoint
        {
            get
            {
                return this.instanceEndpointField;
            }
            set
            {
                this.instanceEndpointField = value;
            }
        }

        /// <remarks/>
        public string VM
        {
            get
            {
                return this.vmField;
            }
            set
            {
                this.vmField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }
}
