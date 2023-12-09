﻿namespace Movies.Api;

public class ApiEndpoints
{
    private const string ApiBase = "api";
    public static class Movies
    {
        private const string Base = $"{ApiBase}/movies";

        //here are the list of all the endpoints
        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
    }
    
}