Shader "Unlit/HeatmapShader"
{
	Properties
		//Provide proper attribution and heavily modify, Ideally contribute to Open source at the same time

	{
	  _MainTex("Texture", 2D) = "white" {}
	  _Color0("Color 0",Color) = (0,0,0,1)
		_Color1("Color 1",Color) = (0,.9,.2,1)
		_Color2("Color 2",Color) = (.9,1,.3,1)
		_Color3("Color 3",Color) = (.9,.7,.1,1)
		_Color4("Color 4",Color) = (1,0,0,1)

		_Range0("Range 0",Range(0,1)) = 0.
		_Range1("Range 1",Range(0,1)) = 0.25
		_Range2("Range 2",Range(0,1)) = 0.5
		_Range3("Range 3",Range(0,1)) = 0.75
		_Range4("Range 4",Range(0,1)) = 1

		_Diameter("Diameter",Range(0,1)) = 1.0
		_Strength("Strength",Range(.1,4)) = 1.0
		_PulseSpeed("Pulse Speed",Range(0,5)) = 0
	}
		SubShader
	  {
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #pragma multi_compile_fog

		  #include "UnityCG.cginc"

		  struct appdata
		  {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			//FRAGCOORD0
			//Allows displaying of 
			UNITY_VERTEX_INPUT_INSTANCE_ID  //PER UNITY DOCUMENTATION https://docs.unity3d.com/Manual/SinglePassInstancing.html 

		  };

		  struct v2f
		  {
			float2 uv : TEXCOORD0;
			UNITY_FOG_COORDS(1) //Likely not needed
			float4 vertex : SV_POSITION;

			UNITY_VERTEX_OUTPUT_STEREO

		  };

		  sampler2D _MainTex;
		  float4 _MainTex_ST;

		  float4 _Color0;
		  float4 _Color1;
		  float4 _Color2;
		  float4 _Color3;
		  float4 _Color4;


		  float _Range0;
		  float _Range1;
		  float _Range2;
		  float _Range3;
		  float _Range4;
		  float _Diameter;
		  float _Strength;

		  float _PulseSpeed;

		  v2f vert(appdata v)
		  {
			v2f o;

			//Calculates/sets up built in steroeyeindex shader variables based on which eye the shader is being rendered in 
			UNITY_SETUP_INSTANCE_ID(v); //https://docs.unity3d.com/Manual/SinglePassInstancing.html

			//tells gpu which eye to render to based on STEREOEYEINDEX
			UNITY_INITIALIZE_OUTPUT(v2f, o); // https://docs.unity3d.com/Manual/SinglePassInstancing.html
			//Initialised all 2f values to 0 (may break things)
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);  //https://docs.unity3d.com/Manual/SinglePassInstancing.html

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		  }
		  //----

		  float3 colors[5]; //colors for point ranges
		  float pointranges[5];  //ranges of values used to determine color values
		  float _Hits[2 * 1800]; //passed in array of pointranges 3floats/point, x,y,intensity 
		  //1200 is max allowed. (cannot have more than this in shader) as goes over 16 bit integer limit for arraysize
		
		  //Using the StructuredBuffer, much more data can be passed into the shader, ensuring that the heatmap can handle long studies or staring at objects for long time
		  uniform StructuredBuffer<float> _HitBuffer;
		  //counter for access index
		  int _HitCount = 0;

		  //flag that allows easy, per object toggling of the heatmap.
		  int _DisplayHeatMapFlag = 0;

		  //initialises the colours (passed to script)
		  //Allows users to set colours if they want "Hot areas" to be a different colour ect
		  void initalize()
		  {
			colors[0] = _Color0;
			colors[1] = _Color1;
			colors[2] = _Color2;
			colors[3] = _Color3;
			colors[4] = _Color4;
			pointranges[0] = _Range0;
			pointranges[1] = _Range1;
			pointranges[2] = _Range2;
			pointranges[3] = _Range3;
			pointranges[4] = _Range4;
		  }

		  //get the colour of the heatmap from each pixel, this mostly eric albers code...
		  float3 getHeatForPixel(float weight)
		  {
			if (weight <= pointranges[0])
			{
			  return colors[0];
			}
			if (weight >= pointranges[4])
			{
			  return colors[4];
			}
			for (int i = 1; i < 5; i++)
			{
			  if (weight < pointranges[i]) //if weight is between this point and the point before its range
			  {
				float dist_from_lower_point = weight - pointranges[i - 1];
				float size_of_point_range = pointranges[i] - pointranges[i - 1];

				float ratio_over_lower_point = dist_from_lower_point / size_of_point_range;

				//now with ratio or percentage (0-1) into the point range, multiply color ranges to get color

				float3 color_range = colors[i] - colors[i - 1];

				float3 color_contribution = color_range * ratio_over_lower_point;

				float3 new_color = colors[i - 1] + color_contribution;
				return new_color;

			  }
			}
			return colors[0];
		  }

		  //method that could be used to make heatmap "heat" be affected by time
		  float3 getHeatAndTime(float weight, float desiredTime)
		  {

			  float3 pixelHeat = getHeatForPixel(weight);

			  return pixelHeat;

		  }

		  //Note: if distance is > 1.0, zero contribution, 1.0 is 1/2 of the 2x2 uv size
		  //Limitation with this, takes distance in TEXTURE COORDS, Could be extended to take a circular radius in realspace coords, but this is very difficult to do, and outof scope
		  float distsq(float2 a, float2 b)
		  {
			float area_of_effect_size = _Diameter;

			return  pow(max(0.0, 1.0 - distance(a, b) / area_of_effect_size), 2.0);
		  }


		  //Called for each pixel, SV_target is colour
		
			 //Called for each pixel, SV_target is colour
		  fixed4 frag(v2f i) : SV_Target
		  {
			  fixed4 col = tex2D(_MainTex, i.uv);
			  //Just display the regular texture if the heatmap is turned off.
			  if (_DisplayHeatMapFlag == 0)
			  {
				  return col;
			  }
			  
			initalize();
			float2 uv = i.uv;
			uv = uv * 4.0 - float2(2.0,2.0);  //our texture uv range is -2 to 2

			float totalWeight = 0.0;
			for (float i = 0.0; i < _HitCount; i++)
			{
				float2 work_pt = float2(_HitBuffer[i * 3], _HitBuffer[i * 3 + 1]);

			  
			  float pt_intensity = 0.1; //Static (changed form OSS)

			  totalWeight += 0.5 * distsq(uv, work_pt) * pt_intensity * _Strength;
			}

			//
			return col + float4(getHeatForPixel(totalWeight), .5);
		  }



		fixed4 fragAlt(v2f i): SV_Target
		{
			fixed4 outPutColour = tex2D(_MainTex, i.uv);
				
			//initialise the colours ect
			initalize();
			float2 uv = i.uv;
			uv = uv * 4.0 - float2(2.0, 2.0);  //our texture uv range is -2 to 2


			float totalWeight = 0.0;
			for (float i = 0.0; i < _HitCount; i++)
			{
				float2 work_pt = float2(_Hits[i * 3], _Hits[i * 3 + 1]);
				float pt_intensity = _Hits[i * 3 + 2];

				totalWeight += 0.5 * distsq(uv, work_pt) * pt_intensity * _Strength * (1 + sin(_Time.y * _PulseSpeed));
			}
			//return col + float4(getHeatForPixel(totalWeight), .5);

		  
		}


			
			
		//flag to tell shader end of code (hlsl/cgproramg)
		  ENDCG
		}
	  }
}
