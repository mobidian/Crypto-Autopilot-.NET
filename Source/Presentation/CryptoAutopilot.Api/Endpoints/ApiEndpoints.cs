namespace CryptoAutopilot.Api.Endpoints;

public static class ApiEndpoints
{
    private const string ApiBase = "api";


    public static class Data
    {
        private const string Base = $"{ApiBase}/data";

        public static class Market
        {
            private const string Base = $"{Data.Base}/market";

            public const string GetAllKlines = $"{Base}/klines/{{contractName}}/{{min:int:min(1):max(43200)}}";
        }

        public static class Trading
        {
            private const string Base = $"{Data.Base}/trading";
            
            public const string GetAllOrders = $"{Base}/orders";
            public const string GetAllPositions = $"{Base}/positions";
        }
    }

    public static class Strategies
    {
        private const string Base = $"{ApiBase}/strategies";

        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;

        public const string Stop = $"{Base}/stop/{{id:guid}}";
    }
}
