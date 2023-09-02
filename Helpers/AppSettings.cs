namespace WebApi.Helpers;

public class GoogleConfig
{
    public string SharedFolder { get; set; }

    public string CertsFolder { get; set; }

}

public class ElasticSearchConfig
{
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FingerPrint { get; set; }
}

public class AppSettings
{
    public string Secret { get; set; }

    // refresh token time to live (in days), inactive tokens are
    // automatically deleted from the database after this time
    public int RefreshTokenTTL { get; set; }

    public string EmailFrom { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }

    public GoogleConfig Google { get; set; }

    public ElasticSearchConfig Elasticsearch { get; set; }
}