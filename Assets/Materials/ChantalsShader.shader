// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ChantalsShader" {
    Properties{
    [Toggle] _useCoord("use Coord", Float) = 0
             _maxCoord("max Coord", Float) = 6
    }
        SubShader{
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // include file that contains UnityObjectToWorldNormal helper function
                #include "UnityCG.cginc"

            bool _useCoord;
            float _maxCoord;
            struct VertInput {
                float4 pos : POSITION;
                fixed4 color : COLOR0;
                
            };
            struct VertOutput {
                // we'll output world space normal as one of regular ("texcoord") interpolators
                half3 worldNormal : TEXCOORD0;
                float4 pos : SV_POSITION;
                half3 color: COLOR;
                //float3 wpos: POSITION;
            };

    // vertex shader: takes object space normal as input too
            VertOutput vert( VertInput i) {
                        VertOutput o;
                o.pos = UnityObjectToClipPos(i.pos);
                float3 wpos = mul(unity_ObjectToWorld, i.pos).xyz;
                // UnityCG.cginc file contains function to transform
                // normal from object to world space, use that
                //o.color = i.pos.xyz;
                //if (!_useCoord) o.color = i.color;// abs((wpos - floor(wpos)) - 0.5) * 2;
               // else 
                    o.color = abs((wpos/ _maxCoord - floor(wpos/ _maxCoord)) - 0.5) ;
                
             //   o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }

            half4 frag(VertOutput o) : SV_Target
            {
                half4 c = 0;
                // normal is a 3D vector with xyz components; in -1..1
                // range. To display it as color, bring the range into 0..1
                // and put into red, green, blue components
               // c.rgb = o.SV_POSITION;
                return half4(o.color, 0.5f);
        }
ENDCG
}
    }
}
