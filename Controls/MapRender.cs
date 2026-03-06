using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PokemonEssentialsEditorEvs.Models;

namespace PokemonEssentialsEditorEvs.Controls
{
    public class MapRenderer : Control
    {
        // Propiedad para recibir la matriz de datos
        public static readonly StyledProperty<MapMetricsData?> MapMetricsProperty =
            AvaloniaProperty.Register<MapRenderer, MapMetricsData?>(nameof(MapMetrics));

        public MapMetricsData? MapMetrics
        {
            get => GetValue(MapMetricsProperty);
            set => SetValue(MapMetricsProperty, value);
        }

        // Propiedad para recibir la imagen del tileset
        public static readonly StyledProperty<Bitmap?> TilesetImageProperty =
            AvaloniaProperty.Register<MapRenderer, Bitmap?>(nameof(TilesetImage));

        public Bitmap? TilesetImage
        {
            get => GetValue(TilesetImageProperty);
            set => SetValue(TilesetImageProperty, value);
        }

        // Le decimos a Avalonia que si alguna de estas propiedades cambia, debe redibujar
        static MapRenderer()
        {
            AffectsRender<MapRenderer>(MapMetricsProperty, TilesetImageProperty);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (MapMetrics != null)
            {
                // Devolvemos el tamaño real del mapa en píxeles
                return new Size(MapMetrics.Width * 32, MapMetrics.Height * 32);
            }
            return base.MeasureOverride(availableSize);
        }
        // Aquí ocurre la magia del renderizado acelerado por hardware
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (MapMetrics?.TileData == null || TilesetImage == null)
                return;

            var tileData = MapMetrics.TileData;
            int zSize = tileData.Length;
            int ySize = tileData[0].Length;
            int xSize = tileData[0][0].Length;

            // Dibujamos capa por capa (Z), fila por fila (Y), columna por columna (X)
            for (int z = 0; z < zSize; z++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        int tileId = tileData[z][y][x];

                        if (tileId == 0) continue; // 0 es transparente

                        if (tileId >= 384)
                        {
                            // Es un tile normal. Calculamos su posición en la imagen original
                            int realId = tileId - 384;
                            int srcX = (realId % 8) * 32;
                            int srcY = (realId / 8) * 32;

                            // Rectángulo de origen (el pedacito de la imagen)
                            var srcRect = new Rect(srcX, srcY, 32, 32);
                            // Rectángulo de destino (dónde lo dibujamos en la pantalla)
                            var destRect = new Rect(x * 32, y * 32, 32, 32);

                            // ¡Estampamos el gráfico!
                            context.DrawImage(TilesetImage, srcRect, destRect);
                        }
                        else
                        {
                            // Es un Autotile (IDs 48 al 383). 
                            // Como su lógica es compleja, por ahora dibujaremos un cuadro semitransparente 
                            // para saber que ahí hay "algo" (ej. agua o un camino).
                            var destRect = new Rect(x * 32, y * 32, 32, 32);
                            context.FillRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)), destRect);
                        }
                    }
                }
            }

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)), 1);
            // Dibujamos una cuadrícula encima para facilitar la edición

            for (int x = 0; x <= xSize; x++)
            {
                context.DrawLine(pen, new Point(x * 32, 0), new Point(x * 32, ySize * 32));
            }
            for (int y = 0; y <= ySize; y++)
            {
                context.DrawLine(pen, new Point(0, y * 32), new Point(xSize * 32, y * 32));
            }
        }
        
    }
}