
Shader "Heatmaps/Particles/AlphaBlend" {
    Category {
     BindChannels { 
         Bind "Color", color 
         Bind "Vertex", vertex
     }
     SubShader { 
         Lighting Off 
         Fog { Mode Off }
         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
         Pass { } 
     }
 }
}
