shader_type canvas_item;

uniform vec2 light_position; // Normalized position of the light source (no range hint needed)
uniform float light_radius : hint_range(0.0, 1.0); // Normalized radius of the light (in the range of 0.0 to 1.0)

void fragment() {
    // Calculate the distance from the current pixel to the light position
    float distance = length(UV - light_position);
    
    // Calculate the light effect using a smoothstep falloff for the light radius
    float light_factor = smoothstep(light_radius, light_radius - 0.1, distance);
    
    // Define darkness (black with 50% opacity)
    vec4 darkness = vec4(0.0, 0.0, 0.0, 0.5); // Black with 50% opacity
    
    // Define light color (pure white, no alpha - it will affect the darkness)
    vec4 light_color = vec4(1.0, 1.0, 1.0, 0.0); // White, fully transparent (no light by default)
    
    // Mix the darkness and light based on the light factor
    COLOR = mix(darkness, light_color, 1.0 - light_factor);
}
