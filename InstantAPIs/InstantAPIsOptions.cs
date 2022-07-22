﻿using Swashbuckle.AspNetCore.SwaggerGen;

namespace InstantAPIs;

public enum EnableSwagger
{
    None,
    DevelopmentOnly,
    Always
}

public class InstantAPIsOptions
{

    public EnableSwagger? EnableSwagger { get; set; }
    public Action<SwaggerGenOptions>? Swagger { get; set; }
}