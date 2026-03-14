using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PokemonEssentialsEditorEvs.Models;
using System.Collections.Generic;

namespace PokemonEssentialsEditorEvs.Controls
{
    public class MapRenderer : Control
    {
        public static readonly StyledProperty<MapMetricsData?> MapMetricsProperty =
            AvaloniaProperty.Register<MapRenderer, MapMetricsData?>(nameof(MapMetrics));

        public MapMetricsData? MapMetrics
        {
            get => GetValue(MapMetricsProperty);
            set => SetValue(MapMetricsProperty, value);
        }

        public static readonly StyledProperty<Bitmap?> TilesetImageProperty =
            AvaloniaProperty.Register<MapRenderer, Bitmap?>(nameof(TilesetImage));

        public Bitmap? TilesetImage
        {
            get => GetValue(TilesetImageProperty);
            set => SetValue(TilesetImageProperty, value);
        }

        public static readonly StyledProperty<List<Bitmap?>?> AutotilesProperty =
            AvaloniaProperty.Register<MapRenderer, List<Bitmap?>?>(nameof(Autotiles));

        public List<Bitmap?>? Autotiles
        {
            get => GetValue(AutotilesProperty);
            set => SetValue(AutotilesProperty, value);
        }

        // Cache de autotiles ya expandidos — se recalcula solo cuando cambia Autotiles
        private Dictionary<int, (WriteableBitmap[] Tiles, bool IsSimple)> _autotileCache = new();

        static MapRenderer()
        {
            AffectsRender<MapRenderer>(MapMetricsProperty, TilesetImageProperty, AutotilesProperty);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == AutotilesProperty)
                BuildAutotileCache();
        }

        private void BuildAutotileCache()
        {
            _autotileCache.Clear();
            if (Autotiles == null) return;

            for (int i = 0; i < Autotiles.Count; i++)
            {
                var bmp = Autotiles[i];
                if (bmp == null) continue;

                int baseId = (i + 1) * 48;
                _autotileCache[baseId] = AutotileProcessor.Expand(bmp);
                bmp.Dispose();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (MapMetrics != null)
                return new Size(MapMetrics.Width * 32, MapMetrics.Height * 32);
            return base.MeasureOverride(availableSize);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (MapMetrics?.TileData == null || TilesetImage == null)
                return;

            var tileData = MapMetrics.TileData;
            int zSize = tileData.Length;
            int ySize = tileData[0].Length;
            int xSize = tileData[0][0].Length;

            for (int z = 0; z < zSize; z++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        int tileId = tileData[z][y][x];
                        if (tileId == 0) continue;

                        // destRect se declara aqui — visible para todos los casos
                        var destRect = new Rect(x * 32, y * 32, 32, 32);

                        if (tileId >= 384)
                        {
                            // Tile normal del tileset
                            int realId = tileId - 384;
                            int srcX   = (realId % 8) * 32;
                            int srcY   = (realId / 8) * 32;
                            context.DrawImage(TilesetImage, new Rect(srcX, srcY, 32, 32), destRect);
                        }
                        else if (tileId >= 48)
                        {
                            // Autotile (IDs 48-383)
                            int baseId = (tileId / 48) * 48;

                            if (_autotileCache.TryGetValue(baseId, out var autotileData))
                            {
                                // RMXP ya guardo la variante correcta en el tile_id al exportar.
                                // tileId % 48 es el indice exacto (0-47), tanto para autotiles
                                // de conexion (agua, caminos) como para individuales (flores, sombras).
                                // GetVariantIndex solo seria necesario si editaramos el mapa en
                                // tiempo real y necesitaramos recalcular bordes — no es el caso aqui.
                                int variantIndex = tileId % 48;

                                context.DrawImage(autotileData.Tiles[variantIndex],
                                    new Rect(0, 0, 32, 32), destRect);
                            }
                        }
                    }
                }
            }

            // Cuadricula de edicion
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 1);
            for (int x = 0; x <= xSize; x++)
                context.DrawLine(pen, new Point(x * 32, 0), new Point(x * 32, ySize * 32));
            for (int y = 0; y <= ySize; y++)
                context.DrawLine(pen, new Point(0, y * 32), new Point(xSize * 32, y * 32));
        }
    }
}