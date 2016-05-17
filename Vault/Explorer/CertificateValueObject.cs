﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.Common.Vault.Explorer
{
    /// <summary>
    /// Certificate (.cer, .crt, .pfx, .p12, .pfxb64, .p12b64) based secret value JSON object
    /// </summary>
    [JsonObject]
    public class CertificateValueObject
    {
        private X509Certificate2 _cert;

        [JsonProperty]
        public readonly string Data;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string Password;

        [JsonConstructor]
        public CertificateValueObject(string data, string password)
        {
            Data = data;
            Password = password;
            byte[] rawData = Convert.FromBase64String(data);
            _cert = (null == password) ? new X509Certificate2(rawData) : new X509Certificate2(rawData, password, X509KeyStorageFlags.DefaultKeySet);
        }

        public CertificateValueObject(FileInfo file, string password) : 
            this(Convert.ToBase64String(File.ReadAllBytes(file.FullName)), password)
        {
        }

        public void FillTags(ObservableTagItemsCollection tags)
        {
            tags.AddOrReplace(new TagItem("Thumbprint", _cert.Thumbprint.ToLowerInvariant()));
            tags.AddOrReplace(new TagItem("Expiration", _cert.GetExpirationDateString()));
            tags.AddOrReplace(new TagItem("Subject", _cert.Subject.Replace("CN=", "")));
            var sans = 
                from X509Extension ext in _cert.Extensions
                where ext.Oid.Value == "2.5.29.17" // Subject Alternative Name
                select ext.Format(false).Replace("DNS Name=", "");
            tags.AddOrReplace(new TagItem("SAN", string.Join(";", sans)));           
        }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static CertificateValueObject FromJson(string json) => JsonConvert.DeserializeObject<CertificateValueObject>(json);
    }
}