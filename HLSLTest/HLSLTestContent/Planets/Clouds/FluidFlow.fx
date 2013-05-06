//===============================================================================================
//=			Globals																				=
//=																								=
//===============================================================================================
float       timestep;
float       dissipation;		// mass dissipation constant.
float       rdx;				// 1   / grid scale.
float		halfrdx;			// 0.5 / grid scale.
float		alpha=1;
float		rBeta=1;
float2      dxscale;

float4 left = float4(0,0,0,0);
float4 right;
float4 bottom;
float4 top;
static const float EPSILON = 2.4414e-4; // 2^-12

//===============================================================================================
//=			Textures																			=
//=																								=
//===============================================================================================
texture VelocityMap;
texture PressureMap;
texture DensityMap;
texture DivergenceMap;
texture VorticityMap;
//===============================================================================================
//=			Samplers																			=
//=																								=
//===============================================================================================
sampler VorticityMapSampler = sampler_state 
{
    texture = <VorticityMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};
sampler VelocityMapSampler = sampler_state 
{
    texture = <VelocityMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};
sampler PressureMapSampler = sampler_state 
{
    texture = <PressureMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};
sampler DensityMapSampler = sampler_state 
{
    texture = <DensityMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};
sampler DivergenceMapSampler = sampler_state 
{
    texture = <DivergenceMap>;    
    magfilter	= LINEAR; 
	minfilter	= LINEAR; 
	mipfilter	= LINEAR; 
	AddressU	= WRAP;  
	AddressV	= WRAP;
};
//===============================================================================================
//=			get neighbouring values																=
//=																								=
//===============================================================================================
void neighbors(sampler tex, float2 s)
{
  left   = tex2D(tex, s - float2(rdx, 0)); 
  right  = tex2D(tex, s + float2(rdx, 0));
  bottom = tex2D(tex, s - float2(0, rdx));
  top    = tex2D(tex, s + float2(0, rdx));
}
void h1neighbors(sampler tex, float2 s,out float left,out float right, out float bottom, out float top)
{
  left   = tex2D(tex, s - float2(rdx, 0)).x; 
  right  = tex2D(tex, s + float2(rdx, 0)).x;
  bottom = tex2D(tex, s - float2(0, rdx)).x;
  top    = tex2D(tex, s + float2(0, rdx)).x;
}

//===============================================================================================
//=	These methods perform texture lookups at the four nearest neighbors of the position s and	=
//=	bilinearly interpolate them.																=
//===============================================================================================
float4 bilerp(sampler tex, float2 s)
{
  float4 st;
  st.x = -rdx;
  st.y = -rdx;
  st.z = rdx;
  st.w = rdx;
  st.xy+=s;
  st.zw+=s;
  
  float2 t = float2(0.5,0.5);
    
  float4 tex11 = tex2D(tex, st.xy);
  float4 tex21 = tex2D(tex, st.zy);
  float4 tex12 = tex2D(tex, st.xw);
  float4 tex22 = tex2D(tex, st.zw);

  // bilinear interpolation
  return lerp(lerp(tex11, tex21, t.x), lerp(tex12, tex22, t.x), t.y);
}

//===============================================================================================
//=	This program performs a semi-lagrangian advection of a passive field by a moving			=
//= velocity field.																				=
//=																								=
//===============================================================================================
float4 AdvectVelocity(float2 Coords : TEXCOORD0) : COLOR0
{
	float2 pos = Coords;
	float2 vel = tex2D(VelocityMapSampler, Coords).xy;
	vel=2*(vel-0.5);
	pos -= timestep * rdx * vel;
	float4 xNew = dissipation * bilerp(VelocityMapSampler, pos);  
	xNew.w=1;
	return xNew;
}
float4 AdvectDensity(float2 Coords : TEXCOORD0) : COLOR0
{
	float2 vel = tex2D(VelocityMapSampler, Coords).xy;
	vel=2*(vel-0.5);
	
	float2 pos = Coords - timestep * rdx * vel;
	float4 xNew = dissipation * bilerp(DensityMapSampler, pos);  
	xNew.w=1;
	return xNew;
}
//===============================================================================================
//=	This program computes the divergence of the specified vector field "velocity".				=
//=	The divergence is defined as																=
//=     "grad dot v" = partial(v.x)/partial(x) + partial(v.y)/partial(y),						=
//=																								=
//= and it represents the quantity of "stuff" flowing in and out of a parcel of fluid.			=
//= Incompressible fluids must be divergence-free.  In other words this quantity must be zero	=
//=	everywhere.																					=
//===============================================================================================
float4 divergence(float2 Coords : TEXCOORD0) : COLOR0
{
  float4 vL, vR, vB, vT;
  neighbors(VelocityMapSampler, Coords);
  
  float4 div =  (right.x - left.x + top.y - bottom.y);
  div*=10;
  div.w=1;
  return div;
} 
//===============================================================================================
//=	This program performs a single Jacobi relaxation step for a poisson equation of the form	=
//=																								=
//=              Laplacian(U) = b																=
//=																								=
//= where U = (u, v) and Laplacian(U) is defined as												=
//=																								=
//= grad(div x) = grad(grad dot x) = partial^2(u)/(partial(x))^2 + partial^2(v)/(partial(y))^2	=
//=																								=
//= A solution of the equation can be found iteratively, by using this iteration:				=
//=																								=
//=   U'(i,j) = (U(i-1,j) + U(i+1,j) + U(i,j-1) + U(i,j+1) + b) * 0.25							=
//=																								=
//===============================================================================================
float4 jacobi(float2 Coords : TEXCOORD0) : COLOR0  
{
  float4 xNew=float4(0,0,0,0);
  
  float xL, xR, xB, xT;
  neighbors(DivergenceMapSampler, Coords);
  
  float bC = tex2D(PressureMapSampler, Coords).x;

  xNew.x = (left.x + right.x + bottom.x + top.x + alpha * bC) * rBeta;
  xNew.w=1;
  return xNew;
} 

//===============================================================================================
//=	This program implements the final step in the fluid simulation.  After the poisson solver	=
//=	has iterated to find the pressure disturbance caused by the divergence of the velocity		=
//= field, the gradient of that pressure needs to be subtracted from this divergent velocity to =
//= get a divergence-free velocity field:														=
//=																								=
//= v-zero-divergence = v-divergent -  grad(p)													=
//=																								=
//= The gradient(p) is defined:																	=
//=	    grad(p) = (partial(p)/partial(x), partial(p)/partial(y))								=
//=																								=
//= The discrete form of this is:																=
//=     grad(p) = ((p(i+1,j) - p(i-1,j)) / 2dx, (p(i,j+1)-p(i,j-1)) / 2dy)						=
//=																								=
//= where dx and dy are the dimensions of a grid cell.											=
//=																								=
//= This program computes the gradient of the pressure and subtracts it from the velocity to	=
//= get a divergence free velocity.																=
//===============================================================================================
float4 gradient(float2 Coords : TEXCOORD0) : COLOR0  
{
  float pL, pR, pB, pT;
  
  h1neighbors(PressureMapSampler, Coords, pL, pR, pB, pT);

  float2 grad = float2(pR - pL, pT - pB) * rdx;

  float4 uNew = tex2D(VelocityMapSampler, Coords);
  uNew.xy -= grad;
  uNew.w=1;
  return uNew;
} 

//===============================================================================================
//=			Vorticity																			=
//=																								=
//===============================================================================================
float4 vorticity(float2 Coords : TEXCOORD0) : COLOR0
{
  float4 uL, uR, uB, uT;
  neighbors(VelocityMapSampler, Coords);
  
  //float vort = halfrdx * ((right.y - left.y) - (top.x - bottom.x));
  float vort = 10000*halfrdx * ((top.y - left.y) - (right.x - bottom.x));
  return float4(vort,0,0,1);
} 
//===============================================================================================
//=		The second pass of vorticity confinement computes a vorticity confinement				=
//=		force field and applies it to the velocity field to arrive at a new velocity field.		=
//===============================================================================================
float4 vortForce(float2 Coords : TEXCOORD0) : COLOR0
{
  float vL, vR, vB, vT, vC;
  h1neighbors(VorticityMapSampler, Coords, vL, vR, vB, vT);
  
  vC = tex2D(VorticityMapSampler, Coords).x;
  
  float2 force = halfrdx * float2(abs(vT) - abs(vB), abs(vR) - abs(vL));
  
  // safe normalize
  float magSqr = max(EPSILON, dot(force, force)); 
  force = force * sqrt(magSqr); 
  
  force *= dxscale * vC * float2(1, -1);

  float4 uNew = tex2D(VelocityMapSampler, Coords);

  uNew.xy += timestep * force;
  return uNew;
} 


//===============================================================================================
//=			techniques																			=
//=																								=
//===============================================================================================
technique AdvectVelocity
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 AdvectVelocity();
    }
}
technique AdvectDensity
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 AdvectDensity();
    }
}
technique Vorticity
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 vorticity();
    }
}
technique Divergence
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 divergence();
    }
}
technique Jacobi
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 jacobi();
    }
}
technique Gradient
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 gradient();
    }
}
technique ApplyVorticity
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 vortForce();
    }
}