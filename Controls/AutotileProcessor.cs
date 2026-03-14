using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PokemonEssentialsEditorEvs.Controls
{
    /// <summary>
    ///
    /// RESPONSABILIDADES
    ///   Expand()          — Convierte una imagen de autotile (96x128 o simple)
    ///                       en 48 WriteableBitmaps de 32x32 listos para dibujar.
    ///   GetVariantIndex() — Dado un tile (x,y,z), devuelve que variante (0-47)
    ///                       le corresponde segun sus 8 vecinos en el mapa.
    ///
    /// USO TIPICO
    ///   // Al cargar el proyecto:
    ///   var (tiles, isSimple) = AutotileProcessor.Expand(bitmap);
    ///   _autotileCache[baseId] = (tiles, isSimple);
    ///
    ///   // En MapRenderer.Render():
    ///   int baseId = (tileId / 48) * 48;
    ///   int idx = isSimple ? 0 : AutotileProcessor.GetVariantIndex(tileData, x, y, z, baseId);
    ///   context.DrawImage(_autotileCache[baseId].Tiles[idx], destRect);
    /// </summary>
    public static class AutotileProcessor
    {
        // Tabla PATTERNS: 48 filas x 4 indices [TopLeft, TopRight, BottomLeft, BottomRight]
        // Cada valor apunta a uno de los 48 mini-tiles de 16x16 extraidos de la imagen 96x128
        // (6 columnas x 8 filas -> indice = fila*6 + columna).
        private static readonly int[,] Patterns =
        {
            {26,27,32,33}, { 4,27,32,33}, {26, 5,32,33}, { 4, 5,32,33},
            {26,27,32,11}, { 4,27,32,11}, {26, 5,32,11}, { 4, 5,32,11},
            {26,27,10,33}, { 4,27,10,33}, {26, 5,10,33}, { 4, 5,10,33},
            {26,27,10,11}, { 4,27,10,11}, {26, 5,10,11}, { 4, 5,10,11},
            {24,25,30,31}, {24, 5,30,31}, {24,25,30,11}, {24, 5,30,11},
            {14,15,20,21}, {14,15,20,11}, {14,15,10,21}, {14,15,10,11},
            {28,29,34,35}, {28,29,10,35}, { 4,29,34,35}, { 4,29,10,35},
            {38,39,44,45}, { 4,39,44,45}, {38, 5,44,45}, { 4, 5,44,45},
            {24,29,30,35}, {14,15,44,45}, {12,13,18,19}, {12,13,18,11},
            {16,17,22,23}, {16,17,10,23}, {40,41,46,47}, { 4,41,46,47},
            {36,37,42,43}, {36, 5,42,43}, {12,17,18,23}, {12,13,42,43},
            {36,41,42,47}, {16,17,46,47}, {12,17,42,47}, { 0, 1, 6, 7}
        };

        // Lookup table: bitmask de 8 bits -> indice de variante 0-47
        // Codificacion del bitmask:
        //   bit 0(  1)=arriba      bit 1(  2)=sup-der
        //   bit 2(  4)=derecha     bit 3(  8)=inf-der
        //   bit 4( 16)=abajo       bit 5( 32)=inf-izq
        //   bit 6( 64)=izquierda   bit 7(128)=sup-izq
        private static readonly int[] BitmaskToIndex =
        {
            46,44,46,44,43,41,43,40,46,44,46,44,43,41,43,40,
            42,32,42,32,35,19,35,18,42,32,42,32,34,17,34,16,
            46,44,46,44,43,41,43,40,46,44,46,44,43,41,43,40,
            42,32,42,32,35,19,35,18,42,32,42,32,34,17,34,16,
            45,39,45,39,33,31,33,29,45,39,45,39,33,31,33,29,
            37,27,37,27,23,15,23,13,37,27,37,27,22,11,22, 9,
            45,39,45,39,33,31,33,29,45,39,45,39,33,31,33,29,
            36,26,36,26,21, 7,21, 5,36,26,36,26,20, 3,20, 1,
            46,44,46,44,43,41,43,40,46,44,46,44,43,41,43,40,
            42,32,42,32,35,19,35,18,42,32,42,32,34,17,34,16,
            46,44,46,44,43,41,43,40,46,44,46,44,43,41,43,40,
            42,32,42,32,35,19,35,18,42,32,42,32,34,17,34,16,
            45,39,45,39,33,31,33,29,45,39,45,39,33,31,33,29,
            37,27,37,27,23,15,23,13,37,27,37,27,22,11,22, 9,
            45,39,45,39,33,31,33,29,45,39,45,39,33,31,33,29,
            36,26,36,26,21, 7,21, 5,36,26,36,26,20, 3,20, 1
        };

        // ---- EXPAND ----
        
        public static (WriteableBitmap[] Tiles, bool IsSimple) Expand(string imagePath)
        {
            using var src = new Bitmap(imagePath);
            return Expand(src);
        }

        public static (WriteableBitmap[] Tiles, bool IsSimple) Expand(Bitmap src)
        {
            int w = src.PixelSize.Width;
            int h = src.PixelSize.Height;

            // Autotile complejo: alto exactamente 128px Y ancho multiplo de 96.
            //   96x128  = complejo estatico (1 frame)
            //   192x128 = complejo animado  (2 frames)
            //   768x128 = complejo animado  (8 frames, ej: Sea.png)
            // En todos los casos usamos solo los primeros 96px (frame 0).
            //
            // Cualquier otra dimension es simple:
            //   32x32, 160x32, 32x128 -> tomamos el primer tile 32x32.
            bool isComplex = (h == 128 && w >= 96 && w % 96 == 0);

            if (isComplex)
                return (ExpandComplex(src), false);

            var single = CropToWriteable(src, 0, 0, 32, 32);
            var simple = new WriteableBitmap[48];
            for (int i = 0; i < 48; i++) simple[i] = single;
            return (simple, true);
        }

        // ---- GET VARIANT INDEX ----

        /// <summary>
        /// Calcula el indice de variante (0-47) para el autotile en la celda (x, y, z).
        /// tileData tiene la forma [z][y][x] igual que en el JSON exportado.
        /// baseId = (tileId / 48) * 48
        /// </summary>
        /// 
        /// Obsoleto 
        public static int GetVariantIndex(int[][][] tileData, int x, int y, int z, int baseId)
        {
            int height = tileData[z].Length;
            int width  = tileData[z][0].Length;

            bool IsSame(int nx, int ny)
            {
                // Cast a uint: si nx<0 o nx>=width ambas condiciones fallan en una comparacion
                if ((uint)nx >= (uint)width || (uint)ny >= (uint)height) return false;
                int tid = tileData[z][ny][nx];
                return tid > 0 && (tid / 48) * 48 == baseId;
            }

            int bits = 0;

            // Cardinales - siempre cuentan
            if (IsSame(x,   y-1)) bits |= 1;    // arriba
            if (IsSame(x+1, y  )) bits |= 4;    // derecha
            if (IsSame(x,   y+1)) bits |= 16;   // abajo
            if (IsSame(x-1, y  )) bits |= 64;   // izquierda

            // Diagonales - solo cuentan si AMBOS cardinales adyacentes estan presentes
            if ((bits &  1) != 0 && (bits &  4) != 0 && IsSame(x+1, y-1)) bits |= 2;   // sup-der
            if ((bits &  4) != 0 && (bits & 16) != 0 && IsSame(x+1, y+1)) bits |= 8;   // inf-der
            if ((bits & 16) != 0 && (bits & 64) != 0 && IsSame(x-1, y+1)) bits |= 32;  // inf-izq
            if ((bits & 64) != 0 && (bits &  1) != 0 && IsSame(x-1, y-1)) bits |= 128; // sup-izq

            return BitmaskToIndex[bits];
        }

        // ---- HELPERS INTERNOS ----

        private static WriteableBitmap[] ExpandComplex(Bitmap src)
        {
            int srcW = src.PixelSize.Width;   // 96
            int srcH = src.PixelSize.Height;  // 128
            int srcStride = srcW * 4;
            var raw = new byte[srcStride * srcH];

            unsafe
            {
                fixed (byte* ptr = raw)
                {
                    // Forzamos Bgra8888 para garantizar que srcStride (w * 4) sea siempre correcto
                    src.CopyPixels(
                        new PixelRect(0, 0, srcW, srcH),
                        (nint)ptr, raw.Length, srcStride);
                }
            }

            var tiles = new WriteableBitmap[48];
            for (int p = 0; p < 48; p++)
            {
                // Pasamos el array raw y los índices de los 4 mini-tiles directamente
                tiles[p] = AssembleTileDirect(raw, srcStride, 
                    Patterns[p, 0], Patterns[p, 1], Patterns[p, 2], Patterns[p, 3]);
            }
            return tiles;
        }
        
         // Helper ultra-rápido para copiar bloques de 16x16
        private static unsafe void CopyMiniTile(uint* src, uint* dst, int srcStep, int dstStep, int miniIdx, int destX, int destY)
        {
            // Calcular origen del mini-tile en la imagen de 96x128
            int srcX = (miniIdx % 6) * 16;
            int srcY = (miniIdx / 6) * 16;

            for (int row = 0; row < 16; row++)
            {
                uint* srcRow = src + ((srcY + row) * srcStep) + srcX;
                uint* dstRow = dst + ((destY + row) * dstStep) + destX;

                // Copiamos 16 píxeles (uints) de una vez
                for (int col = 0; col < 16; col++)
                {
                    dstRow[col] = srcRow[col];
                }
            }
        }

        // Ensamblar Tiles

        private static WriteableBitmap AssembleTileDirect(byte[] raw, int srcStride, int tlIdx, int trIdx, int blIdx, int brIdx)
        {
            var tile = new WriteableBitmap(
                new PixelSize(32, 32),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);

            using var fb = tile.Lock();
            unsafe
            {
                var dst = (uint*)fb.Address;
                int dstStep = fb.RowBytes / 4;

                // Convertimos el array de bytes a un puntero de uint para lectura ultra rápida
                fixed (byte* rawPtr = raw)
                {
                    uint* src = (uint*)rawPtr;
                    int srcStep = srcStride / 4;

                    // Copiar Top-Left (16x16)
                    CopyMiniTile(src, dst, srcStep, dstStep, tlIdx, 0, 0);
                    // Copiar Top-Right (16x16)
                    CopyMiniTile(src, dst, srcStep, dstStep, trIdx, 16, 0);
                    // Copiar Bottom-Left (16x16)
                    CopyMiniTile(src, dst, srcStep, dstStep, blIdx, 0, 16);
                    // Copiar Bottom-Right (16x16)
                    CopyMiniTile(src, dst, srcStep, dstStep, brIdx, 16, 16);
                }
            }
            return tile;
        }

        private static WriteableBitmap CropToWriteable(Bitmap src, int srcX, int srcY, int w, int h)
        {
            int fullStride = src.PixelSize.Width * 4;
            var raw        = new byte[fullStride * src.PixelSize.Height];

            unsafe
            {
                fixed (byte* ptr = raw)
                {
                    src.CopyPixels(
                        new PixelRect(0, 0, src.PixelSize.Width, src.PixelSize.Height),
                        (nint)ptr, raw.Length, fullStride);
                }
            }

            var dst = new WriteableBitmap(
                new PixelSize(w, h),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);

            using var fb = dst.Lock();
            unsafe
            {
                var dstPtr  = (byte*)fb.Address;
                int dstStep = fb.RowBytes;

                for (int row = 0; row < h; row++)
                {
                    var srcSpan = raw.AsSpan((srcY + row) * fullStride + srcX * 4, w * 4);
                    var dstSpan = new Span<byte>(dstPtr + row * dstStep, w * 4);
                    srcSpan.CopyTo(dstSpan);
                }
            }
            return dst;
        }
    }
}
