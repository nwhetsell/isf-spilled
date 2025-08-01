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
            "NAME": "fluidSpeed",
            "LABEL": "Fluid speed",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 10
        },
        {
            "NAME": "fluidHeight",
            "LABEL": "Fluid height",
            "TYPE": "float",
            "DEFAULT": 650,
            "MIN": 0,
            "MAX": 1000
        },
        {
            "NAME": "spread",
            "LABEL": "Spread (whole number)",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 1,
            "MAX": 7
        },
        {
            "NAME": "specularReflectionAmount",
            "LABEL": "Specular reflection amount",
            "TYPE": "float",
            "DEFAULT": 1,
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
        },
        {
            "NAME": "agitation",
            "LABEL": "Agitation (whole number)",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 10
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

// #define SUPPORT_EVEN_ROTNUM

// Hash function from <https://www.shadertoy.com/view/4djSRW>, MIT-licensed:
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
float hash11(float p)
{
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}


void main()
{
    vec2 pos = gl_FragCoord.xy;
    vec2 inverseSize = 1. / RENDERSIZE;
    vec2 uv = pos * inverseSize;

    if (PASSINDEX == 0) // ShaderToy Buffer A
    {
        float rnd = hash11(TIME / RENDERSIZE.x);

        int RotNum = 2 * int(agitation) + 1;
        float ang = 2. * 3.1415926535 / float(RotNum);
        mat2 m = mat2( cos(ang), sin(ang),
                      -sin(ang), cos(ang));
#ifdef SUPPORT_EVEN_ROTNUM
        mat2 mh = mat2( cos(ang * 0.5), sin(ang * 0.5),
                       -sin(ang * 0.5), cos(ang * 0.5));
#endif

        vec2 b = vec2(cos(ang * rnd), sin(ang * rnd));
        vec2 v = vec2(0.);

        float bbMax = 0.7 * RENDERSIZE.y;
        bbMax *= bbMax;

        for (int l = 0; l < 20; l++) {
            if (dot(b, b) > bbMax) {
                break;
            }

            vec2 p = b;

            for (int i = 0; i < RotNum; i++) {
                vec2 pos_plus_p = pos + p;
                vec2 rotated_b =
#ifdef SUPPORT_EVEN_ROTNUM
                                 -mh *
#endif
                                       // this is faster but works only for odd RotNum
                                       b;
                float rotated_b_magnitude_squared = dot(rotated_b, rotated_b);

                float rot = 0.;
                for (int _ = 0; _ < RotNum; _++) {
                    rot += dot(
                        IMG_NORM_PIXEL(bufferA, fract((pos_plus_p + rotated_b) / RENDERSIZE)).xy - vec2(0.5),
                        rotated_b.yx * vec2(1., -1.)
                    );
                    rotated_b = m * rotated_b;
                }
                float rotation = rot / float(RotNum) / rotated_b_magnitude_squared;

                v += p.yx * rotation;
                p = m * p;
            }

            b *= 2.;
        }

        gl_FragColor = IMG_NORM_PIXEL(bufferA, fract((pos + fluidSpeed * vec2(-1., 1.) * v) / RENDERSIZE));

        // add a little "motor" in the center
        vec2 scr = 2. * (uv - motorLocation);
        gl_FragColor.xy += motorSize * scr / (10. * dot(scr, scr) + motorAttenuation);

        gl_FragColor = (1. - inputImageAmount) * gl_FragColor + inputImageAmount * IMG_PIXEL(inputImage, pos);

        if (FRAMEINDEX < 5) {
            gl_FragColor = IMG_PIXEL(inputImage, pos);
        }
    }
    else if (PASSINDEX == 1) // ShaderToy Image
    {
        vec2 d = vec2(inverseSize.y, 0.);
        vec3 n = vec3(
            (length(IMG_NORM_PIXEL(bufferA, uv + d.xy).xyz) - length(IMG_NORM_PIXEL(bufferA, uv - d.xy).xyz)) * RENDERSIZE.y,
            (length(IMG_NORM_PIXEL(bufferA, uv + d.yx).xyz) - length(IMG_NORM_PIXEL(bufferA, uv - d.yx).xyz)) * RENDERSIZE.y,
            1000. - fluidHeight
        );

        vec3 spread_n = n;
        for (int i = 1; i < int(spread); i++) {
            spread_n *= n;
        }

        n = normalize(spread_n);
        vec3 light = normalize(vec3(1., 1., 2.));
        float diff = clamp(dot(n, light), 0.5, 1.);
        float spec = clamp(dot(reflect(light, n), vec3(0., 0., -1.)), 0., 1.);
        spec = pow(spec, 36.) * 2.5;
    	gl_FragColor = IMG_NORM_PIXEL(bufferA, uv) * vec4(diff) + specularReflectionAmount * vec4(spec);
    }
}
