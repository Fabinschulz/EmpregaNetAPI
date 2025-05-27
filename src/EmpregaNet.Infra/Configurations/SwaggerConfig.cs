using Microsoft.AspNetCore.Builder;


namespace EmpregaNet.Infra.Configurations
{
    public static class SwaggerConfig
    {
        public static WebApplicationBuilder AddSwaggerDoc(this WebApplicationBuilder builder)
        {
            
           
            return builder;
        }
    }
}