using System.ComponentModel;

namespace TheTechIdea.Beep.MVVM
{
    /// <summary>
    /// ViewModel for industrial connections like OPC/OPC-UA
    /// </summary>
    public class IndustrialConnectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Connection Settings
        private string _serverUrl = "opc.tcp://localhost:4840";
        public string ServerUrl
        {
            get => _serverUrl;
            set { _serverUrl = value; OnPropertyChanged(nameof(ServerUrl)); }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private string _clientId = "BeepOPCClient";
        public string ClientId
        {
            get => _clientId;
            set { _clientId = value; OnPropertyChanged(nameof(ClientId)); }
        }

        private string _sessionName = "BeepSession";
        public string SessionName
        {
            get => _sessionName;
            set { _sessionName = value; OnPropertyChanged(nameof(SessionName)); }
        }

        private int _sessionTimeout = 60000;
        public int SessionTimeout
        {
            get => _sessionTimeout;
            set { _sessionTimeout = value; OnPropertyChanged(nameof(SessionTimeout)); }
        }

        private int _operationTimeout = 30000;
        public int OperationTimeout
        {
            get => _operationTimeout;
            set { _operationTimeout = value; OnPropertyChanged(nameof(OperationTimeout)); }
        }

        private bool _autoReconnect = true;
        public bool AutoReconnect
        {
            get => _autoReconnect;
            set { _autoReconnect = value; OnPropertyChanged(nameof(AutoReconnect)); }
        }

        private int _reconnectInterval = 5000;
        public int ReconnectInterval
        {
            get => _reconnectInterval;
            set { _reconnectInterval = value; OnPropertyChanged(nameof(ReconnectInterval)); }
        }
        #endregion

        #region SSL Configuration
        private bool _enableSSL = true;
        public bool EnableSSL
        {
            get => _enableSSL;
            set { _enableSSL = value; OnPropertyChanged(nameof(EnableSSL)); }
        }

        private string _sslVersion = "TLSv1.3";
        public string SSLVersion
        {
            get => _sslVersion;
            set { _sslVersion = value; OnPropertyChanged(nameof(SSLVersion)); }
        }

        private string _clientCertificate;
        public string ClientCertificate
        {
            get => _clientCertificate;
            set { _clientCertificate = value; OnPropertyChanged(nameof(ClientCertificate)); }
        }

        private string _certificatePassword;
        public string CertificatePassword
        {
            get => _certificatePassword;
            set { _certificatePassword = value; OnPropertyChanged(nameof(CertificatePassword)); }
        }

        private bool _verifySSL = true;
        public bool VerifySSL
        {
            get => _verifySSL;
            set { _verifySSL = value; OnPropertyChanged(nameof(VerifySSL)); }
        }

        private string _caCertificate;
        public string CACertificate
        {
            get => _caCertificate;
            set { _caCertificate = value; OnPropertyChanged(nameof(CACertificate)); }
        }

        private int _sslTimeout = 30000;
        public int SSLTimeout
        {
            get => _sslTimeout;
            set { _sslTimeout = value; OnPropertyChanged(nameof(SSLTimeout)); }
        }
        #endregion

        #region OPC Settings
        private string _securityPolicy = "None";
        public string SecurityPolicy
        {
            get => _securityPolicy;
            set { _securityPolicy = value; OnPropertyChanged(nameof(SecurityPolicy)); }
        }

        private string _messageSecurityMode = "None";
        public string MessageSecurityMode
        {
            get => _messageSecurityMode;
            set { _messageSecurityMode = value; OnPropertyChanged(nameof(MessageSecurityMode)); }
        }

        private string _userIdentityType = "Anonymous";
        public string UserIdentityType
        {
            get => _userIdentityType;
            set { _userIdentityType = value; OnPropertyChanged(nameof(UserIdentityType)); }
        }

        private string _applicationUri = "urn:BeepOPCClient";
        public string ApplicationUri
        {
            get => _applicationUri;
            set { _applicationUri = value; OnPropertyChanged(nameof(ApplicationUri)); }
        }

        private string _applicationName = "Beep OPC Client";
        public string ApplicationName
        {
            get => _applicationName;
            set { _applicationName = value; OnPropertyChanged(nameof(ApplicationName)); }
        }

        private string _productUri = "urn:BeepOPCClient";
        public string ProductUri
        {
            get => _productUri;
            set { _productUri = value; OnPropertyChanged(nameof(ProductUri)); }
        }

        private int _maxStringLength = 65535;
        public int MaxStringLength
        {
            get => _maxStringLength;
            set { _maxStringLength = value; OnPropertyChanged(nameof(MaxStringLength)); }
        }

        private int _maxByteStringLength = 65535;
        public int MaxByteStringLength
        {
            get => _maxByteStringLength;
            set { _maxByteStringLength = value; OnPropertyChanged(nameof(MaxByteStringLength)); }
        }

        private int _maxArrayLength = 65535;
        public int MaxArrayLength
        {
            get => _maxArrayLength;
            set { _maxArrayLength = value; OnPropertyChanged(nameof(MaxArrayLength)); }
        }

        private int _maxMessageSize = 4194304;
        public int MaxMessageSize
        {
            get => _maxMessageSize;
            set { _maxMessageSize = value; OnPropertyChanged(nameof(MaxMessageSize)); }
        }

        private int _maxBufferSize = 65535;
        public int MaxBufferSize
        {
            get => _maxBufferSize;
            set { _maxBufferSize = value; OnPropertyChanged(nameof(MaxBufferSize)); }
        }

        private int _channelLifetime = 300000;
        public int ChannelLifetime
        {
            get => _channelLifetime;
            set { _channelLifetime = value; OnPropertyChanged(nameof(ChannelLifetime)); }
        }

        private int _securityTokenLifetime = 3600000;
        public int SecurityTokenLifetime
        {
            get => _securityTokenLifetime;
            set { _securityTokenLifetime = value; OnPropertyChanged(nameof(SecurityTokenLifetime)); }
        }

        private bool _enableSubscriptions = true;
        public bool EnableSubscriptions
        {
            get => _enableSubscriptions;
            set { _enableSubscriptions = value; OnPropertyChanged(nameof(EnableSubscriptions)); }
        }

        private int _publishingInterval = 1000;
        public int PublishingInterval
        {
            get => _publishingInterval;
            set { _publishingInterval = value; OnPropertyChanged(nameof(PublishingInterval)); }
        }

        private int _maxNotificationsPerPublish = 1000;
        public int MaxNotificationsPerPublish
        {
            get => _maxNotificationsPerPublish;
            set { _maxNotificationsPerPublish = value; OnPropertyChanged(nameof(MaxNotificationsPerPublish)); }
        }

        private bool _enableTimestamps = true;
        public bool EnableTimestamps
        {
            get => _enableTimestamps;
            set { _enableTimestamps = value; OnPropertyChanged(nameof(EnableTimestamps)); }
        }

        private bool _enableEvents = false;
        public bool EnableEvents
        {
            get => _enableEvents;
            set { _enableEvents = value; OnPropertyChanged(nameof(EnableEvents)); }
        }

        private bool _enableHistoricalAccess = false;
        public bool EnableHistoricalAccess
        {
            get => _enableHistoricalAccess;
            set { _enableHistoricalAccess = value; OnPropertyChanged(nameof(EnableHistoricalAccess)); }
        }

        private string _namespaceUri = "urn:DefaultNamespace";
        public string NamespaceUri
        {
            get => _namespaceUri;
            set { _namespaceUri = value; OnPropertyChanged(nameof(NamespaceUri)); }
        }

        private bool _useBinaryEncoding = true;
        public bool UseBinaryEncoding
        {
            get => _useBinaryEncoding;
            set { _useBinaryEncoding = value; OnPropertyChanged(nameof(UseBinaryEncoding)); }
        }

        private bool _useCompression = false;
        public bool UseCompression
        {
            get => _useCompression;
            set { _useCompression = value; OnPropertyChanged(nameof(UseCompression)); }
        }

        private int _keepAliveInterval = 10000;
        public int KeepAliveInterval
        {
            get => _keepAliveInterval;
            set { _keepAliveInterval = value; OnPropertyChanged(nameof(KeepAliveInterval)); }
        }
        #endregion
    }
}