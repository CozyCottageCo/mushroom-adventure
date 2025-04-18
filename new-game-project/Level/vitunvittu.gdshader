shader_type canvas_item;

uniform vec2 light_position; // normalized (0–1) screen position
uniform float light_radius : hint_range(0.0, 1.0) = 0.2;
uniform vec4 darkness_color = vec4(0.0, 0.0, 0.0, 0.8); // semi-transparent black

void fragment() {
    vec2 screen_pos = SCREEN_UV;
float aspect_ratio = 640.0 / 360.0;
    screen_pos.x *= aspect_ratio;
    float dist = distance(screen_pos, light_position);

    // Calculate fade factor: 0.0 at center, 1.0 at edge and beyond
    float fade = clamp((dist - (light_radius * 0.5)) / (light_radius * 0.5), 0.0, 1.0);

    // Blend between full transparency and darkness based on distance
    COLOR = mix(vec4(0.0, 0.0, 0.0, 0.0), darkness_color, fade);
}
