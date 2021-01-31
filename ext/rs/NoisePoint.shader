Shader "Lxm/NoisePoint"
{
    Properties
    {
		//方格登分Mask纹理
        _MaskTex ("MaskTex", 2D) = "white" {}
		//圆贴图
		_PointTex("PointTex", 2D) = "white" {}
		//圆贴图
		_PointTex2("PointTex2", 2D) = "white" {}
		//圆半径（不可小于等分）
		_radius("radius", Range(0,1)) = 0.15
		//圆半径（缩放用）要比圆半径小
		_radiusScale("_radiusScale", Range(0.001,1)) = 0.14
		//等分数
		_dxy("dxy", Range(5,40)) = 5
		//圆心位置强度
		_pointNoise("pointNoise", float) = 10
		//Spark大小
		_sparkAffect("sparkAffect", float) = 10
			//辅助线
		_helper("helper", Range(0,1)) = 0
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		// No culling or depth
        Cull Off 
		ZWrite Off 
		ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _PointTex;
			sampler2D _PointTex2;
			fixed _radius;
			fixed _radiusScale;
			fixed _dxy;
			fixed _pointNoise;

			fixed _sparkAffect;

			fixed _helper;

			//噪波函数（b站搬运工）
			fixed N21(fixed2 p)
			{
				p = frac(p*fixed2(233.34, 851.73));
				p += dot(p, p + 23.45);
				return frac(p.x*p.y);
			}

			//噪波函数（b站搬运工）
			fixed2 N22(fixed2 p)
			{
				fixed n = N21(p);
				return fixed2(n, N21(p + n));
			}

			//点的圆心
			fixed2 pointPos(fixed2 id)
			{
				fixed2 n = N22(id);
				fixed x = sin(_pointNoise*n.x);
				fixed y = cos(_pointNoise *n.y);
				//用来防止点超出方格
				return fixed2(x, y) * (0.5 - _radius);
			}

            fixed4 frag (v2f i) : SV_Target
            {
				fixed2 uv = i.uv;
				uv *= _dxy;
				//缩放成网格的uv坐标（1/dxy之后）
				fixed2 gv = frac(uv) - 0.5;
				//噪波处理前的uv值[0,_dxy]
				fixed2 id = floor(uv);
				//最终颜色
				fixed4 col = fixed4(0, 0, 0, 0);
				//方格mask值采样
				fixed4 maska = tex2D(_PointTex2, (i.uv - 1/ _dxy / 2));
				//颜色叠加
				fixed4 add = fixed4(1, 1, 1,1);
				//噪波处理后的uv值[0,1]
				fixed2 p1 = pointPos(id);
				//画点
				fixed d = length(gv - p1);
				//位置变化
				fixed scale = sin(_Time.z + p1.x * 10) * 0.5 + 0.5;

				//纯色点
				fixed m = smoothstep(_radius * scale, (_radius - _radiusScale) * scale, d);
				col = m * add;

				//spark发亮
				//float j = d * _sparkAffect;
				//float m = 1 / dot(j,j) * scale;
				
				//采样纹理
				//fixed no = 1-step(_radius * scale, d);
				//fixed2 newUV = fixed2(gv.x - p1.x + 0.5, gv.y- p1.y + 0.5);
				//col = no * tex2D(_PointTex, fixed2(newUV));

				col.a *= maska.x;

				//网格辅助参考线
				if (gv.x > 0.48 || gv.y > 0.48) col = float4(1, 0, 0, 1);
				
				return col;
            }
            ENDCG
        }
    }
}
