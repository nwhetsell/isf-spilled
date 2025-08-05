This is an [ISF shader](https://isf.video) for single-pass computational fluid
dynamics, converted from
[this ShaderToy shader](https://www.shadertoy.com/view/MsGSRd) by
[Florian Berger](https://www.flockaroo.at).

The ShaderToy shader is distributed under the
[Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported](https://creativecommons.org/licenses/by-nc-sa/3.0/)
license (CC BY-NC-SA 3.0). This adaptation of the ShaderToy shader includes a
portion of [Hash without Sine](https://www.shadertoy.com/view/4djSRW) by
[David Hoskins](https://www.shadertoy.com/user/Dave_Hoskins), which is
distributed under the [MIT License](https://opensource.org/license/mit).

This is a multi-pass shader that is intended to be used with floating-point
buffers. Not all ISF hosts support floating-point buffers.
[Videosync](https://videosync.showsync.com/download) supports floating-point
buffers in
[v2.0.12](https://support.showsync.com/release-notes/videosync/2.0#2012) and
later, but https://editor.isf.video does not appear to support floating-point
buffers. This shader will produce *very* different output if floating-point
buffers are not used.
