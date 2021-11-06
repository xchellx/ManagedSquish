/* -----------------------------------------------------------------------------

	Copyright (c) 2006 Simon Brown                          si@sjbrown.co.uk

	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files (the 
	"Software"), to	deal in the Software without restriction, including
	without limitation the rights to use, copy, modify, merge, publish,
	distribute, sublicense, and/or sell copies of the Software, and to 
	permit persons to whom the Software is furnished to do so, subject to 
	the following conditions:

	The above copyright notice and this permission notice shall be included
	in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
	OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
	CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
	SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	
   -------------------------------------------------------------------------- */

// ManagedSquish - Copyright (c) 2011-12 Rodrigo 'r2d2rigo' Díaz
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ManagedSquish
{
    /// <summary>
    /// Wrapper for libsquish DXT compression/decompression library.
    /// </summary>
    public static class Squish
    {

        private delegate int GetStorageRequirementsDelegate(int width, int height, int flags);

        private delegate void CompressFunctionDelegate(IntPtr rgba, IntPtr block, int flags);

        private delegate void CompressMaskedDelegate(IntPtr rgba, int mask, IntPtr block, int flags);

        private delegate void DecompressDelegate(IntPtr rgba, IntPtr block, int flags);

        private delegate void CompressImageDelegate(IntPtr rgba, int width, int height, IntPtr blocks, int flags);

        private delegate void DecompressImageDelegate(IntPtr rgba, int width, int height, IntPtr blocks, int flags);

        private static readonly GetStorageRequirementsDelegate GetStorageRequirementsFunction;

        private static readonly CompressFunctionDelegate CompressFunction;

        private static readonly CompressMaskedDelegate CompressMaskedFunction;

        private static readonly DecompressDelegate DecompressFunction;

        private static readonly CompressImageDelegate CompressImageFunction;

        private static readonly DecompressImageDelegate DecompressImageFunction;

        static Squish() {
            if (Environment.Is64BitProcess)
            {
                GetStorageRequirementsFunction = NativeMethods_x64.GetStorageRequirements;
                CompressFunction = NativeMethods_x64.Compress;
                CompressMaskedFunction = NativeMethods_x64.CompressMasked;
                DecompressFunction = NativeMethods_x64.Decompress;
                CompressImageFunction = NativeMethods_x64.CompressImage;
                DecompressImageFunction = NativeMethods_x64.DecompressImage;
            }
            else
            {
                GetStorageRequirementsFunction = NativeMethods_x86.GetStorageRequirements;
                CompressFunction = NativeMethods_x86.Compress;
                CompressMaskedFunction = NativeMethods_x86.CompressMasked;
                DecompressFunction = NativeMethods_x86.Decompress;
                CompressImageFunction = NativeMethods_x86.CompressImage;
                DecompressImageFunction = NativeMethods_x86.DecompressImage;
            }
        }

        /// <summary>
        /// Returns the final size in bytes of DXT data compressed with the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="width">Source image width.</param>
        /// <param name="height">Source image height.</param>
        /// <param name="flags">Compression parameters.</param>
        /// <returns>Size in bytes of the DXT data.</returns>
        public static int GetStorageRequirements(int width, int height, SquishFlags flags)
        {
            return GetStorageRequirementsFunction(width, height, (int)flags);
        }

        /// <summary>
        /// Compress a 4x4 pixel block using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Source RGBA block.</param>
        /// <param name="block">Output DXT compressed block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void Compress(IntPtr rgba, IntPtr block, SquishFlags flags)
        {
            CompressFunction(rgba, block, (int)flags);
        }

        /// <summary>
        /// Compress a 4x4 pixel block using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Source RGBA block.</param>
        /// <param name="flags">Compression flags.</param>
        /// <returns>Output DXT compressed block.</returns>
        public static byte[] Compress(byte[] rgba, SquishFlags flags)
        {
            byte[] compressedData = new byte[GetStorageRequirements(4, 4, flags)];
            GCHandle pinnedData = GCHandle.Alloc(compressedData, GCHandleType.Pinned);
            GCHandle pinnedRgba = GCHandle.Alloc(rgba, GCHandleType.Pinned);
            CompressFunction(pinnedRgba.AddrOfPinnedObject(), pinnedData.AddrOfPinnedObject(), (int)flags);
            pinnedRgba.Free();
            pinnedData.Free();
            return compressedData;
        }

        /// <summary>
        /// Compress a 4x4 pixel block using the parameters specified in <paramref name="flags" />. The <paramref name="mask" /> parameter is a used as 
        /// a bit mask to specifify what pixels are valid for compression, corresponding the lowest bit to the first pixel.
        /// </summary>
        /// <param name="rgba">Source RGBA block.</param>
        /// <param name="mask">Pixel bit mask.</param>
        /// <param name="block">Output DXT compressed block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressMasked(IntPtr rgba, int mask, IntPtr block, SquishFlags flags)
        {
            CompressMaskedFunction(rgba, mask, block, (int)flags);
        }

        /// <summary>
        /// Compress a 4x4 pixel block using the parameters specified in <paramref name="flags" />. The <paramref name="mask" /> parameter is a used as 
        /// a bit mask to specifify what pixels are valid for compression, corresponding the lowest bit to the first pixel.
        /// </summary>
        /// <param name="rgba">Source RGBA block.</param>
        /// <param name="mask">Pixel bit mask.</param>
        /// <param name="flags">Compression flags.</param>
        /// <returns>Output DXT compressed block.</returns>
        public static byte[] CompressMasked(byte[] rgba, int mask, SquishFlags flags)
        {
            byte[] compressedData = new byte[GetStorageRequirements(4, 4, flags)];
            GCHandle pinnedData = GCHandle.Alloc(compressedData, GCHandleType.Pinned);
            GCHandle pinnedRgba = GCHandle.Alloc(rgba, GCHandleType.Pinned);
            CompressMaskedFunction(pinnedRgba.AddrOfPinnedObject(), mask, pinnedData.AddrOfPinnedObject(), (int)flags);
            pinnedRgba.Free();
            pinnedData.Free();
            return compressedData;
        }

        /// <summary>
        /// Decompresses a 4x4 pixel block using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Output RGBA decompressed block.</param>
        /// <param name="block">Source DXT block.</param>
        /// <param name="flags">Decompression flags.</param>
        public static void Decompress(IntPtr rgba, IntPtr block, SquishFlags flags)
        {
            DecompressFunction(rgba, block, (int)flags);
        }

        /// <summary>
        /// Decompresses a 4x4 pixel block using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="block">Source DXT block.</param>
        /// <param name="flags">Decompression flags.</param>
        /// <returns>Output RGBA decompressed block.</returns>
        public static byte[] Decompress(byte[] block, SquishFlags flags)
        {
            byte[] decompressedData = new byte[64];
            GCHandle pinnedData = GCHandle.Alloc(decompressedData, GCHandleType.Pinned);
            GCHandle pinnedBlock = GCHandle.Alloc(block, GCHandleType.Pinned);
            DecompressFunction(pinnedData.AddrOfPinnedObject(), pinnedBlock.AddrOfPinnedObject(), (int)flags);
            pinnedBlock.Free();
            pinnedData.Free();
            return decompressedData;
        }

        /// <summary>
        /// Compresses an image using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Source RGBA image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="blocks">Output DXT compressed image.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, SquishFlags flags)
        {
            CompressImageFunction(rgba, width, height, blocks, (int)flags);
        }

        /// <summary>
        /// Compresses an image using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Source RGBA image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="flags">Compression flags.</param>
        /// <returns>Output DXT compressed image.</returns>
        public static byte[] CompressImage(byte[] rgba, int width, int height, SquishFlags flags)
        {
            byte[] compressedData = new byte[GetStorageRequirements(4, 4, flags)];
            GCHandle pinnedData = GCHandle.Alloc(compressedData, GCHandleType.Pinned);
            GCHandle pinnedRgba = GCHandle.Alloc(rgba, GCHandleType.Pinned);
            CompressImageFunction(pinnedRgba.AddrOfPinnedObject(), width, height, pinnedData.AddrOfPinnedObject(), (int)flags);
            pinnedRgba.Free();
            pinnedData.Free();
            return compressedData;
        }

        /// <summary>
        /// Decompresses an image using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="rgba">Output RGBA decompressed image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="blocks">Source DXT compressed image.</param>
        /// <param name="flags">Decompression flags.</param>
        public static void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, SquishFlags flags)
        {
            DecompressImageFunction(rgba, width, height, blocks, (int)flags);
        }

        /// <summary>
        /// Decompresses an image using the parameters specified in <paramref name="flags" />.
        /// </summary>
        /// <param name="blocks">Source DXT compressed image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="flags">Decompression flags.</param>
        /// <returns>Output RGBA decompressed image.</returns>
        public static byte[] DecompressImage(byte[] blocks, int width, int height, SquishFlags flags)
        {
            byte[] decompressedData = new byte[width * height * 4];
            GCHandle pinnedData = GCHandle.Alloc(decompressedData, GCHandleType.Pinned);
            GCHandle pinnedBlocks = GCHandle.Alloc(blocks, GCHandleType.Pinned);
            DecompressImageFunction(pinnedData.AddrOfPinnedObject(), width, height, pinnedBlocks.AddrOfPinnedObject(), (int)flags);
            pinnedBlocks.Free();
            pinnedData.Free();
            return decompressedData;
        }

        private static class NativeMethods_x86
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?GetStorageRequirementsEx@NativeSquish@@YAHHHH@Z", ExactSpelling = true)]
            private static extern int GetStorageRequirementsEx_x86(int width, int height, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressEx@NativeSquish@@YAXPBEPAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressEx_x86(byte* rgba, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressMaskedEx@NativeSquish@@YAXPBEHPAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressMaskedEx_x86(byte* rgba, int mask, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?DecompressEx@NativeSquish@@YAXPAEPBXH@Z", ExactSpelling = true)]
            private unsafe static extern void DecompressEx_x86(byte* rgba, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressImageEx@NativeSquish@@YAXPBEHHPAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressImageEx_x86(byte* rgba, int width, int height, void* blocks, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x86", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?DecompressImageEx@NativeSquish@@YAXPAEHHPBXH@Z", ExactSpelling = true)]
            private unsafe static extern void DecompressImageEx_x86(byte* rgba, int width, int height, void* blocks, int flags);

            public static int GetStorageRequirements(int width, int height, int flags)
                => GetStorageRequirementsEx_x86(width, height, flags);

            public unsafe static void Compress(IntPtr rgba, IntPtr block, int flags)
                => CompressEx_x86((byte*)rgba.ToPointer(), block.ToPointer(), flags);

            public unsafe static void CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags)
                => CompressMaskedEx_x86((byte*)rgba.ToPointer(), mask, block.ToPointer(), flags);

            public unsafe static void Decompress(IntPtr rgba, IntPtr block, int flags)
                => DecompressEx_x86((byte*)rgba.ToPointer(), block.ToPointer(), flags);

            public unsafe static void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags)
                => CompressImageEx_x86((byte*)rgba.ToPointer(), width, height, blocks.ToPointer(), flags);

            public unsafe static void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags)
                => DecompressImageEx_x86((byte*)rgba.ToPointer(), width, height, blocks.ToPointer(), flags);
        }

        private class NativeMethods_x64
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?GetStorageRequirementsEx@NativeSquish@@YAHHHH@Z", ExactSpelling = true)]
            private static extern int GetStorageRequirementsEx_x64(int width, int height, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressEx@NativeSquish@@YAXPEBEPEAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressEx_x64(byte* rgba, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressMaskedEx@NativeSquish@@YAXPEBEHPEAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressMaskedEx_x64(byte* rgba, int mask, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?DecompressEx@NativeSquish@@YAXPEAEPEBXH@Z", ExactSpelling = true)]
            private unsafe static extern void DecompressEx_x64(byte* rgba, void* block, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?CompressImageEx@NativeSquish@@YAXPEBEHHPEAXH@Z", ExactSpelling = true)]
            private unsafe static extern void CompressImageEx_x64(byte* rgba, int width, int height, void* blocks, int flags);

            [SuppressUnmanagedCodeSecurity]
            [DllImport("squish_x64", CallingConvention = CallingConvention.Cdecl,
                EntryPoint = "?DecompressImageEx@NativeSquish@@YAXPEAEHHPEBXH@Z", ExactSpelling = true)]
            private unsafe static extern void DecompressImageEx_x64(byte* rgba, int width, int height, void* blocks, int flags);

            public static int GetStorageRequirements(int width, int height, int flags)
                => GetStorageRequirementsEx_x64(width, height, flags);

            public unsafe static void Compress(IntPtr rgba, IntPtr block, int flags)
                 => CompressEx_x64((byte*)rgba.ToPointer(), block.ToPointer(), flags);

            public unsafe static void CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags)
                 => CompressMaskedEx_x64((byte*)rgba.ToPointer(), mask, block.ToPointer(), flags);

            public unsafe static void Decompress(IntPtr rgba, IntPtr block, int flags)
                 => DecompressEx_x64((byte*)rgba.ToPointer(), block.ToPointer(), flags);

            public unsafe static void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags)
                 => CompressImageEx_x64((byte*)rgba.ToPointer(), width, height, blocks.ToPointer(), flags);

            public unsafe static void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags)
                 => DecompressImageEx_x64((byte*)rgba.ToPointer(), width, height, blocks.ToPointer(), flags);
        }
    }
}
