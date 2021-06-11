#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
//layout(location = 1) in vec2 aTexCoord;
//out vec2 texCoord;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;

void main(){
	//texCoord = aTexCoord; //tyt harus sama antara di vert dan frag jd texCoord bisa aku ganti texCoordOut tapi
						  //vert dan fragnya harus diatur biar sama
	gl_Position = vec4(aPosition,1.0) * transform * view * projection;
	FragPos = vec3(vec4(aPosition, 1.0) * transform);
	Normal = aNormal * mat3(transpose(inverse(transform)));

}

