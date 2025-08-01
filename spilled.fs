/*{
    "CATEGORIES": [
        "Filter",
        "Generator"
    ],
    "CREDIT": "Florian Berger <https://www.shadertoy.com/user/flockaroo>",
    "DESCRIPTION": "Single-pass computational fluid dynamics, converted from <https://www.shadertoy.com/view/MsGSRd>",
    "INPUTS": [
        {
            "NAME" : "inputImage",
            "TYPE" : "image"
        },
        {
            "NAME": "inputImageAmount",
            "LABEL": "Input image amount",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": 0,
            "MAX": 1
        },
        {
            "NAME": "motorLocation",
            "LABEL": "Motor location",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [0, 0],
            "MAX": [1, 1]
        },
        {
            "NAME": "motorSize",
            "LABEL": "Motor size",
            "TYPE": "float",
            "DEFAULT": 0.01,
            "MIN": 0,
            "MAX": 1
        },
        {
            "NAME": "motorAttenuation",
            "LABEL": "Motor attenuation",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0,
            "MAX": 1
        }
    ],
    "ISFVSN": "2",
    "PASSES": [
        {
            "TARGET": "bufferA",
            "PERSISTENT": true,
            "FLOAT": true
        },
        {

        }
    ]
}
*/

//
// ShaderToy Buffer A
//

#define fragColor gl_FragColor
#define fragCoord gl_FragCoord
#define iFrame FRAMEINDEX
#define iResolution RENDERSIZE

#define RotNum 5
//#define SUPPORT_EVEN_ROTNUM

#define Res  RENDERSIZE


const float ang = 2. * 3.1415926535 / float(RotNum);
mat2 m = mat2( cos(ang), sin(ang),
              -sin(ang), cos(ang));
mat2 mh = mat2( cos(ang * 0.5), sin(ang * 0.5),
               -sin(ang * 0.5), cos(ang * 0.5));

// Hash function from (https://www.shadertoy.com/view/4djSRW), MIT-licensed:
//
// Copyright © 2014 David Hoskins.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the “Software”), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
vec4 hash42(vec2 p)
{
	vec4 p4 = fract(vec4(p.xyxy) * vec4(0.1031, 0.1030, 0.0973, 0.1099));
    p4 += dot(p4, p4.wzxy + 33.33);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

vec4 randS(vec2 uv)
{
    return hash42(uv) - vec4(0.5);
}

float getRot(vec2 pos, vec2 b)
{
    vec2 p = b;
    float rot = 0.;
    for (int i = 0; i < RotNum; i++) {
        rot += dot(IMG_NORM_PIXEL(bufferA, fract((pos + p) / Res.xy)).xy - vec2(0.5), p.yx * vec2(1, -1));
        p = m * p;
    }
    return rot / float(RotNum) / dot(b, b);
}


float getVal(vec2 uv)
{
    return length(IMG_NORM_PIXEL(bufferA, uv).xyz);
}

vec2 getGrad(vec2 uv, float delta)
{
    vec2 d = vec2(delta, 0.);
    return vec2(
        getVal(uv + d.xy) - getVal(uv - d.xy),
        getVal(uv + d.yx) - getVal(uv - d.yx)
    ) / delta;
}


void main()
{
    if (PASSINDEX == 0) // ShaderToy Buffer A
    {
        vec2 pos = fragCoord.xy;
        float rnd = randS(vec2(TIME / Res.x, 0.5 / Res.y)).x;

        vec2 b = vec2(cos(ang * rnd), sin(ang * rnd));
        vec2 v = vec2(0.);

        float bbMax = 0.7 * Res.y;
        bbMax *= bbMax;

        for (int l = 0; l < 20; l++) {
            if (dot(b, b) > bbMax) {
                break;
            }

            vec2 p = b;

            for (int i = 0; i < RotNum; i++) {
                v += p.yx * getRot(pos + p,
    #ifdef SUPPORT_EVEN_ROTNUM
                                            -mh *
    #endif
                                                  // this is faster but works only for odd RotNum
                                                  b);
                p = m * p;
            }

            b *= 2.;
        }

        fragColor = IMG_NORM_PIXEL(bufferA, fract((pos + 2. * vec2(-1., 1.) * v) / Res.xy));

        // add a little "motor" in the center
        vec2 scr = 2. * ((fragCoord.xy / Res.xy) - motorLocation);
        fragColor.xy += motorSize * scr.xy / (10. * dot(scr, scr) + motorAttenuation);

        fragColor = (1. - inputImageAmount) * fragColor + inputImageAmount * IMG_PIXEL(inputImage, fragCoord.xy);

        if (iFrame < 5) {
            fragColor = IMG_PIXEL(inputImage, fragCoord.xy);
        }
    }
    else if (PASSINDEX == 1) // ShaderToy Image
    {
        vec2 uv = fragCoord.xy / iResolution.xy;
        vec3 n = vec3(getGrad(uv, 1. / iResolution.y), 150.);
        //n *= n;
        n = normalize(n);
        vec3 light = normalize(vec3(1., 1., 2.));
        float diff = clamp(dot(n, light), 0.5, 1.);
        float spec = clamp(dot(reflect(light, n), vec3(0., 0., -1.)), 0., 1.);
        spec = pow(spec, 36.) * 2.5;
        // spec=0.0;
    	fragColor = IMG_NORM_PIXEL(bufferA, uv) * vec4(diff) + vec4(spec);
    }
}
