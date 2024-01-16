namespace WebApiUtils.ApiAddresses
{
    public static class ApiDictionary
    {
        private static string protocol = "https";
        private static string port = "8081";

        public static NamedApiMethods BrandApi { get; private set; } = new NamedApiMethods(protocol, "brand_api", port);
        public static NamedApiMethods ModelApi { get; private set; } = new NamedApiMethods(protocol, "model_api", port);
        public static NamedApiMethods ClientApi { get; private set; } = new NamedApiMethods(protocol, "client_api", port);
        public static NamedApiMethods SalerApi { get; private set; } = new NamedApiMethods(protocol, "saler_api", port);

        public static NamedApiMethods CarApi { get; private set; } = new NamedApiMethods(protocol, "car_api", port);
        public static RentApiMethods CarRentApi { get; private set; } = new RentApiMethods(protocol, "carrent_api", port);

        public static string IdentityServer => $"{protocol}://identity_server:{port}";
    }
}
