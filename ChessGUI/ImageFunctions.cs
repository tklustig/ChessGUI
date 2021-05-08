using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ChessGUI {
    public class ImageFunctions {

        #region Veraendere Bildgroesse
        /// <summary>
        /// Resizes the image.
        /// </summary>
        /// <param name="img">The image</param>
        /// <param name="targetWidth">Width of the target.</param>
        /// <param name="targetHeight">Height of the target.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>MemoryStream of the resized image.</returns>
        public MemoryStream ResizeImage(Image img, byte targetWidth, byte targetHeight, ImageFormat imageFormat) {
            return CropAndResizeImage(img, targetWidth, targetHeight, 0, 0, img.Width, img.Height, imageFormat);
        }
        #endregion

        #region gekapselte Logik
        /// <summary>
        /// Crops the and resize image.
        /// </summary>
        /// <param name="img">The image</param>
        /// <param name="targetWidth">Width of the target.</param>
        /// <param name="targetHeight">Height of the target.</param>
        /// <param name="x1">The position x1.</param>
        /// <param name="y1">The position y1.</param>
        /// <param name="x2">The position x2.</param>
        /// <param name="y2">The position y2.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>MemoryStream of the cropped and resized image.</returns>
        private MemoryStream CropAndResizeImage(Image img, int targetWidth, int targetHeight, int x1, int y1, int x2, int y2, ImageFormat imageFormat) {
            var bmp = new Bitmap(targetWidth, targetHeight);
            Graphics g = Graphics.FromImage(bmp);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            int width = x2 - x1;
            int height = y2 - y1;

            g.DrawImage(img, new Rectangle(0, 0, targetWidth, targetHeight), x1, y1, width, height, GraphicsUnit.Pixel);

            var memStream = new MemoryStream();
            bmp.Save(memStream, imageFormat);
            return memStream;
        }
        #endregion
    }
}
