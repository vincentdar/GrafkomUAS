#version 330 core
out vec4 FragColor;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    sampler2D diffuse_sampler;
    sampler2D specular_sampler;
    float shininess; //Shininess is the power the specular light is raised to
};
struct Light {
    vec3 position;
    //vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
//We create the light and the material struct as uniforms.
uniform Light light;
uniform Material material;

uniform vec3 viewPos;
in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;
void main()
{
    //ambient
    //float ambientStrength = 0.1;
    //vec3 ambient = ambientStrength * lightColor;
    //vec3 ambient = light.ambient * material.ambient; //Remember to use the material here.
    //vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    vec3 ambient = light.ambient * vec3(texture(material.diffuse_sampler, TexCoords)) * material.ambient;

    //diffuse
    vec3 norm = normalize(Normal);
    //vec3 lightDir = normalize(lightPos - FragPos); //Note: The light is pointing from the light to the fragment
    vec3 lightDir = normalize(light.position - FragPos);
    //vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(norm, lightDir), 0.0); //We make sure the value is non negative with the max function.
    //vec3 diffuse = diff * lightColor;
    //vec3 diffuse = light.diffuse * (diff * material.diffuse); //Remember to use the material here.
    //vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse_sampler, TexCoords))
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse_sampler, TexCoords)) * material.diffuse;

    //specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    //float spec = pow(max(dot(viewDir, reflectDir), 0.0), 256); //The 32 is the shininess of the material.
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    //vec3 specular = specularStrength * spec * lightColor;
    //vec3 specular = light.specular * (spec * material.specular); //Remember to use the material here.
    //vec3 specular = light.specular * spec * vec3(texture(material.specular_sampler, TexCoords))
    vec3 specular = light.specular * spec * vec3(texture(material.specular_sampler, TexCoords)) * material.specular;
    //vec3 result = (ambient + diffuse + specular) * objectColor;
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}