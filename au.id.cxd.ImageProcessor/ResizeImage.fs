namespace au.id.cxd.ImageProcessor

open System
open System.IO
open System.Drawing
open Microsoft.FSharp.Math

module ResizeImage =
    
    /// <summary>
    /// get images for a directory
    /// retrieves jpg and png images
    /// </summary>
    let readDir dir = 
           let dirInfo = new DirectoryInfo(dir)
           Array.append (dirInfo.GetFiles("*.jpg", SearchOption.TopDirectoryOnly)) (dirInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly))

    
    /// <summary>
    /// Resize an image at a path
    /// return the bitmap resized
    /// </summary>
    let resizeImage percent (path:string) (newpath:string) =
            let original = new Bitmap(path)
            let w = Convert.ToDouble(original.Width) * percent
            let h = Convert.ToDouble(original.Height) * percent
            let copy = new Bitmap(original, Convert.ToInt32(w), Convert.ToInt32(h))
            let qualityParam = new Imaging.EncoderParameter(Imaging.Encoder.Quality, 85L)
            // Jpeg image codec
            let jpegCodec = Imaging.ImageCodecInfo.GetImageEncoders().[1]             
            let encoderParams = [|qualityParam|]
            let encparams = new Imaging.EncoderParameters(Param = encoderParams)
            copy.Save(newpath, jpegCodec, encparams)


    /// <summary>
    /// Resize all images in a given directory to a supplied percent.
    /// </summary>
    let resizeDirectory percent (dir:string) =
           Array.iter (fun (item:FileInfo) -> 
                            Console.WriteLine("{0}", item.FullName)
                            resizeImage percent item.FullName (item.FullName.ToLower().Replace(".jpg", "_resize.jpg").Replace(".png", "_resize.jpg"))) (readDir dir)

     
    